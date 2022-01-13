using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerZoomInCard : MonoBehaviour
{
    public Image focusCard;
    
    private List<RaycastResult> raycastResult;
    private int cardLayer;
    private int showLayer;

    private void Start()
    {
        cardLayer = LayerMask.NameToLayer("Card");
        showLayer = LayerMask.NameToLayer("Show Card");
        enabled = false;
    }

    private void Update()
    {
        ZoomInCard();
    }

    private void ZoomInCard ()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;
        raycastResult = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResult);

        if (raycastResult.Count > 0)
        {
            GameObject target = raycastResult[0].gameObject;

            if (target.layer == cardLayer || target.layer == showLayer)
            {
                focusCard.sprite = target.GetComponent<Image>().sprite;
                focusCard.gameObject.SetActive(true);
            }
            else
            {
                focusCard.gameObject.SetActive(false);
            }
            
        }
        else
        {
            focusCard.gameObject.SetActive(false);
        }
    }

    public void PauseScript ()
    {
        focusCard.gameObject.SetActive(false);
        enabled = false;
    }
}
