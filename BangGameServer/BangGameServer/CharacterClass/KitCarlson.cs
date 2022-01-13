
namespace BangGameServer
{
    public class KitCarlson : Player
    {
        public KitCarlson () : base()
        {
            character = Character.KitCarlson;
        }

        public override void DrawCard()
        {
            int[] drawInfor = GetDrawCardInfor(3);

            ToClient.SendToAll(Header.KitCarlsonDraw, drawInfor);
        }

        public override void KitCarlsonRestoreCard (int card)
        {
            if (card > 80)
            {
                card = cardList[cardList.Count - 1];
            }

            cardList.Remove(card);
            CardManager.instance.cardList.Add(card);

            ToClient.SendToAll(Header.KitCarlsonRestoreCard, playerIndex, card);
        }
    }
}