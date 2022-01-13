
namespace BangGameServer
{
    public class Jourdonnais : Player
    {
        public Jourdonnais () : base()
        {
            character = Character.Jourdonnais;
        }

        public override void BangCardOpenCheck (Player attacker, int damage)
        {
            Card card = (Card)CardManager.instance.PopCardList();
            CardManager.instance.usedCardList.Add((int)card);
            if (card.ToString()[0] == 'H')
            {
                damage--;
            }
            
            if (damage > 0 && haveBarile)
            {
                Card card2 = (Card)CardManager.instance.PopCardList();
                CardManager.instance.usedCardList.Add((int)card2);
                if (card2.ToString()[0] == 'H')
                {
                    damage--;
                }

                haveDamage = damage;
                ToClient.SendToAll(Header.BangCardOpen, playerIndex, damage, 2, (int)card, (int)card2);
            }
            else
            {
                haveDamage = damage;
                ToClient.SendToAll(Header.BangCardOpen, playerIndex, damage, 1, (int)card);
            }
        }
    }
}