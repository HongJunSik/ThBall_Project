using DefineServerUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
public class ServerManager : TSingleton<ServerManager>
{
    string _ip;
    const short _port = 9999;

    Socket _sock;
    Queue<Packet> _sendQ = new Queue<Packet>();
    Queue<Packet> _recvQ = new Queue<Packet>();

    bool _isConnectFailed = false;
    int _retryCount = 3;
    int _tryReceive = 0;

    bool _isEnd = false;
    long _uuid;

    protected override void Init()
    {
        base.Init();
    }

    void Start()
    {
        SendFuncStart();
        RecvFuncStart();
    }

    // 서버에서 데이터 받아오기
    void Update()
    {
        if (_sock != null && _sock.Connected)
        {
            if (_sock.Poll(0, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[ConvertPacketFunc._maxByte];

                int recvLength = _sock.Receive(buffer);
                if (recvLength > 0)
                {
                    _recvQ.Enqueue((Packet)ConvertPacketFunc.ByteArrayToStructure(buffer, typeof(Packet), buffer.Length));
                }
            }
        }
        else
        {
            // 로딩 프로세스......
            if (_isConnectFailed)
            {
                // 메세지창을 띄우고 메세지창을 클릭하면 앱 종료.
                Debug.Log("서버가 어디갔지...?");
            }
            else
            {
                // 로딩이 계속 돈다.
                Debug.Log("아무 일도 없다...");
            }
        }
    }

    #region[서버에 연결하기]
    public void NetConnect()
    {
        //내 ip주소 가져오는 코드
        IPHostEntry iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in iPHostEntry.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                _ip = ip.ToString();
            }
        }
        StartCoroutine(Connectings(_ip, _port));
    }
    IEnumerator Connectings(string ipAddr, short port)
    {
        int cnt = 0;
        while (true)
        {
            try
            {
                _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                //_sock.Connect(ipAddr, port);
                _sock.Connect(new IPEndPoint(IPAddress.Parse(ipAddr), port));
                //_sock.Connect(new IPEndPoint(IPAddress.Loopback, port));
                SceneControlManager._instance.StartLoginScene();
                break;
            }
            catch (Exception ex)
            {
                // 서버와 접속이 되지 않습니다.... 메세지 출력.
                cnt++;
                if (cnt > _retryCount)
                {
                    Debug.Log(ex.Message);
                    _isConnectFailed = true;
                    break;
                }
            }
            if (_isConnectFailed)
            {
                yield return new WaitForSeconds(3);
            }
        }
    }
    #endregion

    #region[서버와 send & receive]
    void SendFuncStart()
    {
        StartCoroutine(SendProcess());
    }
    void RecvFuncStart()
    {
        StartCoroutine(ReceiveProcess());
    }
    IEnumerator SendProcess()
    {
        while (!_isEnd)
        {
            if (_sendQ.Count > 0)
            {
                Packet packet = _sendQ.Dequeue();
                byte[] buffer = ConvertPacketFunc.StructureToByteArray(packet);
                _sock.Send(buffer);
            }
            yield return new WaitForSeconds(_tryReceive);
        }
    }
    IEnumerator ReceiveProcess()
    {
        while (!_isEnd)
        {
            if (_recvQ.Count > 0)
            {
                Packet packet = _recvQ.Dequeue();
                switch ((eReceiveMessage)packet._protocolID)
                {
                    case eReceiveMessage.GivingUUID:
                        {
                            ReceiveConnectGivingUUID(packet);
                            break;
                        }
                    case eReceiveMessage.IDcheck:
                        {
                            ReceiveCheckID(packet);
                            break;
                        }
                    case eReceiveMessage.LoginInfo:
                        {
                            ReceiveLoginInfoResult(packet);
                            break;
                        }
                    case eReceiveMessage.Login:
                        {
                            ReceiveLoginSuccess(packet);
                            break;
                        }
                    case eReceiveMessage.NickNameSet:
                        {
                            ReceiveNicknameResult(packet);
                            break;
                        }
                }
            }
            yield return new WaitForSeconds(_tryReceive);
        }
    }
    #endregion

    #region[서버로 보내는 함수]
    public void SendCheckID(string id) // 아이디 중복체크
    {
        Send_IDcheck send_IDcheck;
        send_IDcheck._ID = id;
        send_IDcheck._UUID = _uuid;

        byte[] data = ConvertPacketFunc.StructureToByteArray(send_IDcheck);
        Packet send = ConvertPacketFunc.CreatePack((int)eSendMessage.IDcheck, _uuid, data.Length, data);

        _sendQ.Enqueue(send);
    }
    public void SendLoginInfo() // 로그인 정보 보내기
    {
        UserInfos._instance.SetLoginInfo(_uuid, PlayerPrefs.GetString("ID"), PlayerPrefs.GetString("PW"));
        Send_LoginInfo send_LoginInfo;
        send_LoginInfo._UUID = _uuid;
        send_LoginInfo._ID = UserInfos._instance._myID;
        send_LoginInfo._PW = UserInfos._instance._myPW;
        send_LoginInfo._NICKNAME = UserInfos._instance._myNick;

        byte[] data = ConvertPacketFunc.StructureToByteArray(send_LoginInfo);
        Packet send = ConvertPacketFunc.CreatePack((int)eSendMessage.LoginInfo, _uuid, data.Length, data);

        _sendQ.Enqueue(send);
    }
    public void SendLogin(string id, string pw)
    {
        Send_Login send_Login;
        send_Login._UUID = _uuid;
        send_Login._ID = id;
        send_Login._PW = pw;
        send_Login._NICKNAME = "";

        byte[] data = ConvertPacketFunc.StructureToByteArray(send_Login);
        Packet send = ConvertPacketFunc.CreatePack((int)eSendMessage.Login, _uuid, data.Length, data);

        _sendQ.Enqueue(send);
    }
    public void SendSetNickname(string nick)
    {
        Send_NickNameSet nickSet;
        nickSet._UUID = _uuid;
        nickSet._ID = UserInfos._instance._myID;
        nickSet._NICKNAME = nick;

        byte[] data = ConvertPacketFunc.StructureToByteArray(nickSet);
        Packet send = ConvertPacketFunc.CreatePack((int)eSendMessage.NickNameSet, _uuid, data.Length, data);

        _sendQ.Enqueue(send);
    }
    #endregion

    #region[받은 리시브 처리함수]
    void ReceiveConnectGivingUUID(Packet packet) // 서버에 연결 시 받음
    {
        _uuid = packet._targetID;
    }
    void ReceiveCheckID(Packet packet)
    {
        Receive_IDcheck receive_IDcheck = (Receive_IDcheck)ConvertPacketFunc.ByteArrayToStructure(packet._datas, typeof(Receive_IDcheck), packet._totalSize);
        LoginSceneUI loginScene = GameObject.Find("LoginSceneUI").GetComponent<LoginSceneUI>();
        loginScene.IDcheck(receive_IDcheck._ID);
    }
    void ReceiveLoginInfoResult(Packet packet)
    {
        LoginSceneUI loginScene = GameObject.Find("LoginSceneUI").GetComponent<LoginSceneUI>();
        loginScene.CreateSuccess(true);
    }
    void ReceiveLoginSuccess(Packet packet)
    {
        Receive_Login success = (Receive_Login)ConvertPacketFunc.ByteArrayToStructure(packet._datas, typeof(Receive_Login), packet._totalSize);
        LoginSceneUI loginScene = GameObject.Find("LoginSceneUI").GetComponent<LoginSceneUI>();
        loginScene.LoginSuccess(success._NICKNAME, success._UUID, success._ID, success._PW);
    }
    void ReceiveNicknameResult(Packet packet)
    {
        Receive_NickNameSet result = (Receive_NickNameSet)ConvertPacketFunc.ByteArrayToStructure(packet._datas, typeof(Receive_NickNameSet), packet._totalSize);

        LobbySceneUI lobbyScene = GameObject.Find("LobbySceneUI").GetComponent<LobbySceneUI>();
        lobbyScene.NickSetResult(result._NICKNAME.Equals("1"));
    }
    #endregion
}
