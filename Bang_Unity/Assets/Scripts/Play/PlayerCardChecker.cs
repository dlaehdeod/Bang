using UnityEngine;
using UnityEngine.UI;

public class PlayerCardChecker
{
    public enum PlayerUseCard
    {
        None, Mirino, Bang, Missed, Winchester, Diligenza, Diligenza2, Prigione, Volcanic,
        WellsFargo, Emporio, Barile, Indian, Panico, CatBalou, RevCarabine, Schofield,
        Remington, Dinamite, Duello, Saloon, Beer, Mustang, Gatling
    }

    public PlayerCanvas[] playerCanvas;
    public Character playerCharacter;
    public Vector2 currentMousePosition;

    public Image focusCard;
    public int bang;
    public Text bangCountText;

    public int playerNumber;
    public int playerCount;

    public Transform passiveMessage;
    public Transform activeMessage;
    public Transform selectedCard;

    public PlayerUseCard playerUseCard;
    private Color selectedColor;

    private int cardLayer;
    private int showLayer;
    private float cardCheckRadius;

    public PlayerCardChecker ()
    {
        selectedCard = null;
        cardCheckRadius = 250.0f;
        cardLayer = LayerMask.NameToLayer("Card");
        showLayer = LayerMask.NameToLayer("Show Card");
        selectedColor = new Color(0.5f, 0.5f, 0.5f);
    }

    public void SetBangCount (int bangCount)
    {
        if (playerCharacter == Character.WillyTheKid)
        {
            bang = 1000;
        }
        else
        {
            bang = bangCount;
        }
    }

    public void ShowBangCount ()
    {
        UpdateBangCountText();
    }

    public void HideBangCount ()
    {
        bangCountText.text = "";
    }

    private void UpdateBangCountText ()
    {
        string countStr = bang > 1 ? "∞" : bang.ToString();
        bangCountText.text = "사용가능 Bang 횟수 : " + countStr;
    }

    public bool IsSameTransform (Transform target)
    {
        return selectedCard == target;
    }

    public bool TargetLayerIsCardLayer (GameObject target)
    {
        return target.layer == cardLayer;
    }

    public bool TargetLayerIsShowLayer (GameObject target)
    {
        return target.layer == showLayer;
    }

    public void UpdateSelectedCardPosition(Vector3 position)
    {
        selectedCard.position = position;
    }

    public void ShowPossibleSelectCard (GameObject target)
    {
        selectedCard = target.transform;
        focusCard.sprite = target.GetComponent<Image>().sprite;
        focusCard.gameObject.SetActive(true);

        SetCardUseTypeMessage(selectedCard);
    }

    public void JustShowCard (GameObject target)
    {
        selectedCard = null;
        passiveMessage.gameObject.SetActive(false);
        activeMessage.gameObject.SetActive(false);

        focusCard.sprite = target.GetComponent<Image>().sprite;
        focusCard.gameObject.SetActive(true);
    }

    public void SetCardHoldingColor ()
    {
        focusCard.gameObject.SetActive(false);
        selectedCard.GetComponent<Image>().color = selectedColor;
    }

    private void SetCardUseTypeMessage(Transform card)
    {
        string name = card.name.Substring(3);

        switch (name)
        {
            case "Bang":
            case "Prigione":
            case "Panico":
            case "CatBalou":
            case "Duello":
                passiveMessage.gameObject.SetActive(false);
                activeMessage.gameObject.SetActive(true);
                break;

            case "Missed":
                if (playerCharacter == Character.CalamityJanet)
                {
                    passiveMessage.gameObject.SetActive(false);
                    activeMessage.gameObject.SetActive(true);
                }
                else
                {
                    passiveMessage.gameObject.SetActive(false);
                    activeMessage.gameObject.SetActive(false);
                }
                break;

            case "Beer":
                if (GetLiveCount() > 2)
                {
                    passiveMessage.gameObject.SetActive(true);
                    activeMessage.gameObject.SetActive(false);
                }
                else
                {
                    passiveMessage.gameObject.SetActive(false);
                    activeMessage.gameObject.SetActive(false);
                }
                break;
                
            default:
                passiveMessage.gameObject.SetActive(true);
                activeMessage.gameObject.SetActive(false);
                break;
        }
    }

