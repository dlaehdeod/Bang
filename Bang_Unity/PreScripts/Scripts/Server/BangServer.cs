using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

public class BangServer
{
    public int byteSize;
    private const int port = 9898;

    private List<Socket> clientList;
    private Socket server;
    private IPEndPoint endPoint;

    public delegate void EnQueueInMessageReceiver(Socket clientSocket, byte[] byteMessage);
    public EnQueueInMessageReceiver enQueueInMessageReceiver = null;

    public bool isPlaying;

    public void CloseServer()
    {
        server.Close();
    }

    public BangServer()
    {
        isPlaying = false;
        byteSize = 512;

        clientList = new List<Socket>();
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        endPoint = new IPEndPoint(IPAddress.Any, port);

        server.Bind(endPoint);
        server.Listen(10);

        SocketAsyncEventArgs acceptArgs = new SocketAsyncEventArgs();
        acceptArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptCompleted);

        server.AcceptAsync(acceptArgs);
    }

    private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success && !isPlaying)
        {
            Socket client = e.AcceptSocket;

            lock (clientList)
            {
                if (clientList.Count >= 7)
                {
                    return;
                }
                clientList.Add(client);
            }

            SendToOne(client, MessageManager.MakeByteMessage(Header.Chatting, "<color=yellow>접속을 환영합니다.</color>"));

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();

            byte[] acceptData = new byte[byteSize];
            args.SetBuffer(acceptData, 0, acceptData.Length);

            args.UserToken = clientList;
            args.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveCompleted);
            client.ReceiveAsync(args);

            e.AcceptSocket = null;
            server.AcceptAsync(e);
        }
    }

    private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
    {
        Socket clientSocket = (Socket)sender;

        if (clientSocket.Connected)
        {
            enQueueInMessageReceiver(clientSocket, e.Buffer);

            e.SetBuffer(new byte[byteSize], 0, byteSize);
            clientSocket.ReceiveAsync(e);
        }
        else
        {
            DisConnectClient(clientSocket);
        }
    }

    public void DisConnectClient (Socket clientSocket)
    {
        int removeIndex = 0;

        lock (clientList)
        {
            removeIndex = clientList.IndexOf(clientSocket);            
            clientList.RemoveAt(removeIndex);
        }

        SendToAll(MessageManager.MakeByteMessage(Header.LeavePlayer, removeIndex));

        clientSocket.Disconnect(false);
        clientSocket.Dispose();
        clientSocket.Close();
    }

    public void SendToAll(byte[] message)
    {
        for (int i = 0; i < clientList.Count; ++i)
        {
            SendToOne(clientList[i], message);
        }
    }

    public void SendToOne(Socket client, byte[] message)
    {
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.SetBuffer(message, 0, message.Length);
        client.SendAsync(args);
    }

    public void SendToOne(int index, byte[] message)
    {
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.SetBuffer(message, 0, message.Length);
        clientList[index].SendAsync(args);
    }

    public int GetClientSocketOrder (Socket clientSocket)
    {
        for (int i = 0; i < clientList.Count; ++i)
        {
            if (clientList[i] == clientSocket)
            {
                return i;
            }
        }

        return -1;
    }

    public void SetAllClientId()
    {
        for (int i = 0; i < clientList.Count; ++i)
        {
            SendToOne(clientList[i], MessageManager.MakeByteMessage(Header.SetPlayerId, i));
        }
    }

    public int GetClientCount ()
    {
        return clientList.Count;
    }
}