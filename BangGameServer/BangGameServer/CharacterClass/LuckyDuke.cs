
namespace BangGameServer
{
    public class LuckyDuke : Player
    {
        public LuckyDuke () : base()
        {
            character = Character.LuckyDuke;
        }

        private Card[] LuckyDukeGetTwoOpenCard ()
        {
            Card[] card = new Card[2];

            card[0] = (Card)CardManager.instance.PopCardList();
            card[1] = (Card)CardManager.instance.PopCardList();
            CardManager.instance.usedCardList.Add((int)card[0]);
            CardManager.instance.usedCardList.Add((int)card[1]);

            return card;
        }

        public override void BangCardOpenCheck(Player attacker, int damage)
        {
            if (haveBarile)
            {
                Card[] card = LuckyDukeGetTwoOpenCard();

                if (card[0].ToString()[0] == 'H' || card[1].ToString()[0] == 'H')
                {
                    damage--;
                }

                haveDamage = damage;
                ToClient.SendToAll(Header.BangCardDoubleOpen, playerIndex, damage, card.Length, (int)card[0], (int)card[1]);
            }
            else
            {
                haveDamage = damage;
                BangMakeChooseMissedCard(damage);
            }
        }

        public override void PrigioneCardOpen()
        {
            havePrigione = false;

            Card[] card = LuckyDukeGetTwoOpenCard();

            for (int i = 0; i < card.Length; ++i)
            {
                if (card[i].ToString()[0] ==  'H')
                {
                    escapePrigione = true;
                    break;
                }
            }

            ToClient.SendToAll(Header.PrigioneCardOpen, playerIndex, card.Length, (int)card[0], (int)card[1]);
        }

        public override void DinamiteCardOpen()
        {
            haveDinamite = false;

            Card[] card = LuckyDukeGetTwoOpenCard();
            int dinamiteHit = 0;
            int dinamiteExplosion = 0;

            for (int i = 0; i < card.Length; ++i)
            {
                string cardName = card[i].ToString();

                if (cardName[0] == 'S' && '2' <= cardName[1] && cardName[1] <= '9')
                {
                    dinamiteHit++;
                }
            }

            if (dinamiteHit == 2)
            {
                dinamiteExplosion = 1;
            }

            ToClient.SendToAll(Header.DinamiteCardOpen, playerIndex, dinamiteExplosion, card.Length, (int)card[0], (int)card[1]);
        }
    }
}