    public void ResetSelectedCard (Vector2 selectedPosition)
    {
        if (selectedCard != null)
        {
            selectedCard.position = selectedPosition;
            selectedCard.GetComponent<Image>().color = Color.white;
        }
        selectedCard = null;
    }

    public void MoveResetPlayerUseCard ()
    {
        switch (playerUseCard)
        {
            case PlayerUseCard.Beer:
            case PlayerUseCard.Saloon:
            case PlayerUseCard.Emporio:
            case PlayerUseCard.Barile:
            case PlayerUseCard.Diligenza:
            case PlayerUseCard.Diligenza2:
            case PlayerUseCard.WellsFargo:
            case PlayerUseCard.Gatling:
            case PlayerUseCard.Indian:
            case PlayerUseCard.Mirino:
            case PlayerUseCard.Mustang:
            case PlayerUseCard.Volcanic:
            case PlayerUseCard.Schofield:
            case PlayerUseCard.Remington:
            case PlayerUseCard.RevCarabine:
            case PlayerUseCard.Winchester:
                playerUseCard = PlayerUseCard.None;
                break;
            default:
                break;
        }
    }

    public void ShowCardRangeAndSetUseCard()
    {
        string name = selectedCard.name.Substring(3);

        switch (name)
        {
            case "Bang":
                if (bang > 0)
                {
                    ShowPossibleBangTargetArea();
                    playerUseCard = PlayerUseCard.Bang;
                }
                break;

            case "Missed":
                if (bang > 0 && playerCharacter == Character.CalamityJanet)
                {
                    ShowPossibleBangTargetArea();
                    playerUseCard = PlayerUseCard.Bang;
                }
                break;

            case "Beer":
                if (GetLiveCount() > 2)
                {
                    playerUseCard = PlayerUseCard.Beer;
                }
                break;

            case "Dinamite":
                ShowPossibleDinamiteTargetArea();
                playerUseCard = PlayerUseCard.Dinamite;
                break;

            case "Prigione":
                ShowPossiblePrigioneTargetArea();
                playerUseCard = PlayerUseCard.Prigione;
                break;

            case "Panico":
                ShowPossiblePanicoTargetArea();
                playerUseCard = PlayerUseCard.Panico;
                break;

            case "CatBalou":
                ShowPossibleCatBalouTargetArea();
                playerUseCard = PlayerUseCard.CatBalou;
                break;

            case "Duello":
                ShowPossibleDuelloTargetArea();
                playerUseCard = PlayerUseCard.Duello;
                break;

            case "Saloon":
                playerUseCard = PlayerUseCard.Saloon;
                break;

            case "Diligenza":
            case "Diligenza2":
                playerUseCard = PlayerUseCard.Diligenza;
                break;

            case "Barile":
                playerUseCard = PlayerUseCard.Barile;
                break;

            case "Mirino":
                playerUseCard = PlayerUseCard.Mirino;
                break;

            case "Mustang":
                playerUseCard = PlayerUseCard.Mustang;
                break;

            case "WellsFargo":
                playerUseCard = PlayerUseCard.WellsFargo;
                break;

            case "Emporio":
                playerUseCard = PlayerUseCard.Emporio;
                break;

            case "Indian":
                playerUseCard = PlayerUseCard.Indian;
                break;

            case "Gatling":
                playerUseCard = PlayerUseCard.Gatling;
                break;

            case "Volcanic":
                playerUseCard = PlayerUseCard.Volcanic;
                break;

            case "Schofield":
                playerUseCard = PlayerUseCard.Schofield;
                break;

            case "Remington":
                playerUseCard = PlayerUseCard.Remington;
                break;

            case "RevCarabine":
                playerUseCard = PlayerUseCard.RevCarabine;
                break;

            case "Winchester":
                playerUseCard = PlayerUseCard.Winchester;
                break;

            default:
                playerUseCard = PlayerUseCard.None;
                break;
        }
    }

