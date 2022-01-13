using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientMessageReceiver : MonoBehaviour
{
    public Chatting chatting;
    public PlayerView playerView;
    private BangClient bangClient;

    private Queue<byte[]> messageQueue;

    private void Start ()
    {
        messageQueue = new Queue<byte[]>();

        bangClient = new BangClient();
        bangClient.enQueueInMessageReceiver += EnqueueMessage;
        bangClient.Connect();

        chatting.bangClient = bangClient;
        playerView.bangClient = bangClient;

        StartCoroutine(ConnectCheck());
    }
    
    private void EnqueueMessage(byte[] message)
    {
        messageQueue.Enqueue(message);
    }

    private IEnumerator ConnectCheck ()
    {
        StartCoroutine(MessageProcessing());

        yield return new WaitForSeconds(2.5f);

        if (!bangClient.isConnecting)
        {
            chatting.PrintMessage("<color=red>접속 실패!</color>");
            yield return new WaitForSeconds(1.0f);
            SceneChange.instance.NextScene("Main");
        }
    }

    private IEnumerator MessageProcessing()
    {
        while (messageQueue.Count == 0)
        {
            yield return null;
        }

        chatting.PrintFirstMessage(MessageManager.GetBodyToString(messageQueue.Dequeue()));

        while (true)
        {
            yield return new WaitForSeconds(0.01f);

            if (messageQueue.Count > 0)
            {
                Header header = MessageManager.GetHeader(messageQueue.Peek());

                switch (header)
                {
                    case Header.SetPlayerId:
                        BangClient.playerNumber = MessageManager.GetBodyToInt(messageQueue.Dequeue());
                        chatting.ChattingPermission();
                        break;
                    case Header.Chatting:
                        chatting.PrintMessage(MessageManager.GetBodyToString(messageQueue.Dequeue()));
                        break;
                    case Header.ShowPlayerState:
                        playerView.ShowPlayerState(MessageManager.GetBodyToString(messageQueue.Dequeue()));
                        break;
                    case Header.SetPlayerJop:
                        playerView.DisappearWaitingObject();
                        playerView.SetPlayerJop(MessageManager.GetBodyToInt(messageQueue.Dequeue()));
                        break;
                    case Header.DistributeJop:
                        messageQueue.Dequeue();
                        playerView.DistributeJopCard();
                        break;
                    case Header.SetPlayerCharacter:
                        playerView.SelectCharacter(MessageManager.GetBodyToInt(messageQueue.Peek(), 4),
                                                   MessageManager.GetBodyToInt(messageQueue.Dequeue(), 8));
                        break;
                    case Header.OwnerDisconnect:
                        ShutDown();
                        break;
                    default:
                        print("Client) Not Defined header = " + header);
                        break;
                }
            }
        }
    }

    private void ShutDown ()
    {
        chatting.PrintMessage(MessageManager.GetBodyToString(messageQueue.Dequeue()));
        bangClient.SendToServer(MessageManager.MakeByteMessage(Header.Disconnect));
        SceneChange.instance.NextScene("Main");
    }
}