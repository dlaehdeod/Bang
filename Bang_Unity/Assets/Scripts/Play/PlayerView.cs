using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChooseInfor
{
    public Transform cardTransform;
    public int cardValue;
}

public class PlayerView : MonoBehaviour
{
    public PlayerController playerController;
    public PlayerZoomInCard playerZoomInCard;
    public PlayerMenuView playerMenuView;
    
    public CardTransform cardTransform;
    public Transform backGroundImage;
    public Transform jobCardParent;
    public Transform characterCardParent;
    public Transform playCardParent;
    public Transform center;
    public PassTurn passTurn;

    public GameObject playerCanvasPrefab;
    public Transform playerCanvasParent;
    public InformationView informationView;

    public PlayerChooseWindow playerChooseWindow;
    public PlayerCardStealMode playerCardStealMode;
    public Transform usedCard;

    private PlayerCanvas[] playerCanvas;
    private int playerSelectTime;
    private int playerCount;

    private int openJobCount;
    private bool openJob;

    private void Start()
    {
        InitCardTransform();
        playerSelectTime = 30;
        openJobCount = 0;
        openJob = false;
    }

    private void InitCardTransform ()
    {
        cardTransform = new CardTransform();
        cardTransform.SetPlayCard(playCardParent);
        cardTransform.SetJobCard(jobCardParent);
        cardTransform.SetCharacterCard(characterCardParent);
    }

    public void ReadyToStartGame(int currentCount)
    {
        SoundManager.instance.SetBackGroundMusic(true);
        playerCount = currentCount;
        CreatePlayerCanvas();

        playerCardStealMode.SetPlayerCanvas(playerCanvas);
        playerController.SetPlayerCanvas(playerCanvas);
        playerController.SetBangCountText();

        ToServer.SendToServer(Header.ReadyToStartGameCompleted, BangClient.playerNumber);
    }
    
    private void CreatePlayerCanvas()
    {
        PlayerPositionAndRotation pr = new PlayerPositionAndRotation();
        pr.Initialize(playerCount);

        List<Vector2> positionList = pr.GetPositionList();
        List<Quaternion> rotationList = pr.GetRotationList();

        playerCanvas = new PlayerCanvas[playerCount];

        for (int i = 0; i < playerCount; ++i)
        {
            playerCanvas[i] = Instantiate(playerCanvasPrefab, playerCanvasParent).GetComponent<PlayerCanvas>();

            int indexOffset = (i - BangClient.playerNumber + playerCount) % playerCount;
            playerCanvas[i].transform.localPosition = positionList[indexOffset];
            playerCanvas[i].transform.localRotation = rotationList[indexOffset];

            playerCanvas[i].center = center;
        }

        playerCanvas[BangClient.playerNumber].SetBasePlayerPosition();
    }

    public void SetPlayerName(int index, string name)
    {
        playerCanvas[index].nameText.text = name;
    }

    public void SetPlayerJob(int index, int job)
    {
        Transform target = cardTransform.jobCard[job];

        playerCanvas[index].SetJobCard(target, job);

        if ((Job)job == Job.Sheriff)
        {
            target.GetChild(0).gameObject.SetActive(false);
        }

        if (BangClient.playerNumber == index)
        {
            informationView.ShowJobCard(target, job);
        }
    }

    public void ChooseCharacter (int playerIndex , int character1, int character2)
    {
        if (BangClient.playerNumber != playerIndex)
        {
            return;
        }

        ChooseInfor[] chooseInfor = new ChooseInfor[2];

        chooseInfor[0].cardTransform = CardTransform.instance.characterCard[character1];
        chooseInfor[0].cardValue = character1;
        chooseInfor[1].cardTransform = CardTransform.instance.characterCard[character2];
        chooseInfor[1].cardValue = character2;
        
        playerChooseWindow.SetTitle("캐릭터를 선택하세요!");
        playerChooseWindow.SetResponseHeader(Header.SetPlayerCharacter);
        playerChooseWindow.SetSelectCount(1);
        playerChooseWindow.SetWindow(chooseInfor);
    }

    public void SetPlayerCharacter(int index, int character)
    {
        Transform target = cardTransform.characterCard[character];

        playerCanvas[index].SetCharacterCard(target, index, character);

        if (BangClient.playerNumber == index)
        {
            playerController.SetPlayerCharacter(character);
            informationView.ShowMessage("다른 플레이어를 기다리는 중입니다.", true);
        }
    }

    public void SetPlayerLife(int index, int life)
    {
        playerCanvas[index].SetLifeCard(life, index);
    }

