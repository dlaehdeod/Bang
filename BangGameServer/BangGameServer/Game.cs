using System;
using System.Collections.Generic;

namespace BangGameServer
{
    public class Game
    {
        public static Game instance = null;

        public Player[] player;
        public string[] playerName;
        public Job[] playerJob;

        public bool isPlaying;
        public int playerTurn;
        public int playerCount;

        private List<int> recentUseCard;
        private List<int> emporioCard;
        private int duelloNextTarget;
        private int notSelectCard;

        public Game ()
        {
            instance = this;

            playerName = new string[7];
            playerJob = new Job[7];

            notSelectCard = 80;
            recentUseCard = new List<int>();
        }

        public void RemovePlayer(int index, int playerCount)
        {
            for (int playerIndex = index; playerIndex < playerCount; ++playerIndex)
            {
                playerName[playerIndex] = playerName[playerIndex + 1];
            }
        }

        public void SetPlayerCount (int playerCount)
        {
            this.playerCount = playerCount;
        }
        
        public void SetPlayer()
        {
            player = new Player[playerCount];
        }

        public void SetPlayerLife ()
        {
            for (int playerIndex = 0; playerIndex < playerCount; ++playerIndex)
            {
                player[playerIndex].SetPlayerLife();
            }
        }

        public void SetPlayerTurn()
        {
            int bangCount = player[playerTurn].bangCount;
            ToClient.SendToAll(Header.SetPlayerTurn, playerTurn, bangCount);

            Console.WriteLine(playerTurn + "의 턴입니다.");
        }

        public void SetPlayerCard()
        {
            for (int playerIndex = 0; playerIndex < playerCount; ++playerIndex)
            {
                player[playerIndex].SetPlayerCard();
            }
        }
        
        public void SetPlayerName()
        {
            for (int index = 0; index < playerCount; ++index)
            {
                string name = playerName[index];
                ToClient.SendToAll(Header.SetPlayerName, index, name);
            }
        }

        public void SetPlayerJob ()
        {
            CardManager.instance.SetPlayerJob(playerCount);
        }

        public void PlayerMakeChooseCharacter()
        {
            CardManager.instance.PlayerMakeChooseCharacter(playerCount);
        }

        public void SetPlayerCharacter(int index, Character character)
        {
            switch (character)
            {
                case Character.BartCassidy:
                    player[index] = new BartCassidy();
                    break;
                case Character.BlackJack:
                    player[index] = new BlackJack();
                    break;
                case Character.CalamityJanet:
                    player[index] = new CalamityJanet();
                    break;
                case Character.ElGringo:
                    player[index] = new ElGringo();
                    break;
                case Character.JesseJones:
                    player[index] = new JesseJones();
                    break;
                case Character.Jourdonnais:
                    player[index] = new Jourdonnais();
                    break;
                case Character.KitCarlson:
                    player[index] = new KitCarlson();
                    break;
                case Character.LuckyDuke:
                    player[index] = new LuckyDuke();
                    break;
                case Character.PaulRegret:
                    player[index] = new PaulRegret();
                    break;
                case Character.PedroRamirez:
                    player[index] = new PedroRamirez();
                    break;
                case Character.RoseDoolan:
                    player[index] = new RoseDoolan();
                    break;
                case Character.SidKetchum:
                    player[index] = new SidKetchum();
                    break;
                case Character.SlabTheKiller:
                    player[index] = new SlabTheKiller();
                    break;
                case Character.SuzyLafayette:
                    player[index] = new SuzyLafayette();
                    break;
                case Character.VultureSame:
                    player[index] = new VultureSame();
                    break;
                case Character.WillyTheKid:
                    player[index] = new WillyTheKid();
                    break;
            }

            player[index].playerIndex = index;
            player[index].job = playerJob[index];
        }

        public void PlayerCardCountCheck_PassTurn ()
        {
            if (player[playerTurn].character != Character.SidKetchum &&
                player[playerTurn].ValidCardCount())
            {
                SetNextPlayerTurn();
            }
            else
            {
                player[playerTurn].RequestDropCard();
            }
        }

