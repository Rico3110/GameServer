using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Structures;
using Shared.Game;

namespace GameServer
{
    class ServerSend
    {
        public static void SendTCPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].tcp.SendData(packet);
        }

        public static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(packet);
            }
            PacketBuffer.Add(packet.ToArray());
        }

        public static void SendTCPDataToAll(int exceptClient,Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if(i != exceptClient)
                {
                    Server.clients[i].tcp.SendData(packet);
                }
                
            }
        }

        public static void Welcome(int toClient, string msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(msg);
                packet.Write(toClient);

                Server.StartPingTest();
                SendTCPData(toClient, packet);
            }
        }

        public static void Ping(int toClient,long ping)
        {
            //Send ping to Client
            using (Packet packet = new Packet((int)ServerPackets.ping))
            {
                packet.Write(ping);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }
        public static void InitGameLogic(int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.initGameLogic))
            {
                //Send HexGrid
                packet.Write(GameLogic.grid);
                //Send ownPlayer
                Player ownPlayer = Server.clients[toClient].Player;
                packet.Write(ownPlayer.Name);
                if (ownPlayer.Tribe == null)
                {
                    packet.Write((byte)255);
                }
                else
                {
                    packet.Write(ownPlayer.Tribe.Id);
                }
                packet.Write(ownPlayer.Position);
                packet.Write(ownPlayer.TroopInventory);

                //Send other Players
                packet.Write(GameLogic.Players.Count);
                foreach (Player player in GameLogic.Players)
                {
                    packet.Write(player.Name);
                    Tribe tribe = player.Tribe;
                    if (tribe == null)
                        packet.Write((byte)255);
                    else
                        packet.Write(player.Tribe.Id);
                    packet.Write(player.Position);
                    packet.Write(player.TroopInventory);
                }

                SendTCPData(toClient, packet);
            }   
        }   
    
        public static Packet GameState()
        {
            Packet packet = new Packet();

            packet.Write(GameLogic.grid);

            //Send other Players
            packet.Write(GameLogic.Players.Count);
            foreach (Player player in GameLogic.Players)
            {
                packet.Write(player.Name);
                Tribe tribe = player.Tribe;
                if (tribe == null)
                    packet.Write((byte)255);
                else
                    packet.Write(player.Tribe.Id);
                packet.Write(player.Position);
                packet.Write(player.TroopInventory);
            }
            return packet;
        }

        public static void SendStructure(HexCoordinates coordinates, Structure structure)
        {
            using (Packet packet = new Packet((int)ServerPackets.sendStructure))
            {
                packet.Write(coordinates);
                packet.Write(structure);
                SendTCPDataToAll(packet);
            }
        }

        public static void SendGameTick()
        {
            using (Packet packet = new Packet((int)ServerPackets.gameTick))
            {
                SendTCPDataToAll(packet);
            }
        }
        
        public static void BroadcastUpgradeBuilding(HexCoordinates coordinates)
        {
            using (Packet packet = new Packet((int)ServerPackets.upgradeBuilding))
            {
                packet.Write(coordinates);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastApplyBuild(HexCoordinates coords, Type buildingType, byte tribeID)
        {
            using (Packet packet = new Packet((int)ServerPackets.applyBuild))
            {
                packet.Write(coords);
                packet.Write(buildingType);
                packet.Write(tribeID);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastApplyBuildHQ(HexCoordinates coords)
        {
            using (Packet packet = new Packet((int)ServerPackets.applyBuildHQ))
            {
                packet.Write(coords);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastPlayer(Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastPlayer))
            {
                packet.Write(player.Name);
                if (player.Tribe == null)
                {
                    packet.Write((byte)255);
                }
                else 
                {
                    packet.Write(player.Tribe.Id);
                }
                packet.Write(player.Position);
                packet.Write(player.TroopInventory);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastTribe(Tribe tribe)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastTribe))
            {
                packet.Write(tribe.Id);
                packet.Write(tribe.HQ.Cell.coordinates);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastMoveTroops(Player player, HexCoordinates coordinates, TroopType type, int amount)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastMoveTroops))
            {
                packet.Write(player.Name);
                packet.Write(coordinates);
                packet.Write((byte)type);
                packet.Write(amount);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastFight(Player player, HexCoordinates coordinates)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastFight))
            {
                packet.Write(player.Name);
                packet.Write(coordinates);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastHarvest(byte tribeID, HexCoordinates coordinates)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastHarvest))
            {
                packet.Write(tribeID);
                packet.Write(coordinates);
                SendTCPDataToAll(packet);
            }

        }

        public static void BroadcastChangeAllowedRessource(HexCoordinates originCoordinates, HexCoordinates destinationCoordinates, RessourceType ressourceType, bool newValue)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastChangeAllowedRessource))
            {
                packet.Write(originCoordinates);
                packet.Write(destinationCoordinates);
                packet.Write((byte)ressourceType);
                packet.Write(newValue);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastChangeTroopRecipeOfBarracks(HexCoordinates barracks, TroopType troopType)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastChangeTroopRecipeOfBarracks))
            {
                packet.Write(barracks);
                packet.Write((byte)troopType);
            }
        }

        public static void BroadcastChangeStrategyOfProtectedBuilding(HexCoordinates coordinates, int oldIndex, int newIndex)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastChangeStrategyOfProtectedBuilding))
            {
                packet.Write(coordinates);
                packet.Write(oldIndex);
                packet.Write(newIndex);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastChangeStrategyOfPlayer(string playerName, int oldIndex, int newIndex)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastChangeStrategyOfName))
            {
                packet.Write(playerName);
                packet.Write(oldIndex);
                packet.Write(newIndex);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastChangeStrategyActivePlayer(string playerName, TroopType type, bool newValue)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastChangeStrategyActivePlayer))
            {
                packet.Write(playerName);
                packet.Write((byte)type);
                packet.Write(newValue);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastChangeStrategyActiveBuilding(HexCoordinates coordinates, TroopType type, bool newValue)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastChangeStrategyActiveBuilding))
            {
                packet.Write(coordinates);
                packet.Write((byte)type);
                packet.Write(newValue);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastMoveRessources(HexCoordinates originCoordinates, HexCoordinates destinationCoordinates, RessourceType type, int amount)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastMoveRessources))
            {
                packet.Write(originCoordinates);
                packet.Write(destinationCoordinates);
                packet.Write((byte)type);
                packet.Write(amount);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastChangeRessourceLimit(HexCoordinates coordinates, RessourceType type, int newValue)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastChangeRessourceLimit))
            {
                packet.Write(coordinates);
                packet.Write((byte)type);
                packet.Write(newValue);
                SendTCPDataToAll(packet);
            }
        }
    }
}
