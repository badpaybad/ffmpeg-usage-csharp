using Ffmpeg.FaceRecognition;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ffmpeg.UnitTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            new FaceDetection().WithInputFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FaceTest/omt2.jpg"))
                .TestDnnCaffeModel();

            //var xxx = new FaceDetection().WithInputFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FaceTest/omt2.jpg"))
            //     .CompareTo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FaceTest/kien2.png"));

            //foreach (var x in xxx)
            //{
            //    x.Face.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"FaceTest/kien2__{(int)x.PredictionResult.Distance}.jpg"));
            //}

            //  Task.Run(async () =>
            //{
            //    //List<Task< FfmpegSampleUsageRenderImagesToVideo.SampleResult >> resultAsync = new List<Task<FfmpegSampleUsageRenderImagesToVideo.SampleResult>>();
            //    ////if want to do stress test i < 100
            //    //for (var i = 0; i < 1; i++)
            //    //{
            //    //    resultAsync.Add(Task.Run(() => new FfmpegSampleUsageRenderImagesToVideo().Convert()));

            //    //}
            //    //var result = await Task.WhenAll(resultAsync);
            //    //Console.WriteLine($"Total in miliseconds: {result.Sum(i => i.TotalRunInMiliseconds)}");

            //    //foreach (var r in result)
            //    //{
            //    //    Console.WriteLine($"Success:{r.ConvertResult.Success}:InSeconds:{r.ConvertResult.ConvertInMiliseconds / 1000}=>{r.FileVideo}");
            //    //}
            //});

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
