using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Shared.Communication;
using Shared.Game;

namespace GameServer
{
    class Client
    {
        public static int dataBufferSize = 4096;
        public int id;
        public TCP tcp;

        public Client(int clientID)
        {
            id = clientID;
            tcp = new TCP(id);
        }

        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private Packet recievedData;
            private byte[] recieveBuffer;

            public TCP(int id)
            {
                this.id = id;
            }

            public void Connect(TcpClient socket)
            {
                this.socket = socket;
                this.socket.ReceiveBufferSize = dataBufferSize;
                this.socket.SendBufferSize = dataBufferSize;

                stream = this.socket.GetStream();

                recievedData = new Packet();
                recieveBuffer = new byte[dataBufferSize];

                stream.BeginRead(recieveBuffer, 0, dataBufferSize, RecieveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server!");

                ServerSend.SendHexGrid(GameLogic.grid, id);
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if(socket != null)
                    {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error sending Data to Player {id} via TCP: {e}.");
                }
            }

            private void RecieveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = stream.EndRead(result);
                    if(byteLength <= 0)
                    {
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(recieveBuffer, data, byteLength);

                    recievedData.Reset(HandleData(data));
                    stream.BeginRead(recieveBuffer, 0, dataBufferSize, RecieveCallback, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error recieving TCP data {ex}");
                    Server.clients[id].Disconnect();
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;

                recievedData.SetBytes(data);

                if (recievedData.UnreadLength() >= 4)
                {
                    packetLength = recievedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= recievedData.UnreadLength())
                {
                    byte[] packetBytes = recievedData.ReadBytes(packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetID = packet.ReadInt();
                            Server.packetHandlers[packetID](id,packet);
                        }
                    });

                    packetLength = 0;

                    if (recievedData.UnreadLength() >= 4)
                    {
                        packetLength = recievedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            public void Disconnect()
            {
                socket.Close();
                stream = null;
                recievedData = null;
                recieveBuffer = null;
                socket = null;
            }
        }

        public void Disconnect()
        {
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

            //TODO
            //player = null;
            tcp.Disconnect();
        }

    }
}
