using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfos : TSingleton<UserInfos>
{
    long UUID;
    string ID;
    string PW;
    string NICKNAME;

    public long _myUUID
    {
        get { return UUID; }
    }
    public string _myID
    {
        get { return ID; }
    }
    public string _myPW
    {
        get { return PW; }
    }
    public string _myNick
    {
        get { return NICKNAME; }
    }
    protected override void Init()
    {
        base.Init(); 
    }

    public void SetLoginInfo(long uuid,string id, string pw)
    {
        UUID = uuid;
        ID = id;
        PW = pw;
        NICKNAME = string.Empty;
    }
    public void SetNickNameInfo(string nickname)
    {
        NICKNAME = nickname;
    }
}
