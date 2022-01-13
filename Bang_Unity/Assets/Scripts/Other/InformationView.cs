using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InformationView : MonoBehaviour
{
    public static InformationView instance;

    private Image background;
    private Text inforText;
    private Transform clickText;

    private bool isWaiting;

    private bool textFadeDoing;
    private bool textSkipDoing;
    
    private Coroutine textFadeCoroutine;
    private Coroutine textSkipCoroutine;

    private void Start()
    {
        instance = this;

        background = transform.GetChild(0).GetComponent<Image>();
        inforText = background.transform.GetChild(0).GetComponent<Text>();
        clickText = background.transform.GetChild(1);
    }

    public void ShowJobCard (Transform target, int job)
    {
        string[] jobName = { "보안관", "배신자", "무법자", "무법자", "부관", "무법자", "부관" };
        string message = "당신은 " + jobName[job] + " 입니다.";

        inforText.text = message;
        isWaiting = true;
       
        StartCoroutine(JobCardAnimation(target));
    }

    private void CheckPreMessage ()
    {
        if (textFadeDoing)
        {
            StopCoroutine(textFadeCoroutine);
            textFadeDoing = false;
        }
        if (textSkipDoing)
        {
            StopCoroutine(textSkipCoroutine);
            textSkipDoing = false;
        }
    }

    public void ShowMessage(string message, bool autoSkip = false, float showTime = 0.5f)
    {
        CheckPreMessage();

        transform.parent.gameObject.SetActive(true);
        inforText.text = message;
        isWaiting = true;

        if (autoSkip)
        {
            textSkipCoroutine = StartCoroutine(PlayerShowCheck(false, showTime));
        }
        textFadeCoroutine = StartCoroutine(TextFadeInAndFadeOut());
    }

    private IEnumerator JobCardAnimation(Transform target)
    {
        float ratio = 0.0f;

        target.GetChild(0).gameObject.SetActive(false);

        while (ratio < 1.0f)
        {
            yield return null;
            ratio += 0.025f;
            target.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 3.0f, ratio);
        }

        StartCoroutine(PlayerShowCheck(true));
        yield return StartCoroutine(TextFadeInAndFadeOut());

        while (ratio > 0.0f)
        {
            yield return null;
            ratio -= 0.025f;
            target.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 3.0f, ratio);
        }

        yield return new WaitForSeconds(1.0f);
        ToServer.SendToServer(Header.SetPlayerJobCompleted, BangClient.playerNumber);
    }

    private IEnumerator PlayerShowCheck (bool showText, float nextTime = 1.0f)
    {
        textSkipDoing = true;
        yield return new WaitForSeconds(0.25f);

        float autoNextTime = 0.0f;
        if (showText)
        {
            clickText.gameObject.SetActive(true);
        }

        while (autoNextTime < nextTime)
        {
            yield return new WaitForSeconds(0.01f);
            autoNextTime += 0.01f;

            if (Input.GetMouseButtonDown(0))
            {
                break;
            }
        }
        
        isWaiting = false;
        textSkipDoing = false;
        clickText.gameObject.SetActive(false);
    }

    private IEnumerator TextFadeInAndFadeOut ()
    {
        textFadeDoing = true;
        background.gameObject.SetActive(true);
        float alpha = 0.0f;

        Color backGroundColor = background.color;
        Color textColor = inforText.color;

        backGroundColor.a = 0.0f;
        textColor.a = 0.0f;

        background.color = backGroundColor;
        inforText.color = textColor;

        while (alpha < 1.0f)
        {
            yield return null;
            alpha += 0.05f;
            backGroundColor.a = alpha;
            textColor.a = alpha;
            background.color = backGroundColor;
            inforText.color = textColor;
        }

        while (isWaiting)
        {
            yield return new WaitForSeconds(0.05f);
        }
        
        while (alpha > 0.0f)
        {
            yield return null;
            alpha -= 0.05f;
            backGroundColor.a = alpha;
            textColor.a = alpha;
            background.color = backGroundColor;
            inforText.color = textColor;
        }

        textFadeDoing = false;
        background.gameObject.SetActive(false);
    }
}