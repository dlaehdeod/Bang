using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerChooseWindow : MonoBehaviour
{
    public PlayerController playerController;
    public bool otherPlayerEmporio;

    private List<int> cardNumber;

    private GameObject notResistButton;
    private Transform selectCards;
    private Text titleText;
    private Text chooseTime;
    private int originSizeLimit;

    private Header responseHeader;
    private int needSelectCount;
    private int showCardCount;

    private float imageWidth;
    private int selectLayer;
    private int timer;

    private Coroutine timerCoroutine;
    private bool coroutineDoing;

    private void Awake()
    {
        cardNumber = new List<int>();

        notResistButton = transform.Find("Not Resist Button").gameObject;
        selectCards = transform.Find("Select Cards");
        chooseTime = transform.Find("Timer Background").GetChild(0).GetComponent<Text>();
        titleText = transform.Find("Title Background").GetChild(0).GetComponent<Text>();
        selectLayer = LayerMask.NameToLayer("Select Card");

        originSizeLimit = 7;

        Image cardImage = selectCards.GetChild(0).GetComponent<Image>();
        imageWidth = cardImage.rectTransform.rect.width;

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (otherPlayerEmporio)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            SelectCardAndSendToServer();
        }
    }

    private void SelectCardAndSendToServer ()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;
        List<RaycastResult> raycastResult = new List<RaycastResult>();  
        EventSystem.current.RaycastAll(pointer, raycastResult);

        if (raycastResult.Count > 0 && raycastResult[0].gameObject.layer == selectLayer)
        {
            for (int i = 0; i < selectCards.childCount; ++i)
            {
                if (selectCards.GetChild(i) == raycastResult[0].gameObject.transform)
                {
                    int number = cardNumber[i];

                    cardNumber.RemoveAt(i);
                    selectCards.GetChild(i).gameObject.SetActive(false);
                    selectCards.GetChild(i).SetSiblingIndex(selectCards.childCount - 1);

                    ToServer.SendToServer(responseHeader, BangClient.playerNumber, number);

                    needSelectCount--;
                    if (needSelectCount <= 0)
                    {
                        if (isSidKetchum)
                        {
                            SetTitle("카드를 추가로 버려 체력을 회복할 수 있습니다.");
                            notResistButton.gameObject.SetActive(true);
                        }
                        else
                        {
                            DeactiveWindow();
                            return;
                        }
                    }

                
                    showCardCount--;
                    CardSort();
                    break;
                }
            }
        }
    }

    public void SetActiveNotResistButton (bool value)
    {
        notResistButton.SetActive(value);
    }

    public void SetTitle (string message)
    {
        titleText.text = message;
    }

    public void SetResponseHeader (Header header)
    {
        responseHeader = header;
    }

    public void SetSelectCount (int count)
    {
        needSelectCount = count;
    }

    private bool isSidKetchum;

    public void SetSidKetchum ()
    {
        isSidKetchum = true;
    }

    public void SetWindow (ChooseInfor[] chooseInfor, int showTime = 20)
    {
        DeactiveSelectCards();
        otherPlayerEmporio = false;
        gameObject.SetActive(true);
        playerController.PauseScript();

        timer = showTime;
        showCardCount = chooseInfor.Length;
        ExtendLackSelectCard(showCardCount);

        cardNumber.Clear();

        SetChildActive(showTime > 0);

        for (int i = 0; i < showCardCount; ++i)
        {
            Image showCard = selectCards.GetChild(i).GetComponent<Image>();
            showCard.sprite = chooseInfor[i].cardTransform.GetComponent<Image>().sprite;
            showCard.gameObject.SetActive(true);
            
            cardNumber.Add(chooseInfor[i].cardValue);
        }

        CardSort();

        timerCoroutine = StartCoroutine(SetTimer());
    }

    public void SetEmporioWindow (ChooseInfor[] chooseInfor)
    {
        DeactiveSelectCards();
        gameObject.SetActive(true);
        otherPlayerEmporio = true;

        chooseTime.text = "";
        SetChildActive(true);
        notResistButton.SetActive(false);

        showCardCount = chooseInfor.Length;

        ExtendLackSelectCard(showCardCount);

        for (int i = 0; i < showCardCount; ++i)
        {
            Image showCard = selectCards.GetChild(i).GetComponent<Image>();
            showCard.sprite = chooseInfor[i].cardTransform.GetComponent<Image>().sprite;
            showCard.gameObject.SetActive(true);
        }

        CardSort();
    }

    private void DeactiveSelectCards ()
    {
        for (int i = 0; i < selectCards.childCount; ++i)
        {
            selectCards.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void SetChildActive (bool value)
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            if (transform.GetChild(i) == notResistButton.transform)
            {
                continue;
            }
            transform.GetChild(i).gameObject.SetActive(value);
        }
    }

    private void ExtendLackSelectCard (int cardCount)
    {
        while (selectCards.childCount < cardCount)
        {
            Transform target = selectCards.GetChild(0);
            Instantiate(target.gameObject, target.parent);
        }
    }

    private void CardSort ()
    {
        if (showCardCount <= 0)
        {
            return;
        }

        Transform target = selectCards.GetChild(0);

        if (showCardCount <= originSizeLimit)
        {
            target.localPosition = new Vector3(-imageWidth * 0.5f * (showCardCount - 1), 0.0f, 0.0f);
            
            for (int i = 1; i < showCardCount; ++i)
            {
                Transform preTarget = selectCards.GetChild(i - 1);
                target = selectCards.GetChild(i);
                target.localPosition = new Vector3(preTarget.localPosition.x + imageWidth, 0.0f, 0.0f);
            }
        }
        else
        {
            float leftMax = -imageWidth * 0.5f * (originSizeLimit - 1);
            float offset = (leftMax * -2.0f) / (showCardCount - 1);
            
            target.localPosition = new Vector3(leftMax, 0.0f, 0.0f);
            
            for (int i = 1; i < showCardCount; ++i)
            {
                Transform preTarget = selectCards.GetChild(i - 1);
                target = selectCards.GetChild(i);
                target.localPosition = new Vector3(preTarget.localPosition.x + offset, 0.0f, 0.0f);
            }
        }
    }

    private IEnumerator SetTimer ()
    {
        coroutineDoing = true;
        chooseTime.text = timer.ToString();
        while (timer > 0)
        {
            yield return new WaitForSeconds(1.0f);
            timer--;
            chooseTime.text = timer.ToString();
        }

        yield return new WaitForSeconds(0.05f);
        coroutineDoing = false;
        DeactiveWindow();
    }

    public void DeactiveWindow ()
    {
        if (coroutineDoing)
        {
            StopCoroutine(timerCoroutine);
            coroutineDoing = false;
        }

        if (notResistButton.activeSelf)
        {
            if (needSelectCount > 0 || isSidKetchum)
            {
                ToServer.SendToServer(responseHeader, BangClient.playerNumber, 100);
            }

            needSelectCount = 0;
            gameObject.SetActive(false);
        }
        else
        {
            if (needSelectCount > 0)
            {
                StartCoroutine(SendDelayMessage());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        isSidKetchum = false;
    }

    public void GameOverDeactive ()
    {
        if (coroutineDoing)
        {
            StopCoroutine(timerCoroutine);
            coroutineDoing = false;
        }

        needSelectCount = 0;
        otherPlayerEmporio = false;
        SetChildActive(false);
        notResistButton.SetActive(false);
        chooseTime.text = "";
        gameObject.SetActive(false);
    }

    private IEnumerator SendDelayMessage ()
    {
        yield return new WaitForSeconds(0.01f);
        
        for (int i = 0; i < needSelectCount; ++i)
        {
            yield return new WaitForSeconds(0.25f);
            int index = Random.Range(0, cardNumber.Count);
            int value = cardNumber[index];
            cardNumber.RemoveAt(index);
            ToServer.SendToServer(responseHeader, BangClient.playerNumber, value);
        }
        gameObject.SetActive(false);

        if (isSidKetchum)
        {
            ToServer.SendToServer(responseHeader, BangClient.playerNumber, 100);
        }
    }

    public void OnNotResistButtonDown ()
    {
        DeactiveWindow();
    }
}