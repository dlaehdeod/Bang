using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject interactiveObjects;
    public RectTransform introImage;
    public InputField nameInputField;
    public InputField ipInputField;
    public Text noNameText;

    private const string path = @"ipAddress.txt";
    private string ipAddress;

    private void Start ()
    {
        IpTextCheck();

        ipAddress = ipInputField.text;
        StartCoroutine(StartAnimation());
    }

    private void IpTextCheck ()
    {
        if (File.Exists(path))
        {
            using (StreamReader sr = File.OpenText(path))
            {
                string ip = sr.ReadLine();

                if (ip != null)
                {
                    ipInputField.text = ip;
                    ipInputField.placeholder.GetComponent<Text>().text = ip;
                }
            }
        }
    }

    private IEnumerator StartAnimation ()
    {
        Vector2 originPosition = introImage.localPosition;
        Vector2 destination = new Vector2(-480.0f, 0.0f);
        float ratio = 0.0f;
        
        while (ratio < 1.0f)
        {
            ratio += 0.02f;
            yield return null;
            introImage.localPosition = Vector2.Lerp(originPosition, destination, ratio);
        }

        yield return new WaitForSeconds(0.05f);
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

    #region Unity UI Functions

    public void OnIpChangeButtonDown ()
    {
        ipInputField.interactable = true;
    }

    public void OnConnectButtonDown ()
    {
        if (NameIsEmpty())
            return;

        BangClient.ip = ipInputField.text;
        BangClient.playerName = nameInputField.text;

        SaveIpAddress();

        SceneChange.instance.NextScene("Play");
    }

    private void SaveIpAddress ()
    {
        using (StreamWriter sw = File.CreateText(path))
        {
            sw.WriteLine(ipInputField.text);
        }
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
        Application.Quit();
    }

    #endregion
}