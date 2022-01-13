using System.Collections.Generic;

namespace BangGameServer
{
    public class CalamityJanet : Player
    {
        public CalamityJanet () : base()
        {
            character = Character.CalamityJanet;
        }

        public override void MakeChooseCard(Header header, string type)
        {
            wideAttackCheck = 1;
            MakeChooseCard(header);
        }

        private void MakeChooseCard (Header header)
        {
            int frontLength = 2;
            List<int> defenceCard = new List<int>();

            for (int i = 0; i < cardList.Count; ++i)
            {
                Card card = (Card)cardList[i];
                if (card.ToString().Contains("Bang") || card.ToString().Contains("Missed"))
                {
                    defenceCard.Add((int)card);
                }
                else if (life == 1 && card.ToString().Contains("Beer"))
                {
                    defenceCard.Add((int)card);
                }
            }

            int[] defenceInfor = new int[frontLength + defenceCard.Count];
            defenceInfor[0] = playerIndex;
            defenceInfor[1] = defenceCard.Count;

            for (int i = 0; i < defenceCard.Count; ++i)
            {
                defenceInfor[frontLength + i] = defenceCard[i];
            }

            ToClient.SendToAll(header, defenceInfor);
        }

        public override void BangMakeChooseMissedCard(int damage)
        {
            int frontLength = 3;
            List<int> missedCard = new List<int>();

            for (int i = 0; i < cardList.Count; ++i)
            {
                Card card = (Card)cardList[i];
                if (card.ToString().Contains("Missed") || card.ToString().Contains("Bang"))
                {
                    missedCard.Add((int)card);
                }
            }
            
            int[] missedInfor = new int[frontLength + missedCard.Count];
            missedInfor[0] = playerIndex;
            missedInfor[1] = damage;
            missedInfor[2] = missedCard.Count;

            for (int i = 0; i < missedCard.Count; ++i)
            {
                missedInfor[frontLength + i] = missedCard[i];
            }

            ToClient.SendToAll(Header.Bang, missedInfor);
        }
    }
}