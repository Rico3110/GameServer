using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Communication;
using Shared.HexGrid;
using Shared.DataTypes;
using Shared.Structures;
using Shared.Game;

namespace GameServer
{
    class ServerHandle
    {
        public static void WelcomeRecieved(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();
            string username = packet.ReadString();

            Console.WriteLine($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}");

            if(fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIDCheck})!");
            }
            //TODO send player into game
            Player clientPlayer = GameLogic.GetPlayer(username); 
            if (clientPlayer != null)
            {
                foreach(Client client in Server.clients.Values)
                {
                    if (client.Player == clientPlayer)
                    {
                        Console.WriteLine($"Can't assign a Player to \"{username}\" (ID: {fromClient}) because there is already an active client with the username \"{username}\"!");
                        // Server.clients[fromClient].Disconnect();
                    }
                }
                Server.clients[fromClient].Player = clientPlayer;
            }
            else 
            {
                Server.clients[fromClient].Player = GameLogic.AddPlayer(username, null);
            }

            //Stop Timer
            Server.StopPingTest(fromClient);
        }

        public static void HandleMapRequest(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }
            Console.WriteLine("fdsajkllhgdsj");

            ServerSend.InitGameLogic(fromClient);                  
        }

        public static void HandleBuildHQ(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;

            HexCoordinates coordinates = packet.ReadHexCoordinates();
            Headquarter hq = new Headquarter();
            
            if (GameLogic.VerifyBuildHQ(coordinates, hq, player))
            {
                Tribe tribe = GameLogic.ApplyBuildHQ(coordinates, hq);
                player.Tribe = tribe;
                ServerSend.SendStructure(coordinates, hq);
                ServerSend.BroadcastTribe(tribe);
                ServerSend.BroadcastPlayer(player);
            }
        }

        public static void HandlePlaceBuilding(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;

            HexCoordinates coordinates = packet.ReadHexCoordinates();
            Type type = packet.ReadType();
            Building building = (Building)Activator.CreateInstance(type);

            if(GameLogic.VerifyBuild(coordinates, building, player))
            {
                GameLogic.ApplyBuild(coordinates, building, player.Tribe);
                ServerSend.SendStructure(coordinates, building);             
            }
        }

        public static void HandleUpgradeBuilding(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;

            HexCoordinates coordinates = packet.ReadHexCoordinates();
            if (GameLogic.VerifyUpgrade(coordinates, player))
            {
                GameLogic.ApplyUpgrade(coordinates, player.Tribe);
                ServerSend.SendUpgradeBuilding(coordinates);
            }
        }

        public static void HandlePositionUpdate(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            HexCoordinates coordinates = packet.ReadHexCoordinates();
            player.Position = coordinates;
            ServerSend.BroadcastPlayer(player);
        }
    }
}
