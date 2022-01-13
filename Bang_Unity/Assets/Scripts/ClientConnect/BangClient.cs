using System;
using System.Net;
using System.Net.Sockets;

public class BangClient
{
    public static string playerName = "TestName";
    public static int playerNumber = 0;
    public static string ip = "175.193.80.159";
    public static bool isConnected;
    private Socket client;

    private const int byteSize = 512;

    public delegate void EnqueueMessage(byte[] message);
    public EnqueueMessage enqueueMessage = null;

    public void Connect ()
    {
        ToServer.sendMessage = SendToServer;

        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), 9898);

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.RemoteEndPoint = endPoint;
        args.Completed += new EventHandler<SocketAsyncEventArgs>(ConnectCompleted);

        client.ConnectAsync(args);
    }

    public void SendToServer(byte[] message)
    {
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.SetBuffer(message, 0, message.Length);
        client.SendAsync(args);
    }

    public void Close ()
    {
        client.Close();
    }

    private void ConnectCompleted (object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(new byte[byteSize], 0, byteSize);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveCompleted);

            isConnected = true;
            SendToServer(MessageManager.MakeByteMessage(Header.SetClientName, playerName));

            client.ReceiveAsync(args);
        }
    }

    private void ReceiveCompleted (object sender, SocketAsyncEventArgs e)
    {
        if (e.BytesTransferred > 0)
        {
            enqueueMessage(e.Buffer);

            e.SetBuffer(new byte[byteSize], 0, byteSize);
            client.ReceiveAsync(e);
        }
    }
}