using DefineServerUtility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace NetworkTest
{
    public struct stSocket
    {
        public long _uuid;
        public string _id;
        public string _pw;
        public string _nick;
        public Socket _client;


        public stSocket(long uu, Socket socket)
        {
            _uuid = uu;
            _id = string.Empty;
            _pw = string.Empty;
            _nick = string.Empty;
            _client = socket;
        }
    }
    class TCPGserver
    {
        DatabaseManager dbManager;

        short _port;
        long _nowUUID;
        bool _isEnd = false;

        Socket _waitServer;
        Thread _sendThread;
        Thread _receiveThread;

        Queue<Packet> _sendQ = new Queue<Packet>();
        Queue<Packet> _receiveQ = new Queue<Packet>();
        Dictionary<long, stSocket> _clients = new Dictionary<long, stSocket>();
        public void SetDBmanager(DatabaseManager manager, long uuid)
        {
            dbManager = manager;
            _nowUUID = uuid;
        }

        public TCPGserver(short port)
        {
            _port = port;
            try
            {
                _waitServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _waitServer.Bind(new IPEndPoint(IPAddress.Any, _port));
                _waitServer.Listen(ConvertPacketFunc._maxPerson);

                Console.WriteLine("소켓 생성 성공");
            }
            catch (Exception ex)
            {
                Console.WriteLine("소켓 생성 실패");
                Console.WriteLine(ex.Message);

                return;
            }
            _isEnd = false;
            _sendThread = new Thread(() => SendProcess());
            _receiveThread = new Thread(() => ReceiveProcess());

            _sendThread.Start();
            _receiveThread.Start();
        }
        ~TCPGserver()
        {
            ReleaseServer();
        }
        public void ReleaseServer()
        {
            _isEnd = true;
            //_sendThread.Abort();
            //_receiveThread.Abort();
            Console.WriteLine("서버가 닫혔습니다.");
        }
        /// <summary>
        /// 클라이언트에서 종료한 애들
        /// </summary>
        public void CloseSocketProcess()
        {
            long checkKey = 0;
            foreach (long key in _clients.Keys)
            {
                if (_clients[key]._client == null)
                {
                    //remove
                }
            }
        }
        public bool MainProcess()
        {
            Thread.Sleep(500);
            if (_waitServer.Poll(0, SelectMode.SelectRead))
            {
                // 유니티에서 플레이를 눌렀을때
                Console.WriteLine("서버에 클라가 접속했습니다.");

                stSocket newSocket = new stSocket(++_nowUUID, _waitServer.Accept());

                // newSocket에게 아이디를 알려주도록 한다.
                Send_ConnectSuccess subPack;
                subPack._UUID = _nowUUID;

                Packet send = ConvertPacketFunc.CreatePack((int)eSendMessage.Connect_GivingUUID, newSocket._uuid, Marshal.SizeOf(subPack), ConvertPacketFunc.StructureToByteArray(subPack));
                _sendQ.Enqueue(send);
                _clients.Add(newSocket._uuid, newSocket);
            }
            // 리시브 받은걸 처리
            while (_receiveQ.Count > 0)
            {
                Packet packet = _receiveQ.Dequeue();
                switch ((eReceiveMessage)packet._protocolID)
                {
                    case eReceiveMessage.IDcheck:
                        {
                            ReceiveIDcheck(packet);
                            break;
                        }
                    case eReceiveMessage.LoginInfo:
                        {
                            ReceiveLoginInfo(packet);
                            break;
                        }
                    case eReceiveMessage.Login:
                        {
                            ReceiveLogin(packet);
                            break;
                        }
                    case eReceiveMessage.NickNameSet:
                        {
                            ReceiveNickNameSet(packet);
                            break;
                        }
                }
            }
            if (Console.KeyAvailable)
            {
                Console.WriteLine("ESC 누르면 종료함");
                ConsoleKeyInfo keys = Console.ReadKey(true);
                if (keys.Key == ConsoleKey.Escape)
                {
                    return false;
                }
            }
            return true;
        }
        void SendProcess()
        {
            while (!_isEnd)
            {
                if (_sendQ.Count > 0)
                {
                    Packet pack = _sendQ.Dequeue();
                    byte[] buffer = ConvertPacketFunc.StructureToByteArray(pack);
                    if (_clients.ContainsKey(pack._targetID))
                    {
                        Console.WriteLine("(Send) {0}에게 <{1}>를 전송..", pack._targetID, (eSendMessage)pack._protocolID);
                        _clients[pack._targetID]._client.Send(buffer);
                    }
                    else
                    {
                        Console.WriteLine("샌드 프로세스 에러");
                    }
                }
                Thread.Sleep(20);
            }
        }
        // 보내는걸 받아서 receiveQ에 넣음
        void ReceiveProcess()
        {
            while (!_isEnd)
            {
                foreach (long var in _clients.Keys.ToList()) // foreach에서 딕셔너리 값 수정 시 에러 발생하여 list에 저장
                {
                    if (_clients[var]._client != null && _clients[var]._client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buffer = new byte[ConvertPacketFunc._maxByte];
                        try
                        {
                            int recvLength = _clients[var]._client.Receive(buffer);
                            if (recvLength > 0)
                            {
                                Packet pack = (Packet)ConvertPacketFunc.ByteArrayToStructure(buffer, typeof(Packet), buffer.Length);
                                Console.WriteLine("(Receive) {0}의 <{1}>를 받음", pack._targetID, (eReceiveMessage)pack._protocolID);
                                _receiveQ.Enqueue(pack);
                            }
                            else
                            {
                                Console.WriteLine("(Receive) {0}가 나갔습니다.", var);
                                _clients.Remove(var);
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("리시브 실패!");
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                Thread.Sleep(20);
            }
        }
        #region[아이이 중복확인]
        void ReceiveIDcheck(Packet packet)
        {
            Receive_IDcheck receive_IDcheck = (Receive_IDcheck)ConvertPacketFunc.ByteArrayToStructure(packet._datas, typeof(Receive_IDcheck), packet._totalSize);
            stQueryInfo queryInfo;
            queryInfo._targetID = packet._targetID;
            queryInfo._menuNum = (int)eDBFunction.IDCHECK;
            queryInfo._menuText = DefineDBValue._idCheck + "&" + receive_IDcheck._ID + "&" + receive_IDcheck._UUID;

            dbManager.SendAdd(queryInfo);
        }
        public void SendIDcheck(bool result, long uuid, string id)
        {
            byte[] data;
            Packet send;
            Send_IDcheck send_IDcheck;
            if (result)
            {
                send_IDcheck._ID = id;
                send_IDcheck._UUID = uuid;

                data = ConvertPacketFunc.StructureToByteArray(send_IDcheck);
                send = (Packet)ConvertPacketFunc.CreatePack((int)eSendMessage.IDcheck, send_IDcheck._UUID, data.Length, data);
                _sendQ.Enqueue(send);
            }
            else
            {
                send_IDcheck._ID = string.Empty;
                send_IDcheck._UUID = uuid;

                data = ConvertPacketFunc.StructureToByteArray(send_IDcheck);
                send = (Packet)ConvertPacketFunc.CreatePack((int)eSendMessage.IDcheck, send_IDcheck._UUID, data.Length, data);
                _sendQ.Enqueue(send);
            }
        }
        #endregion

        // DB에 회원정보 저장
        void ReceiveLoginInfo(Packet packet)
        {
            Receive_LoginInfo receive_LoginInfo = (Receive_LoginInfo)ConvertPacketFunc.ByteArrayToStructure(packet._datas, typeof(Receive_LoginInfo), packet._totalSize);

            byte[] data;
            Packet send;
            Send_LoginInfo result;
            result._UUID = receive_LoginInfo._UUID;
            result._ID = receive_LoginInfo._ID;
            result._PW = receive_LoginInfo._PW;
            result._NICKNAME = "O";

            data = ConvertPacketFunc.StructureToByteArray(result);
            send = (Packet)ConvertPacketFunc.CreatePack((int)eSendMessage.LoginInfo, result._UUID, data.Length, data);
            _sendQ.Enqueue(send);

            stQueryInfo newInfo = new stQueryInfo();
            newInfo._menuNum = (int)eDBFunction.INSERT;
            newInfo._menuText = string.Format(DefineDBValue._insertLoginInfo, receive_LoginInfo._UUID, receive_LoginInfo._ID, receive_LoginInfo._PW);
            dbManager.SendAdd(newInfo);

            Console.WriteLine("DB에 회원가입 정보 저장");
            return;
        }

        #region[로그인 확인]
        void ReceiveLogin(Packet packet)
        {
            Receive_Login receive_Login = (Receive_Login)ConvertPacketFunc.ByteArrayToStructure(packet._datas, typeof(Receive_Login), packet._totalSize);

            byte[] data;
            Packet send;
            Send_Login success;


            // 동시접속 체크
            foreach (long var in _clients.Keys)
            {
                if (_clients[var]._id.Equals(receive_Login._ID) && var != receive_Login._UUID)
                {
                    success._UUID = packet._targetID;
                    success._ID = receive_Login._ID;
                    success._PW = receive_Login._PW;
                    success._NICKNAME = "0";

                    data = ConvertPacketFunc.StructureToByteArray(success);
                    send = (Packet)ConvertPacketFunc.CreatePack((int)eSendMessage.Login, success._UUID, data.Length, data);
                    _sendQ.Enqueue(send);
                    Console.WriteLine("이미 접속중인 ID입니다.");
                    return;
                }
            }

            // 서버 리스트에 정보저장
            foreach (long var in _clients.Keys.ToList())
            {
                if (var == receive_Login._UUID)
                {
                    stSocket socket = _clients[var];
                    socket._id = receive_Login._ID;
                    socket._pw = receive_Login._PW;

                    _clients[var] = socket;
                }
            }

            stQueryInfo queryInfo;
            queryInfo._targetID = packet._targetID;
            queryInfo._menuNum = (int)eDBFunction.LOGINCHECK;
            queryInfo._menuText = DefineDBValue._loginCheck + "&" + receive_Login._UUID + "&" + receive_Login._ID + "&" + receive_Login._PW;
            dbManager.SendAdd(queryInfo);
        }
        public void SendLoginSuccess(bool result, long uuid, string id, string pw, string nick)
        {
            byte[] data;
            Packet send;
            Receive_Login success;
            if (result)
            {
                success._UUID = uuid;
                success._ID = id;
                success._PW = pw;
                success._NICKNAME = nick;

                data = ConvertPacketFunc.StructureToByteArray(success);
                send = (Packet)ConvertPacketFunc.CreatePack((int)eSendMessage.Login, success._UUID, data.Length, data);
                _sendQ.Enqueue(send);
            }
            else
            {
                success._UUID = uuid;
                success._ID = id;
                success._PW = pw;
                success._NICKNAME = "1";

                data = ConvertPacketFunc.StructureToByteArray(success);
                send = (Packet)ConvertPacketFunc.CreatePack((int)eSendMessage.Login, success._UUID, data.Length, data);
                _sendQ.Enqueue(send);
            }
        }
        #endregion

        void ReceiveNickNameSet(Packet packet)
        {
            Receive_NickNameSet nickSet = (Receive_NickNameSet)ConvertPacketFunc.ByteArrayToStructure(packet._datas, typeof(Receive_NickNameSet), packet._totalSize);

            stQueryInfo queryInfo;
            queryInfo._targetID = packet._targetID;
            queryInfo._menuNum = (int)eDBFunction.SETNICKNAME;
            queryInfo._menuText = DefineDBValue._nicknameCheck + "&" + nickSet._UUID + "&" + nickSet._ID + "&" + nickSet._NICKNAME;
            dbManager.SendAdd(queryInfo);
        }
        public void SendNicknameSet(bool result, long uuid, string id)
        {
            byte[] data;
            Packet send;
            Send_NickNameSet nickResult;

            nickResult._UUID = uuid;
            nickResult._ID = id;
            nickResult._NICKNAME = result ? "1" : "-1";

            data = ConvertPacketFunc.StructureToByteArray(nickResult);
            send = (Packet)ConvertPacketFunc.CreatePack((int)eSendMessage.NickNameSet, uuid, data.Length, data);
            _sendQ.Enqueue(send);
        }
    }
}
