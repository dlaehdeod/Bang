
namespace BangGameServer
{
    public class PedroRamirez : Player
    {
        public PedroRamirez () : base()
        {
            character = Character.PedroRamirez;
        }

        public override void DrawCard ()
        {
            if (CardManager.instance.usedCardList.Count > 0)
            {
                ToClient.SendToAll(Header.PedroRamirezSelectCard, playerIndex);
            }
            else
            {
                base.DrawCard();
            }
        }

        public override void PedroRamirezDrawCard (int card)
        {
            int notUsedCard = 100;

            if (card == notUsedCard)
            {
                base.DrawCard();
            }
            else
            {
                int[] drawInfor = GetDrawCardInfor(1);
                int recentUsedCard = CardManager.instance.PopUsedCardList();

                ToClient.SendToAll(Header.PedroRamirezDrawCard, playerIndex, recentUsedCard);
                ToClient.SendToAll(Header.DrawCard, drawInfor);
            }
        }
    }
}