    private void ShowPossibleBangTargetArea()
    {
        int playerRange = playerCanvas[playerNumber].GetRange();

        int[] defaultDistance = new int[playerCount];
        int[] positionDistance = GetPositionDistance();

        for (int i = 0; i < playerCount; ++i)
        {
            if (playerNumber == i || !playerCanvas[i].isPlaying)
            {
                continue;
            }
            defaultDistance[i] = playerCanvas[i].GetDistance();

            if (playerRange >= defaultDistance[i] + positionDistance[i])
            {
                playerCanvas[i].SetArea(true);
            }
        }
    }

    private int[] GetPositionDistance()
    {
        int[] near = new int[playerCount];

        int index = (playerNumber + 1) % playerCount;
        int distance = 1;

        while (index != playerNumber)
        {
            if (playerCanvas[index].isPlaying)
            {
                near[index] = distance;
                distance++;
            }

            index = (index + 1) % playerCount;
        }

        index = (playerNumber - 1 + playerCount) % playerCount;
        distance = 1;
        while (index != playerNumber)
        {
            if (playerCanvas[index].isPlaying)
            {
                near[index] = Mathf.Min(near[index], distance);
                distance++;
            }

            index = (index - 1 + playerCount) % playerCount;
        }

        return near;
    }

    private int GetLiveCount()
    {
        int liveCount = 0;

        for (int i = 0; i < playerCount; ++i)
        {
            if (playerCanvas[i].isPlaying)
            {
                liveCount++;
            }
        }

        return liveCount;
    }

    private void ShowPossibleDinamiteTargetArea()
    {
        int targetIndex = (BangClient.playerNumber + 1) % playerCount;

        while (!playerCanvas[targetIndex].isPlaying)
        {
            targetIndex = (targetIndex + 1) % playerCount;
        }

        playerCanvas[targetIndex].SetArea(true);
    }

    private void ShowPossiblePrigioneTargetArea()
    {
        for (int i = 0; i < playerCount; ++i)
        {
            if (playerNumber == i ||
               !playerCanvas[i].isPlaying ||
               playerCanvas[i].job == Job.Sheriff)
            {
                continue;
            }

            playerCanvas[i].SetArea(true);
        }
    }

    private void ShowPossiblePanicoTargetArea()
    {
        int playerRange = playerCanvas[playerNumber].GetPanicoRange() + 1;

        int[] defaultDistance = new int[playerCount];
        int[] positionDistance = GetPositionDistance();

        for (int i = 0; i < playerCount; ++i)
        {
            if (playerNumber == i || !playerCanvas[i].isPlaying || playerCanvas[i].GetAllCard(i) == 0)
            {
                continue;
            }

            defaultDistance[i] = playerCanvas[i].GetDistance();

            if (playerRange >= defaultDistance[i] + positionDistance[i])
            {
                playerCanvas[i].SetArea(true);
            }
        }
    }

    private void ShowPossibleCatBalouTargetArea()
    {
        for (int i = 0; i < playerCount; ++i)
        {
            if (playerNumber == i || !playerCanvas[i].isPlaying || playerCanvas[i].GetAllCard(i) == 0)
            {
                continue;
            }

            playerCanvas[i].SetArea(true);
        }
    }

    private void ShowPossibleDuelloTargetArea()
    {
        for (int i = 0; i < playerCount; ++i)
        {
            if (playerNumber == i || !playerCanvas[i].isPlaying)
            {
                continue;
            }

            playerCanvas[i].SetArea(true);
        }
    }
    
