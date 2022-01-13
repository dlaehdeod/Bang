using System;
using System.Collections.Generic;

namespace BangGameServer
{
    public class Player
    {
        public bool isPlaying;

        public List<int> cardList;
        public List<int> equipCardList;

        public Job job;
        public Character character;

        public string name;
        public int playerIndex;
        public int life;
        public int maxLife;
        public int bangCount;

        public int emporioCheck;
        public int wideAttackCheck;
        public int prigioneCard;

        public int forLifeDropCardCount;

        protected int haveDamage;
        protected bool haveBarile;
        protected bool haveDinamite;
        protected bool havePrigione;
        protected bool escapePrigione;
        
        public virtual void KitCarlsonRestoreCard(int card) { }
        public virtual void PedroRamirezDrawCard(int card) { }
        public virtual void SuzyLafayetteCardCheck() { }
        public virtual void SidKetchumRecoveryLife() { }

        public Player()
        {
            bangCount = 1;
            wideAttackCheck = 0;
            prigioneCard = 0;
            isPlaying = true;
            cardList = new List<int>();
            equipCardList = new List<int>();
        }

        public void SetPlayerCard()
        {
            int frontLength = 2;
            int[] cardInfor = new int[frontLength + life];
            cardInfor[0] = playerIndex;
            cardInfor[1] = life;

            for (int i = 0; i < life; ++i)
            {
                cardInfor[frontLength + i] = CardManager.instance.PopCardList();
                cardList.Add(cardInfor[frontLength + i]);
            }

            ToClient.SendToAll(Header.SetPlayerCard, cardInfor);
        }

        public virtual void SetPlayerLife()
        {
            life = 4;

            if (job == Job.Sheriff)
            {
                life++;
            }

            maxLife = life;

            ToClient.SendToAll(Header.SetPlayerLife, playerIndex, life);
        }

        public bool ValidCardCount()
        {
            return cardList.Count <= life;
        }

        public void PlayerTurn()
        {
            if (haveDinamite && havePrigione)
            {
                HaveDinamiteAndPrigione();
            }
            else if (havePrigione)
            {
                PrigioneCardOpen();
            }
            else if (haveDinamite)
            {
                DinamiteCardOpen();
            }
            else if (prigioneCard > 0)
            {
                DropPrigioneCard();

                if (escapePrigione)
                {
                    DrawCard();
                }
                else
                {
                    Game.instance.SetNextPlayerTurn();
                }

                escapePrigione = false;
            }
            else
            {
                DrawCard();
            }
        }

        private void HaveDinamiteAndPrigione()
        {
            int dinamite = 0;
            int prigione = 0;

            for (int i = 0; i < equipCardList.Count; ++i)
            {
                string cardName = ((Card)equipCardList[i]).ToString().Substring(3);

                if (cardName == "Dinamite")
                {
                    dinamite = equipCardList[i];
                }
                else if (cardName == "Prigione")
                {
                    prigione = equipCardList[i];
                }
            }

            ToClient.SendToAll(Header.CardOpenOrderSelect, playerIndex, dinamite, prigione);
        }

        public void ResponseCardOpenOrder_Prigione_Dinamite(int dinamiteFirst)
        {
            if (dinamiteFirst > 0)
            {
                DinamiteCardOpen();
            }
            else
            {
                PrigioneCardOpen();
            }
        }

        public virtual void DinamiteCardOpen()
        {
            haveDinamite = false;

            int cardCount = 1;
            int dinamiteExplosion = 0;
            Card card = (Card)CardManager.instance.PopCardList();
            CardManager.instance.usedCardList.Add((int)card);

            string cardName = card.ToString();

            if (cardName[0] == 'S' && '2' <= cardName[1] && cardName[1] <= '9')
            {
                dinamiteExplosion = 1;
            }

            ToClient.SendToAll(Header.DinamiteCardOpen, playerIndex, dinamiteExplosion, cardCount, (int)card);
        }

        public virtual void PrigioneCardOpen()
        {
            havePrigione = false;

            int cardCount = 1;
            Card card = (Card)CardManager.instance.PopCardList();
            CardManager.instance.usedCardList.Add((int)card);

            if (card.ToString()[0] == 'H')
            {
                escapePrigione = true;
            }

            ToClient.SendToAll(Header.PrigioneCardOpen, playerIndex, cardCount, (int)card);
        }

        public void StealCard(Player targetPlayer, int card)
        {
            if (card >= 80)
            {
                card = targetPlayer.cardList[0];
            }

            int isEquipCard = targetPlayer.RemoveCard(card);

            if (isEquipCard > 0)
            {
                UpdateEquipState(targetPlayer, card);
            }

            targetPlayer.SuzyLafayetteCardCheck();

            ToClient.SendToAll(Header.StealCard, playerIndex, targetPlayer.playerIndex, card, isEquipCard);
            cardList.Add(card);
            
            //엘그링고가 광역기를 맞은 상황 일 수 있기 때문에 광역기 체크를 해준다.
            WideAttackCheckAndNextTurn();
        }

