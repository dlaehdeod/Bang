using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public PassTurn passTurn;
    public Text bangCountText;
    public Image focusCard;

    private PlayerCardChecker playerCardChecker;
    
    private List<RaycastResult> raycastResult;

    private Vector3 currentMousePosition;
    private Vector3 preMousePosition;
    private Vector3 selectedInterval;

    private Vector2 selectedPosition;

    private bool cardHolding;
    private float cardUseMoved;

    public void SetPlayerCanvas(PlayerCanvas[] canvas)
    {
        playerCardChecker.playerCanvas = canvas;
        playerCardChecker.playerCount = canvas.Length;
        playerCardChecker.playerNumber = BangClient.playerNumber;
    }

    public void SetBangCountText ()
    {
        playerCardChecker.bangCountText = bangCountText;
    }

    public void SetPlayerCharacter(int character)
    {
        playerCardChecker.playerCharacter = (Character)character;
    }

    public void SetBangCount (int bangCount)
    {
        playerCardChecker.SetBangCount(bangCount);
    }

    public void ShowBangCount ()
    {
        playerCardChecker.ShowBangCount();
    }

    public void HideBangCount ()
    {
        playerCardChecker.HideBangCount();
    }

    public void PauseScript()
    {
        if (enabled == true)
        {
            playerCardChecker.ResetSelectedCard(selectedPosition);
            Idle();
            enabled = false;
        }
    }

    private void Start()
    {
        playerCardChecker = new PlayerCardChecker();

        playerCardChecker.focusCard = focusCard;
        playerCardChecker.passiveMessage = focusCard.transform.Find("Passive");
        playerCardChecker.activeMessage = focusCard.transform.Find("Active");

        preMousePosition = Vector3.zero;
        cardUseMoved = Screen.width / 200.0f;

        enabled = false;
    }

    private void Update()
    {
        currentMousePosition = Input.mousePosition;
        playerCardChecker.currentMousePosition = currentMousePosition;

        if (cardHolding)
        {
            if (Vector2.Distance(currentMousePosition, preMousePosition) >= cardUseMoved)
            {
                playerCardChecker.MoveResetPlayerUseCard();
            }

            playerCardChecker.UpdateSelectedCardPosition(currentMousePosition + selectedInterval);
        }
        else if (Vector2.Distance(currentMousePosition, preMousePosition) >= Mathf.Epsilon)
        {
            preMousePosition = currentMousePosition;
            MousePositionCheck();
        }

        if (Input.GetMouseButtonDown(0))
        {
            MouseButtonDown();
        }

        if (Input.GetMouseButtonUp(0))
        {
            MouseButtonUp();
        }
    }

    private void MousePositionCheck()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;
        raycastResult = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResult);

        if (raycastResult.Count > 0)
        {
            GameObject target = raycastResult[0].gameObject;

            if (playerCardChecker.IsSameTransform (target.transform))
            {
                return;
            }

            if (playerCardChecker.TargetLayerIsCardLayer(target))
            {
                playerCardChecker.ShowPossibleSelectCard(target);
            }
            else if (playerCardChecker.TargetLayerIsShowLayer(target))
            {
                playerCardChecker.JustShowCard(target);
            }
            else
            {
                Idle();
            }
        }
        else
        {
            Idle();
        }
    }

    private void MouseButtonDown()
    {
        if (playerCardChecker.selectedCard == null)
        {
            return;
        }

        playerCardChecker.ShowCardRangeAndSetUseCard();
        playerCardChecker.SetCardHoldingColor();

        cardHolding = true;
        selectedPosition = playerCardChecker.selectedCard.position;

        selectedInterval = selectedPosition - (Vector2)Input.mousePosition;
    }

    private void MouseButtonUp()
    {
        if (!cardHolding)
        {
            return;
        }

        bool useCardSuccess = playerCardChecker.UseCard();

        if (useCardSuccess)
        {
            passTurn.StopTimer();
            enabled = false;   
        }
        else
        {
            playerCardChecker.ResetSelectedCard(selectedPosition);
        }

        Idle();
        playerCardChecker.playerUseCard = 0;
    }

    private void Idle()
    {
        cardHolding = false;
        preMousePosition = Vector3.zero;
        focusCard.gameObject.SetActive(false);
        playerCardChecker.selectedCard = null;
    }
}