    public void SetPlayerCard(int index, int[] card)
    {
        Transform[] target = new Transform[card.Length];

        for (int i = 0; i < card.Length; ++i)
        {
            target[i] = cardTransform.playCard[card[i]];
        }

        playerCanvas[index].SetPlayerCard(index, target);
    }

    public void SetPlayerTurn(int index, int bangCount)
    {
        playerController.HideBangCount();
        
        StartCoroutine(PlayerTurn(index, bangCount));
    }

    private IEnumerator PlayerTurn(int index, int bangCount)
    {
        yield return new WaitForSeconds(1.75f);
        string message;

        if (BangClient.playerNumber == index)
        {
            message = "당신의 차례입니다.";

            passTurn.ReadyToPlayerTurn(playerSelectTime);

            playerZoomInCard.PauseScript();

            playerController.enabled = true;
            playerController.SetBangCount(bangCount);
            playerController.ShowBangCount();
        }
        else
        {
            playerZoomInCard.enabled = true;
            message = playerCanvas[index].nameText.text + "님의 차례입니다.";
        }

        informationView.ShowMessage(message, true);
    }

    public void DrawCard(int index, params int[] card)
    {
        Transform[] target = new Transform[card.Length];

        for (int i = 0; i < card.Length; ++i)
        {
            target[i] = cardTransform.playCard[card[i]];
        }

        playerCanvas[index].DrawCard(index, target);
    }

    public void BlackJackDraw(int index, int[] card)
    {
        StartCoroutine(DelayBlackJackDrawCard(index, card));
    }

    private IEnumerator DelayBlackJackDrawCard(int index, int[] card)
    {
        yield return new WaitForSeconds(1.0f);

        Transform[] target = new Transform[card.Length];

        for (int i = 0; i < card.Length; ++i)
        {
            target[i] = cardTransform.playCard[card[i]];
        }

        playerCanvas[index].BlackJackDrawAndCardOpen(index, center, target);

        string message = playerCanvas[index].nameText.text + "블랙잭의 카드오픈!";
        informationView.ShowMessage(message, true);
    }

    public void KitCarlsonDraw(int index, int[] card)
    {
        StartCoroutine(DelayKitCarlsonDraw(index, card));
    }

    private IEnumerator DelayKitCarlsonDraw(int index, int[] card)
    {
        yield return new WaitForSeconds(0.5f);
        DrawCard(index, card);
        yield return new WaitForSeconds(2.0f);

        if (BangClient.playerNumber == index)
        {
            playerZoomInCard.PauseScript();

            ChooseInfor[] chooseInfor = GetChoosePlayCardInfor(card);

            playerChooseWindow.SetTitle("되돌려 놓을 카드 1장을 선택하세요.");
            playerChooseWindow.SetResponseHeader(Header.KitCarlsonRestoreCard);
            playerChooseWindow.SetSelectCount(1);
            playerChooseWindow.SetWindow(chooseInfor, 20);
        }
        else
        {
            string message = playerCanvas[index].nameText.text + " 님이 카드를 선택하고 있습니다.";
            informationView.ShowMessage(message, true);
        }
    }

    private ChooseInfor[] GetChoosePlayCardInfor(int[] card)
    {
        ChooseInfor[] chooseInfor = new ChooseInfor[card.Length];

        for (int i = 0; i < card.Length; ++i)
        {
            chooseInfor[i].cardTransform = cardTransform.playCard[card[i]];
            chooseInfor[i].cardValue = card[i];
        }

        return chooseInfor;
    }

    public void KitCarlsonRestoreCard(int playerIndex, int card)
    {
        Transform target = cardTransform.playCard[card];

        playerCanvas[playerIndex].KitCarlsonRestoreCard(target, playCardParent);
    }

    public void PedroRamirezSelectCard(int index)
    {
        StartCoroutine(DelayPedroRamirezSelectCard(index));
    }

    private IEnumerator DelayPedroRamirezSelectCard(int index)
    {
        yield return new WaitForSeconds(1.25f);

        if (BangClient.playerNumber == index)
        {
            informationView.ShowMessage("가져올 카드를 선택하세요!", true);
            playerCardStealMode.PedroRamirezTurn();
        }
        else
        {
            string message = playerCanvas[index].nameText.text + " 님이 카드를 선택하고 있습니다.";
            informationView.ShowMessage(message, true);
        }
    }

    public void PedroRamirezDrawCard(int index, int card)
    {
        Transform selectCard = cardTransform.playCard[card];
        selectCard.GetChild(0).gameObject.SetActive(true);

        playerCanvas[index].DrawCard(index, selectCard);
    }

