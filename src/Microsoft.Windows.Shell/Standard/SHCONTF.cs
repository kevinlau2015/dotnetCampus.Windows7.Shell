﻿namespace Standard
{
    internal enum SHCONTF
    {
        CHECKING_FOR_CHILDREN = 16, // 0x00000010
        FOLDERS = 32, // 0x00000020
        NONFOLDERS = 64, // 0x00000040
        INCLUDEHIDDEN = 128, // 0x00000080
        INIT_ON_FIRST_NEXT = 256, // 0x00000100
        NETPRINTERSRCH = 512, // 0x00000200
        SHAREABLE = 1024, // 0x00000400
        STORAGE = 2048, // 0x00000800
        NAVIGATION_ENUM = 4096, // 0x00001000
        FASTITEMS = 8192, // 0x00002000
        FLATLIST = 16384, // 0x00004000
        ENABLE_ASYNC = 32768, // 0x00008000
    }
}
