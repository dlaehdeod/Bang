using System;
using System.Collections.Generic;
using System.Text;

namespace BangGameServer
{
    public class MainEntry
    {
        static void Main(string[] args)
        {
            BangServer bangServer = new BangServer();

            while (true)
            {
                string input = Console.ReadLine();

                if (input == "quit")
                {
                    bangServer.SendToAll(MessageManager.MakeByteMessage(Header.ShutDown));
                    break;
                }
                if (input == "reset")
                {
                    bangServer.Close();
                    bangServer = new BangServer();
                    continue;
                }

                bangServer.SendToAll(MessageManager.MakeByteMessage(Header.Chatting, input));
            }

            Console.WriteLine("서버를 종료 중..");
            System.Threading.Thread.Sleep(1000);
        }
    }
}