    public void JesseJonesSelectCard(int index)
    {
        StartCoroutine(DelayJesseJonesSelectCard(index));
    }

    private IEnumerator DelayJesseJonesSelectCard(int index)
    {
        yield return new WaitForSeconds(1.25f);

        if (BangClient.playerNumber == index)
        {
            informationView.ShowMessage("가져올 카드를 선택하세요!", true);
            playerCardStealMode.JesseJonesTurn();
        }
        else
        {
            string message = playerCanvas[index].nameText.text + " 님이 카드를 선택하고 있습니다.";
            informationView.ShowMessage(message, true);
        }
    }

    public void StealCard(int fromIndex, int targetIndex, int card, int isEquipCard)
    {
        Transform stealCard = cardTransform.playCard[card];
        stealCard.GetChild(0).gameObject.SetActive(true);

        playerCanvas[fromIndex].StealCard(fromIndex, stealCard);
        playerCanvas[targetIndex].PlayerGameCardSort(playerCanvas[targetIndex].gameCard.childCount);

        if (isEquipCard > 0)
        {
            UpdateEquipState(targetIndex, card);
        }
    }

    private void UpdateEquipState(int playerIndex, int card)
    {
        string cardName = ((Card)card).ToString().Substring(3);

        switch (cardName)
        {
            case "Winchester":
            case "RevCarabine":
            case "Remington":
            case "Schofield":
            case "Volcanic":

                if (BangClient.playerNumber == playerIndex)
                {
                    playerController.SetBangCount(1);

                    if (playerController.enabled)
                    {
                        playerController.ShowBangCount();
                    }
                }

                playerCanvas[playerIndex].gunRange = 1;
                break;

            case "Mustang":
                playerCanvas[playerIndex].mustangDistance = 0;
                break;

            case "Mirino":
                playerCanvas[playerIndex].mirinoRange = 0;
                break;

            default:
                break;
        }
    }

    public void ElGringoStealCard(int fromIndex, int targetIndex)
    {
        StartCoroutine(DelayElGringoStealCard(fromIndex, targetIndex));
    }

    private IEnumerator DelayElGringoStealCard(int fromIndex, int targetIndex)
    {
        yield return new WaitForSeconds(0.5f);

        if (BangClient.playerNumber == fromIndex)
        {
            playerZoomInCard.PauseScript();

            informationView.ShowMessage("빼앗을 카드를 선택하세요!", true);
            playerCardStealMode.ElGringoStealCard(targetIndex);
        }
        else
        {
            string message = playerCanvas[fromIndex].nameText.text + " 님이 카드를 선택하고 있습니다.";
            informationView.ShowMessage(message, true);
        }
    }

    public void BangCardOpen(int targetIndex, int damage, int[] card)
    {
        StartCoroutine(DelayBangCardOpen(targetIndex, damage, card));
    }

    private IEnumerator DelayBangCardOpen(int targetIndex, int damage, int[] card)
    {
        yield return new WaitForSeconds(1.0f);

        Transform[] target = new Transform[card.Length];

        for (int i = 0; i < card.Length; ++i)
        {
            target[i] = cardTransform.playCard[card[i]];
        }

        playerCanvas[targetIndex].BangCardOpen(targetIndex, damage, target, usedCard);
        string message = playerCanvas[targetIndex].nameText.text + "카드오픈!";
        informationView.ShowMessage(message, true);
    }

    public void BangCardDoubleOpen(int targetIndex, int damage, int[] card)
    {
        StartCoroutine(DelayCardDoubleOpen(targetIndex, damage, card));
    }

    private IEnumerator DelayCardDoubleOpen(int targetIndex, int damage, int[] card)
    {
        yield return new WaitForSeconds(1.0f);

        Transform[] target = new Transform[card.Length];

        for (int i = 0; i < card.Length; ++i)
        {
            target[i] = cardTransform.playCard[card[i]];
        }

        playerCanvas[targetIndex].BangCardDoubleOpen(targetIndex, damage, target, usedCard);
        string message = playerCanvas[targetIndex].nameText.text + "럭키듀크의 카드오픈!";
        informationView.ShowMessage(message, true);
    }

    public void Bang(int targetIndex, int damage, int[] card)
    {
        playerZoomInCard.PauseScript();

        StartCoroutine(DelayBang(targetIndex, damage, card));
    }

