using Ffmpeg.Core;
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
        static async Task Main(string[] args)
        {
            List<Task<FfmpegSampleUsage.SampleResult>> cmds = new List<Task<FfmpegSampleUsage.SampleResult>>();

            for (var i = 0; i < 1; i++)
            {
                cmds.Add(new FfmpegSampleUsage().Convert());
            }

            var result = await Task.WhenAll(cmds);

            Console.WriteLine($"Total: {result.Sum(i=>i.ConvertResult.ConvertInMiliseconds)}");

            foreach (var r in result)
            {
                Console.WriteLine($"Success:{r.ConvertResult.Success}:InSeconds:{r.ConvertResult.ConvertInMiliseconds / 1000}=>{r.FileVideo}");
            }

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

    public class FfmpegSampleUsage
    {
        Random _rnd = new Random();
        public FfmpegSampleUsage()
        {

        }
        public List<string> ListImageFile()
        {
            return Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImageTest/imgs"))
                .Select(i => i).ToList();
        }

        public List<string> ListAudioFile()
        {
            return Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImageTest/audio"))
                .Select(i => i).ToList();
        }

        public async Task<SampleResult> Convert()
        {
            List<string> audios = ListAudioFile();
            string audioFile = audios[_rnd.Next(0, audios.Count - 1)];

            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"ImageTest/results");

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string fileOutput = Path.Combine(dir, $"video_{DateTime.Now.Ticks}.mp4");

            var cmd = new FFmpegCommandBuilder().WithFileAudio(audioFile)
                .AddFileInput(ListImageFile().ToArray())
                .WithFileOutput(fileOutput)
                .WithVideoDuration(60)
                .ToCommand();

            Console.WriteLine(fileOutput);

            var r = await new FfmpegCommandRunner().Run(cmd);

            return new SampleResult
            {
                ConvertResult = r,
                FileVideo = fileOutput
            };
        }

        public class SampleResult
        {
            public string FileVideo { get; set; }

            public FfmpegCommandResult ConvertResult { get; set; }
        }
    }
}
