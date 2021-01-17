using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Shared.Structures;
using Shared.Game;

namespace GameServer
{
    class Program
    {
        private static bool isRunning = false;

        static void Main(string[] args)
        {
            Console.Title = "Game Server";
            isRunning = true;


            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(50, 42069);

            
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main Thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime nextLoop = DateTime.Now;
            int counter = 0;
            
            while (isRunning)
            {
                while(nextLoop < DateTime.Now)
                {
                    GameServer.MainThread.Update();
                    
                    counter++;
                    if (counter == 90)
                    {
                        
                        GameLogic.DoTick();
                        ServerSend.SendGameTick();
                        counter = 0;
                    }

                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if(nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