        public void UpdateEquipState(Player updatePlayer, int card)
        {
            string cardName = ((Card)card).ToString().Substring(3);

            switch (cardName)
            {
                case "Volcanic":
                    updatePlayer.bangCount = 1;
                    break;

                case "Prigione":
                    updatePlayer.havePrigione = false;
                    updatePlayer.escapePrigione = false;
                    updatePlayer.prigioneCard = 0;
                    break;

                case "Barile":
                    updatePlayer.haveBarile = false;
                    break;

                case "Dinamite":
                    updatePlayer.haveDinamite = false;
                    break;

                default:
                    break;
            }
        }

        public virtual void Bang(Player target, int bangCard)
        {
            ToClient.SendToAll(Header.AttackCardMove, playerIndex, target.playerIndex, bangCard);

            int damage = 1;

            SuzyLafayetteCardCheck();
            target.BangCardOpenCheck(this, damage);
        }

        public virtual void BangCardOpenCheck(Player attacker, int damage)
        {
            if (haveBarile)
            {
                int cardCount = 1;
                Card card = (Card)CardManager.instance.PopCardList();
                CardManager.instance.usedCardList.Add((int)card);

                if (card.ToString()[0] == 'H')
                {
                    damage--;
                }
                haveDamage = damage;
                ToClient.SendToAll(Header.BangCardOpen, playerIndex, damage, cardCount, (int)card);
            }
            else
            {
                haveDamage = damage;
                BangMakeChooseMissedCard(damage);
            }
        }

        public virtual void BangMakeChooseMissedCard(int damage)
        {
            int frontLength = 3;
            List<int> missedCard = new List<int>();

            for (int i = 0; i < cardList.Count; ++i)
            {
                Card card = (Card)cardList[i];
                if (card.ToString().Contains("Missed"))
                {
                    missedCard.Add((int)card);
                }

                if (life == 1 && card.ToString().Contains("Beer"))
                {
                    missedCard.Add((int)card);
                }
            }

            int[] missedInfor = new int[frontLength + missedCard.Count];
            missedInfor[0] = playerIndex;
            missedInfor[1] = damage;
            missedInfor[2] = missedCard.Count;

            for (int i = 0; i < missedCard.Count; ++i)
            {
                missedInfor[frontLength + i] = missedCard[i];
            }

            ToClient.SendToAll(Header.Bang, missedInfor);
        }

        public void BangResponse(int card)
        {
            if (card <= 80)
            {
                DropCard(card);

                haveDamage--;
                if (haveDamage <= 0)
                {
                    ToClient.SendToAll(Header.ContinueGame, Game.instance.playerTurn);
                }
            }
            else
            {
                TakeDamage();
            }
        }

        public virtual void TakeDamage()
        {
            haveDamage = 0;
            life--;

            ToClient.SendToAll(Header.UpdateLife, playerIndex, life);

            if (life <= 0)
            {
                OpenJob(Game.instance.playerTurn);
            }
            else
            {
                WideAttackCheckAndNextTurn();
            }
        }

        protected void WideAttackCheckAndNextTurn()
        {
            Player[] player = Game.instance.player;

            bool isFinish = true;
            wideAttackCheck = 0;

            for (int i = 0; i < player.Length; ++i)
            {
                if (player[i].isPlaying && player[i].wideAttackCheck > 0)
                {
                    isFinish = false;
                    Console.WriteLine(playerIndex + ") 수행완료. 아직 안끝남.");
                    break;
                }
            }

            if (isFinish)
            {
                ToClient.SendToAll(Header.ContinueGame, Game.instance.playerTurn);
            }

            Console.WriteLine(playerIndex + " wideAttackCheck result = " + isFinish);
        }

        public void GetCard(int card)
        {
            cardList.Add(card);

            ToClient.SendToAll(Header.EmporioGetCard, playerIndex, card);
        }

        public virtual void MakeChooseCard(Header header, string type)
        {
            int frontLength = 2;
            List<int> cardType = new List<int>();

            for (int i = 0; i < cardList.Count; ++i)
            {
                Card card = (Card)cardList[i];
                if (card.ToString().Contains(type))
                {
                    cardType.Add((int)card);
                }
                else if (life == 1 && card.ToString().Contains("Beer"))
                {
                    cardType.Add((int)card);
                }
            }
            
            int[] chooseInfor = new int[frontLength + cardType.Count];
            chooseInfor[0] = playerIndex;
            chooseInfor[1] = cardType.Count;

            for (int i = 0; i < cardType.Count; ++i)
            {
                chooseInfor[frontLength + i] = cardType[i];
            }

            ToClient.SendToAll(header, chooseInfor);
        }

