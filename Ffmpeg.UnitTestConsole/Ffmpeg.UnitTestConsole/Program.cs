using Ffmpeg.Core;
using System;

namespace Ffmpeg.UnitTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

           var temp= new FfmpegCommandRunner().Run();
          
            while (true)
            {
                Console.WriteLine("Type quit to exist");
                var cmd = Console.ReadLine();
                if (cmd == "quit")
                {
                    Environment.Exit(0);
                }
            }
            
        }
    }
}
