using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static BangServer bangServer = null;

    public GameObject interactiveObjects;

    public RectTransform introImage;
    public InputField nameInputField;
    public InputField ipInputField;
    public Text noNameText;

    private string ipAddress;
    private bool isOwner;

    private void Start ()
    {
        ipAddress = ipInputField.text;
        StartCoroutine(StartAnimation());
    }

    private IEnumerator StartAnimation ()
    {
        Vector2 originPosition = introImage.localPosition;
        Vector2 destination = new Vector2(-480.0f, 0.0f);
        float moveRatio = 0.0f;

        while (moveRatio < 1.0f)
        {
            //for test
            moveRatio += 1.0f;
            //end test

            moveRatio += 0.01f;
            yield return null;
            introImage.localPosition = Vector2.Lerp(originPosition, destination, moveRatio);
        }

        interactiveObjects.SetActive(true);

        nameInputField.ActivateInputField();
    }

    private bool NameIsEmpty ()
    {
        if (nameInputField.text.Trim() == "")
        {
            noNameText.gameObject.SetActive(true);
            return true;
        }

        return false;
    }

    private void ReadyToPlay ()
    {
        if (bangServer != null)
        {
            bangServer.CloseServer();
            bangServer = null;
        }

        if (isOwner)
        {
            bangServer = new BangServer();
        }

        BangClient.isOwner = isOwner;
        BangClient.ip = ipInputField.text;
        BangClient.playerName = nameInputField.text;

        SceneChange.instance.NextScene("Play");
    }

    #region Unity UI Functions

    public void OnCreateButtonDown ()
    {
        if (NameIsEmpty())
            return;

        isOwner = true;
        ReadyToPlay();
    }

    public void OnIpChangeButtonDown ()
    {
        ipInputField.interactable = true;
    }

    public void OnConnectButtonDown ()
    {
        if (NameIsEmpty())
            return;

        isOwner = false;
        ReadyToPlay();
    }

    public void OnIpChanged ()
    {
        if (ipInputField.text.Trim() == "")
        {
            ipInputField.text = ipAddress;
        }
    }

    public void OnExitButtonDown()
    {
        if (bangServer != null)
        {
            bangServer.CloseServer();
        }

        Application.Quit();
    }

    #endregion
}