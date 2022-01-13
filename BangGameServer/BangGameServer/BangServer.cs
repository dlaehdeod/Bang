using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


namespace BangGameServer
{
    public class BangServer
    {
        private Game game;

        private int byteSize;
        private const int port = 9898;

        private List<Socket> clientList;
        private Socket server;
        private IPEndPoint endPoint;
        private int[] playerResponseCheck;

        private const string welcome = "<color=yellow>접속을 환영합니다.</color>";

        public BangServer()
        {
            game = new Game();
            
            new CardManager();

            ToClient.sendToClients = SendToAll;

            byteSize = 512;

            clientList = new List<Socket>();
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            endPoint = new IPEndPoint(IPAddress.Any, port);

            server.Bind(endPoint);
            server.Listen(10);

            SocketAsyncEventArgs acceptArgs = new SocketAsyncEventArgs();
            acceptArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptCompleted);

            server.AcceptAsync(acceptArgs);

            PrintCreateServerMessage();
        }

        private void PrintCreateServerMessage()
        {
            IPAddress[] host = Dns.GetHostAddresses(Dns.GetHostName());

            for (int i = 0; i < host.Length; ++i)
            {
                if (host[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    Console.WriteLine("[{0}] 서버가 정상적으로 작동중입니다.", host[i]);
                    return;
                }
            }
        }

        public void Close()
        {
            for (int i = 0; i < clientList.Count; ++i)
            {
                SendToOne(clientList[i], MessageManager.MakeByteMessage(Header.ShutDown));
            }

            server.Close();
        }

        public void SendToAll(byte[] message)
        {
            int playerCount = clientList.Count;
            for (int i = 0; i < playerCount; ++i)
            {
                SendToOne(clientList[i], message);
            }
        }

        public void SendToOne(Socket client, byte[] message)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(message, 0, message.Length);
            client.SendAsync(args);
        }

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                lock (clientList)
                {
                    Socket client = e.AcceptSocket;

                    if (clientList.Count >= 7 || game.isPlaying)
                    {
                        Console.WriteLine("{0} 접속시도했지만 돌려보냈습니다.", client.RemoteEndPoint);

                        SendToOne(client, MessageManager.MakeByteMessage(Header.BackToMain));
                        FreeSocket(client);
                        return;
                    }
                    clientList.Add(client);

                    Console.WriteLine("{0} 플레이어가 접속하였습니다. 현재 플레이어 수 {1}", client.RemoteEndPoint, clientList.Count);
                    SendToOne(client, MessageManager.MakeByteMessage(Header.Welcome, welcome));

                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();

                    byte[] acceptData = new byte[byteSize];
                    args.SetBuffer(acceptData, 0, acceptData.Length);

                    args.UserToken = clientList;
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveCompleted);
                    client.ReceiveAsync(args);

