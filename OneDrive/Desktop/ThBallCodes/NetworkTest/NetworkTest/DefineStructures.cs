using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DefineServerUtility
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Packet
    {
        // 프로토콜 넘버.
        [MarshalAs(UnmanagedType.U4)] public int _protocolID;
        // _data에 들어가는 구조체의 실질 메모리 크기.
        [MarshalAs(UnmanagedType.U2)] public short _totalSize;
        // 신호를 받을 주체
        [MarshalAs(UnmanagedType.U8)] public long _targetID;
        // 실제 정보 구조체
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1002)] public byte[] _datas;
    }

    #region[Send Struct]
    [StructLayout(LayoutKind.Sequential)]
    public struct Send_ConnectSuccess
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Send_IDcheck
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _ID;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Send_LoginInfo
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _ID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _PW;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)] public string _NICKNAME;
    }
    //[StructLayout(LayoutKind.Sequential)]
    //public struct Send_LoginInfoResult
    //{
    //    [MarshalAs(UnmanagedType.U8)] public long _UUID;
    //    [MarshalAs(UnmanagedType.Bool)] public bool _RESULT;
    //}
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Send_Login
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _ID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _PW;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)] public string _NICKNAME;
    }
    //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    //public struct Send_Login_Success
    //{
    //    [MarshalAs(UnmanagedType.U8)] public long _UUID;
    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _ID;
    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _PW;
    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)] public string _NICKNAME;
    //}
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Send_NickNameSet
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _ID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)] public string _NICKNAME;
    }
    //[StructLayout(LayoutKind.Sequential)]
    //public struct Send_NickNameResult
    //{
    //    [MarshalAs(UnmanagedType.U8)] public long _UUID;
    //    [MarshalAs(UnmanagedType.Bool)] public bool _RESULT;
    //}
    #endregion

    #region[Receive Struct]
    [StructLayout(LayoutKind.Sequential)]
    public struct Receive_ConnectSuccess
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Receive_IDcheck
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _ID;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Receive_LoginInfo
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _ID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _PW;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)] public string _NICKNAME;
    }
    //[StructLayout(LayoutKind.Sequential)]
    //public struct Receive_LoginInfoResult
    //{
    //    [MarshalAs(UnmanagedType.U8)] public long _UUID;
    //    [MarshalAs(UnmanagedType.Bool)] public bool _RESULT;
    //}
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Receive_Login
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _ID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _PW;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)] public string _NICKNAME;
    }
    //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    //public struct Receive_Login_Success
    //{
    //    [MarshalAs(UnmanagedType.U8)] public long _UUID;
    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _ID;
    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _PW;
    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)] public string _NICKNAME;
    //}
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Receive_NickNameSet
    {
        [MarshalAs(UnmanagedType.U8)] public long _UUID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string _ID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)] public string _NICKNAME;
    }
    //[StructLayout(LayoutKind.Sequential)]
    //public struct Receive_NickNameResult
    //{
    //    [MarshalAs(UnmanagedType.U8)] public long _UUID;
    //    [MarshalAs(UnmanagedType.Bool)] public bool _RESULT;
    //}
    #endregion
}
