using System;
using System.Collections.Generic;
using System.Text;

namespace BangGameServer
{
    public class CardManager
    {
        public static CardManager instance = null;

        private const int characterLength = 16;
        private const int gameCardLength = 80;

        public List<int> cardList;
        public List<int> usedCardList;

        public CardManager ()
        {
            instance = this;
            cardList = CreateRandomList(gameCardLength);
            usedCardList = new List<int>();
        }

        public void SetPlayerJob (int playerCount)
        {
            List<int> randomList = CreateRandomList(playerCount);

            for (int job = 0; job < playerCount; ++job)
            {
                int playerIndex = PopList(randomList);

                Game.instance.playerJob[playerIndex] = (Job)job;
                ToClient.SendToAll(Header.SetPlayerJob, playerIndex, job);

                if (job == (int)Job.Sheriff)
                {
                    Game.instance.playerTurn = playerIndex;
                }

                Console.WriteLine("{0}플레이어의 직업은 : {1}", playerIndex + 1, (Job)job);
            }
        }

        public void PlayerMakeChooseCharacter (int playerCount)
        {
            List<int> randomList = CreateRandomList(characterLength);

            for (int playerIndex = 0; playerIndex < playerCount; ++playerIndex)
            {
                int character1 = PopList(randomList);
                int character2 = PopList(randomList);

                ToClient.SendToAll(Header.ChooseCharacter, playerIndex, character1, character2);
            }
        }

        public int[] GetNextDrawCard(int drawCount)
        {
            int[] drawCard = new int[drawCount];

            for (int i = 0; i < drawCount; ++i)
            {
                drawCard[i] = PopCardList();
            }

            return drawCard;
        }

        private List<int> CreateRandomList (int length)
        {
            List<int> tempList = new List<int>();
            List<int> randomList = new List<int>();

            for (int i = 0; i < length; ++i)
            {
                tempList.Add(i);
            }

            Random random = new Random();

            for (int i = 0; i < length; ++i)
            {
                int index = random.Next(0, tempList.Count);
                int value = tempList[index];

                randomList.Add(value);
                tempList.RemoveAt(index);
            }

            return randomList;
        }

        public int PopCardList ()
        {
            MakeValidCard();
            return PopList(cardList);
        }

        public void MakeValidCard()
        {
            if (cardList.Count <= 1)
            {
                CardShuffle();
            }
        }

        private void CardShuffle()
        {
            ToClient.SendToAll(Header.CardShuffle);

            List<int> shuffleList = new List<int>();
            Random random = new Random();

            while (usedCardList.Count > 1)
            {
                int index = random.Next(0, usedCardList.Count - 1);
                int value = usedCardList[index];

                usedCardList.RemoveAt(index);
                shuffleList.Add(value);
            }

            for (int i = 0; i < cardList.Count; ++i)
            {
                shuffleList.Add(cardList[i]);
            }

            cardList = shuffleList;
        }

        public int PopUsedCardList ()
        {
            return PopList(usedCardList);
        }

        private int PopList(List<int> list)
        {
            int value = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);

            return value;
        }
    }
}
