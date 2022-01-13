using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PassTurn : MonoBehaviour
{
    public PlayerController playerController;
    [HideInInspector]
    public int timer;
    public Text timerText;

    private Coroutine timerCoroutine;

    public void ReadyToPlayerTurn (int time)
    {
        gameObject.SetActive(true);
        timer = time;

        timerCoroutine = StartCoroutine(SetTimer());
    }

    public void StopTimer ()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        gameObject.SetActive(false);
    }

    public void StartTimer ()
    {
        gameObject.SetActive(true);
        timer = timer < 30 ? 30 : timer;
        timerCoroutine = StartCoroutine(SetTimer());
    }

    private IEnumerator SetTimer ()
    {
        timerText.text = timer.ToString();

        while (timer > 0)
        {
            yield return new WaitForSeconds(1.0f);
            timer--;
            timerText.text = timer.ToString();
        }

        playerController.PauseScript();
        yield return new WaitForSeconds(0.1f);
        PassTurnButtonDown();
    }

    public void PassTurnButtonDown()
    {
        playerController.PauseScript();
        playerController.HideBangCount();

        ToServer.SendToServer(Header.PassTurn, BangClient.playerNumber);
        gameObject.SetActive(false);
    }
}