        public void ResponseDropCard(int playerIndex, int card)
        {
            if (player[playerIndex].character == Character.SidKetchum)
            {
                SidKetchumResponseDropCard(playerIndex, card);
                return;
            }

            player[playerIndex].DropCard(card);
        
            if (player[playerIndex].ValidCardCount())
            {
                SetNextPlayerTurn();
            }
        }

        private void SidKetchumResponseDropCard (int playerIndex, int card)
        {
            if (card > notSelectCard)
            {
                player[playerIndex].SidKetchumRecoveryLife();
                SetNextPlayerTurn();
                return;
            }

            player[playerIndex].DropCard(card);
            player[playerIndex].forLifeDropCardCount++;
        }

        public void SetNextPlayerTurn()
        {
            playerTurn = (playerTurn + 1) % playerCount;
            MakeValidPlayerTurn();
            PlayerTurn();
        }

        private void MakeValidPlayerTurn()
        {
            while (!player[playerTurn].isPlaying)
            {
                playerTurn = (playerTurn + 1) % playerCount;
            }
        }

        public void PlayerTurn ()
        {
            player[playerTurn].PlayerTurn();
        }

        public void BlackJackDrawMore ()
        {
            player[playerTurn].DrawCard(1);
        }

        public void KitCarlsonRestoreCard(int playerIndex, int card)
        {
            player[playerIndex].KitCarlsonRestoreCard(card);
        }

        public void PedroRamirezDrawCard (int card)
        {
            player[playerTurn].PedroRamirezDrawCard(card);
        }

        public void JesseJonesStealAndDrawCard(int targetIndex, int card)
        {
            if (targetIndex >= playerCount)
            {
                player[playerTurn].DrawCard(2);
            }
            else
            {
                player[playerTurn].StealCard(player[targetIndex], card);
                player[playerTurn].DrawCard(1);
            }
        }

        public void ElGringoStealCard(int fromIndex, int card)
        {
            player[fromIndex].StealCard(player[playerTurn], card);
        }

        public void Bang(int target, int card)
        {
            AddRecentUseCard(playerTurn, card);

            player[playerTurn].Bang(player[target], card);
        }

        private void AddRecentUseCard(int playerIndex, int card)
        {
            player[playerIndex].cardList.Remove(card);
            recentUseCard.Add(card);
        }

        public void BangCardOpenCompleted(int target, int damage)
        {
            if (damage > 0)
            {
                player[target].BangMakeChooseMissedCard(damage);
            }
            else
            {
                DropRecentUseCard();
                ToClient.SendToAll(Header.ContinueGame, playerTurn);
            }
        }

        private void DropRecentUseCard ()
        {
            int[] cardInfor = CreateCardInfor(playerTurn, recentUseCard.ToArray());

            RecentUseCardSetUserCard();

            ToClient.SendToAll(Header.DropCard, cardInfor);
        }

        private void RecentUseCardSetUserCard ()
        {
            for (int i = 0; i < recentUseCard.Count; ++i)
            {
                CardManager.instance.usedCardList.Add(recentUseCard[i]);
            }

            recentUseCard.Clear();
        }

        public void BangResponse(int playerIndex, int card)
        {
            DropRecentUseCard();

            player[playerIndex].BangResponse(card);
        }

        public void Emporio(int card)
        {
            player[playerTurn].DropCard(card);

            SetEmporioCard();
            SendEmporioInfor(playerTurn, emporioCard.ToArray());
        }

        private void SetEmporioCard ()
        {
            emporioCard = new List<int>();

            for (int i = 0; i < playerCount; ++i)
            {
                if (player[i].isPlaying)
                {
                    emporioCard.Add(CardManager.instance.PopCardList());

                    player[i].emporioCheck = 1;
                }
            }
        }

        private void SendEmporioInfor(int playerIndex, int[] card)
        {
            int[] emporioInfor = CreateCardInfor(playerIndex, card);

            ToClient.SendToAll(Header.Emporio, emporioInfor);
        }

