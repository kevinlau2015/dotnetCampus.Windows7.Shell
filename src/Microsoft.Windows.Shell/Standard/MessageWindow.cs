﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace Standard
{
    internal sealed class MessageWindow : DispatcherObject, IDisposable
    {
        private static readonly WndProc s_WndProc = new WndProc(MessageWindow._WndProc);
        private static readonly Dictionary<IntPtr, MessageWindow> s_windowLookup = new Dictionary<IntPtr, MessageWindow>();
        private WndProc _wndProcCallback;
        private string _className;
        private bool _isDisposed;

        public IntPtr Handle { get; private set; }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public MessageWindow(
          CS classStyle,
          WS style,
          WS_EX exStyle,
          Rect location,
          string name,
          WndProc callback)
        {
            this._wndProcCallback = callback;
            this._className = "MessageWindowClass+" + Guid.NewGuid().ToString();
            WNDCLASSEX lpwcx = new WNDCLASSEX()
            {
                cbSize = Marshal.SizeOf(typeof(WNDCLASSEX)),
                style = classStyle,
                lpfnWndProc = MessageWindow.s_WndProc,
                hInstance = NativeMethods.GetModuleHandle((string)null),
                hbrBackground = NativeMethods.GetStockObject(StockObject.NULL_BRUSH),
                lpszMenuName = "",
                lpszClassName = this._className
            };
            int num = (int)NativeMethods.RegisterClassEx(ref lpwcx);
            GCHandle gcHandle = new GCHandle();
            try
            {
                gcHandle = GCHandle.Alloc((object)this);
                IntPtr lpParam = (IntPtr)gcHandle;
                this.Handle = NativeMethods.CreateWindowEx(exStyle, this._className, name, style, (int)location.X, (int)location.Y, (int)location.Width, (int)location.Height, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, lpParam);
            }
            finally
            {
                gcHandle.Free();
            }
        }

        ~MessageWindow() => this._Dispose(false, false);

        public void Dispose()
        {
            this._Dispose(true, false);
            GC.SuppressFinalize((object)this);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing")]
        private void _Dispose(bool disposing, bool isHwndBeingDestroyed)
        {
            if (this._isDisposed)
                return;
            this._isDisposed = true;
            IntPtr hwnd = this.Handle;
            string className = this._className;
            if (isHwndBeingDestroyed)
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => MessageWindow._DestroyWindow(IntPtr.Zero, className)));
            else if (this.Handle != IntPtr.Zero)
            {
                if (this.CheckAccess())
                    MessageWindow._DestroyWindow(hwnd, className);
                else
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => MessageWindow._DestroyWindow(hwnd, className)));
            }
            MessageWindow.s_windowLookup.Remove(hwnd);
            this._className = (string)null;
            this.Handle = IntPtr.Zero;
        }

        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
        private static IntPtr _WndProc(IntPtr hwnd, WM msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr zero = IntPtr.Zero;
            MessageWindow messageWindow = (MessageWindow)null;
            if (msg == WM.CREATE)
            {
                messageWindow = (MessageWindow)GCHandle.FromIntPtr(((CREATESTRUCT)Marshal.PtrToStructure(lParam, typeof(CREATESTRUCT))).lpCreateParams).Target;
                MessageWindow.s_windowLookup.Add(hwnd, messageWindow);
            }
            else if (!MessageWindow.s_windowLookup.TryGetValue(hwnd, out messageWindow))
                return NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam);
            Assert.IsNotNull<MessageWindow>(messageWindow);
            WndProc wndProcCallback = messageWindow._wndProcCallback;
            IntPtr num = wndProcCallback == null ? NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam) : wndProcCallback(hwnd, msg, wParam, lParam);
            if (msg == WM.NCDESTROY)
            {
                messageWindow._Dispose(true, true);
                GC.SuppressFinalize((object)messageWindow);
            }
            return num;
        }

        private static object _DestroyWindow(IntPtr hwnd, string className)
        {
            Utility.SafeDestroyWindow(ref hwnd);
            NativeMethods.UnregisterClass(className, NativeMethods.GetModuleHandle((string)null));
            return (object)null;
        }
    }
}
