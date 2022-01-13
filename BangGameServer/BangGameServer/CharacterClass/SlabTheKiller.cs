
namespace BangGameServer
{
    public class SlabTheKiller : Player
    {
        public SlabTheKiller () : base()
        {
            character = Character.SlabTheKiller;
        }

        public override void Bang (Player target, int bangCard)
        {
            ToClient.SendToAll(Header.AttackCardMove, playerIndex, target.playerIndex, bangCard);

            int damage = 2;
            
            target.BangCardOpenCheck(this, damage);
        }
    }
}