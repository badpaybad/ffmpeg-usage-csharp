using Ffmpeg.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ffmpeg.UnitTestConsole
{
    public class FfmpegSampleUsageRenderImagesToVideo
    {
        Random _rnd = new Random();
        public FfmpegSampleUsageRenderImagesToVideo()
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

        public SampleResult Convert()
        {
            List<string> audios = ListAudioFile();
            string audioFile = audios[_rnd.Next(0, audios.Count - 1)];

            var dir = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImageTest/results"));

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string fileOutput = Path.Combine(dir, $"video_{DateTime.Now.Ticks}.mp4");

            var cmd = new FFmpegCommandBuilder()
                .WithFileAudio(audioFile)
                .AddFileInput(ListImageFile().Take(3).Select(i => new FileInput
                {
                    FullPathFile = i
                }).ToArray())
                .WithFileOutput(fileOutput)
                .WithVideoDurationInSeconds(30)
                .WithFadeTransition("fadewhite")
                .AddGifOverlay(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImageTest/gif/heart.gif"), _rnd.Next(1, 10))
                .AddGifOverlay(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImageTest/gif/sunset.gif"), _rnd.Next(10, 19))
                .AddImageOverLay(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImageTest/gif/2.jpg"), _rnd.Next(11,16),2,200,200,320)
                .WithFadeDurationInSeconds(1)
                .ToCommandXfade();

            Console.WriteLine(fileOutput);

            var r = new FfmpegCommandExecuter().Run(cmd);

            return new SampleResult
            {
                ConvertResult = r,
                FileVideo = fileOutput,
                TotalRunInMiliseconds= r.ConvertInMiliseconds+ r.SubResult.Select(i=>i.ConvertInMiliseconds).Sum()
            };
        }

        public class SampleResult
        {
            public string FileVideo { get; set; }

            public FfmpegConvertedResult ConvertResult { get; set; }

            public decimal TotalRunInMiliseconds { get; set; }
        }
    }
}
