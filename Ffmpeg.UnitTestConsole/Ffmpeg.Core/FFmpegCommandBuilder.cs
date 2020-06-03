using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ffmpeg.Core
{
    public class FFmpegCommandBuilder
    {
        List<string> _fileInput = new List<string>();

        string _fileAudio;
        string _fileOutput;
        int _duration;
        string _videoScale = "1080:720";

        string _dirOutput;
        string _fileOutputName;
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
        public FFmpegCommandBuilder WithVideoDurationInSeconds(int duration)
        {
            _duration = duration;
            return this;
        }
        public FfmpegCommandOutput ToCommand()
        {
            var timeForEachImage = (int)Math.Round((decimal)_duration / _fileInput.Count, 0);
            if (timeForEachImage <= 0) timeForEachImage = 1;

            var timeFadeOut = timeForEachImage - 1;
            if (timeFadeOut <= 0) timeFadeOut = 1;

            var mainCmd = BuildFfmpegCommandTransitionFade(_fileInput, _fileOutput, timeForEachImage, timeFadeOut);
            if (mainCmd.Length <=4000)
            {
                //https://support.microsoft.com/en-us/help/830473/command-prompt-cmd-exe-command-line-string-limitation
                return new FfmpegCommandOutput
                {
                    FileOutput = _fileOutput,
                    FfmpegCommand = mainCmd
                };
            }

            List<FfmpegCommandOutput> subFileList = new List<FfmpegCommandOutput>();

            SplitToRun(_fileInput, (itms, idx) =>
            {
                var subFileName = Path.Combine(_dirOutput, idx + "_" + _fileOutputName);

                var subCmd = BuildFfmpegCommandTransitionFade(itms, subFileName, timeForEachImage, timeFadeOut);

                subFileList.Add(new FfmpegCommandOutput
                {
                    FfmpegCommand = subCmd,
                    FileOutput = subFileName
                });
            });

            var cmdMain = new FfmpegCommandOutput
            {
                FfmpegCommand = BuildFfmpegConcatVideo(subFileList.Select(i => i.FileOutput).ToList(), _fileOutput),
                FileOutput = _fileOutput,
                SubFileOutput = subFileList
            };

            return cmdMain;
        }

        public string BuildFfmpegCommandTransitionFade(List<string> fileInput, string fileOutput, int timeForEachImage, int timeFadeOut)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            string cmd = $"\"{ffmpegCmd}\"";

            string filterLoopInput = "";
            string filterAfter = "";
            for (int i = 0; i < fileInput.Count; i++)
            {
                string f = fileInput[i];
                cmd += $" -loop 1 -t {timeForEachImage} -i \"{f}\"";

                filterLoopInput += $"[{i}:v]scale={_videoScale}:force_original_aspect_ratio=decrease,pad={_videoScale}:(ow-iw)/2:(oh-ih)/2,setsar=1,fade=t=out:st={timeFadeOut}:d=1[v{i}];";

                filterAfter += $"[v{i}]";
            }


            if (!string.IsNullOrEmpty(_fileAudio))
            {
                cmd += $" -i \"{_fileAudio}\"";
            }
            cmd += $" -filter_complex \"{filterLoopInput}{filterAfter} concat=n={fileInput.Count}:v=1:a=0,format=yuv420p[v]\"";
            cmd += $" -map \"[v]\" -map {fileInput.Count}:a -shortest \"{fileOutput}\"";

            while (cmd.IndexOf("\\") >= 0)
            {
                cmd = cmd.Replace("\\", "/");
            }

            return cmd;
        }

        public string BuildFfmpegConcatVideo(List<string> filesInput, string fileOutput)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            // return $"\"{ffmpegCmd}\" -i \"concat: {string.Join("|", filesInput)}\" -c copy \"{fileOutput}\"";
            var filter = "";

            for (int i = 0; i < filesInput.Count; i++)
            {
                string f = filesInput[i];
                ffmpegCmd += $" -i \"{f}\"";
                filter += $"[{i}:v][{i}:a]";
            }

            ffmpegCmd += $" -filter_complex \"{filter} concat=n={filesInput.Count}:v=1:a=1 [v] [a]\"";
            ffmpegCmd += $" -map \"[v]\" -map \"[a]\" -shortest \"{fileOutput}\"";
            return ffmpegCmd;
        }

        public static void SplitToRun<T>(List<T> allItems, Action<List<T>, int> doBatch, int batchSize = 10)
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