    private IEnumerator DelayBang(int targetIndex, int damage, int[] card)
    {
        yield return new WaitForSeconds(1.0f);

        if (BangClient.playerNumber == targetIndex)
        {
            ChooseInfor[] chooseInfor = GetChoosePlayCardInfor(card);

            playerChooseWindow.SetTitle("공격 당했습니다! 빗나감 카드 " + damage.ToString() + "장이 필요합니다.");
            playerChooseWindow.SetResponseHeader(Header.BangResponse);
            playerChooseWindow.SetSelectCount(damage);
            playerChooseWindow.SetWindow(chooseInfor);
            playerChooseWindow.SetActiveNotResistButton(true);
        }
        else
        {
            string message = playerCanvas[targetIndex].nameText.text + " 님이 카드를 선택하고 있습니다.";
            informationView.ShowMessage(message, true);
        }
    }

    public void AttackCardMove(int fromIndex, int targetIndex, int card)
    {
        cardTransform.playCard[card].SetParent(playerCanvas[targetIndex].attackedCard);
        playerCanvas[fromIndex].PlayerGameCardSort(playerCanvas[fromIndex].gameCard.childCount);

        Transform attackCard = cardTransform.playCard[card];
        playerCanvas[targetIndex].EquipAttackCard(attackCard);
    }

    public void Emporio(int playerIndex, int[] card)
    {
        playerZoomInCard.PauseScript();

        StartCoroutine(DelayEmporio(playerIndex, card));
    }

    private IEnumerator DelayEmporio(int playerIndex, int[] card)
    {
        yield return new WaitForSeconds(1.0f);

        playerChooseWindow.otherPlayerEmporio = true;
        playerChooseWindow.SetTitle(playerCanvas[playerIndex].nameText.text + "님이 카드를 고르는 중입니다.");

        ChooseInfor[] chooseInfor = GetChoosePlayCardInfor(card);

        if (BangClient.playerNumber == playerIndex)
        {
            playerChooseWindow.otherPlayerEmporio = false;
            playerChooseWindow.SetTitle("가져갈 카드를 1장 선택하세요.");
            playerChooseWindow.SetResponseHeader(Header.EmporioGetCard);
            playerChooseWindow.SetSelectCount(1);
            playerChooseWindow.SetWindow(chooseInfor);
        }
        else
        {
            playerChooseWindow.SetEmporioWindow(chooseInfor);
        }
    }

    public void EmporioGetCard(int playerIndex, int card)
    {
        Transform target = cardTransform.playCard[card];

        playerCanvas[playerIndex].DrawCard(playerIndex, target);
    }

    public void EmporioFinish(int playerIndex)
    {
        playerChooseWindow.DeactiveWindow();
        ContinueGame(playerIndex);
    }
    
    public void ContinueGame(int index)
    {
        if (BangClient.playerNumber == index)
        {
            passTurn.StartTimer();
            playerController.enabled = true;
        }
        else
        {
            playerZoomInCard.enabled = true;
            informationView.ShowMessage(playerCanvas[index].nameText.text + "님이 카드를 선택하고 있습니다.", true);
        }
    }

    public void Gatling(int targetIndex, int[] card)
    {
        StartCoroutine(DelayGatling(targetIndex, card));
    }

    private IEnumerator DelayGatling(int targetIndex, int[] card)
    {
        yield return new WaitForSeconds(1.0f);

        if (BangClient.playerNumber == targetIndex)
        {
            playerZoomInCard.PauseScript();

            ChooseInfor[] chooseInfor = GetChoosePlayCardInfor(card);

            playerChooseWindow.SetTitle("기관총 난사! 빗나감 카드가 필요합니다.");
            playerChooseWindow.SetResponseHeader(Header.WideAttackResponse);
            playerChooseWindow.SetSelectCount(1);
            playerChooseWindow.SetWindow(chooseInfor);
            playerChooseWindow.SetActiveNotResistButton(true);
        }
    }

    public void GatlingDropCard(int playerIndex, int card)
    {
        DropCard(playerIndex, card);

        if (BangClient.playerNumber == playerIndex)
        {
            informationView.ShowMessage("기관총 공격! 다른 플레이어들이 카드를 선택중입니다.", true, 1.0f);
        }
    }

    public void Indian(int targetIndex, int[] card)
    {
        StartCoroutine(DelayIndian(targetIndex, card));
    }

    private IEnumerator DelayIndian(int targetIndex, int[] card)
    {
        yield return new WaitForSeconds(1.0f);

        if (BangClient.playerNumber == targetIndex)
        {
            playerZoomInCard.PauseScript();

            ChooseInfor[] chooseInfor = new ChooseInfor[card.Length];
            for (int i = 0; i < card.Length; ++i)
            {
                chooseInfor[i].cardTransform = cardTransform.playCard[card[i]];
                chooseInfor[i].cardValue = card[i];
            }

            playerChooseWindow.SetTitle("인디언의 습격! 뱅 카드가 필요합니다.");
            playerChooseWindow.SetResponseHeader(Header.WideAttackResponse);
            playerChooseWindow.SetSelectCount(1);
            playerChooseWindow.SetWindow(chooseInfor);
            playerChooseWindow.SetActiveNotResistButton(true);
        }
    }
    
