using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class PlayerCardStealMode : MonoBehaviour
{
    private const int justDrawCard = 100;
    public Image drawBackfaceCard;
    public Transform usedCard;

    private PlayerCanvas[] playerCanvas;
    private Stack<Image> showCardStack;
    private Stack<Image> backfaceCardStack;
    private Text stealTimeText;
    
    private bool isJesseJones;
    private bool isPedroRamirez;
    private bool isElGringo;
    private bool isUsePanicoCard;
    private bool isUseCatBalouCard;
    private int attackTarget;

    private Color stealPossibleColor;
    private int stealTime;
    private int stealLayer;
    private int defaultLayer;
    private int showCardLayer;
    private int playerCount;
    
    private void Awake()
    {
        defaultLayer = LayerMask.NameToLayer("UI");
        stealLayer = LayerMask.NameToLayer("Steal Card");
        showCardLayer = LayerMask.NameToLayer("Show Card");

        stealPossibleColor = new Color(0.25f, 0.5f, 1.0f);
        showCardStack = new Stack<Image>();
        backfaceCardStack = new Stack<Image>();
        stealTimeText = drawBackfaceCard.transform.GetChild(0).GetComponent<Text>();

        stealTime = 20;
        gameObject.SetActive(false);
    }

    public void SetPlayerCanvas (PlayerCanvas[] canvas)
    {
        playerCanvas = canvas;
        playerCount = canvas.Length;
    }

    public void PedroRamirezTurn()
    {
        gameObject.SetActive(true);
        isPedroRamirez = true;
        StartCoroutine(SetPedroRamirez());
    }

    private IEnumerator SetPedroRamirez()
    {
        yield return new WaitForSeconds(0.5f);

        drawBackfaceCard.color = stealPossibleColor;
        drawBackfaceCard.gameObject.layer = stealLayer;
        backfaceCardStack.Push(drawBackfaceCard);

        Image recentCard = usedCard.GetChild(usedCard.childCount - 1).GetComponent<Image>();
        recentCard.color = stealPossibleColor;
        recentCard.gameObject.layer = stealLayer;
        showCardStack.Push(recentCard);

        StartCoroutine(SetStealTimer(stealTime));
    }

    public void JesseJonesTurn()
    {
        gameObject.SetActive(true);
        isJesseJones = true;
        StartCoroutine(SetJesseJones());
    }

    private IEnumerator SetJesseJones()
    {
        yield return new WaitForSeconds(0.5f);

        drawBackfaceCard.color = stealPossibleColor;
        drawBackfaceCard.gameObject.layer = stealLayer;
        backfaceCardStack.Push(drawBackfaceCard);

        for (int playerIndex = 0; playerIndex < playerCount; ++playerIndex)
        {
            if (playerIndex == BangClient.playerNumber)
            {
                continue;
            }

            Transform gameCard = playerCanvas[playerIndex].gameCard;

            for (int childIndex = 0; childIndex < gameCard.childCount; ++childIndex)
            {
                Image image = gameCard.GetChild(childIndex).GetChild(0).GetComponent<Image>();
                image.color = stealPossibleColor;
                image.gameObject.layer = stealLayer;

                backfaceCardStack.Push(image);
            }
        }

        StartCoroutine(SetStealTimer(stealTime));
    }
    
    public void ElGringoStealCard(int targetIndex)
    {
        gameObject.SetActive(true);
        isElGringo = true;
        StartCoroutine(SetElGringo(targetIndex));
    }

    private IEnumerator SetElGringo(int targetIndex)
    {
        yield return new WaitForSeconds(0.5f);

        Transform targetCard = playerCanvas[targetIndex].gameCard;
        for (int i = 0; i < targetCard.childCount; ++i)
        {
            Image image = targetCard.GetChild(i).GetChild(0).GetComponent<Image>();
            image.color = stealPossibleColor;
            image.gameObject.layer = stealLayer;

            backfaceCardStack.Push(image);
        }

        StartCoroutine(SetStealTimer(stealTime));
    }

    public void UseCatBalouCard(int targetIndex)
    {
        attackTarget = targetIndex;

        gameObject.SetActive(true);
        isUseCatBalouCard = true;

        StartCoroutine(SetUseCard(targetIndex));
    }

    public void UsePanicoCard (int targetIndex)
    {
        attackTarget = targetIndex;

        gameObject.SetActive(true);
        isUsePanicoCard = true;

        StartCoroutine(SetUseCard(targetIndex));
    }

    private IEnumerator SetUseCard (int targetIndex)
    {
        yield return new WaitForSeconds(0.5f);

        Transform targetGameCard = playerCanvas[targetIndex].gameCard;
        Transform targetEquipCard = playerCanvas[targetIndex].equipCard;
        Transform targetEquipFrontCard = playerCanvas[targetIndex].equipFrontCard;

        for (int i = 0; i < targetGameCard.childCount; ++i)
        {
            Image image = targetGameCard.GetChild(i).GetChild(0).GetComponent<Image>();
            image.color = stealPossibleColor;
            image.gameObject.layer = stealLayer;

            backfaceCardStack.Push(image);
        }

        for (int i = 0; i < targetEquipFrontCard.childCount; ++i)
        {
            Image image = targetEquipFrontCard.GetChild(i).GetComponent<Image>();
            image.color = stealPossibleColor;
            image.gameObject.layer = stealLayer;

            showCardStack.Push(image);
        }

        if  (targetEquipCard.childCount >= 3)
        {
            Image image = targetEquipCard.GetChild(2).GetComponent<Image>();
            image.color = stealPossibleColor;
            image.gameObject.layer = stealLayer;

            showCardStack.Push(image);
        }

        StartCoroutine(SetStealTimer(stealTime)); 
    }

    private IEnumerator SetStealTimer (int stealTime)
    {
        while (stealTime >= 0)
        {
            stealTimeText.text = stealTime.ToString();
            yield return new WaitForSeconds(1.0f);
            stealTime--;
        }

        if (isJesseJones)
        {
            ToServer.SendToServer(Header.JesseJonesDrawCard, justDrawCard, 0);
        }
        else if (isPedroRamirez)
        {
            ToServer.SendToServer(Header.PedroRamirezDrawCard, justDrawCard);
        }
        else if (isElGringo)
        {
            ToServer.SendToServer(Header.ElGringoStealCardCompleted, BangClient.playerNumber, justDrawCard);
        }
        else if (isUsePanicoCard)
        {
            ToServer.SendToServer(Header.StealCard, attackTarget, justDrawCard);
        }
        else if (isUseCatBalouCard)
        {
            ToServer.SendToServer(Header.CatBalou, attackTarget, justDrawCard);
        }

        DeactiveStealMode();
    }

    public void DeactiveStealMode ()
    {
        while (showCardStack.Count > 0)
        {
            Image image = showCardStack.Pop();
            image.color = Color.white;
            image.gameObject.layer = showCardLayer;
        }

        while (backfaceCardStack.Count > 0)
        {
            Image image = backfaceCardStack.Pop();
            image.color = Color.white;
            image.gameObject.layer = defaultLayer;
        }

        stealTimeText.text = "";
        isJesseJones = false;
        isPedroRamirez = false;
        isElGringo = false;
        isUseCatBalouCard = false;
        isUsePanicoCard = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectValidCard();
        }
    }

    private void SelectValidCard()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;
        List<RaycastResult> raycastResult = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResult);

        if (raycastResult.Count > 0 && raycastResult[0].gameObject.layer == stealLayer)
        {
            if (isJesseJones)
            {
                JesseJonesSelect(raycastResult[0].gameObject);
            }
            else if (isPedroRamirez)
            {
                PedroRamirezSelect(raycastResult[0].gameObject);
            }
            else if (isElGringo)
            {
                ElGringoSelect(raycastResult[0].gameObject);
            }
            else if (isUsePanicoCard)
            {
                CardSelect(Header.StealCard, raycastResult[0].gameObject);
            }
            else if (isUseCatBalouCard)
            {
                CardSelect(Header.CatBalouCompleted, raycastResult[0].gameObject);
            }

            DeactiveStealMode();
        }
    }

    private void JesseJonesSelect(GameObject target)
    {
        if (target == drawBackfaceCard.gameObject)
        {
            ToServer.SendToServer(Header.JesseJonesDrawCard, justDrawCard, 0);
        }
        else
        {
            JesseJonesDrawCard(target.transform.parent);
        }
    }

    private void JesseJonesDrawCard(Transform target)
    {
        int card = CardTransform.instance.FindPlayCard(target);

        for (int playerNumber = 0; playerNumber < playerCanvas.Length; ++playerNumber)
        {
            if (playerNumber == BangClient.playerNumber)
            {
                continue;
            }

            if (target.parent.parent == playerCanvas[playerNumber].transform)
            {
                ToServer.SendToServer(Header.JesseJonesDrawCard, playerNumber, card);
                return;
            }
        }
    }
    private void PedroRamirezSelect(GameObject target)
    {
        int selectCard = justDrawCard;

        if (target != drawBackfaceCard.gameObject)
        {
            selectCard = 0;
        }

        ToServer.SendToServer(Header.PedroRamirezDrawCard, selectCard);
    }

    private void ElGringoSelect(GameObject target)
    {
        Transform targetTransform = target.transform.parent;
        int card = CardTransform.instance.FindPlayCard(targetTransform);

        ToServer.SendToServer(Header.ElGringoStealCardCompleted, BangClient.playerNumber, card);
    }

    private void CardSelect(Header responseHeader, GameObject target)
    {
        int card;

        if (target.name.Contains("Backface"))
        {
            card = CardTransform.instance.FindPlayCard(target.transform.parent);
        }
        else
        {
            card = CardTransform.instance.FindPlayCard(target.transform);
        }

        ToServer.SendToServer(responseHeader, attackTarget, card);
    }
}