        private int[] CreateCardInfor (int playerIndex, int[] card)
        {
            int frontLength = 2;
            int[] cardInfor = new int[frontLength + card.Length];
            cardInfor[0] = playerIndex;
            cardInfor[1] = card.Length;

            for (int i = 0; i < card.Length; ++i)
            {
                cardInfor[frontLength + i] = card[i];
            }

            return cardInfor;
        }

        public void EmporioGetCard(int playerIndex, int card)
        {
            if (card > notSelectCard)
            {
                card = emporioCard[0];
            }

            emporioCard.Remove(card);

            player[playerIndex].GetCard(card);
            player[playerIndex].emporioCheck = 0;

            EmporioSelectNextPlayer(playerIndex);
        }

        private void EmporioSelectNextPlayer (int playerIndex)
        {
            for (int i = 0; i < playerCount; ++i)
            {
                playerIndex = (playerIndex + 1) % playerCount;

                if (player[playerIndex].isPlaying && player[playerIndex].emporioCheck > 0)
                {
                    SendEmporioInfor(playerIndex, emporioCard.ToArray());
                    return;
                }
            }
            
            ToClient.SendToAll(Header.EmporioFinish, playerTurn);
        }

        public void Gatling(int card)
        {
            player[playerTurn].DropCard(card, Header.GatlingDropCard);

            SetWideAttack(Header.Gatling, "Missed");
        }

        public void Indian(int card)
        {
            player[playerTurn].DropCard(card, Header.IndianDropCard);

            SetWideAttack(Header.Indian, "Bang");
        }

        private void SetWideAttack (Header header, string cardType)
        {
            for (int i = 0; i < playerCount; ++i)
            {
                if (i == playerTurn || !player[i].isPlaying)
                {
                    continue;
                }

                player[i].wideAttackCheck = 1;
                player[i].MakeChooseCard(header, cardType);
            }
        }
        
        public void WideAttackResponse(int playerIndex, int card)
        {
            player[playerIndex].WideAttackResponse(card);
        }

        public void Duello(int targetIndex, int card)
        {
            AddRecentUseCard(playerTurn, card);

            duelloNextTarget = playerTurn;
            
            ToClient.SendToAll(Header.AttackCardMove, playerTurn, targetIndex, card);

            player[targetIndex].MakeChooseCard(Header.Duello, "Bang");
        }

        public void DuelloResponse(int responseIndex, int card)
        {
            if (card <= 80)
            {
                AddRecentUseCard(responseIndex, card);

                if (DefenceByBeerCard (card))
                {
                    DropRecentUseCard();

                    player[responseIndex].SuzyLafayetteCardCheck();
                    player[duelloNextTarget].SuzyLafayetteCardCheck();
                    
                    ToClient.SendToAll(Header.ContinueGame, playerTurn);
                }
                else
                {
                    ToClient.SendToAll(Header.AttackCardMove, responseIndex, duelloNextTarget, card);
                    player[duelloNextTarget].MakeChooseCard(Header.Duello, "Bang");
                    duelloNextTarget = responseIndex;
                }
            }
            else
            {
                DropRecentUseCard();

                player[duelloNextTarget].SuzyLafayetteCardCheck();
                player[responseIndex].TakeDuelloDamage(duelloNextTarget);
            }
        }

        private bool DefenceByBeerCard (int card)
        {
            return ((Card)card).ToString().Contains("Beer");
        }

        public void EquipGun(int card, int range)
        {
            player[playerTurn].EquipGun(card, range);
        }

        public void Mirino(int card)
        {
            player[playerTurn].Mirino(card);
        }

        public void Barile(int card)
        {
            player[playerTurn].Barile(card);
        }

        public void Mustang(int card)
        {
            player[playerTurn].Mustang(card);
        }

        public void WellsFargo(int card)
        {
            player[playerTurn].WellsFargo(card);
        }

        public void Diligenza(int card)
        {
            player[playerTurn].Diligenza(card);
        }

        public void Saloon(int card)
        {
            player[playerTurn].Saloon(card);
        }

