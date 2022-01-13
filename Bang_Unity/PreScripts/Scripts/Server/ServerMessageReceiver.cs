using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerMessageReceiver : MonoBehaviour
{
    private struct Node
    {
        public Node (Socket socket, byte[] message)
        {
            clientSocket = socket;
            byteMessage = message;
        }

        public Socket clientSocket;
        public byte[] byteMessage;
    }

    private Queue<Node> messageQueue;

    private BangServer bangServer;
    private Game game;

    private void Start()
    {
        if (BangClient.isOwner)
        {
            messageQueue = new Queue<Node>();

            bangServer = new BangServer();
            bangServer.enQueueInMessageReceiver += EnqueueMessage;
            game = new Game(bangServer);

            StartCoroutine(MessageProcessing());
        }
        else
        {
            Destroy(this);
        }
    }

    private void EnqueueMessage(Socket clientSocket, byte[] byteMessage)
    {
        messageQueue.Enqueue(new Node(clientSocket, byteMessage));
    }

    private IEnumerator MessageProcessing()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.01f);

            if (messageQueue.Count > 0)
            {
                Socket clientSocket = messageQueue.Peek().clientSocket;
                byte[] message = messageQueue.Dequeue().byteMessage;

                Header header = MessageManager.GetHeader(message);
            
                switch (header)
                {
                    case Header.Disconnect:
                        bangServer.DisConnectClient(clientSocket);
                        break;
                    case Header.LeavePlayer:
                        int index = MessageManager.GetBodyToInt(message);
                        bangServer.SendToAll(MessageManager.MakeByteMessage(Header.Chatting, "<color=orange>" + game.GetPlayerName(index) + "님이 게임을 떠났습니다.</color>"));
                        game.RemovePlayerName(index, bangServer.GetClientCount());
                        break;
                    case Header.Chatting:
                        bangServer.SendToAll(message);
                        break;
                    case Header.ClientJoin:
                        int id = bangServer.GetClientSocketOrder(clientSocket);
                        SetPlayerNameAndJoinNotify(id, message);
                        bangServer.SetAllClientId();
                        UpdatePlayerView();
                        break;
                    case Header.Ready:
                        SetPlayerReady(message);
                        UpdatePlayerView();
                        break;
                    case Header.OwnerDisconnect:
                        bangServer.SendToAll(MessageManager.MakeByteMessage(Header.OwnerDisconnect, "<color=red>서버가 종료되었습니다.</color>"));
                        break;
                    case Header.GameStart:
                        ValidGameCheckAndStart();
                        break;
                    case Header.DistributeJopFinished:
                        game.FinishCheckAndNext();
                        break;
                    default:
                        print("Server) Not Defined Header = " + header);
                        break;
                }
            }
        }
    }

    private void SetPlayerNameAndJoinNotify(int index, byte[] message)
    {
        string name = MessageManager.GetBodyToString(message);
        game.SetPlayerName(index, name);
        bangServer.SendToAll(MessageManager.MakeByteMessage(Header.Chatting, "<color=orange>" + name + "님이 접속하였습니다.</color>"));
    }

    private void UpdatePlayerView()
    {
        if (game.IsGamePlaying())
        {
            //게임이 시작중인 경우는 대기뷰를 업데이트하지 않는다.
        }
        else
        {
            int playerCount = bangServer.GetClientCount();

            string message = "현재 인원 " + playerCount.ToString() + "/7";
            for (int i = 0; i < playerCount; ++i)
            {
                message += "\n" + game.GetPlayerName(i);
                if (game.GetPlayerReady(i))
                {
                    message += " (Ready)";
                }
            }

            bangServer.SendToAll(MessageManager.MakeByteMessage(Header.ShowPlayerState, message));
        }
    }

    private void SetPlayerReady(byte[] message)
    {
        int index = MessageManager.GetBodyToInt(message);
        game.SetPlayerReady(index);
    }

    private void ValidGameCheckAndStart()
    {
        if (game.IsGamePlaying())
        { 
            return;
        }

        int playerCount = bangServer.GetClientCount();
        
        if (playerCount == 1)
        {
            bangServer.SendToAll(MessageManager.MakeByteMessage(Header.Chatting, "<color=red>혼자서는 플레이 할 수 없습니다.</color>"));
            return;
        }

        if (!game.IsAllPlayerReady(playerCount))
        {
            bangServer.SendToAll(MessageManager.MakeByteMessage(Header.Chatting, "<color=red>모든 플레이어가 준비해야 시작 가능합니다.</color>"));
            return;
        }

        game.SetPlayerCount(playerCount);
        game.PlayerJopSetting();
    }
}