    public void IndianDropCard(int playerIndex, int card)
    {
        DropCard(playerIndex, card);

        if (BangClient.playerNumber == playerIndex)
        {
            informationView.ShowMessage("인디언 공격! 다른 플레이어들이 카드를 선택중입니다.", true, 1.0f);
        }
    }

    public void Duello(int playerIndex, int[] card)
    {
        StartCoroutine(DelayDuello(playerIndex, card));
    }

    private IEnumerator DelayDuello(int playerIndex, int[] card)
    {
        yield return new WaitForSeconds(1.0f);

        if (BangClient.playerNumber == playerIndex)
        {
            playerZoomInCard.PauseScript();

            ChooseInfor[] chooseInfor = GetChoosePlayCardInfor(card);
        
            playerChooseWindow.SetTitle("결투! 뱅 카드가 필요합니다.");
            playerChooseWindow.SetResponseHeader(Header.DuelloResponse);
            playerChooseWindow.SetSelectCount(1);
            playerChooseWindow.SetWindow(chooseInfor);
            playerChooseWindow.SetActiveNotResistButton(true);
        }
        else
        {
            informationView.ShowMessage(playerCanvas[playerIndex].nameText.text + "님이 카드를 선택중입니다.", true, 0.5f);
        }
    }

    public void EquipGun(int playerIndex, int range, int card)
    {
        string message = "";

        switch (range)
        {
            case 1:
                message = "Volcanic!";
                if (BangClient.playerNumber == playerIndex)
                {
                    playerController.SetBangCount(1000);
                    playerController.ShowBangCount();
                }
                break;
            case 2:
                message = "Schofield!";
                break;
            case 3:
                message = "Remington!";
                break;
            case 4:
                message = "RevCarabine!";
                break;
            case 5:
                message = "Winchester!";
                break;
            default:
                break;
        }

        playerCanvas[playerIndex].gunRange = range;
        Transform target = cardTransform.playCard[card];
        playerCanvas[playerIndex].EquipMiddleCard(target);
        StartCoroutine(DelayStartContinueGame(playerIndex, message));
    }

    private IEnumerator DelayStartContinueGame(int playerIndex, string message)
    {
        informationView.ShowMessage(message, true, 0.25f);
        yield return new WaitForSeconds(1.25f);
        ContinueGame(playerIndex);
    }

    public void EquipFrontCard(int index, int card)
    {
        Transform target = cardTransform.playCard[card];

        playerCanvas[index].EquipFrontCard(target);
    }

    public void Mirino(int playerIndex, int card)
    {
        string message = "조준경 장착!";
        playerCanvas[playerIndex].mirinoRange = 1;

        EquipFrontCard(playerIndex, card);
        StartCoroutine(DelayStartContinueGame(playerIndex, message));
    }

    public void Barile(int playerIndex, int card)
    {
        string message = "술통 장착!";

        EquipFrontCard(playerIndex, card);
        StartCoroutine(DelayStartContinueGame(playerIndex, message));
    }

    public void Mustang(int playerIndex, int card)
    {
        string message = "야생마!";
        playerCanvas[playerIndex].mustangDistance = 1;

        EquipFrontCard(playerIndex, card);
        StartCoroutine(DelayStartContinueGame(playerIndex, message));
    }

    public void WellsFargo(int playerIndex, int[] card)
    {
        string message = "웰스파고!!!";

        StartCoroutine(DelayDrawCardAndContinueGame(playerIndex, card, message));
    }

    public void Diligenza(int playerIndex, int[] card)
    {
        string message = "역마차!";

        StartCoroutine(DelayDrawCardAndContinueGame(playerIndex, card, message));
    }

    private IEnumerator DelayDrawCardAndContinueGame(int playerIndex, int[] drawCard, string message)
    {
        informationView.ShowMessage(message, true, 1.0f);
        yield return new WaitForSeconds(0.5f);

        DrawCard(playerIndex, drawCard);
        yield return new WaitForSeconds(drawCard.Length);
        ContinueGame(playerIndex);
    }

    public void Saloon(int playerIndex, int[] life)
    {
        string message = "주점! 모두의 체력 회복!";

        for (int i = 0; i < playerCount; ++i)
        {
            playerCanvas[i].UpdateLife(life[i]);
        }

        StartCoroutine(DelayStartContinueGame(playerIndex, message));
    }