                    e.AcceptSocket = null;
                    server.AcceptAsync(e);
                }
            }
        }

        private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            Socket clientSocket = (Socket)sender;

            if (clientSocket.Connected)
            {
                bool isConnected = MessageProcessing(clientSocket, e.Buffer);
                
                if (isConnected)
                {
                    e.SetBuffer(new byte[byteSize], 0, byteSize);
                    clientSocket.ReceiveAsync(e);
                }
            }
            else
            {
                DisConnectClient(clientSocket);
            }
        }

        private void DisConnectClient(Socket clientSocket)
        {
            int removeIndex = clientList.IndexOf(clientSocket);
            if (removeIndex == -1)
            {
                return;
            }

            clientList.RemoveAt(removeIndex);

            Console.WriteLine("{0} 플레이어가 접속을 종료했습니다. 현재 플레이어 수 {1}", clientSocket.RemoteEndPoint, clientList.Count);
            SendToAll(MessageManager.MakeByteMessage(Header.Chatting, "<color=orange>" + game.playerName[removeIndex] + "님이 게임을 떠났습니다.</color>"));

            if (game.isPlaying)
            {
                game.OutOfGame();
            }
            else
            {
                UpdatePlayerWaitingView();
                ResetClientId(clientList);
            }

            game.RemovePlayer(removeIndex, clientList.Count);

            FreeSocket(clientSocket);
        }

        private void FreeSocket(Socket client)
        {
            client.Disconnect(true);
            client.Dispose();
            client.Close();
        }

        private void UpdatePlayerWaitingView()
        {
            if (game.isPlaying)
            {
                return;
            }

            int playerCount = clientList.Count;
            string message = "현재 인원 " + playerCount.ToString() + "/7";

            for (int i = 0; i < playerCount; ++i)
            {
                message += "\n" + game.playerName[i];
            }
            SendToAll(MessageManager.MakeByteMessage(Header.ShowPlayerState, message));
        }

        private void ResetClientId(List<Socket> clientList)
        {
            for (int i = 0; i < clientList.Count; ++i)
            {
                SendToOne(clientList[i], MessageManager.MakeByteMessage(Header.SetPlayerNumber, i));
            }
        }

        private bool MessageProcessing(Socket clientSocket, byte[] message)
        {
            Header header = MessageManager.GetHeader(message);

            Console.WriteLine(header);

            switch (header)
            {
                case Header.Disconnect:
                    DisConnectClient(clientSocket);
                    return false;

                case Header.SetClientName:
                    SetClientNameAndSendPlayerId(clientSocket, message);
                    UpdatePlayerWaitingView();
                    break;

                case Header.Chatting:
                    SendToAll(message);
                    break;

                case Header.GameStartButtonDown:
                    ValidPlayerCheck_Start();
                    break;

                case Header.ReadyToStartGameCompleted:
                    ReadyToStartGameCompletedCheck_NameAndJobSetting(message);
                    break;

                case Header.SetPlayerJobCompleted:
                    SetPlayerJobCompletedCheck_ClientMakeChooseCharacter(message);
                    break;

                case Header.SetPlayerCharacter:
                    SendToAll(message);
                    SetPlayerCharacter(message);
                    break;

                case Header.SetPlayerCharacterCompleted:
                    SetPlayerCharacterCompletedCheck_SetPlayerLife(message);
                    break;

                case Header.SetPlayerLifeCompleted:
                    SetPlayerLifeCompletedCheck_SetPlayerCard(message);
                    break;

                case Header.SetPlayerCardCompleted:
                    SetPlayerCardCompletedCheck_NextPlayerStateCheck(message);
                    break;

                case Header.BlackJackDrawMore:
                    BlackJackDrawMore(message);
                    break;

                case Header.KitCarlsonRestoreCard:
                    KitCarlsonRestoreCard(message);
                    break;

                case Header.PedroRamirezDrawCard:
                    PedroRamirezDrawCard(message);
                    break;

                case Header.JesseJonesDrawCard:
                    JesseJonesDrawCard(message);
                    break;

                case Header.ElGringoStealCardCompleted:
                    ElGringoStealCardCompleted(message);
                    break;

                case Header.Bang:
                    Bang(message);
                    break;

                case Header.BangCardOpenCompleted:
                    BangCardOpenCompleted(message);
                    break;

                case Header.BangResponse:
                    BangResponse(message);
                    break;

                case Header.Emporio:
                    Emporio(message);
                    break;

                case Header.EmporioGetCard:
                    EmporioGetCard(message);
                    break;

                case Header.Gatling:
                    Gatling(message);
                    break;

                case Header.Indian:
                    Indian(message);
                    break;

                case Header.WideAttackResponse:
                    WideAttackResponse(message);
                    break;

                case Header.Duello:
                    Duello(message);
                    break;

                case Header.DuelloResponse:
                    DuelloResponse(message);
                    break;

                case Header.EquipGun:
                    EquipGun(message);
                    break;

                case Header.Mirino:
                    Mirino(message);
                    break;

                case Header.Barile:
                    Barile(message);
                    break;

                case Header.Mustang:
                    Mustang(message);
                    break;

                case Header.WellsFargo:
                    WellsFargo(message);
                    break;

                case Header.Diligenza:
                    Diligenza(message);
                    break;

                case Header.Saloon:
                    Saloon(message);
                    break;

                case Header.Beer:
                    Beer(message);
                    break;

                case Header.Panico:
                    Panico(message);
                    break;

                case Header.StealCard:
                    StealCard(message);
                    break;

                case Header.CatBalou:
                    CatBalou(message);
                    break;

                case Header.CatBalouCompleted:
                    CatBalouCompleted(message);
                    break;

                case Header.CardOpenOrderSelectCompleted:
                    CardOpenOrderSelectCompleted(message);
                    break;

                case Header.Prigione:
                    Prigione(message);
                    break;

                case Header.PrigioneCardOpenCompleted:
                    PrigioneCardOpenCompleted(message);
                    break;

                case Header.Dinamite:
                    Dinamite(message);
                    break;

                case Header.DinamiteCardOpenCompleted:
                    DinamiteCardOpenCompleted(message);
                    break;

                case Header.DinamiteExplosionCompleted:
                    DinamiteExplosionCompleted(message);
                    break;

                case Header.PassTurn:
                    ValidCardCheck_PassTurn();
                    break;

                case Header.ResponseDropCard:
                    ResponseDropCard(message);
                    break;

                case Header.OpenJobCompleted:
                    OpenJobCompleted(message);
                    break;

                default:
                    Console.WriteLine("정해지지 않은 헤더 : " + header);
                    break;
            }

            return true;
        }

        private void SetClientNameAndSendPlayerId(Socket clientSocket, byte[] message)
        {
            int index = clientList.IndexOf(clientSocket);
            string name = MessageManager.GetBodyToString(message);
            game.playerName[index] = name;

            SendToOne(clientSocket, MessageManager.MakeByteMessage(Header.SetPlayerNumber, index));
            SendToAll(MessageManager.MakeByteMessage(Header.Chatting, "<color=orange>" + game.playerName[index] + "님이 접속하였습니다.</color>"));
        }

        private void ValidPlayerCheck_Start()
        {
            Console.WriteLine(game.isPlaying);

            if (game.isPlaying)
            {
                return;
            }
            
            int playerCount = clientList.Count;
            Console.WriteLine(playerCount);

            if (playerCount == 1)
            {
                SendToAll(MessageManager.MakeByteMessage(Header.Chatting, "<color=red>혼자서는 플레이할 수 없습니다.</color>"));
            }
            else
            {
                game.isPlaying = true;
                playerResponseCheck = new int[playerCount];

                game.SetPlayerCount(playerCount);
                game.SetPlayer();

                SendToAll(MessageManager.MakeByteMessage(Header.ReadyToStartGame, playerCount));
            }
        }

        private void ReadyToStartGameCompletedCheck_NameAndJobSetting(byte[] message)
        {
            int index = MessageManager.GetBodyToInt(message);
            playerResponseCheck[index] = 1;

            if (AllPlayerResponseCheck())
            {
                ResetPlayerResponse();
                game.SetPlayerName();
                game.SetPlayerJob();
            }
        }

        private void SetPlayerJobCompletedCheck_ClientMakeChooseCharacter(byte[] message)
        {
            int index = MessageManager.GetBodyToInt(message);
            playerResponseCheck[index] = 1;

            if (AllPlayerResponseCheck())
            {
                ResetPlayerResponse();
                game.PlayerMakeChooseCharacter();
            }
        }

        private void SetPlayerCharacter(byte[] message)
        {
            int playerNumber = MessageManager.GetBodyToInt(message);
            Character character = (Character)(MessageManager.GetBodyToInt(message, 8));
            
            game.SetPlayerCharacter(playerNumber, character);
            
            Console.WriteLine("[" + playerNumber + "] 의 캐릭터 : " + character);
        }

        private void SetPlayerCharacterCompletedCheck_SetPlayerLife(byte[] message)
        {
            int index = MessageManager.GetBodyToInt(message);
            playerResponseCheck[index] = 1;

            if (AllPlayerResponseCheck())
            {
                ResetPlayerResponse();
                game.SetPlayerLife();
            }
        }

        private void SetPlayerLifeCompletedCheck_SetPlayerCard(byte[] message)
        {
            int index = MessageManager.GetBodyToInt(message);
            playerResponseCheck[index] = 1;

            if (AllPlayerResponseCheck())
            {
                ResetPlayerResponse();
                game.SetPlayerCard();
            }
        }

        private void SetPlayerCardCompletedCheck_NextPlayerStateCheck(byte[] message)
        {
            int index = MessageManager.GetBodyToInt(message);
            playerResponseCheck[index] = 1;

            if (AllPlayerResponseCheck())
            {
                Console.WriteLine("게임이 시작되었습니다!");

                ResetPlayerResponse();
                game.PlayerTurn();
            }
        }

        private bool AllPlayerResponseCheck()
        {
            for (int i = 0; i < playerResponseCheck.Length; ++i)
            {
                if (playerResponseCheck[i] == 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void ResetPlayerResponse()
        {
            for (int i = 0; i < playerResponseCheck.Length; ++i)
            {
                playerResponseCheck[i] = 0;
            }
        }

        private void BlackJackDrawMore(byte[] message)
        {
            int moreDraw = MessageManager.GetBodyToInt(message);

            if (moreDraw > 0)
            {
                game.BlackJackDrawMore();
            }
            game.SetPlayerTurn();
        }
        
        private void KitCarlsonRestoreCard(byte[] message)
        {
            int playerIndex = MessageManager.GetBodyToInt(message);
            int card = MessageManager.GetBodyToInt(message, 8);

            game.KitCarlsonRestoreCard(playerIndex, card);
            game.SetPlayerTurn();
        }

        private void PedroRamirezDrawCard(byte[] message)
        {
            int card = MessageManager.GetBodyToInt(message);

            game.PedroRamirezDrawCard(card);
            game.SetPlayerTurn();
        }

        private void JesseJonesDrawCard(byte[] message)
        {
            int playerIndex = MessageManager.GetBodyToInt(message);
            int card = MessageManager.GetBodyToInt(message, 8);

            game.JesseJonesStealAndDrawCard(playerIndex, card);
            game.SetPlayerTurn();
        }

        private void ElGringoStealCardCompleted(byte[] message)
        {
            int fromIndex = MessageManager.GetBodyToInt(message);
            int card = MessageManager.GetBodyToInt(message, 8);

            game.ElGringoStealCard(fromIndex, card);
        }

        private void Bang(byte[] message)
        {
            int targetIndex = MessageManager.GetBodyToInt(message);
            int card = MessageManager.GetBodyToInt(message, 8);

            game.Bang(targetIndex, card);
        }

        private void BangCardOpenCompleted(byte[] message)
        {
            int targetIndex = MessageManager.GetBodyToInt(message);
            int damage = MessageManager.GetBodyToInt(message, 8);

            game.BangCardOpenCompleted(targetIndex, damage);
        }

        private void BangResponse(byte[] message)
        {
            int defenceIndex = MessageManager.GetBodyToInt(message);
            int defenceCard = MessageManager.GetBodyToInt(message, 8);

            game.BangResponse(defenceIndex, defenceCard);
        }

        private void Emporio(byte[] message)
        {
            int card = MessageManager.GetBodyToInt(message);

            game.Emporio(card);
        }

        private void EmporioGetCard(byte[] message)
        {
            int playerIndex = MessageManager.GetBodyToInt(message);
            int card = MessageManager.GetBodyToInt(message, 8);

            game.EmporioGetCard(playerIndex, card);
        }

        private void Gatling(byte[] message)
        {
            int card = MessageManager.GetBodyToInt(message);

            game.Gatling(card);
        }

        private void Indian(byte[] message)
        {
            int card = MessageManager.GetBodyToInt(message);

            game.Indian(card);
        }

        private void WideAttackResponse(byte[] message)
        {
            int playerIndex = MessageManager.GetBodyToInt(message);
            int card = MessageManager.GetBodyToInt(message, 8);

            game.WideAttackResponse(playerIndex, card);
        }

        private void Duello(byte[] message)
        {
            int targetIndex = MessageManager.GetBodyToInt(message);
            int card = MessageManager.GetBodyToInt(message, 8);

            game.Duello(targetIndex, card);
        }

        private void DuelloResponse(byte[] message)
        {
            int playerIndex = MessageManager.GetBodyToInt(message);
            int card = MessageManager.GetBodyToInt(message, 8);

            game.DuelloResponse(playerIndex, card);
        }

        private void EquipGun(byte[] message)
        {
            int card = MessageManager.GetBodyToInt(message);
            int range = MessageManager.GetBodyToInt(message, 8);

            game.EquipGun(card, range);
        }

        private void Mirino(byte[] message)
        {
            int card = MessageManager.GetBodyToInt(message);

            game.Mirino(card);
        }

        private void Barile(byte[] message)
        {
            int card = MessageManager.GetBodyToInt(message);

            game.Barile(card);
        }

        private void Mustang(byte[] message)
        {
            int card = MessageManager.GetBodyToInt(message);

            game.Mustang(card);
        }

        private void WellsFargo(byte[] message)
        {
            int card = MessageManager.GetBodyToInt(message);

            game.WellsFargo(card);
        }

        private void Diligenza(byte[] message)
        {
            int card = MessageManager.GetBodyToInt(message);

            game.Diligenza(card);
        }

        private void Saloon(byte[] message)
        {
            int card = MessageManager.GetBodyToInt(message);

            game.Saloon(card);
        }

        private void Beer(byte[] message)
        {
            int card = MessageManager.GetBodyToInt(message);

            game.Beer(card);
        }

        private void Panico(byte[] message)
        {
            int targetIndex = MessageManager.GetBodyToInt(message);
            int card = MessageManager.GetBodyToInt(message, 8);

            game.Panico(targetIndex, card);
        }

        private void StealCard(byte[] message)
        {
            int targetIndex = MessageManager.GetBodyToInt(message);
            int card = MessageManager.GetBodyToInt(message, 8);

            game.StealCard(targetIndex, card);
        }

        private void CatBalou(byte[] message)
        {
            int targetIndex = MessageManager.GetBodyToInt(message);
            int card = MessageManager.GetBodyToInt(message, 8);

            game.CatBalou(targetIndex, card);
        }

        private void CatBalouCompleted(byte[] message)
        {
            int targetIndex = MessageManager.GetBodyToInt(message);
            int card = MessageManager.GetBodyToInt(message, 8);

            game.CatBalouDropCard(targetIndex, card);
        }

        private void CardOpenOrderSelectCompleted(byte[] message)
        {
            int playerIndex = MessageManager.GetBodyToInt(message);
            int card = MessageManager.GetBodyToInt(message, 8);

            game.CardOpenOrderSelectCompleted(playerIndex, card);
        }

        private void Prigione(byte[] message)
        {
            int targetIndex = MessageManager.GetBodyToInt(message);
            int card = MessageManager.GetBodyToInt(message, 8);

            game.Prigione(targetIndex, card);
        }

        private void PrigioneCardOpenCompleted(byte[] message)
        {
            int playerIndex = MessageManager.GetBodyToInt(message);

            game.PrigioneCardOpenCompleted(playerIndex);
        }

        private void Dinamite(byte[] message)
        {
            int targetIndex = MessageManager.GetBodyToInt(message);
            int dinamiteCard = MessageManager.GetBodyToInt(message, 8);

            game.Dinamite(targetIndex, dinamiteCard);
        }

        private void DinamiteCardOpenCompleted(byte[] message)
        {
            int playerIndex = MessageManager.GetBodyToInt(message);
            int dinamiteExplosion = MessageManager.GetBodyToInt(message, 8);

            game.DinamiteCardOpenCompleted(playerIndex, dinamiteExplosion);
        }

        private void DinamiteExplosionCompleted (byte[] message)
        {
            int playerIndex = MessageManager.GetBodyToInt(message);
            int isLiving = MessageManager.GetBodyToInt(message, 8);

            game.DinamiteExplosionCompleted(playerIndex, isLiving);
        }

        private void ValidCardCheck_PassTurn()
        {
            game.PlayerCardCountCheck_PassTurn();
        }

        private void ResponseDropCard(byte[] message)
        {
            int playerIndex = MessageManager.GetBodyToInt(message);
            int card = MessageManager.GetBodyToInt(message, 8);

            game.ResponseDropCard(playerIndex, card);
        }

        private void OpenJobCompleted(byte[] message)
        {
            int openPlayerIndex = MessageManager.GetBodyToInt(message);
            int job = MessageManager.GetBodyToInt(message, 8);
            int attackerIndex = MessageManager.GetBodyToInt(message, 12);

            game.OpenJobCompleted(openPlayerIndex, job, attackerIndex);
        }
    }
}