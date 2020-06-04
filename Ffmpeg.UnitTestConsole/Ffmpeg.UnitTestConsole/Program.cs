using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ffmpeg.UnitTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
          {
              List<Task< FfmpegSampleUsageRenderImagesToVideo.SampleResult >> resultAsync = new List<Task<FfmpegSampleUsageRenderImagesToVideo.SampleResult>>();
              for (var i = 0; i < 1; i++)
              {
                  resultAsync.Add(Task.Run(() => new FfmpegSampleUsageRenderImagesToVideo().Convert()));

              }
              var result = await Task.WhenAll(resultAsync);
              Console.WriteLine($"Total in miliseconds: {result.Sum(i => i.ConvertResult.ConvertInMiliseconds) + result.Sum(i=>i.ConvertResult.SubResult.Select(i=>i.ConvertInMiliseconds).Sum()) }");

              foreach (var r in result)
              {
                  Console.WriteLine($"Success:{r.ConvertResult.Success}:InSeconds:{r.ConvertResult.ConvertInMiliseconds / 1000}=>{r.FileVideo}");
              }
          });

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