        public void WideAttackResponse(int card)
        {
            wideAttackCheck = 0;

            if (card <= 80)
            {
                DropCard(card);
                WideAttackCheckAndNextTurn();
            }
            else
            {
                TakeDamage();
            }
        }

        public virtual void TakeDuelloDamage(int from)
        {
            life--;

            ToClient.SendToAll(Header.UpdateLife, playerIndex, life);

            if (life <= 0)
            {
                OpenJob(from);
            }
            else
            {
                SuzyLafayetteCardCheck();
                WideAttackCheckAndNextTurn();
            }
        }

        public void DropCard(int card, Header header = Header.DropCard)
        {
            RemoveCard(card);
            CardManager.instance.usedCardList.Add(card);

            ToClient.SendToAll(header, playerIndex, 1, card);
        }

        public int RemoveCard(int card)
        {
            bool isEquipCard = equipCardList.Remove(card);
            
            cardList.Remove(card);

            SuzyLafayetteCardCheck();

            return isEquipCard ? 1 : 0;
        }

        public void OpenJob(int from = -1)
        {
            isPlaying = false;

            if (from >= 0)
            {
                ToClient.SendToAll(Header.OpenJob, playerIndex, (int)job, from);
            }
            else
            {
                int attackerNull = 100;
                ToClient.SendToAll(Header.OpenJob, playerIndex, (int)job, attackerNull);
            }
        }

        public void SheriffKilledViceDropAllCard()
        {
            int frontLength = 2;
            int[] cardInfor = new int[frontLength + cardList.Count];
            cardInfor[0] = playerIndex;
            cardInfor[1] = cardList.Count;

            for (int i = 0; i < cardList.Count; ++i)
            {
                cardInfor[frontLength + i] = cardList[i];
                CardManager.instance.usedCardList.Add(cardList[i]);
            }
            cardList.Clear();

            ToClient.SendToAll(Header.DropCard, cardInfor);
        }

        public void Defeat()
        {
            int frontLength = 2;
            int cardCount = cardList.Count;
            int equipCardCount = equipCardList.Count;

            int[] cardInfor = new int[frontLength + cardCount + equipCardCount];
            cardInfor[0] = playerIndex;
            cardInfor[1] = cardCount + equipCardCount;

            for (int i = 0; i < cardCount; ++i)
            {
                cardInfor[frontLength + i] = cardList[i];
                CardManager.instance.usedCardList.Add(cardList[i]);
            }

            for (int i = 0; i < equipCardCount; ++i)
            {
                cardInfor[frontLength + cardCount + i] = equipCardList[i];
                CardManager.instance.usedCardList.Add(equipCardList[i]);
            }

            cardList.Clear();
            equipCardList.Clear();

            ToClient.SendToAll(Header.Defeat, cardInfor);
        }

        public void EquipGun(int card, int range)
        {
            DropOverlapCard("Volcanic");
            DropOverlapCard("Schofield");
            DropOverlapCard("Remington");
            DropOverlapCard("RevCarabine");
            DropOverlapCard("Winchester");

            cardList.Remove(card);
            equipCardList.Add(card);

            if (range == 1)
            {
                bangCount = 1000;
            }

            SuzyLafayetteCardCheck();

            ToClient.SendToAll(Header.EquipGun, playerIndex, range, card);
        }
        
        private void DropOverlapCard(string cardName)
        {
            for (int i = 0; i < equipCardList.Count; ++i)
            {
                Card equipCard = (Card)equipCardList[i];

                if (equipCard.ToString().Substring(3) == cardName)
                {
                    equipCardList.RemoveAt(i);
                    CardManager.instance.usedCardList.Add((int)equipCard);

                    ToClient.SendToAll(Header.DropCard, playerIndex, 1, (int)equipCard);
                    return;
                }
            }
        }

        public void Mirino(int card)
        {
            DropOverlapCard("Mirino");

            cardList.Remove(card);
            equipCardList.Add(card);

            ToClient.SendToAll(Header.Mirino, playerIndex, card);
        }

        public void Barile(int card)
        {
            DropOverlapCard("Barile");

            cardList.Remove(card);
            equipCardList.Add(card);

            haveBarile = true;

            ToClient.SendToAll(Header.Barile, playerIndex, card);
        }

        public void Mustang(int card)
        {
            DropOverlapCard("Mustang");

            cardList.Remove(card);
            equipCardList.Add(card);

            ToClient.SendToAll(Header.Mustang, playerIndex, card);
        }

        public void WellsFargo(int card)
        {
            int[] drawCardInfor = GetDrawCardInfor(3);

            DropCard(card);
            ToClient.SendToAll(Header.WellsFargo, drawCardInfor);
        }

