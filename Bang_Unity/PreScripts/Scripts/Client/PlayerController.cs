using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public Image focusCard;

    private List<RaycastResult> raycastResult;
    private Transform selectedCard;

    Vector2 curMousePosition;
    Vector2 preMousePosition;

    private Color selectedColor;
    private Vector2 selectedPosition;
    private Vector2 selectedInterval;
    private bool isShowingCard;
    private int cardLayer;

    private bool cardHolding;

    private void Start()
    {
        selectedCard = null;
        cardLayer = LayerMask.NameToLayer("Card");
        preMousePosition = Vector3.zero;
        selectedColor = new Color(1.0f, 0.5f, 0.5f);
    }

    private void Update()
    {
        curMousePosition = (Vector2)Input.mousePosition;

        if (cardHolding)
        {
            selectedCard.position = curMousePosition + selectedInterval;
        }
        else if (Vector2.Distance(curMousePosition, preMousePosition) >= Mathf.Epsilon)
        {
            print(Mathf.Epsilon);
            preMousePosition = curMousePosition;
            MousePositionCheck();
        }

        //마우스 왼쪽버튼. 또는 핸드폰 터치
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
        //마우스 이벤트를 감지한다. 마우스 위치에서 UI를 감지하는 레이케스트를 받아온다.
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;
        raycastResult = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResult);

        if (raycastResult.Count > 0)
        {
            //이전에 선택된 오브젝트와 감지중인 오브젝트가 동일한 경우 업데이트 할 필요가 없다.
            if (selectedCard == raycastResult[0].gameObject.transform)
            {
                return;
            }

            if (raycastResult[0].gameObject.layer == cardLayer)
            {
                isShowingCard = true;
                selectedCard = raycastResult[0].gameObject.transform;
                focusCard.sprite = selectedCard.GetComponent<Image>().sprite;

                focusCard.gameObject.SetActive(true);
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

    void Idle ()
    {
        isShowingCard = false;
        focusCard.gameObject.SetActive(false);
        selectedCard = null;
    }

    void MouseButtonDown()
    {
        if (!isShowingCard)
            return;

        cardHolding = true;
        selectedPosition = selectedCard.position;
        selectedInterval = selectedPosition - (Vector2)Input.mousePosition;
        focusCard.gameObject.SetActive(false);
        selectedCard.GetComponent<Image>().color = selectedColor;
    }

    void MouseButtonUp()
    {
        //올바른 곳으로 드랍이 된 경우.
        //서버에서 처리를 할지 클라이언트에서 처리를 할지 결정해야 한다.



        //올바르지 못한 곳으로 드랍이 된 경우.

        if (selectedCard != null)
        {
            selectedCard.position = selectedPosition;
            selectedCard.GetComponent<Image>().color = Color.white;
        }


        //최종 수행 결과

        cardHolding = false;
        isShowingCard = false;
        selectedCard = null;
    }
}