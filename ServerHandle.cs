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
using UnityEngine;

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
            ServerSend.InitGameLogic(fromClient);
        }

        public static void HandleMapRequest(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

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
                ServerSend.BroadcastApplyBuildHQ(coordinates);
                ServerSend.BroadcastPlayer(player);
                Console.WriteLine("Player: " + player.Name + " successfully placed a HQ.");
            }
            else
            {
                Console.WriteLine("Player: " + player.Name + " failed to build HQ");
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

            if (GameLogic.VerifyBuild(coordinates, type, player))
            {
                GameLogic.ApplyBuild(coordinates, type, player.Tribe);
                ServerSend.BroadcastApplyBuild(coordinates, type, player.Tribe.Id);             
                Console.WriteLine("Player: " + player.Name + " of tribe " + player.Tribe.Id.ToString() + " successfully placed a " + building.GetName() + ".");
            }
            else
            {
                Console.WriteLine("Player: " + player.Name + " of tribe " + player.Tribe.Id.ToString() + " failed to build " + building.GetName() + ".");
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
                ServerSend.BroadcastUpgradeBuilding(coordinates);
                Console.WriteLine("Player: " + player.Name + " of tribe " + player.Tribe.Id.ToString() + " successfully upgraded a building at " + coordinates.ToString() + ".");
            }
            else
            {
                Console.WriteLine("Player: " + player.Name + " of tribe " + player.Tribe.Id.ToString() + " failed upgrade building at " + coordinates.ToString() + "");
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

        public static void HandleJoinTribe(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            HexCoordinates coordinates = packet.ReadHexCoordinates();
            if (GameLogic.PlayerInRange(coordinates, player) && player.Tribe == null)
            {
                HexCell cell = GameLogic.grid.GetCell(coordinates);
                if (cell != null)
                {
                    Structure hq = cell.Structure;
                    if (hq is Headquarter)
                    {
                        player.Tribe = GameLogic.GetTribe(((Headquarter)hq).Tribe);
                        ServerSend.BroadcastPlayer(player);
                        Console.WriteLine("Player: " + player.Name + " successfully joined the tribe " + player.Tribe.Id.ToString() + ".");
                        return;
                    }
                }
            }
            Console.WriteLine("Player: " + player.Name + " failed to join the tribe " + player.Tribe.Id.ToString() + ".");
        }

        public static void HandleMoveTroops(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            HexCoordinates coordinates = packet.ReadHexCoordinates();
            TroopType troopType = (TroopType)packet.ReadByte();
            int amount = packet.ReadInt();

            Player player = Server.clients[fromClient].Player;
            TroopInventory buildingInventory = ((ProtectedBuilding) GameLogic.grid.GetCell(coordinates).Structure).TroopInventory;
            
            if (GameLogic.MoveTroops(player, coordinates, troopType, amount))
            {
                ServerSend.BroadcastMoveTroops(player, coordinates, troopType, amount);
                Console.WriteLine("Player: " + player.Name + "of tribe " + player.Tribe.Id.ToString() + " successfully exchanged " + amount.ToString() + troopType.ToString() + " with a building at" + coordinates.ToString() + ".");
            }
            else
            {
                Console.WriteLine("Player: " + player.Name + "of tribe " + player.Tribe.Id.ToString() + " failed to exchange " + amount.ToString() + troopType.ToString() +" with building at " + coordinates.ToString() + ".");
            }
        }

        public static void HandleFight(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            HexCoordinates coordinates = packet.ReadHexCoordinates();

            if(GameLogic.PlayerInRange(coordinates, player))
            {
                GameLogic.Fight(player, coordinates);
                ServerSend.BroadcastFight(player, coordinates);
                Console.WriteLine("Player: " + player.Name + "of tribe" + player.Tribe.Id.ToString() + " successfully fought a building at " + coordinates.ToString() + ".");
            }
            else
            {
                Console.WriteLine("Player: " + player.Name + "of tribe" + player.Tribe.Id.ToString() + " failed to fight a building at " + coordinates.ToString() + ".");
            }

        }

        public static void HandleHarvest(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            HexCoordinates coordinates = packet.ReadHexCoordinates();

            if (player.Tribe != null)
            {
                if (GameLogic.Harvest(player.Tribe.Id, coordinates))
                {
                    ServerSend.BroadcastHarvest(player.Tribe.Id, coordinates);
                    Console.WriteLine("Player: " + player.Name + "of tribe" + player.Tribe.Id.ToString() + " successfully harvested ressource at " + coordinates.ToString() + ".");
                    return;
                }
                Console.WriteLine("Player: " + player.Name + "of tribe" + player.Tribe.Id.ToString() + " failed to harvest ressource at " + coordinates.ToString() + ".");
            }
        }

        public static void HandleChangeAllowedRessource(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            HexCoordinates originCoordinates = packet.ReadHexCoordinates();

            if (GameLogic.PlayerInRange(originCoordinates, player))
            {
                HexCoordinates destinationCoordinates = packet.ReadHexCoordinates();
                RessourceType ressourceType = (RessourceType)packet.ReadByte();
                bool newValue = packet.ReadBool();
                if (GameLogic.ChangeAllowedRessource(originCoordinates, destinationCoordinates, ressourceType, newValue))
                    ServerSend.BroadcastChangeAllowedRessource(originCoordinates, destinationCoordinates, ressourceType, newValue);
            }

        }
    }
}
