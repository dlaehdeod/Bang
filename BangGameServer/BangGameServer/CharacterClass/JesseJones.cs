
namespace BangGameServer
{
    public class JesseJones : Player
    {
        public JesseJones () : base()
        {
            character = Character.JesseJones;
        }

        public override void DrawCard ()
        {
            ToClient.SendToAll(Header.JesseJonesSelectCard, playerIndex);
        }
    }
}