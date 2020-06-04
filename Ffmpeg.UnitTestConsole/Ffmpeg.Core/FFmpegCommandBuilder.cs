using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ffmpeg.Core
{
    public class FFmpegCommandBuilder
    {
        List<string> _xfade = new List<string> {
        "fade",
"wipeleft",
"wiperight",
"wipeup",
"wipedown",
"slideleft",
"slideright",
"slideup",
"slidedown",
"circlecrop",
"rectcrop",
"distance",
"fadeblack",
"fadewhite",
"radial",
"smoothleft",
"smoothright",
"smoothup",
"smoothdown",
"circleopen",
"circleclose",
"vertopen",
"vertclose",
"horzopen",
"horzclose",
"dissolve",
"pixelize",
"diagtl",
"diagtr",
"diagbl",
"diagbr",
"hlslice",
"hrslice",
"vuslice",
"vdslice"
        };

        List<string> _fileInput = new List<string>();

        string _fileAudio;
        string _fileOutput;
        decimal _videoDuration;
        string _videoScale = "1080:720";

        string _dirOutput;
        string _fileOutputName;
        private decimal _fadeDuration = 1;

        Random _rnd = new Random();

        public FFmpegCommandBuilder AddFileInput(params string[] files)
        {
            foreach (var file in files)
            {
                _fileInput.Add(file);
            }

            return this;
        }
        public FFmpegCommandBuilder WithFileAudio(string file)
        {
            _fileAudio = file;
            return this;
        }
        public FFmpegCommandBuilder WithFileOutput(string file)
        {
            _fileOutput = file;

            _fileOutput = _fileOutput.Replace("\\", "/");

            var idx = _fileOutput.LastIndexOf("/");

            _dirOutput = _fileOutput.Substring(0, idx);
            _fileOutputName = _fileOutput.Substring(idx + 1);
            return this;
        }
        public FFmpegCommandBuilder WithVideoDurationInSeconds(decimal duration)
        {
            _videoDuration = duration;
            return this;
        }
        public FFmpegCommandBuilder WithFadeDurationInSeconds(decimal duration)
        {
            _fadeDuration = duration;
            return this;
        }
        public FfmpegCommandOutput ToCommand()
        {
            var timeForEachImage = _videoDuration / _fileInput.Count;

            var fadeDuration = _fadeDuration;
            if (fadeDuration > timeForEachImage)
            {
                fadeDuration = timeForEachImage;
            }

            List<FfmpegCommandOutput> subFileList = new List<FfmpegCommandOutput>();

            if (_fileInput.Count % 2 != 0)
            {
                _fileInput.Add(_fileInput[0]);
            }

            SplitToRun(_fileInput, (itms, idx) =>
            {
                var subFileName = Path.Combine(_dirOutput, idx + "_" + _fileOutputName);

                var subCmd = BuildFfmpegCommandImageTransitionXFade(itms, subFileName, timeForEachImage, fadeDuration, _xfade[_rnd.Next(0, _xfade.Count - 1)]);

                subFileList.Add(new FfmpegCommandOutput
                {
                    FfmpegCommand = subCmd,
                    FileOutput = subFileName
                });
            });

            var cmdMain = new FfmpegCommandOutput
            {
                FfmpegCommand = BuildFfmpegConcatVideo(subFileList.Select(i => i.FileOutput).ToList(), _fileOutput, timeForEachImage * 2, fadeDuration),
                FileOutput = _fileOutput,
                SubFileOutput = subFileList
            };

            return cmdMain;
        }

        /// <summary>
        /// only suport 2 file input
        /// </summary>
        /// <param name="fileInput"></param>
        /// <param name="fileOutput"></param>
        /// <param name="timeForEachImage"></param>
        /// <param name="fadeDuration"></param>
        /// <param name="fadeMethod"></param>
        /// <returns></returns>
        string BuildFfmpegCommandImageTransitionXFade(List<string> fileInput, string fileOutput, decimal timeForEachImage, decimal fadeDuration
           , string fadeMethod = "distance")
        {
            //https://trac.ffmpeg.org/wiki/Xfade

            timeForEachImage = Math.Round(timeForEachImage, 1);
            fadeDuration = Math.Round(fadeDuration, 1);

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            string cmd = $"\"{ffmpegCmd}\"";
            string filter = "";
            string filterFadeIndex = "";
            string filterFadeConcat = "";
            string filterScaleImage = "";
            var offset = timeForEachImage - fadeDuration;

            for (int i = 0; i < fileInput.Count; i++)
            {
                string f = fileInput[i];
                cmd += $" -loop 1 -t {timeForEachImage + fadeDuration} -i \"{f}\"";
                filterScaleImage += $"[{i}:v]scale={_videoScale}:force_original_aspect_ratio=decrease,pad={_videoScale}:(ow-iw)/2:(oh-ih)/2,setsar=1[v{i}];";

                filterFadeIndex += $"[v{i}]";
            }

            cmd += $" -filter_complex \"{filterScaleImage}{filterFadeIndex}xfade=transition={fadeMethod}:duration={fadeDuration}:offset={offset},format=yuv420p[v]\"";
            cmd += $" -map \"[v]\" -shortest \"{fileOutput}\"";

            while (cmd.IndexOf("\\") >= 0)
            {
                cmd = cmd.Replace("\\", "/");
            }

            return cmd;
        }

        public string BuildFfmpegConcatVideo(List<string> filesInput, string fileOutput, decimal timeForEachImage, decimal fadeDuration
            , string fadeMethod = "distance")
        {

            timeForEachImage = Math.Round(timeForEachImage, 1);
            fadeDuration = Math.Round(fadeDuration, 1);

            var timeFadeOut = timeForEachImage - fadeDuration;
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");


            string cmd = $"\"{ffmpegCmd}\"";

            var filterFadeVideo = "";
            var filterIndex = "";

            for (int i = 0; i < filesInput.Count; i++)
            {
                string f = filesInput[i];
                cmd += $" -i \"{f}\"";
                filterFadeVideo += $"[{i}:v]scale={_videoScale}:force_original_aspect_ratio=decrease,pad={_videoScale}:(ow-iw)/2:(oh-ih)/2,setsar=1,fade=t=out:st={timeFadeOut}:d={fadeDuration}[v{i}];";
                filterIndex += $"[v{i}]";
            }
            if (!string.IsNullOrEmpty(_fileAudio))
            {
                cmd += $" -i \"{_fileAudio}\"";
            }

            cmd += $" -filter_complex \"{filterFadeVideo}{filterIndex} concat=n={filesInput.Count}:v=1:a=0,format=yuv420p[v]\"";
            if (!string.IsNullOrEmpty(_fileAudio))
            {
                cmd += $" -map \"[v]\" -map {filesInput.Count}:a -shortest \"{fileOutput}\"";
            }
            else
            {
                cmd += $" -map \"[v]\" -map -shortest \"{fileOutput}\"";
            }

            return cmd;
        }

        public static void SplitToRun<T>(List<T> allItems, Action<List<T>, int> doBatch, int batchSize = 2)
        {
            if (allItems == null || allItems.Count == 0) return;
            var total = allItems.Count;
            var skip = 0;
            int batchCount = 0;
            while (true)
            {
                var batch = allItems.Skip(skip).Take(batchSize).Distinct().ToList();

                if (batch == null || batch.Count == 0) { return; }

                doBatch(batch, batchCount);

                batchCount++;

                skip = skip + batchSize;

                total = total - batchSize;
            }

        }
    }
}

