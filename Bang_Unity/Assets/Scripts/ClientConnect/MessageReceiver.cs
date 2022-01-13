using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageReceiver : MonoBehaviour
{
    public BangClient bangClient;
    public PlayerMenuView playerMenuView;
    public PlayerView playerView;
    public Chatting chatting;

    private Queue<byte[]> messageQueue;

    void Start()
    {
        messageQueue = new Queue<byte[]>();

        bangClient = new BangClient();
        bangClient.Connect();
        bangClient.enqueueMessage = EnqueueMessage;

        StartCoroutine(ConnectCheck());
        StartCoroutine(MessageProcessing());
    }

    private void EnqueueMessage (byte[] message)
    {
        messageQueue.Enqueue(message);
    }

    private IEnumerator ConnectCheck ()
    {
        yield return new WaitForSeconds(2.0f);
        if (!BangClient.isConnected)
        {
            chatting.PrintMessage("<color=red>접속에 실패하였습니다.</color>");
            yield return new WaitForSeconds(0.5f);
            SceneChange.instance.NextScene("Main");
        }
    }

    private void BackToMain (byte[] message)
    {
        StartCoroutine(ConnectCheck());
    }

    private IEnumerator MessageProcessing ()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.01f);

            while (messageQueue.Count > 0)
            {
                Header header = MessageManager.GetHeader(messageQueue.Peek());

                switch (header)
                {
                    case Header.Welcome:
                        PrintWelcome(messageQueue.Dequeue());
                        break;

                    case Header.Chatting:
                        PrintChatting(messageQueue.Dequeue());
                        break;

                    case Header.SetPlayerNumber:
                        SetPlayerNumber_ChattingPermission_ShowStartButton(messageQueue.Dequeue());
                        break;

                    case Header.ShowPlayerState:
                        ShowPlayerState(messageQueue.Dequeue());
                        break;

                    case Header.ReadyToStartGame:
                        ReadyToStartGame(messageQueue.Dequeue());
                        break;

                    case Header.SetPlayerName:
                        SetPlayerName(messageQueue.Dequeue());
                        break;

                    case Header.SetPlayerJob:
                        SetPlayerJob(messageQueue.Dequeue());
                        break;

                    case Header.ChooseCharacter:
                        ChooseCharacter(messageQueue.Dequeue());
                        break;

                    case Header.SetPlayerCharacter:
                        SetPlayerCharacter(messageQueue.Dequeue());
                        break;

                    case Header.SetPlayerLife:
                        SetPlayerLife(messageQueue.Dequeue());
                        break;

                    case Header.SetPlayerCard:
                        SetPlayerCard(messageQueue.Dequeue());
                        break;
                        
                    case Header.SetPlayerTurn:
                        SetPlayerTurn(messageQueue.Dequeue());
                        break;

                    case Header.DrawCard:
                        DrawCard(messageQueue.Dequeue());
                        break;

                    case Header.BlackJackDraw:
                        BlackJackDraw(messageQueue.Dequeue());
                        break;

                    case Header.KitCarlsonDraw:
                        KitCarlsonDraw(messageQueue.Dequeue());
                        break;

                    case Header.KitCarlsonRestoreCard:
                        KitCarlsonRestoreCard(messageQueue.Dequeue());
                        break;

                    case Header.PedroRamirezSelectCard:
                        PedroRamirezSelectCard(messageQueue.Dequeue());
                        break;

                    case Header.PedroRamirezDrawCard:
                        PedroRamirezDrawCard(messageQueue.Dequeue());
                        break;

                    case Header.JesseJonesSelectCard:
                        JesseJonesSelectCard(messageQueue.Dequeue());
                        break;

                    case Header.ElGringoStealCard:
                        ElGringoStealCard(messageQueue.Dequeue());
                        break;

                    case Header.StealCard:
                        StealCard(messageQueue.Dequeue());
                        break;

                    case Header.BangCardOpen:
                        BangCardOpen(messageQueue.Dequeue());
                        break;

                    case Header.BangCardDoubleOpen:
                        BangCardDoubleOpen(messageQueue.Dequeue());
                        break;

                    case Header.Bang:
                        Bang(messageQueue.Dequeue());
                        break;

                    case Header.AttackCardMove:
                        AttackCardMove(messageQueue.Dequeue());
                        break;

                    case Header.Emporio:
                        Emporio(messageQueue.Dequeue());
                        break;

                    case Header.EmporioGetCard:
                        EmporioGetCard(messageQueue.Dequeue());
                        break;

                    case Header.EmporioFinish:
                        EmporioFinish(messageQueue.Dequeue());
                        break;

                    case Header.Gatling:
                        Gatling(messageQueue.Dequeue());
                        break;

                    case Header.GatlingDropCard:
                        GatlingDropCard(messageQueue.Dequeue());
                        break;

                    case Header.Indian:
                        Indian(messageQueue.Dequeue());
                        break;

                    case Header.IndianDropCard:
                        IndianDropCard(messageQueue.Dequeue());
                        break;

                    case Header.Duello:
                        Duello(messageQueue.Dequeue());
                        break;

                    case Header.EquipGun:
                        EquipGun(messageQueue.Dequeue());
                        break;

                    case Header.Mirino:
                        Mirino(messageQueue.Dequeue());
                        break;

                    case Header.Barile:
                        Barile(messageQueue.Dequeue());
                        break;

                    case Header.Mustang:
                        Mustang(messageQueue.Dequeue());
                        break;

                    case Header.WellsFargo:
                        WellsFargo(messageQueue.Dequeue());
                        break;

                    case Header.Diligenza:
                        Diligenza(messageQueue.Dequeue());
                        break;

                    case Header.Saloon:
                        Saloon(messageQueue.Dequeue());
                        break;

                    case Header.Beer:
                        Beer(messageQueue.Dequeue());
                        break;

                    case Header.Panico:
                        Panico(messageQueue.Dequeue());
                        break;

                    case Header.CatBalou:
                        CatBalou(messageQueue.Dequeue());
                        break;

                    case Header.CatBalouDropCard:
                        CatBalouDropCard(messageQueue.Dequeue());
                        break;

                    case Header.CardOpenOrderSelect:
                        CardOpenOrderSelect(messageQueue.Dequeue());
                        break;

                    case Header.Prigione:
                        Prigione(messageQueue.Dequeue());
                        break;

                    case Header.PrigioneCardOpen:
                        PrigioneCardOpen(messageQueue.Dequeue());
                        break;

                    case Header.Dinamite:
                        Dinamite(messageQueue.Dequeue());
                        break;

                    case Header.DinamiteCardOpen:
                        DinamiteCardOpen(messageQueue.Dequeue());
                        break;

                    case Header.DinamiteExplosion:
                        DinamiteExplosion(messageQueue.Dequeue());
                        break;

                    case Header.DinamitePass:
                        DinamitePass(messageQueue.Dequeue());
                        break;

                    case Header.UpdateLife:
                        UpdateLife(messageQueue.Dequeue());
                        break;

                    case Header.CardShuffle:
                        CardShuffle(messageQueue.Dequeue());
                        break;

                    case Header.DropCard:
                        DropCard(messageQueue.Dequeue());
                        break;

                    case Header.RequestDropCard:
                        RequestDropCard(messageQueue.Dequeue());
                        break;

                    case Header.ContinueGame:
                        ContinueGame(messageQueue.Dequeue());
                        break;

                    case Header.OpenJob:
                        OpenJob(messageQueue.Dequeue());
                        break;

                    case Header.VultureSameBringCard:
                        VultureSameBringCard(messageQueue.Dequeue());
                        break;

                    case Header.Defeat:
                        Defeat(messageQueue.Dequeue());
                        break;                    

                    case Header.ShutDown:
                        ShutDown(messageQueue.Dequeue());
                        break;

                    case Header.GameOver:
                        GameOver(messageQueue.Dequeue());
                        break;

                    case Header.BackToMain:
                        BackToMain(messageQueue.Dequeue());
                        break;

                    default:
                        messageQueue.Dequeue();
                        print("Not Defined Header : " + header);
                        break;
                }
            }
        }
    }

    private void PrintWelcome (byte[] message)
    {
        string welcomeMessage = MessageManager.GetBodyToString(message);
       
        chatting.PrintFirstMessage(welcomeMessage);
    }

    private void PrintChatting (byte[] message)
    {
        string chattingMessage = MessageManager.GetBodyToString(message);
        
        chatting.PrintMessage(chattingMessage);
    }

    private void SetPlayerNumber_ChattingPermission_ShowStartButton(byte[] message)
    {
        BangClient.playerNumber = MessageManager.GetBodyToInt(message);

        chatting.ChattingPermission();
        playerMenuView.ActiveFirstPlayerStartButton();
    }

    private void ShowPlayerState (byte[] message)
    {
        string playerState = MessageManager.GetBodyToString(message);
        
        playerMenuView.ShowPlayerState(playerState);
    }

    private void ReadyToStartGame (byte[] message)
    {
        int playerCount = MessageManager.GetBodyToInt(message);
       
        playerView.ReadyToStartGame(playerCount);
        playerMenuView.SetActive(false);
    }

    private void SetPlayerName (byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        string playerName = MessageManager.GetBodyToString(message, 8);

        playerView.SetPlayerName(playerIndex, playerName);
    }

    private void SetPlayerJob (byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int job = MessageManager.GetBodyToInt(message, 8);

        playerView.SetPlayerJob(playerIndex, job);
    }
    
    private void ChooseCharacter (byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int character1 = MessageManager.GetBodyToInt(message, 8);
        int character2 = MessageManager.GetBodyToInt(message, 12);

        playerView.ChooseCharacter(playerIndex, character1, character2);
    }

    private void SetPlayerCharacter (byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int character = MessageManager.GetBodyToInt(message, 8);

        playerView.SetPlayerCharacter(playerIndex, character);
    }

    private void SetPlayerLife (byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int life = MessageManager.GetBodyToInt(message, 8);

        playerView.SetPlayerLife(playerIndex, life);
    }

    private int[] CreateCardInfor(byte[] message, int startIndex = 8)
    {
        int length = MessageManager.GetBodyToInt(message, startIndex);
        int[] card = new int[length];
        startIndex += 4;

        for (int i = 0; i < length; ++i)
        {
            card[i] = MessageManager.GetBodyToInt(message, startIndex);
            startIndex += 4;
        }

        return card;
    }

    private void SetPlayerCard(byte[] message)
    {
        int targetIndex = MessageManager.GetBodyToInt(message);
        int[] card = CreateCardInfor(message);

        playerView.SetPlayerCard(targetIndex, card);
    }

    private void SetPlayerTurn (byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int bangCount = MessageManager.GetBodyToInt(message, 8);

        playerView.SetPlayerTurn(playerIndex, bangCount);
    }

    private void DrawCard(byte[] message)
    {
        int targetIndex = MessageManager.GetBodyToInt(message);
        int[] card = CreateCardInfor(message);

        playerView.DrawCard(targetIndex, card);
    }

    private void BlackJackDraw(byte[] message)
    {
        int targetIndex = MessageManager.GetBodyToInt(message);
        int[] card = CreateCardInfor(message);

        playerView.BlackJackDraw(targetIndex, card);
    }

    private void KitCarlsonDraw(byte[] message)
    {
        int targetIndex = MessageManager.GetBodyToInt(message);
        int[] card = CreateCardInfor(message);

        playerView.KitCarlsonDraw(targetIndex, card);
    }

    private void KitCarlsonRestoreCard (byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int card = MessageManager.GetBodyToInt(message, 8);

        playerView.KitCarlsonRestoreCard(playerIndex, card);
    }

    private void PedroRamirezSelectCard(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);

        playerView.PedroRamirezSelectCard(playerIndex);
    }

    private void PedroRamirezDrawCard (byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int card = MessageManager.GetBodyToInt(message, 8);

        playerView.PedroRamirezDrawCard(playerIndex, card);
    }

    private void JesseJonesSelectCard (byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);

        playerView.JesseJonesSelectCard(playerIndex);
    }

    private void ElGringoStealCard (byte[] message)
    {
        int fromIndex = MessageManager.GetBodyToInt(message);
        int targetIndex = MessageManager.GetBodyToInt(message, 8);

        playerView.ElGringoStealCard(fromIndex, targetIndex);
    }

    private void StealCard(byte[] message)
    {
        int fromIndex = MessageManager.GetBodyToInt(message);
        int targetIndex = MessageManager.GetBodyToInt(message, 8);
        int card = MessageManager.GetBodyToInt(message, 12);
        int isEquipCard = MessageManager.GetBodyToInt(message, 16);

        playerView.StealCard(fromIndex, targetIndex, card, isEquipCard);
    }

    private void BangCardOpen(byte[] message)
    {
        int targetIndex = MessageManager.GetBodyToInt(message);
        int damage = MessageManager.GetBodyToInt(message, 8);
        int[] card = CreateCardInfor(message, 12);

        playerView.BangCardOpen(targetIndex, damage, card);
    }

    private void BangCardDoubleOpen(byte[] message)
    {
        int targetIndex = MessageManager.GetBodyToInt(message);
        int damage = MessageManager.GetBodyToInt(message, 8);
        int[] card = CreateCardInfor(message, 12);

        playerView.BangCardDoubleOpen(targetIndex, damage, card);
    }

    private void Bang(byte[] message)
    {
        int targetIndex = MessageManager.GetBodyToInt(message);
        int damage = MessageManager.GetBodyToInt(message, 8);
        int[] card = CreateCardInfor(message, 12);

        playerView.Bang(targetIndex, damage, card);
    }

    private void AttackCardMove(byte[] message)
    {
        int fromIndex = MessageManager.GetBodyToInt(message);
        int targetIndex = MessageManager.GetBodyToInt(message, 8);
        int card = MessageManager.GetBodyToInt(message, 12);

        playerView.AttackCardMove(fromIndex, targetIndex, card);
    }

    private void Emporio(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int[] card = CreateCardInfor(message);

        playerView.Emporio(playerIndex, card);
    }

    private void EmporioGetCard(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int card = MessageManager.GetBodyToInt(message, 8);

        playerView.EmporioGetCard(playerIndex, card);
    }

    private void EmporioFinish(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);

        playerView.EmporioFinish(playerIndex);
    }

    private void Gatling(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int[] card = CreateCardInfor(message);

        playerView.Gatling(playerIndex, card);
    }

    private void GatlingDropCard(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        //cardCount = MessageManager.GetBodyToInt(message, 8)
        int card = MessageManager.GetBodyToInt(message, 12);

        playerView.GatlingDropCard(playerIndex, card);
    }

    private void Indian(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int[] card = CreateCardInfor(message);

        playerView.Indian(playerIndex, card);
    }

    private void IndianDropCard(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        //cardCount = MessageManager.GetBodyToInt(message, 8)
        int card = MessageManager.GetBodyToInt(message, 12);

        playerView.IndianDropCard(playerIndex, card);
    }

    private void Duello(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int[] card = CreateCardInfor(message);

        playerView.Duello(playerIndex, card);
    }

    private void EquipGun(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int range = MessageManager.GetBodyToInt(message, 8);
        int card = MessageManager.GetBodyToInt(message, 12);

        playerView.EquipGun(playerIndex, range, card);
    }

    private void Mirino(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int card = MessageManager.GetBodyToInt(message, 8);

        playerView.Mirino(playerIndex, card);
    }

    private void Barile(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int card = MessageManager.GetBodyToInt(message, 8);

        playerView.Barile(playerIndex, card);
    }

    private void Mustang(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int card = MessageManager.GetBodyToInt(message, 8);

        playerView.Mustang(playerIndex, card);
    }

    private void WellsFargo(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int[] card = CreateCardInfor(message);

        playerView.WellsFargo(playerIndex, card);
    }

    private void Diligenza(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int[] card = CreateCardInfor(message);

        playerView.Diligenza(playerIndex, card);
    }

    private void Saloon(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int[] life = CreateCardInfor(message);

        playerView.Saloon(playerIndex, life);
    }

    private void Beer(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int life = MessageManager.GetBodyToInt(message, 8);

        playerView.Beer(playerIndex, life);
    }

    private void Panico(byte[] message)
    {
        int fromIndex = MessageManager.GetBodyToInt(message);
        int targetIndex = MessageManager.GetBodyToInt(message, 8);
        int card = MessageManager.GetBodyToInt(message, 12);

        playerView.Panico(fromIndex, targetIndex, card);
    }

    private void CatBalou(byte[] message)
    {
        int fromIndex = MessageManager.GetBodyToInt(message);
        int targetIndex = MessageManager.GetBodyToInt(message, 8);
        int card = MessageManager.GetBodyToInt(message, 12);

        playerView.CatBalou(fromIndex, targetIndex, card);
    }

    private void CatBalouDropCard (byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int card = MessageManager.GetBodyToInt(message, 8);
        int equipCard = MessageManager.GetBodyToInt(message, 12);

        playerView.CatBalouDropCard(playerIndex, card, equipCard);
    }

    private void CardOpenOrderSelect(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int dinamite = MessageManager.GetBodyToInt(message, 8);
        int prigione = MessageManager.GetBodyToInt(message, 12);

        playerView.CardOpenOrderSelect(playerIndex, dinamite, prigione);
    }

    private void Prigione(byte[] message)
    {
        int fromIndex = MessageManager.GetBodyToInt(message);
        int targetIndex = MessageManager.GetBodyToInt(message, 8);
        int card = MessageManager.GetBodyToInt(message, 12);

        playerView.Prigione(fromIndex, targetIndex, card);
    }

    private void PrigioneCardOpen(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int[] card = CreateCardInfor(message);

        playerView.PrigioneCardOpen(playerIndex, card);
    }

    public void Dinamite(byte[] message)
    {
        int fromIndex = MessageManager.GetBodyToInt(message);
        int targetIndex = MessageManager.GetBodyToInt(message, 8);
        int dinamiteCard = MessageManager.GetBodyToInt(message, 12);

        playerView.Dinamite(fromIndex, targetIndex, dinamiteCard);
    }

    private void DinamiteCardOpen(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int dinamiteExplosion = MessageManager.GetBodyToInt(message, 8);
        int[] card = CreateCardInfor(message, 12);

        playerView.DinamiteCardOpen(playerIndex, dinamiteExplosion, card);
    }

    private void DinamiteExplosion(byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int dinamite = MessageManager.GetBodyToInt(message, 8);
        int life = MessageManager.GetBodyToInt(message, 12);
        int isLiving = MessageManager.GetBodyToInt(message, 16);

        playerView.DinamiteExplosion(playerIndex, dinamite, life, isLiving);
    }

    private void DinamitePass(byte[] message)
    {
        int fromIndex = MessageManager.GetBodyToInt(message);
        int targetIndex = MessageManager.GetBodyToInt(message, 8);
        int dinamite = MessageManager.GetBodyToInt(message, 12);

        playerView.DinamitePass(fromIndex, targetIndex, dinamite);
    }

    private void UpdateLife (byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int life = MessageManager.GetBodyToInt(message, 8);

        playerView.UpdateLife(playerIndex, life);
    }

    private void CardShuffle (byte[] message)
    {
        playerView.CardShuffle();
    }

    private void DropCard(byte[] message)
    {
        int targetIndex = MessageManager.GetBodyToInt(message);
        int[] card = CreateCardInfor(message);

        playerView.DropCard(targetIndex, card);
    }

    private void RequestDropCard(byte[] message)
    {
        int targetIndex = MessageManager.GetBodyToInt(message);
        int mustDropCount = MessageManager.GetBodyToInt(message, 8);
        int[] card = CreateCardInfor(message, 12);

        playerView.PlayerMustDropCard(targetIndex, mustDropCount, card);
    }

    private void ContinueGame (byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);

        playerView.ContinueGame(playerIndex);
    }

    private void OpenJob(byte[] message)
    {
        int openPlayerIndex = MessageManager.GetBodyToInt(message);
        int openPlayerJob = MessageManager.GetBodyToInt(message, 8);
        int attackerIndex = MessageManager.GetBodyToInt(message, 12);

        playerView.OpenJob(openPlayerIndex, openPlayerJob, attackerIndex);
    }

    private void VultureSameBringCard(byte[] message)
    {
        int targetIndex = MessageManager.GetBodyToInt(message);
        int vultureSameIndex = MessageManager.GetBodyToInt(message, 8);
        int[] card = CreateCardInfor(message, 12);

        playerView.VultureSameBringCard(targetIndex, vultureSameIndex, card);
    }

    private void Defeat (byte[] message)
    {
        int playerIndex = MessageManager.GetBodyToInt(message);
        int[] card = CreateCardInfor(message);

        playerView.Defeat(playerIndex, card);
    }

    private void GameOver (byte[] message)
    {
        string gameOverMessage = MessageManager.GetBodyToString(message);

        playerView.GameOver(gameOverMessage);
    }

    private void ShutDown (byte[] message)
    {
        chatting.PrintMessage("<color=red>서버가 차단되었습니다. 게임이 종료됩니다.</color>");
        StartCoroutine(GameExit());
    }

    private IEnumerator GameExit ()
    {
        yield return new WaitForSeconds(1.0f);
        Application.Quit();
    }

    private void OnDestroy()
    {
        bangClient.SendToServer(MessageManager.MakeByteMessage(Header.Disconnect));
        bangClient.Close();
    }
}