        public void Beer(int card)
        {
            player[playerTurn].Beer(card);
        }

        public void Panico(int target, int card)
        {
            AddRecentUseCard(playerTurn, card);

            ToClient.SendToAll(Header.Panico, playerTurn, target, card);
        }

        public void StealCard(int targetIndex, int card)
        {
            card = MakeValidTargetCard(targetIndex, card);

            DropRecentUseCard();
            player[playerTurn].StealCard(player[targetIndex], card);
        }

        public void CatBalou(int target, int card)
        {
            AddRecentUseCard(playerTurn, card);
            
            ToClient.SendToAll(Header.CatBalou, playerTurn, target, card);
        }

        public void CatBalouDropCard(int targetIndex, int card)
        {
            card = MakeValidTargetCard(targetIndex, card);

            DropRecentUseCard();
            player[targetIndex].CatBalouDropCard(card);

            ToClient.SendToAll(Header.ContinueGame, playerTurn);
        }

        private int MakeValidTargetCard (int targetIndex, int card)
        {
            if (card > notSelectCard)
            {
                if (player[targetIndex].equipCardList.Count > 0)
                {
                    card = player[targetIndex].equipCardList[0];
                }
                else
                {
                    card = player[targetIndex].cardList[0];
                }
            }

            return card;
        }

        public void CardOpenOrderSelectCompleted(int playerIndex, int card)
        {
            int dinamiteFirst = 0;

            if ((Card)card == Card.H2_Dinamite)
            {
                dinamiteFirst = 1;
            }

            player[playerIndex].ResponseCardOpenOrder_Prigione_Dinamite(dinamiteFirst);
        }

        public void Prigione(int targetIndex, int card)
        {
            player[playerTurn].cardList.Remove(card);
            
            player[targetIndex].EquipPrigioneFrom(playerTurn, card);
        }

        public void PrigioneCardOpenCompleted(int playerIndex)
        {
            player[playerIndex].PlayerTurn();
        }

        public void Dinamite(int targetIndex, int dinamiteCard)
        {
            player[playerTurn].cardList.Remove(dinamiteCard);

            player[targetIndex].EquipDinamiteFrom(playerTurn, dinamiteCard);
        }

        public void DinamiteCardOpenCompleted(int playerIndex, int explosion)
        {
            if (explosion > 0)
            {
                player[playerIndex].DinamiteExplosion();
            }
            else
            {
                int nextPlayer = (playerIndex + 1) % playerCount;
                
                while (!player[nextPlayer].isPlaying)
                {
                    nextPlayer = (nextPlayer + 1) % playerCount;
                }

                player[playerIndex].PassDinamite(player[nextPlayer]);
                player[playerIndex].PlayerTurn();
            }
        }

        public void DinamiteExplosionCompleted (int playerIndex, int isLiving)
        {
            if (isLiving > 0)
            {
                if (player[playerIndex].character == Character.BartCassidy)
                {
                    player[playerIndex].DrawCard(3);
                    Console.WriteLine("동작되는가");
                }

                Console.WriteLine("바트인가?");
                player[playerIndex].PlayerTurn();
            }
            else
            {
                player[playerIndex].OpenJob();
            }
        }

        public void OpenJobCompleted(int openPlayerIndex, int job, int attackerIndex)
        {
            bool gameOver;

            switch ((Job)job)
            {
                case Job.Sheriff:
                    gameOver = KilledSheriff();
                    break;

                case Job.Outlaw:
                case Job.Outlaw2:
                case Job.Outlaw3:
                    gameOver = KilledOutLaw(attackerIndex);
                    break;

                case Job.Vice:
                case Job.Vice2:
                    gameOver = KilledVice(attackerIndex);
                    break;

                case Job.Traitor:
                    gameOver = KilledTaritor();
                    break;

                default:
                    gameOver = false;
                    break;
            }

            if (gameOver)
            {
                isPlaying = false;
                return;
            }

            int vultureSameIndex = GetVultureSameIndex();

            if (vultureSameIndex >= 0)
            {
                VultureSameBringCard(openPlayerIndex, vultureSameIndex);
            }
            else
            {
                player[openPlayerIndex].Defeat();
            }

            if (!player[playerTurn].isPlaying)
            {
                SetNextPlayerTurn();
            }
            else
            {
                ToClient.SendToAll(Header.ContinueGame, playerTurn);
            }
        }