    public void Beer(int playerIndex, int life)
    {
        playerCanvas[playerIndex].UpdateLife(life);
        string message = "맥쥬!";

        StartCoroutine(DelayStartContinueGame(playerIndex, message));
    }

    public void Panico(int fromIndex, int targetIndex, int card)
    {
        AttackCardMove(fromIndex, targetIndex, card);

        StartCoroutine(DelayPanico(fromIndex, targetIndex));
    }

    private IEnumerator DelayPanico(int fromIndex, int targetIndex)
    {
        yield return new WaitForSeconds(1.25f);

        if (BangClient.playerNumber == fromIndex)
        {
            informationView.ShowMessage("가져올 카드를 선택하세요!", true);
            passTurn.StopTimer();
            playerCardStealMode.UsePanicoCard(targetIndex);
        }
        else
        {
            string message = playerCanvas[fromIndex].nameText.text + " 님이 카드를 선택하고 있습니다.";
            informationView.ShowMessage(message, true);
        }
    }

    public void CatBalou(int fromIndex, int targetIndex, int card)
    {
        AttackCardMove(fromIndex, targetIndex, card);

        StartCoroutine(DelayCatBalou(fromIndex, targetIndex));
    }

    private IEnumerator DelayCatBalou(int fromIndex, int targetIndex)
    {
        yield return new WaitForSeconds(1.25f);

        if (BangClient.playerNumber == fromIndex)
        {
            informationView.ShowMessage("버릴 카드를 선택하세요!", true);
            passTurn.StopTimer();
            playerCardStealMode.UseCatBalouCard(targetIndex);
        }
        else
        {
            string message = playerCanvas[fromIndex].nameText.text + " 님이 카드를 선택하고 있습니다.";
            informationView.ShowMessage(message, true);
        }
    }

    public void CatBalouDropCard (int playerIndex, int card, int equipCard)
    {
        DropCard(playerIndex, card);
        
        if (equipCard == 0)
        {
            return;
        }

        UpdateEquipState(playerIndex, card);
    }

    public void CardOpenOrderSelect(int playerIndex, int dinamite, int prigione)
    {
        if (BangClient.playerNumber == playerIndex)
        {
            playerZoomInCard.PauseScript();

            ChooseInfor[] chooseInfor = new ChooseInfor[2];
            chooseInfor[0].cardTransform = CardTransform.instance.playCard[dinamite];
            chooseInfor[0].cardValue = dinamite;

            chooseInfor[1].cardTransform = CardTransform.instance.playCard[prigione];
            chooseInfor[1].cardValue = prigione;

            playerChooseWindow.SetTitle("먼저 카드뽑기를 할 카드를 선택하세요.");
            playerChooseWindow.SetResponseHeader(Header.CardOpenOrderSelectCompleted);
            playerChooseWindow.SetSelectCount(1);
            playerChooseWindow.SetWindow(chooseInfor);
        }
        else
        {
            informationView.ShowMessage(playerCanvas[playerIndex].nameText.text + "님이 카드펼치기 순서를 정하고 있습니다.", true);
        }
    }
    
    public void Prigione(int fromIndex, int targetIndex, int card)
    {
        string message = playerCanvas[targetIndex].nameText.text + "님이 감옥에 갇혔습니다!";

        if (BangClient.playerNumber == targetIndex)
        {
            message = "당신은 감옥에 갇혔습니다!";
        }

        EquipFrontCard(targetIndex, card);
        playerCanvas[fromIndex].PlayerGameCardSort(playerCanvas[fromIndex].gameCard.childCount);

        StartCoroutine(DelayStartContinueGame(fromIndex, message));
    }

    public void PrigioneCardOpen(int playerIndex, int[] card)
    {
        StartCoroutine(DelayPrigioneCardOpen(playerIndex, card));
    }

    private IEnumerator DelayPrigioneCardOpen(int playerIndex, int[] card)
    {
        yield return new WaitForSeconds(0.5f);

        Transform[] target = new Transform[card.Length];

        for (int i = 0; i < card.Length; ++i)
        {
            target[i] = cardTransform.playCard[card[i]];
        }

        playerCanvas[playerIndex].PrigioneCardOpen(playerIndex, target, usedCard);

        string message = playerCanvas[playerIndex].nameText.text + "님의 감옥 탈출카드 오픈!";
        informationView.ShowMessage(message, true);
    }

    public void Dinamite(int fromIndex, int targetIndex, int card)
    {
        string message = "다이너마이트 등장!";

        EquipFrontCard(targetIndex, card);
        playerCanvas[fromIndex].PlayerGameCardSort(playerCanvas[fromIndex].gameCard.childCount);

        StartCoroutine(DelayStartContinueGame(fromIndex, message));
    }