        public void Diligenza(int card)
        {
            int[] drawCardInfor = GetDrawCardInfor(2);

            DropCard(card);
            ToClient.SendToAll(Header.Diligenza, drawCardInfor);
        }

        public void Saloon(int saloonCard)
        {
            DropCard(saloonCard);

            Player[] player = Game.instance.player;

            int frontLength = 2;
            int[] playerLifeInfor = new int[frontLength + player.Length];

            playerLifeInfor[0] = playerIndex;
            playerLifeInfor[1] = player.Length;

            for (int i = 0; i < player.Length; ++i)
            {
                if (player[i].isPlaying && player[i].life < player[i].maxLife)
                {
                    player[i].life++;
                }
                playerLifeInfor[frontLength + i] = player[i].life;
            }

            ToClient.SendToAll(Header.Saloon, playerLifeInfor);
        }

        public void Beer(int beerCard)
        {
            DropCard(beerCard);

            life++;
            if (life > maxLife)
            {
                life = maxLife;
            }

            ToClient.SendToAll(Header.Beer, playerIndex, life);
        }

        public virtual void DrawCard()
        {
            int[] drawInfor = GetDrawCardInfor(2);

            ToClient.SendToAll(Header.DrawCard, drawInfor);
            Game.instance.SetPlayerTurn();
        }

        public void DrawCard(int drawCount)
        {
            int[] drawInfor = GetDrawCardInfor(drawCount);

            ToClient.SendToAll(Header.DrawCard, drawInfor);
        }

        protected int[] GetDrawCardInfor(int drawCount)
        {
            int frontLength = 2;
            int[] drawInfor = new int[frontLength + drawCount];
            drawInfor[0] = playerIndex;
            drawInfor[1] = drawCount;

            int[] drawCard = CardManager.instance.GetNextDrawCard(drawCount);

            for (int i = 0; i < drawCount; ++i)
            {
                drawInfor[frontLength + i] = drawCard[i];
                cardList.Add(drawCard[i]);
            }

            return drawInfor;
        }

        public void CatBalouDropCard (int card)
        {
            int isEquipCard = RemoveCard(card);

            if (isEquipCard != 0)
            {
                UpdateEquipState(this, card);
                string cardName = ((Card)card).ToString().Substring(3);

                switch (cardName)
                {
                    case "Volcanic":
                        bangCount = 1;
                        break;

                    case "Prigione":
                        havePrigione = false;
                        escapePrigione = false;
                        prigioneCard = 0;
                        break;

                    case "Barile":
                        haveBarile = false;
                        break;

                    case "Dinamite":
                        haveDinamite = false;
                        break;

                    default:
                        break;
                }
            }

            ToClient.SendToAll(Header.CatBalouDropCard, playerIndex, card, isEquipCard);
        }

        public void EquipPrigioneFrom(int fromIndex, int card)
        {
            DropOverlapCard("Prigione");

            equipCardList.Add(card);
            havePrigione = true;
            prigioneCard = card;

            ToClient.SendToAll(Header.Prigione, fromIndex, playerIndex, card);
        }

        public void DropPrigioneCard()
        {
            DropCard(prigioneCard);
            prigioneCard = 0;
        }

        public void EquipDinamiteFrom(int fromIndex, int card)
        {
            equipCardList.Add(card);
            haveDinamite = true;

            ToClient.SendToAll(Header.Dinamite, fromIndex, playerIndex, card);
        }

        public void DinamiteExplosion()
        {
            int isLiving = 1;

            life -= 3;
            if (life <= 0)
            {
                life = 0;
                isLiving = 0;
            }

            ToClient.SendToAll(Header.DinamiteExplosion, playerIndex, (int)Card.H2_Dinamite, life, isLiving);
        }

        public void PassDinamite(Player nextPlayer)
        {
            int dinamite = (int)Card.H2_Dinamite;
            equipCardList.Remove(dinamite);
            nextPlayer.equipCardList.Add(dinamite);
            nextPlayer.haveDinamite = true;

            ToClient.SendToAll(Header.DinamitePass, playerIndex, nextPlayer.playerIndex, dinamite);
        }

        public void RequestDropCard()
        {
            int frontLength = 3;
            int haveCardCount = cardList.Count;
            int mustDropCount = haveCardCount - life;
            
            int[] dropInfor = new int[frontLength + haveCardCount];

            dropInfor[0] = playerIndex;
            dropInfor[1] = mustDropCount;
            dropInfor[2] = haveCardCount;

            for (int i = 0; i < haveCardCount; ++i)
            {
                dropInfor[frontLength + i] = cardList[i];
            }

            ToClient.SendToAll(Header.RequestDropCard, dropInfor);
        }
    }
}