        private int GetVultureSameIndex()
        {
            for (int i = 0; i < playerCount; ++i)
            {
                if (player[i].isPlaying && player[i].character == Character.VultureSame)
                {
                    return i;
                }
            }

            return -1;
        }

        private void VultureSameBringCard(int openPlayerIndex, int vultureSameIndex)
        {
            int frontLength = 3;
            int cardCount = player[openPlayerIndex].cardList.Count;
            int equipCardCount = player[openPlayerIndex].equipCardList.Count;
            int[] cardInfor = new int[frontLength + cardCount + equipCardCount];
            
            cardInfor[0] = openPlayerIndex;
            cardInfor[1] = vultureSameIndex;
            cardInfor[2] = cardCount + equipCardCount;

            for (int i = 0; i < cardCount; ++i)
            {
                cardInfor[frontLength + i] = player[openPlayerIndex].cardList[i];
                player[vultureSameIndex].cardList.Add(cardInfor[frontLength + i]);
            }

            for (int i = 0; i < equipCardCount; ++i)
            {
                cardInfor[frontLength + cardCount + i] = player[openPlayerIndex].equipCardList[i];
                player[vultureSameIndex].cardList.Add(cardInfor[frontLength + cardCount + i]);
            }

            player[openPlayerIndex].cardList.Clear();
            player[openPlayerIndex].equipCardList.Clear();

            ToClient.SendToAll(Header.VultureSameBringCard, cardInfor);
        }

        private bool KilledSheriff()
        {
            bool existOutlaw = false;

            for (int i = 0; i < playerCount; ++i)
            {
                if (player[i].isPlaying && ExistOutlaw(i))
                {
                    existOutlaw = true;
                    break;
                }
            }

            if (existOutlaw)
            {
                ToClient.SendToAll(Header.GameOver, "무법자의 승리입니다!");
            }
            else
            {
                ToClient.SendToAll(Header.GameOver, "배신자의 승리입니다!");
            }

            return true;
        }

        private bool ExistOutlaw (int index)
        {
            switch (player[index].job)
            {
                case Job.Outlaw:
                case Job.Outlaw2:
                case Job.Outlaw3:
                    return true;
            }

            return false;
        }

        private bool KilledVice(int attackerIndex)
        {
            if (attackerIndex <= 7 && player[attackerIndex].job == Job.Sheriff)
            {
                player[attackerIndex].SheriffKilledViceDropAllCard();
            }

            return false;
        }

        private bool KilledOutLaw(int attackerIndex)
        {
            bool existOutlawOrTaritor = false;

            for (int i = 0; i < playerCount; ++i)
            {
                if ((player[i].isPlaying && ExistOutlaw(i))
                    || (player[i].isPlaying && player[i].job == Job.Traitor))
                {
                    existOutlawOrTaritor = true;
                    break;
                }
            }

            if (existOutlawOrTaritor)
            {
                if (attackerIndex <= 7)
                {
                    player[attackerIndex].DrawCard(3);
                }

                return false;
            }
            else
            {
                ToClient.SendToAll(Header.GameOver, "보안관의 승리입니다!");
                return true;
            }
        }

        private bool KilledTaritor()
        {
            bool existOutlaw = false;

            for (int i = 0; i < playerCount; ++i)
            {
                if (player[i].isPlaying && ExistOutlaw(i))
                {
                    existOutlaw = true;
                    break;
                }
            }

            if (!existOutlaw)
            {
                ToClient.SendToAll(Header.GameOver, "보안관의 승리입니다!");
                return true;
            }

            return false;
        }

        public void OutOfGame ()
        {
            isPlaying = false;
            ToClient.SendToAll(Header.GameOver, "플레이어가 나갔습니다. 게임을 다시 시작합니다.");
        }
    }
}