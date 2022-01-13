
namespace BangGameServer
{
    public class SidKetchum : Player
    {
        public SidKetchum () : base()
        {
            character = Character.SidKetchum;
            forLifeDropCardCount = 0;
        }

        public override void SidKetchumRecoveryLife ()
        {
            life += forLifeDropCardCount / 2;
            if (life > maxLife)
            {
                life = maxLife;
            }

            forLifeDropCardCount = 0;
            ToClient.SendToAll(Header.UpdateLife, playerIndex, life);
        }
    }
}