    public bool UseCard()
    {
        selectedCard.GetComponent<Image>().color = Color.white;
        
        switch (playerUseCard)
        {
            case PlayerUseCard.Bang:
                return PlayerUseCard_Bang();

            case PlayerUseCard.Beer:
                return PlayerUseCard_Beer();

            case PlayerUseCard.Prigione:
                return PlayerUseCard_Prigione();

            case PlayerUseCard.Dinamite:
                return PlayerUseCard_Dinamite();

            case PlayerUseCard.Panico:
                return PlayerUseCard_Panico();

            case PlayerUseCard.CatBalou:
                return PlayerUseCard_CatBalou();

            case PlayerUseCard.Duello:
                return PlayerUseCard_Duello();

            case PlayerUseCard.Saloon:
                PlayerUseCard_Saloon();
                return true;

            case PlayerUseCard.Diligenza:
            case PlayerUseCard.Diligenza2:
                PlayerUseCard_Diligenza();
                return true;

            case PlayerUseCard.WellsFargo:
                PlayerUseCard_WellsFargo();
                return true;

            case PlayerUseCard.Mustang:
                PlayerUseCard_Mustang();
                return true;

            case PlayerUseCard.Barile:
                PlayerUseCard_Barile();
                return true;

            case PlayerUseCard.Mirino:
                PlayerUseCard_Mirino();
                return true;

            case PlayerUseCard.Volcanic:
                PlayerUseCard_Volcanic();
                return true;

            case PlayerUseCard.Schofield:
                PlayerUseCard_Schofield();
                return true;

            case PlayerUseCard.Remington:
                PlayerUseCard_Remington();
                return true;

            case PlayerUseCard.RevCarabine:
                PlayerUseCard_RevCarabine();
                return true;

            case PlayerUseCard.Winchester:
                PlayerUseCard_Winchester();
                return true;

            case PlayerUseCard.Indian:
                PlayerUseCard_Indian();
                return true;

            case PlayerUseCard.Gatling:
                PlayerUseCard_Gatling();
                return true;

            case PlayerUseCard.Emporio:
                PlayerUseCard_Emporio();
                return true;

            default:
                return false;
        }
    }

    private bool PlayerUseCard_Bang()
    {
        if (bang <= 0)
        {
            return false;
        }

        int targetIndex = FindBangTargetIndex();

        for (int i = 0; i < playerCount; ++i)
        {
            playerCanvas[i].SetArea(false);
        }

        if (targetIndex == -1)
        {
            return false;
        }
        else
        {
            bang--;
            UpdateBangCountText();

            int card = CardTransform.instance.FindPlayCard(selectedCard);
            ToServer.SendToServer(Header.Bang, targetIndex, card);
            
            return true;
        }
    }

    private int FindBangTargetIndex()
    {
        int playerRange = playerCanvas[playerNumber].GetRange();
        int[] positionDistance = GetPositionDistance();

        int targetIndex = -1;
        float minRadius = cardCheckRadius + 1.0f;

        for (int i = 0; i < playerCount; ++i)
        {
            if (playerNumber == i || !playerCanvas[i].isPlaying)
            {
                continue;
            }

            if (playerRange < playerCanvas[i].GetDistance() + positionDistance[i])
            {
                continue;
            }

            float canvasDistance = Vector2.Distance(currentMousePosition, playerCanvas[i].area.position);

            if (canvasDistance <= cardCheckRadius)
            {
                if (canvasDistance < minRadius)
                {
                    minRadius = canvasDistance;
                    targetIndex = i;
                }
            }
        }

        return targetIndex;
    }

    private bool PlayerUseCard_Beer()
    {
        int card = CardTransform.instance.FindPlayCard(selectedCard);
        ToServer.SendToServer(Header.Beer, card);

        return true;
    }

    private bool PlayerUseCard_Prigione()
    {
        int targetIndex = FindPrigioneTargetIndex();

        for (int i = 0; i < playerCount; ++i)
        {
            playerCanvas[i].SetArea(false);
        }

        if (targetIndex == -1)
        {
            return false;
        }
        else
        {
            int card = CardTransform.instance.FindPlayCard(selectedCard);
            ToServer.SendToServer(Header.Prigione, targetIndex, card);

            return true;
        }
    }

