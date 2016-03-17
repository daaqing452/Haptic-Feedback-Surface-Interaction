using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace WinFormTestApp
{
    class Server
    {
        public TcpListener tcpListener;
        public List<TcpClient> clientList = new List<TcpClient>();
        public object clientListMutex = new object();
        
        public Server(string ip, int port)
        {
            tcpListener = new TcpListener(IPAddress.Parse(ip), port);
            Thread listenThread = new Thread(ListenThread);
            listenThread.Start();
        }

        public void ListenThread()
        {
            tcpListener.Start();
            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                lock (clientListMutex)
                {
                    Console.WriteLine("client join");
                    clientList.Add(client);
                }
                Thread receiveThread = new Thread(ReceiveThread);
                receiveThread.Start(client);
            }
        }

        public void ReceiveThread(object clientObject)
        {
            TcpClient client = (TcpClient)clientObject;
            StreamReader streamReader = new StreamReader(client.GetStream());
            while (true)
            {
                string line = streamReader.ReadLine();
                if (line == null) break;
            }
            lock (clientListMutex)
            {
                Console.WriteLine("client exit");
                clientList.Remove(client);
            }
        }

        public void BroadCast(string info)
        {
            foreach (TcpClient client in clientList)
            {
                StreamWriter streamWriter = new StreamWriter(client.GetStream());
                streamWriter.WriteLine(info);
                streamWriter.Flush();
            }
        }
    }
}
