using DefineServerUtility;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace NetworkTest
{
    public struct stQueryInfo
    {
        public long _targetID; // 기능 불러오기
        public int _menuNum; // 기능 불러오기
        public string _menuText; // Query 저장

        public stQueryInfo(long target,int no, string txt)
        {
            _targetID = target;
            _menuNum = no;
            _menuText = txt;
        }
    }
    class DatabaseManager
    {
        TCPGserver servermanager;
        Queue<stQueryInfo> _menuQ = new Queue<stQueryInfo>();

        string _dbName;//db이름

        MySqlConnection _connection;
        MySqlCommand cmd;
        MySqlDataReader table;

        public string Query;//sql 명령어
        public List<long> uuids = new List<long>();//primary key 저장

        public eDBFunction _selectMenu;

        Thread _runThread;

        public bool _isRun
        { get; set; }

        public DatabaseManager(string dbName, TCPGserver server)
        {
            _dbName = dbName;
            servermanager = server;
            Query = string.Empty;
        }
        //DB 연결
        public bool ConnectDB(string id, string pw)
        {
            string connectInfoText = "Server=" + DefineDBValue._baselocalIP + ";" + "Port=" + DefineDBValue._port.ToString() + ";" + "DataBase=" + _dbName + ";" + "Uid=" + id + ";" + "Pwd=" + pw;
            _connection = new MySqlConnection(connectInfoText);
            try
            {
                _connection.Open();
                _isRun = true;
                _selectMenu = eDBFunction.WAIT;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connect Failed!!");
                //Console.WriteLine(ex.ToString());
                return false;
            }
        }
        //DB연결 종료
        public void TerminateConnection()
        {
            _connection.Close();
            Console.WriteLine("DB서버 접속을 종료합니다.");
        }
        public void Run()
        {
            while (_isRun)
            {
                if (_menuQ.Count > 0)
                {
                    stQueryInfo info = _menuQ.Dequeue();
                    _selectMenu = (eDBFunction)(info._menuNum);
                    Query = info._menuText;

                    switch (_selectMenu)
                    {
                        case eDBFunction.INSERT:
                            InsertData(Query);
                            break;
                        case eDBFunction.DELETE:
                            DeleteData(Query);
                            break;
                        case eDBFunction.IDCHECK:
                            {
                                IDcheck(Query);
                                break;
                            }
                        case eDBFunction.LOGINCHECK:
                            {
                                Logincheck(Query);
                                break;
                            }
                        case eDBFunction.SETNICKNAME:
                            {
                                NickNameSet(Query);
                                break;
                            }
                        case eDBFunction.QUIT:
                            _isRun = false;
                            break;
                    }
                }

            }
        }
        public void InsertData(string vals)
        {
            cmd = new MySqlCommand(vals, _connection);
            if (cmd.ExecuteNonQuery() == 1)
            {
                Console.WriteLine("Insert Success");
            }
            else
            {
                Console.WriteLine("Insert Failed");
            }
            _selectMenu = eDBFunction.WAIT;
        }
        public void DeleteData(string vals)
        {
            //삭제하기
            cmd.CommandText = vals;
            if (cmd.ExecuteNonQuery() == 1)
            {
                Console.WriteLine("Delete Success");
            }
            else
            {
                Console.WriteLine("Delete Failed");
            }
            _selectMenu = eDBFunction.WAIT;
        }

        // Run 스레드 생성 및 실행
        public void RunStart()
        {
            _runThread = new Thread(() => Run());
            _runThread.Start();
        }

        //큐로 명령어 받아오기
        public void SendAdd(stQueryInfo info)
        {
            _menuQ.Enqueue(info);
        }
        #region[기타 함수]
        public long SetUUID()
        {
            uuids.Clear();
            string sql = "select UUID from memberinfo";

            cmd = new MySqlCommand(sql, _connection);
            table = cmd.ExecuteReader();
            while (table.Read())
            {
                for (int n = 0; n < table.FieldCount; n++)
                {
                    uuids.Add(long.Parse(table.GetValue(n).ToString()));
                }
            }
            table.Close();
            return (uuids.Count == 0) ? 10000000000 : uuids[uuids.Count - 1];
        }

        #endregion

        void IDcheck(string vals)
        {
            string sql = vals.Split('&')[0];
            string id = vals.Split('&')[1];
            long uuid = long.Parse(vals.Split('&')[2]);
            cmd = new MySqlCommand(sql, _connection);
            table = cmd.ExecuteReader();
            while (table.Read())
            {
                for (int n = 0; n < table.FieldCount; n++)
                {
                    if (id.Equals(table.GetValue(n)))
                    {
                        table.Close();
                        servermanager.SendIDcheck(false, uuid, id);
                        return;
                    }
                }
            }
            table.Close();
            servermanager.SendIDcheck(true, uuid, id);
            return;
        }
        void Logincheck(string vals)
        {
            string sql = vals.Split('&')[0];
            long uuid = long.Parse(vals.Split('&')[1]);
            string id = vals.Split('&')[2];
            string pw = vals.Split('&')[3];
            string nick = "";
            cmd = new MySqlCommand(sql, _connection);
            table = cmd.ExecuteReader();
            while (table.Read())
            {
                if (id.Equals(table.GetValue(1)) && pw.Equals(table.GetValue(2)))
                {
                    nick = table.GetValue(3).ToString();
                    table.Close();
                    servermanager.SendLoginSuccess(true, uuid, id, pw, nick);
                    return;
                }
            }
            table.Close();
            servermanager.SendLoginSuccess(false, uuid, id, pw, nick);
        }
        void NickNameSet(string vals)
        {
            string sql = vals.Split('&')[0];
            long uuid = long.Parse(vals.Split('&')[1]);
            string id = vals.Split('&')[2];
            string nick = vals.Split('&')[3];
            cmd = new MySqlCommand(sql, _connection);
            table = cmd.ExecuteReader();
            while (table.Read())
            {
                for (int n = 0; n < table.FieldCount; n++)
                {
                    if (nick.Equals(table.GetValue(n)))
                    {
                        table.Close();
                        servermanager.SendNicknameSet(false, uuid, id);
                        Console.WriteLine("닉네임 중복!");
                        return;
                    }
                }
            }
            table.Close();
            string qr = string.Format(DefineDBValue._setNickName, id, nick);
            cmd.CommandText = qr;
            if (cmd.ExecuteNonQuery() == 1)
            {
                servermanager.SendNicknameSet(true, uuid, id);
                Console.WriteLine("닉네임 저장 성공!");
            }
            else
            {
                servermanager.SendNicknameSet(false, uuid, id);
                Console.WriteLine("닉네임 저장 실패!");
            }
        }
    }
}