//public string BuildFfmpegCommandCustomFade(List<string> fileInput, string fileOutput, int timeForEachImage, int duration)
//{
//    //https://trac.ffmpeg.org/wiki/Xfade

//    var timeFadeOut = timeForEachImage - duration;

//    string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

//    string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

//    string cmd = $"\"{ffmpegCmd}\"";

//    string filterFadeTransition = "";
//    string filterAfter = "";
//    for (int i = 0; i < fileInput.Count; i++)
//    {
//        string f = fileInput[i];
//        cmd += $" -loop 1 -t {timeForEachImage} -i \"{f}\"";

//        filterFadeTransition += $"[{i}:v]scale={_videoScale}:force_original_aspect_ratio=decrease,pad={_videoScale}:(ow-iw)/2:(oh-ih)/2,setsar=1,fade=t=out:st={timeFadeOut}:d={duration}[v{i}];";

//        filterAfter += $"[v{i}]";
//    }

//    if (!string.IsNullOrEmpty(_fileAudio))
//    {
//        cmd += $" -i \"{_fileAudio}\"";
//    }
//    cmd += $" -filter_complex \"{filterFadeTransition}{filterAfter} concat=n={fileInput.Count}:v=1:a=0,format=yuv420p[v]\"";
//    cmd += $" -map \"[v]\" -map {fileInput.Count}:a -shortest \"{fileOutput}\"";

//    while (cmd.IndexOf("\\") >= 0)
//    {
//        cmd = cmd.Replace("\\", "/");
//    }

//    return cmd;
//}
