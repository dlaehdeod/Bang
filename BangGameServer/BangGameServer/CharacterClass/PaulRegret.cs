
namespace BangGameServer
{
    public class PaulRegret : Player
    {
        public PaulRegret () : base()
        {
            character = Character.PaulRegret;
        }

        public override void SetPlayerLife()
        {
            life = 3;

            if (job == Job.Sheriff)
            {
                life++;
            }

            maxLife = life;
            ToClient.SendToAll(Header.SetPlayerLife, playerIndex, life);
        }
    }
}