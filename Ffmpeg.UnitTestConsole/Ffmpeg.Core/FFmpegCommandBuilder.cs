using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Ffmpeg.Core
{
    public class FFmpegCommandBuilder
    {
        List<string> _fileInput = new List<string>();

        string _fileAudio;
        string _fileOutput;
        int _duration;
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
            return this;
        }
        public FFmpegCommandBuilder WithVideoDuration(int duration)
        {
            _duration = duration;
            return this;
        }
        public string ToCommand()
        {
            var timeForEachImage = Math.Round((decimal)_duration / _fileInput.Count, 0);
            if (timeForEachImage <= 0) timeForEachImage = 1;

            var timeFadeOut = timeForEachImage - 1;
            if (timeFadeOut <= 0) timeFadeOut = 1;

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");
            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            string cmd = $"\"{ffmpegCmd}\"";

            string filterLoopInput = "";
            string filterAfter = "";
            for (int i = 0; i < _fileInput.Count; i++)
            {
                string f = _fileInput[i];
                cmd += $" -loop 1 -t {timeForEachImage} -i \"{f}\"";

                filterLoopInput += $"[{i}:v]scale = 1280:720:force_original_aspect_ratio = decrease,pad = 1280:720:(ow - iw) / 2:(oh - ih) / 2,setsar = 1,fade = t =out:st = {timeFadeOut}:d = 1[v{i}];";

                filterAfter += $"[v{i}]";
            }


            if (!string.IsNullOrEmpty(_fileAudio))
            {
                cmd += $" -i \"{_fileAudio}\"";
            }
            cmd += $" -filter_complex \"{filterLoopInput}{filterAfter}concat = n = {_fileInput.Count}:v = 1:a = 0,format = yuv420p[v]\"";
            cmd += $" -map \"[v]\" -map {_fileInput.Count}:a -veryfast \"{_fileOutput}\"";

            while (cmd.IndexOf("\\") >= 0)
            {
                cmd = cmd.Replace("\\", "/");
            }

            try {
                File.Delete(_fileOutput);
            } catch { }

            return cmd;
        }
    }
}
