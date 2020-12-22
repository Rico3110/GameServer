using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using Shared.MapGeneration;
using Shared.DataTypes;
using Shared.GameState;
using Shared.GameLogic;
using Shared.HexGrid;
using Shared.Communication;



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

        private static HexMap map;
        public static HexGrid grid;
        public static Shared.GameLogic.GameLogic gameLogic;

        public static void Start(int maxPlayers, int port)
        {
            MaxPlayers = maxPlayers;
            Port = port;

            Console.WriteLine("Starting Server...");
            InitSeverData();

            Console.WriteLine("Generating Map...");
            MapGenerator mapGenerator = new MapGenerator(50.392f, 8.065f, 1);

            gameLogic = new Shared.GameLogic.GameLogic();
            gameLogic.grid = mapGenerator.createMap();
           

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server Started on {Port}.");                   
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
                {(int)ClientPackets.requestBuildingData, ServerHandle.SendBuildingData},
                {(int)ClientPackets.requestBuildBuilding, ServerHandle.TryBuildBuilding}
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
            ServerSend.SendHexMap(ClientID, new HexMap(gameLogic.grid.SerializeData(), gameLogic.grid.chunkCountX, gameLogic.grid.chunkCountZ, 0, 0));
        }
    }
}