    public void DinamiteCardOpen(int playerIndex, int dinamiteExplosion, int[] card)
    {
        StartCoroutine(DelayDinamiteCardOpen(playerIndex, dinamiteExplosion, card));
    }

    private IEnumerator DelayDinamiteCardOpen(int playerIndex, int dinamiteExplosion, int[] card)
    {
        yield return new WaitForSeconds(0.5f);

        Transform[] target = new Transform[card.Length];

        for (int i = 0; i < card.Length; ++i)
        {
            target[i] = cardTransform.playCard[card[i]];
        }

        playerCanvas[playerIndex].DinamiteCardOpen(playerIndex, dinamiteExplosion, target, usedCard);

        string message = playerCanvas[playerIndex].nameText.text + "님의 다이너마이트 카드 오픈!";
        informationView.ShowMessage(message, true);
    }

    public void DinamiteExplosion(int targetIndex, int dinamite, int life, int isLiving)
    {
        StartCoroutine(DinamiteExplosionEffect(targetIndex, dinamite, life, isLiving));
    }

    private IEnumerator DinamiteExplosionEffect(int targetIndex, int dinamite, int life, int isLiving)
    {
        float width = Screen.width * 0.2f;
        float height = Screen.height * 0.2f;

        SoundManager.instance.PlayDynamiteSound();
        informationView.ShowMessage("다이너마이트 폭파!!", true);
        int moveCount = 150;
        while (moveCount > 0)
        {
            yield return new WaitForSeconds(0.01f);
            moveCount--;
            float x = Random.Range(-width, width);
            float y = Random.Range(-height, height);

            width *= 0.963f;
            height *= 0.963f;

            backGroundImage.localPosition = new Vector2(x, y);
        }

        DropCard(targetIndex, dinamite);
        UpdateLife(targetIndex, life);

        if (BangClient.playerNumber == targetIndex)
        {
            ToServer.SendToServer(Header.DinamiteExplosionCompleted, targetIndex, isLiving);
        }
    }

    public void DinamitePass(int fromIndex, int targetIndex, int card)
    {
        EquipFrontCard(targetIndex, card);
        playerCanvas[fromIndex].EquipFrontCardSort();
    }

    public void UpdateLife(int targetIndex, int life)
    {
        playerCanvas[targetIndex].UpdateLife(life);
    }

    public void CardShuffle()
    {
        while (usedCard.childCount > 1)
        {
            Transform card = usedCard.GetChild(0);
            card.SetParent(playCardParent);
            card.localPosition = Vector3.zero;
            card.GetChild(0).gameObject.SetActive(true);
        }
    }

    public void DropCard(int playerIndex, params int[] card)
    {
        StartCoroutine(DelayDropCard(playerIndex, card));
    }

    private IEnumerator DelayDropCard(int playerIndex, int[] card)
    {
        for (int i = 0; i < card.Length; ++i)
        {
            yield return new WaitForSeconds(0.25f);
            Transform target = cardTransform.playCard[card[i]];
            playerCanvas[playerIndex].DropCard(target, usedCard);
        }
    }

    public void PlayerMustDropCard(int playerIndex, int dropCount, int[] card)
    {
        if (BangClient.playerNumber != playerIndex)
        {
            return;
        }

        ChooseInfor[] chooseInfor = GetChoosePlayCardInfor(card);

        playerChooseWindow.SetResponseHeader(Header.ResponseDropCard);
        playerChooseWindow.SetSelectCount(dropCount);
        playerChooseWindow.SetActiveNotResistButton(false);

        if (playerCanvas[BangClient.playerNumber].character == Character.SidKetchum)
        {
            if (dropCount > 0)
            {
                playerChooseWindow.SetTitle("최소 " + dropCount + "장을 버려야합니다. 카드 2장당 체력이 1씩 회복됩니다.");
            }
            else
            {
                playerChooseWindow.SetTitle("카드를 추가로 버려 체력을 회복할 수 있습니다.");
                playerChooseWindow.SetActiveNotResistButton(true);
            }
            
            playerChooseWindow.SetSidKetchum();
            playerChooseWindow.SetWindow(chooseInfor, 20);
        }
        else
        {
            playerChooseWindow.SetTitle("버릴 카드를 선택하세요!");
            playerChooseWindow.SetWindow(chooseInfor, passTurn.timer);
        }
    }

