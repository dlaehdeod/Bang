
namespace BangGameServer
{
    public class ElGringo : Player
    {
        public ElGringo () : base()
        {
            character = Character.ElGringo;
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

        public override void TakeDamage()
        {
            int targetIndex = Game.instance.playerTurn;

            if (Game.instance.player[targetIndex].cardList.Count <= 0)
            {
                base.TakeDamage();
                return;
            }

            life--;
            ToClient.SendToAll(Header.UpdateLife, playerIndex, life);

            if (life <= 0)
            {
                OpenJob(targetIndex);
            }
            else
            {
                ToClient.SendToAll(Header.ElGringoStealCard, playerIndex, targetIndex);
            }
        }

        public override void TakeDuelloDamage (int from)
        {
            if (Game.instance.player[from].cardList.Count <= 0)
            {
                base.TakeDuelloDamage(from);
                return;
            }

            life--;

            ToClient.SendToAll(Header.UpdateLife, playerIndex, life);

            if (life <= 0)
            {
                OpenJob(from);
            }
            else
            {
                ToClient.SendToAll(Header.ElGringoStealCard, playerIndex, from);
            }
        }
    }
}