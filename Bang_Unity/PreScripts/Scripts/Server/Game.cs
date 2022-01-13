using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//jopCard 7장
//characterCard 16장
//playCard 80장

//rule 정식버전
//최소 플레이어는 4인 이상

//플레이어 4인
//보안관1, 배신자1, 무법자2

//플레이어 5인
//보안관1, 부관1, 배신자1, 무법자2

//플레이어 6인
//보안관1, 부관1, 배신자1, 무법자3

//플레이어 7인
//보안관1, 부관2, 배신자2, 무법자3

//rule 섬멸버전
//우선 정식버전만 만들어보자.
//잡이 없다. 캐릭터카드는 3개 중 2개를 선택할 수 있게 한다.
//1vs1, 1vs1vs1, 2vs2 까지만 만들...귀찮다. 모르겠음

public class Game
{
    private Jop[] jops = { Jop.Sheriff, Jop.Taritor, Jop.Outlaw, Jop.Outlaw, Jop.Deputy, Jop.Outlaw, Jop.Deputy };

    private BangServer bangServer;
    private PlayerInformation[] playerInfor;

    private string[] playerName;
    private bool[] playerReady;
    private bool isPlaying;

    private int maxCharacterCard;

    private int finishCount;
    private int playerCount;
    private int rule;

    public Game (BangServer bangServer)
    {
        this.bangServer = bangServer;

        maxCharacterCard = 16;

        playerName = new string[7];
        playerReady = new bool[7];
        isPlaying = false;
    }

    private List<int> CreateRandomList (int length)
    {
        List<int> randomList = new List<int>();

        for (int i=  0; i < length; ++i)
        {
            randomList.Add(i);
        }

        return randomList;
    }

    private int PopRandomList (List<int> randomList)
    {
        int randomIndex = Random.Range(0, randomList.Count);
        int targetIndex = randomList[randomIndex];
        randomList.RemoveAt(randomIndex);

        return targetIndex;
    }

    public void PlayerJopSetting()
    {
        isPlaying = true;
        finishCount = 0;
        playerInfor = new PlayerInformation[playerCount];

        List<int> randomList = CreateRandomList(playerCount);

        for (int i = 0; i < playerCount; ++i)
        {
            int targetIndex = PopRandomList(randomList);
            playerInfor[targetIndex].jop = jops[i];
            bangServer.SendToOne(targetIndex, MessageManager.MakeByteMessage(Header.SetPlayerJop, (int)jops[i]));
        }

        bangServer.SendToAll(MessageManager.MakeByteMessage(Header.DistributeJop));
    }

    public void FinishCheckAndNext ()
    {
        finishCount++;

        bangServer.SendToAll(MessageManager.MakeByteMessage(Header.Chatting, finishCount.ToString()));
        UnityEngine.MonoBehaviour.print("Finished On" + finishCount);

        if (finishCount == bangServer.GetClientCount())
        {
            PlayerCharacterSetting();
        }
    }
    
    public void PlayerCharacterSetting ()
    {
        finishCount = 0;

        List<int> randomList = CreateRandomList(maxCharacterCard);

        for (int i = 0; i < playerCount; ++i)
        {
            int firstCharacter = PopRandomList(randomList);
            int secondCharacter = PopRandomList(randomList);
            bangServer.SendToOne(i, MessageManager.MakeByteMessage(Header.SetPlayerCharacter, firstCharacter, secondCharacter));
        }
    }

    #region Reference from Server or ServerMessageReceiver

    public void SetPlayerCount (int playerCount)
    {
        this.playerCount = playerCount;
    }

    public bool IsGamePlaying()
    {
        return isPlaying;
    }

    public void SetPlayerName (int index, string name)
    {
        playerName[index] = name;
    }

    public string GetPlayerName(int index)
    {
        return playerName[index];
    }

    public void SetPlayerReady (int index)
    {
        playerReady[index] = true;
    }

    public bool GetPlayerReady (int index)
    {
        return playerReady[index];
    }

    public bool IsAllPlayerReady(int playerCount)
    {
        for (int i = 0; i < playerCount; ++i)
        {
            if (!playerReady[i])
            {
                return false;
            }
        }

        return true;
    }

    public void RemovePlayerName (int index, int maxIndex)
    {
        for (int i = index; i < maxIndex; ++i)
        {
            playerReady[i] = playerReady[i + 1];
            playerName[i] = playerName[i + 1];
        }
    }

    #endregion
}