    private int FindPrigioneTargetIndex()
    {
        int targetIndex = -1;
        float minRadius = cardCheckRadius + 1.0f;

        for (int i = 0; i < playerCount; ++i)
        {
            if (playerNumber == i || !playerCanvas[i].isPlaying || playerCanvas[i].job == Job.Sheriff)
            {
                continue;
            }

            float canvasDistance = Vector2.Distance(currentMousePosition, playerCanvas[i].area.position);

            if (canvasDistance <= cardCheckRadius)
            {
                if (canvasDistance < minRadius)
                {
                    minRadius = canvasDistance;
                    targetIndex = i;
                }
            }
        }

        return targetIndex;
    }

    private bool PlayerUseCard_Dinamite()
    {
        int targetIndex = (BangClient.playerNumber + 1) % playerCount;

        while (!playerCanvas[targetIndex].isPlaying)
        {
            targetIndex = (targetIndex + 1) % playerCount;
        }

        playerCanvas[targetIndex].SetArea(false);

        float canvasDistance = Vector2.Distance(currentMousePosition, playerCanvas[targetIndex].area.position);

        if (canvasDistance <= cardCheckRadius)
        {
            int card = CardTransform.instance.FindPlayCard(selectedCard);
            ToServer.SendToServer(Header.Dinamite, targetIndex, card);
            return true;
        }

        return false;
    }

    private bool PlayerUseCard_Panico()
    {
        int targetIndex = FindPanicoTargetIndex();

        for (int i = 0; i < playerCount; ++i)
        {
            playerCanvas[i].SetArea(false);
        }

        if (targetIndex == -1)
        {
            return false;
        }
        else
        {
            int card = CardTransform.instance.FindPlayCard(selectedCard);
            ToServer.SendToServer(Header.Panico, targetIndex, card);

            return true;
        }
    }

    private int FindPanicoTargetIndex()
    {
        int playerRange = playerCanvas[playerNumber].GetPanicoRange() + 1;
        int[] positionDistance = GetPositionDistance();

        int targetIndex = -1;
        float minRadius = cardCheckRadius + 1.0f;

        for (int i = 0; i < playerCount; ++i)
        {
            if (playerNumber == i || !playerCanvas[i].isPlaying || playerCanvas[i].GetAllCard(i) == 0)
            {
                continue;
            }

            if (playerRange < playerCanvas[i].GetDistance() + positionDistance[i])
            {
                continue;
            }

            float canvasDistance = Vector2.Distance(currentMousePosition, playerCanvas[i].area.position);

            if (canvasDistance <= cardCheckRadius)
            {
                if (canvasDistance < minRadius)
                {
                    minRadius = canvasDistance;
                    targetIndex = i;
                }
            }
        }

        return targetIndex;
    }

    private bool PlayerUseCard_CatBalou()
    {
        int targetIndex = FindCatBalouTargetIndex();

        for (int i = 0; i < playerCount; ++i)
        {
            playerCanvas[i].SetArea(false);
        }

        if (targetIndex == -1)
        {
            return false;
        }
        else
        {
            int card = CardTransform.instance.FindPlayCard(selectedCard);
            ToServer.SendToServer(Header.CatBalou, targetIndex, card);

            return true;
        }
    }

    private int FindCatBalouTargetIndex()
    {
        int targetIndex = -1;
        float minRadius = cardCheckRadius + 1.0f;

        for (int i = 0; i < playerCount; ++i)
        {
            if (playerNumber == i || !playerCanvas[i].isPlaying || playerCanvas[i].GetAllCard(i) == 0)
            {
                continue;
            }

            float canvasDistance = Vector2.Distance(currentMousePosition, playerCanvas[i].area.position);

            if (canvasDistance <= cardCheckRadius)
            {
                if (canvasDistance < minRadius)
                {
                    minRadius = canvasDistance;
                    targetIndex = i;
                }
            }
        }

        return targetIndex;
    }

