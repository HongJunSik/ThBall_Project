using System;
using System.Collections.Generic;

namespace DefineServerUtility
{
    public enum eSendMessage
    {
        Connect_GivingUUID = 0,
        IDcheck,
        LoginInfo,
        //LoginInfo_Result,
        Login,
        //Login_Success,
        NickNameSet,
        //NickNameResult,
    }
    public enum eReceiveMessage
    {
        GivingUUID = 0,
        IDcheck,
        LoginInfo,
        //LoginInfo_Result,
        Login,
        //Login_Success,
        NickNameSet,
        //NickNameResult,
    }
}
