using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace NetworkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TCPGserver server = new TCPGserver(9999);
            // 서버와 디비 연결
            DatabaseManager dB_Manager = new DatabaseManager("ThBall", server);

            while (true)
            {
                Console.Write("ID를 입력하세요: "); string id = Console.ReadLine();
                Console.Write("PW를 입력하세요: "); string pw = "";
                ConsoleKeyInfo PWkeyInfo;
                do
                {
                    PWkeyInfo = Console.ReadKey(true);
                    // Skip if Backspace or Enter is Pressed
                    if (PWkeyInfo.Key != ConsoleKey.Backspace && PWkeyInfo.Key != ConsoleKey.Enter)
                    {
                        pw += PWkeyInfo.KeyChar;
                        Console.Write("*");
                    }
                    else
                    {
                        if (PWkeyInfo.Key == ConsoleKey.Backspace && pw.Length > 0)
                        {
                            pw = pw.Substring(0, (pw.Length - 1));
                        }
                    }
                }
                while (PWkeyInfo.Key != ConsoleKey.Enter);
                Console.Clear();
                if (dB_Manager.ConnectDB(id, pw))
                {
                    dB_Manager.RunStart();
                    server.SetDBmanager(dB_Manager, dB_Manager.SetUUID());
                    Console.WriteLine("접속에 성공했습니다.");
                    break;
                }
            }
            while (server.MainProcess())
            {
                server.CloseSocketProcess();
            }

            server.ReleaseServer();
        }
    }
}
