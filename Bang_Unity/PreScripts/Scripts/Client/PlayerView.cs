using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour
{
    public BangClient bangClient;

    public GameObject playerWaitingObject;
    public Text readyButtonText;
    public Text playerStateText;

    private Jop jop;

    private void Start ()
    {
        if (BangClient.isOwner)
        {
            readyButtonText.text = "시작하기";
            bangClient.SendToServer(MessageManager.MakeByteMessage(Header.Ready, BangClient.playerNumber));
        }
    }

    public void SetPlayerJop(int jop)
    {
        this.jop = (Jop)jop;
    }

    public void DisappearWaitingObject()
    {
        playerWaitingObject.SetActive(false);
    }

    public void DistributeJopCard ()
    {
        //카드를 분배한다. 분배 이 후 인물 카드 선택이 이루어져야 한다.
        StartCoroutine(SetJopCard());
        //작업이 끝났다고 가정.
    }

    private IEnumerator SetJopCard()
    {
        print("SetjopCard");
        yield return new WaitForSeconds(1.0f);
        bangClient.SendToServer(MessageManager.MakeByteMessage(Header.DistributeJopFinished));
    }

    public void SelectCharacter (int first, int second)
    {
        //Set Timer
    }

    public void ShowPlayerState (string playerList)
    {
        playerStateText.text = playerList;
    }

    public void OnBackToMenuButtonDown ()
    {
        if (BangClient.isOwner)
        {
            bangClient.SendToServer(MessageManager.MakeByteMessage(Header.OwnerDisconnect));
        }
        else
        {
            bangClient.SendToServer(MessageManager.MakeByteMessage(Header.Disconnect));
            SceneChange.instance.NextScene("Main");
        }
    }

    public void OnReadyButtonDown ()
    {
        if (BangClient.isOwner)
        {
            bangClient.SendToServer(MessageManager.MakeByteMessage(Header.GameStart));
        }
        else
        {
            bangClient.SendToServer(MessageManager.MakeByteMessage(Header.Ready, BangClient.playerNumber));
            playerWaitingObject.transform.GetChild(0).GetComponent<Button>().interactable = false;
            playerWaitingObject.transform.GetChild(1).GetComponent<Button>().interactable = false;
        }
    }
}
