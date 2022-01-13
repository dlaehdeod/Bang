using UnityEngine;
using UnityEngine.UI;

public class PlayerMenuView : MonoBehaviour
{
    private Text playerStateText;

    private void Awake()
    {
        playerStateText = transform.Find("PlayerState Text").GetComponent<Text>();
    }

    public void ShowPlayerState(string playerList)
    {
        playerStateText.text = playerList;
    }

    public void SetActive (bool value, int start = 0)
    {
        for (int i = start; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(value);
        }
    }

    public void ActiveFirstPlayerStartButton()
    {
        if (BangClient.playerNumber != 0)
        {
            return;
        }

        transform.Find("Start Button").gameObject.SetActive(true);
    }

    public void OnExitButtonDown()
    {
        Application.Quit();
    }

    public void OnStartButtonDown()
    {
        ToServer.SendToServer(Header.GameStartButtonDown);
    }
}
