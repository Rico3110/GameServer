using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using Shared.MapGeneration;
using Shared.DataTypes;
using Shared.Game;
using Shared.HexGrid;
using Shared.Communication;
using Shared.Game;
using Shared.Structures;



namespace GameServer
{
    class Server
    {
        public static int MaxPlayers { get;private set; }
        public static int Port { get; private set; }

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        public delegate void PacketHandler(int fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;

        private static Stopwatch sw;
        private static long ping;

        private static bool askForCoordinates = true; 

        public static void Start(int maxPlayers, int port)
        {
            MaxPlayers = maxPlayers;
            Port = port;

            Console.WriteLine("Starting Server...");
            InitSeverData();


            

            if (File.Exists("savegame.hex"))
            {
                LoadGame();
            }
            else
            {
                NewGame();
            }


            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server Started on {Port}.");                   
        }

        private static void NewGame()
        {
            
            //Get User Input for Coordinates
            float lat = 50.232767f;
            float lon = 8.457934f;

            if (askForCoordinates)
            {
                Console.WriteLine("Please enter the latitude of your desired position or leave empty for default position(Darmstadt)...");
                string latS = Console.ReadLine();
                if (latS != "")
                {
                    lat = float.Parse(latS);
                }

                Console.WriteLine("Please enter the longtitude of your desired position or leave empty for default position...");
                string lonS = Console.ReadLine();
                if (lonS != "")
                {
                    lon = float.Parse(lonS);
                }
            }


            Console.WriteLine("Generating Map...");
            Console.WriteLine(lat + " , " + lon);
            MapGenerator mapGenerator = new MapGenerator(lat, lon, 7);

            GameLogic.Init(mapGenerator.CreateMap());
        }

        public static void SaveGame()
        {
            File.WriteAllBytes("savegame.hex", ServerSend.GameState().ToArray());
        }

        private static void LoadGame()
        {
            Console.WriteLine("Loading Game...");
            byte[] gamestate = File.ReadAllBytes("savegame.hex");
            Packet packet = new Packet(gamestate);

            HexGrid hexGrid = packet.ReadHexGrid();

            GameLogic.Init(hexGrid);

            int count = packet.ReadInt();
            for (int i = 0; i < count; i++)
            {
                string playerName = packet.ReadString();

                int tribeId = packet.ReadByte();
                HexCoordinates playerPos = packet.ReadHexCoordinates();
                TroopInventory troopInventory = packet.ReadTroopInventory();

                GameLogic.AddPlayer(playerName, tribeId, playerPos, troopInventory);
            }
            Console.WriteLine("Game Loaded!");
        }

        private static void TCPConnectCallback(IAsyncResult result)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Incoming Connection from {client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if(clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    return;
                }
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to Connect: Server full!");
        }

        private static void InitSeverData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i,new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeRecieved},               
                {(int)ClientPackets.requestHexGrid, ServerHandle.HandleMapRequest},
                {(int)ClientPackets.requestPlaceBuilding, ServerHandle.HandlePlaceBuilding },
                {(int)ClientPackets.requestUpgradeBuilding, ServerHandle.HandleUpgradeBuilding },
                {(int)ClientPackets.positionUpdate, ServerHandle.HandlePositionUpdate },
                {(int)ClientPackets.requestBuildHQ, ServerHandle.HandleBuildHQ },
                {(int)ClientPackets.requestJoinTribe, ServerHandle.HandleJoinTribe},
                {(int)ClientPackets.requestMoveTroops, ServerHandle.HandleMoveTroops},
                {(int)ClientPackets.requestFight, ServerHandle.HandleFight},
                {(int)ClientPackets.requestHarvest, ServerHandle.HandleHarvest},
                {(int)ClientPackets.requestChangeAllowedRessource, ServerHandle.HandleChangeAllowedRessource},
                {(int)ClientPackets.requestChangeTroopRecipeOfBarracks, ServerHandle.HandleChangeTroopRecipeOfBarracks},
                {(int)ClientPackets.requestChangeOrderOfStrategy, ServerHandle.HandleChangeStrategy},
                {(int)ClientPackets.requestChangeActiveOfStrategyPlayer, ServerHandle.HandleChangeActiveOfStrategyPlayer},
                {(int)ClientPackets.requestChangeActiveOfStrategyBuilding, ServerHandle.HandleChangeActiveOfStrategyBuilding},
                {(int)ClientPackets.requestMoveRessource, ServerHandle.HandleMoveRessource},
                {(int)ClientPackets.requestChangeRessourceLimit, ServerHandle.HandleChangeRessourceLimit},
                {(int)ClientPackets.requestUpdateMarketRessource, ServerHandle.HandleUpdateMarketRessource},
                {(int)ClientPackets.requestDestroyBuilding, ServerHandle.HandleDestroyBuilding }
            };
            Console.WriteLine($"Initialized packets");
        }

        public static void StartPingTest()
        {
            sw = new Stopwatch();
            sw.Start();
        }

        public static void StopPingTest(int ClientID)
        {
            sw.Stop();
            ping = sw.ElapsedMilliseconds / 2;

            Console.WriteLine($"Ping to Client {ClientID}: {ping}ms");

            ServerSend.Ping(ClientID, ping);
        }
    }
}
