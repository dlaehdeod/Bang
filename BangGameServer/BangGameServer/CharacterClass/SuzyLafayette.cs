
namespace BangGameServer
{
    public class SuzyLafayette : Player
    {
        public SuzyLafayette () : base()
        {
            character = Character.SuzyLafayette;
        }

        public override void SuzyLafayetteCardCheck ()
        {
            if (cardList.Count <= 0)
            {
                DrawCard(1);
            }
        }
    }
}