    public void OpenJob(int openPlayerIndex, int openPlayerJob, int attackerIndex)
    {
        playerZoomInCard.PauseScript();

        Transform playerJobCard = cardTransform.jobCard[openPlayerJob];
        playerJobCard.SetParent(center.transform);

        playerCanvas[openPlayerIndex].isPlaying = false;
        openJobCount++;
        StartCoroutine(ShowOpenJob(playerJobCard, openPlayerIndex, openPlayerJob, attackerIndex));
    }

    private IEnumerator ShowOpenJob(Transform playerJob, int openPlayerIndex, int openPlayerJob, int attackerIndex)
    {
        while (openJob)
        {
            yield return new WaitForSeconds(1.0f);
        }

        openJob = true;
        yield return new WaitForSeconds(0.75f);

        Vector3 originPos = playerJob.localPosition;
        Quaternion originRot = playerJob.localRotation;
        Vector3 desireSize = Vector3.one * 2.5f;
        yield return new WaitForSeconds(0.5f);
        float moveRatio = 0.0f;

        while (moveRatio < 1.0f)
        {
            yield return new WaitForSeconds(0.01f);
            playerJob.localPosition = Vector3.Lerp(originPos, Vector3.zero, moveRatio);
            playerJob.localRotation = Quaternion.Lerp(originRot, Quaternion.identity, moveRatio);
            playerJob.localScale = Vector3.Lerp(Vector3.one, desireSize, moveRatio);
            moveRatio += 0.025f;
        }

        yield return new WaitForSeconds(1.0f);

        playerJob.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        string[] jobName = { "보안관", "배신자", "무법자", "무법자", "부관", "무법자", "부관" };

        string message = playerCanvas[openPlayerIndex].nameText.text + "님은 " + jobName[openPlayerJob] + "입니다.";
        informationView.ShowMessage(message, true, 1.0f);

        yield return new WaitForSeconds(2.0f);
        playerJob.SetParent(jobCardParent);
        originPos = playerJob.localPosition;

        moveRatio = 0.0f;
        while (moveRatio < 1.0f)
        {
            yield return new WaitForSeconds(0.01f);
            playerJob.localPosition = Vector3.Lerp(originPos, Vector3.zero, moveRatio);
            playerJob.localScale = Vector3.Lerp(desireSize, Vector3.one, moveRatio);
            moveRatio += 0.025f;
        }

        playerJob.localPosition = Vector3.zero;
        playerJob.localRotation = Quaternion.identity;
        playerJob.localScale = Vector3.one;

        playerJob.GetChild(0).gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        if (BangClient.playerNumber == openPlayerIndex)
        {
            ToServer.SendToServer(Header.OpenJobCompleted, openPlayerIndex, openPlayerJob, attackerIndex);
        }
        
        openJob = false;
        openJobCount--;
    }

    public void VultureSameBringCard(int targetIndex, int vultureSameIndex, int[] card)
    {
        Transform[] target = new Transform[card.Length];
        Transform newParent = playerCanvas[vultureSameIndex].gameCard;

        for (int i = 0; i < card.Length; ++i)
        {
            target[i] = cardTransform.playCard[card[i]];
            target[i].SetParent(newParent);

            target[i].GetChild(0).gameObject.SetActive(true);
        }

        playerCanvas[vultureSameIndex].DrawCard(vultureSameIndex, target);
        playerCanvas[targetIndex].Defeat(null, usedCard, characterCardParent);
    }

    public void Defeat(int index, int[] card)
    {
        playerCanvas[index].Defeat(card, usedCard, characterCardParent);
    }

    public void GameOver (string message)
    {
        StartCoroutine(ResetGame(message));
    }
    
    private IEnumerator ResetGame(string message)
    {
        while (openJobCount > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        playerCardStealMode.DeactiveStealMode();
        playerController.PauseScript();
        playerController.HideBangCount();
        playerZoomInCard.PauseScript();
        playerChooseWindow.GameOverDeactive();
        passTurn.StopTimer();

        informationView.ShowMessage(message, true, 1.0f);

        yield return new WaitForSeconds(2.0f);

        cardTransform.ResetPlayCard(playCardParent);
        cardTransform.ResetJobCard(jobCardParent);
        cardTransform.ResetCharacterCard(characterCardParent);
       
        for (int i = 0; i < playerCanvas.Length; ++i)
        {
            Destroy(playerCanvas[i].gameObject);
            playerCanvas[i] = null;
        }

        yield return new WaitForSeconds(1.0f);

        SoundManager.instance.SetBackGroundMusic(false);

        playerMenuView.SetActive(true, 1);
        
        ToServer.SendToServer(Header.SetClientName, BangClient.playerName);
    }
}