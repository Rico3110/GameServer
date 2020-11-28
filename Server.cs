using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using GameServer.Map;
using GameServer.DataTypes;



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

        public static void Start(int maxPlayers, int port)
        {
            MaxPlayers = maxPlayers;
            Port = port;

            Console.WriteLine("Starting Server...");
            InitSeverData();

            uint[,] test = GameServer.Map.MapboxHandle.createMap(49.889347, 8.667032, 1, 1);

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
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeRecieved}
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
            ServerSend.TestArray(ClientID);
        }
    }
}
