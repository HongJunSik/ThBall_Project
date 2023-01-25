using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 클라용 프로토콜과 클라용은 다름
/// 
/// </summary>
namespace DefineServerUtility
{
    public enum eSendMessage
    {
        Connect_GivingUUID = 0,
        IDcheck,
        LoginInfo,
        Login,
        NickNameSet,
    }
    public enum eReceiveMessage
    {
        GivingUUID = 0,
        IDcheck,
        LoginInfo,
        Login,
        NickNameSet,
    }
}
