
namespace BangGameServer
{
    public class BartCassidy : Player
    {
        public BartCassidy () : base()
        {
            character = Character.BartCassidy;
        }

        public override void TakeDamage()
        {
            life--;

            ToClient.SendToAll(Header.UpdateLife, playerIndex, life);
            if (life <= 0)
            {
                OpenJob(Game.instance.playerTurn);
            }
            else
            {
                DrawCard(1);
                WideAttackCheckAndNextTurn();
            }
        }

        public override void TakeDuelloDamage(int from)
        {
            life--;

            ToClient.SendToAll(Header.UpdateLife, playerIndex, life);

            if (life <= 0)
            {
                OpenJob(from);
            }
            else
            {
                DrawCard(1);
                WideAttackCheckAndNextTurn();
            }
        }
    }
}