    private bool PlayerUseCard_Duello()
    {
        int targetIndex = FindDuelloTargetIndex();

        for (int i = 0; i < playerCount; ++i)
        {
            playerCanvas[i].SetArea(false);
        }

        if (targetIndex == -1)
        {
            return false;
        }
        else
        {
            int card = CardTransform.instance.FindPlayCard(selectedCard);
            ToServer.SendToServer(Header.Duello, targetIndex, card);

            return true;
        }
    }

    private int FindDuelloTargetIndex()
    {
        int targetIndex = -1;
        float minRadius = cardCheckRadius + 1.0f;

        for (int i = 0; i < playerCount; ++i)
        {
            if (playerNumber == i || !playerCanvas[i].isPlaying)
            {
                continue;
            }

            float canvasDistance = Vector2.Distance(currentMousePosition, playerCanvas[i].area.position);

            if (canvasDistance <= cardCheckRadius)
            {
                if (canvasDistance < minRadius)
                {
                    minRadius = canvasDistance;
                    targetIndex = i;
                }
            }
        }

        return targetIndex;
    }

    private void PlayerUseCard_Saloon()
    {
        int card = CardTransform.instance.FindPlayCard(selectedCard);
        ToServer.SendToServer(Header.Saloon, card);
    }

    private void PlayerUseCard_Volcanic()
    {
        int card = CardTransform.instance.FindPlayCard(selectedCard);
        ToServer.SendToServer(Header.EquipGun, card, 1);
    }

    private void PlayerUseCard_Schofield()
    {
        int card = CardTransform.instance.FindPlayCard(selectedCard);
        ToServer.SendToServer(Header.EquipGun, card, 2);
    }

    private void PlayerUseCard_Remington()
    {
        int card = CardTransform.instance.FindPlayCard(selectedCard);
        ToServer.SendToServer(Header.EquipGun, card, 3);
    }

    private void PlayerUseCard_RevCarabine()
    {
        int card = CardTransform.instance.FindPlayCard(selectedCard);
        ToServer.SendToServer(Header.EquipGun, card, 4);
    }

    private void PlayerUseCard_Winchester()
    {
        int card = CardTransform.instance.FindPlayCard(selectedCard);
        ToServer.SendToServer(Header.EquipGun, card, 5);
    }

    private void PlayerUseCard_Indian()
    {
        int card = CardTransform.instance.FindPlayCard(selectedCard);
        ToServer.SendToServer(Header.Indian, card);
    }

    private void PlayerUseCard_Gatling()
    {
        int card = CardTransform.instance.FindPlayCard(selectedCard);
        ToServer.SendToServer(Header.Gatling, card);
    }

    private void PlayerUseCard_Emporio()
    {
        int card = CardTransform.instance.FindPlayCard(selectedCard);
        ToServer.SendToServer(Header.Emporio, card);
    }

    private void PlayerUseCard_Diligenza()
    {
        int card = CardTransform.instance.FindPlayCard(selectedCard);
        ToServer.SendToServer(Header.Diligenza, card);
    }

    private void PlayerUseCard_WellsFargo()
    {
        int card = CardTransform.instance.FindPlayCard(selectedCard);
        ToServer.SendToServer(Header.WellsFargo, card);
    }

    private void PlayerUseCard_Mustang()
    {
        int card = CardTransform.instance.FindPlayCard(selectedCard);
        ToServer.SendToServer(Header.Mustang, card);
    }

    private void PlayerUseCard_Barile()
    {
        int card = CardTransform.instance.FindPlayCard(selectedCard);
        ToServer.SendToServer(Header.Barile, card);
    }

    private void PlayerUseCard_Mirino()
    {
        int card = CardTransform.instance.FindPlayCard(selectedCard);
        ToServer.SendToServer(Header.Mirino, card);
    }
}