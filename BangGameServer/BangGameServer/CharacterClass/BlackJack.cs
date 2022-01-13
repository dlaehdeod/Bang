
namespace BangGameServer
{
    public class BlackJack : Player
    {
        public BlackJack () : base()
        {
            character = Character.BlackJack;
        }

        public override void DrawCard()
        {
            int[] drawInfor = GetDrawCardInfor(2);

            ToClient.SendToAll(Header.BlackJackDraw, drawInfor);
        }
    }
}