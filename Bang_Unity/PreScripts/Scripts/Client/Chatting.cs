using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Chatting : MonoBehaviour
{
    public BangClient bangClient;

    public InputField playerInputField;
    public ScrollRect chattingScroll;
    public Text chattingText;

    private int chattingLimit;
    private string[] playerColors = { "yellow", "red", "blue", "#f7f", "#7f7", "orange", "cyan" };
    private string playerColorAndName = string.Empty;

    private void Start()
    {
        chattingLimit = 10000;
        StartCoroutine(AutoFocusInputField());
    }

    public void ChattingPermission ()
    {
        playerInputField.interactable = true;
        playerColorAndName = "<color=" + playerColors[BangClient.playerNumber] + ">" + BangClient.playerName + ")</color>";
    }

    private IEnumerator AutoFocusInputField ()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            playerInputField.ActivateInputField();
        }
    }

    public void PrintFirstMessage (string message)
    {
        chattingText.text = message;
    }

    public void PrintMessage (string message)
    {
        CleanUpTooManyText();
        chattingText.text += "\n" + message;

        StartCoroutine(ChattingViewFocusBottom());
    }
    
    private void CleanUpTooManyText()
    {
        if (chattingText.text.Length >= chattingLimit)
        {
            string reduceText = chattingText.text.Substring(chattingText.text.Length / 2);
            int firstIndex = reduceText.IndexOf("<color=");
            reduceText = reduceText.Substring(firstIndex);

            chattingText.text = reduceText;
        }
    }

    private IEnumerator ChattingViewFocusBottom()
    {
        yield return new WaitForSeconds(0.025f);
        chattingScroll.verticalNormalizedPosition = 0.0f;
    }

    public void OnInputFieldEndEdit()
    {
        if (playerInputField.text == "")
        {
            return;
        }

        string message = playerColorAndName + playerInputField.text;

        bangClient.SendToServer(MessageManager.MakeByteMessage(Header.Chatting, message));

        playerInputField.text = "";
        playerInputField.ActivateInputField();
    }
}