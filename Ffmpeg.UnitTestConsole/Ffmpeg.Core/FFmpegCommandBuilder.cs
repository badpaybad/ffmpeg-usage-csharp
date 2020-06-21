using Ffmpeg.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ffmpeg.Core
{
    public class FileInput
    {
        public int Order { get; set; }
        public string FullPathFile { get; set; }

        public bool IsVideoOrGif { get; set; }

        public string FileName
        {
            get
            {
                var idx = FullPathFile.LastIndexOf("/");

                return FullPathFile.Substring(idx + 1);
            }
        }
    }
    public class ImageOverlayFileConfig
    {
        public string FullPathFile;
        public int FromSeconds;
        public int Duration;
        public int Y;
        public int X;
        public int Width;
        public int Height;
        public string Scale;
        public bool IsGif = true;
    }

    public class FFmpegCommandBuilder
    {
        static List<string> _xfadeVideoConst = new List<string>
        {
            "fade",
"wipeleft",
"wiperight",
"wipeup",
"wipedown",
"slideleft",
"slideright",
"slideup",
"slidedown"
        };


        static List<string> _xfadeImageConst = new List<string> {
      //  "fade",
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
"vdslice"};

        static List<string> _gifScaleConst = new List<string>
        {
            "1080:720","320:240","160:120","720:680"
        };


        List<FileInput> _fileInput = new List<FileInput>();

        string _fileAudio;
        string _fileOutput;
        decimal _videoDuration;
        string _videoScale = "1080:720";

        string _dirOutput;
        string _fileOutputName;
        private decimal _fadeDuration = 1;

        string _fadeMode;

        string _fps = "fps=fps=24";

        static Random _rnd = new Random();

        List<ImageOverlayFileConfig> _fileGifOverlay = new List<ImageOverlayFileConfig>();
        List<ImageOverlayFileConfig> _fileImageOverlay = new List<ImageOverlayFileConfig>();

        List<ImageOverlayFileConfig> _fileVideo = new List<ImageOverlayFileConfig>();

        public FFmpegCommandBuilder()
        {

        }

        public FFmpegCommandBuilder AddFileInput(params FileInput[] files)
        {
            foreach (var file in files)
            {
                _fileInput.Add(file);
            }

            return this;
        }
        /// <summary>
        /// let empty for random
        /// "fade", "wipeleft", "wiperight", "wipeup", "wipedown", "slideleft", "slideright", "slideup", "slidedown", "circlecrop", "rectcrop", "distance", "fadeblack", "fadewhite", "radial", "smoothleft", "smoothright", "smoothup", "smoothdown", "circleopen", "circleclose", "vertopen", "vertclose", "horzopen", "horzclose", "dissolve", "pixelize", "diagtl", "diagtr", "diagbl", "diagbr", "hlslice", "hrslice", "vuslice", "vdslice"
        /// </summary>
        /// <param name="fadeMode"></param>
        /// <returns></returns>
        public FFmpegCommandBuilder WithFadeTransition(string fadeMode)
        {
            _fadeMode = fadeMode;
            return this;
        }
        public FFmpegCommandBuilder WithFileAudio(string file)
        {
            _fileAudio = file;
            return this;
        }
        public FFmpegCommandBuilder AddGifOverlay(string file, int fromSenconds, int duration = 2, int positionX = 0, int positionY = 0, int width = 0, int height = 0)
        {
            var itm = new ImageOverlayFileConfig
            {
                FullPathFile = file,
                FromSeconds = fromSenconds,
                Duration = duration,
                Height = height,
                Width = width,
                X = positionX,
                Y = positionY,
                IsGif = true
            };

            _fileGifOverlay.Add(itm);
            return this;
        }
        public FFmpegCommandBuilder AddImageOverLay(string file, int fromSenconds, int duration = 2, int positionX = 0, int positionY = 0, int width = 0, int height = 0)
        {
            var itm = new ImageOverlayFileConfig
            {
                FullPathFile = file,
                FromSeconds = fromSenconds,
                Duration = duration,
                Height = height,
                Width = width,
                X = positionX,
                Y = positionY,
                IsGif = false
            };

            _fileImageOverlay.Add(itm);
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

        /// <summary>
        /// should depend on audio length
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
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

        public FfmpegCommandLine ToCommandXfade()
        {
            if (_fileInput == null || _fileInput.Count == 0) throw new Exception("No input file. please call function AddFileInput");
            if (_videoDuration < 1) throw new Exception("Video duration do not valid. please call function WithVideoDurationInSeconds");
            if (_fadeDuration < 0) throw new Exception("Video duration do not valid. please call function WithFadeDurationInSeconds");
            if (string.IsNullOrEmpty(_fileOutput)) throw new Exception("File video outputdo not valid. please call function WithFileOutput");

            _fileInput = _fileInput.Where(i => i.FullPathFile
             .EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) == false).ToList();

            var timeForEachImage = (_videoDuration / _fileInput.Count);

            timeForEachImage = Math.Round(timeForEachImage, 2);

            var fadeDuration = _fadeDuration;
            if (fadeDuration > timeForEachImage)
            {
                fadeDuration = timeForEachImage;
            }
            timeForEachImage = timeForEachImage + fadeDuration;

            List<FfmpegCommandLine> cmdsPrepare = new List<FfmpegCommandLine>();

            #region convert images to videos 1-1
            var fileGifts = _fileInput.Where(i => i.FullPathFile
            .EndsWith(".gif", StringComparison.OrdinalIgnoreCase) == true
             && i.FullPathFile
             .EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) == false
            ).ToList();

            foreach (var gf in fileGifts)
            {
                var fileIn = gf.FullPathFile.Trim();
                var fileOut = fileIn + ".mp4";

                cmdsPrepare.Add(new FfmpegCommandLine
                {
                    GroupOrder = gf.Order,
                    FfmpegCommand = BuildCmdForGiftToVideo(fileIn, fileOut, timeForEachImage),
                    FileOutput = fileOut
                });
            }

            List<FileInput> fileImgs = _fileInput.Where(i => i.FullPathFile
             .EndsWith(".gif", StringComparison.OrdinalIgnoreCase) == false
             && i.FullPathFile
             .EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) == false
            ).ToList();

            foreach (var imgf in fileImgs)
            {
                var fileIn = imgf.FullPathFile.Trim();
                var fileOut = fileIn + ".mp4";

                cmdsPrepare.Add(new FfmpegCommandLine
                {
                    GroupOrder = imgf.Order,
                    FfmpegCommand = BuildCmdForImgToVideo(fileIn, fileOut, timeForEachImage),
                    FileOutput = fileOut
                });
            }
            #endregion;

            var groupOrder = 0;

            List<FfmpegCommandLine> listAllSubOrderedCmd = new List<FfmpegCommandLine>();

            #region combine video one by one
            //line by line merger video into one
            groupOrder = groupOrder + 1;

            string latestFileOutputCombined = Path.Combine(_dirOutput, groupOrder + "v" + +0 + "_" + _fileOutputName);
            var latestTimeVideoDuration = timeForEachImage;

            var subCmd = BuildFfmpegCommandTransitionXFade(new FileInput
            {
                FullPathFile = cmdsPrepare[0].FileOutput,
                IsVideoOrGif = true
            }, new FileInput
            {
                FullPathFile = cmdsPrepare[1].FileOutput,
                IsVideoOrGif = true
            }
                , latestFileOutputCombined, latestTimeVideoDuration, timeForEachImage, fadeDuration
                   , _xfadeImageConst[_rnd.Next(0, _xfadeImageConst.Count - 1)]);

            listAllSubOrderedCmd.Add(new FfmpegCommandLine
            {
                FfmpegCommand = subCmd,
                FileOutput = latestFileOutputCombined,
                GroupOrder = groupOrder
            });

            latestTimeVideoDuration = latestTimeVideoDuration + timeForEachImage - fadeDuration;

            for (var i = 2; i < cmdsPrepare.Count; i++)
            {
                var idx = i - 1;
                groupOrder = groupOrder + 1;

                var fileOutput = Path.Combine(_dirOutput, groupOrder + "v" + idx + "_" + _fileOutputName);

                subCmd = BuildFfmpegCommandTransitionXFade(new FileInput
                {
                    FullPathFile = latestFileOutputCombined,
                    IsVideoOrGif = true
                }, new FileInput
                {
                    FullPathFile = cmdsPrepare[i].FileOutput,
                    IsVideoOrGif = true
                }
                    , fileOutput, latestTimeVideoDuration, timeForEachImage, fadeDuration
                    , _xfadeImageConst[_rnd.Next(0, _xfadeImageConst.Count - 1)]);

                listAllSubOrderedCmd.Add(new FfmpegCommandLine
                {
                    FfmpegCommand = subCmd,
                    FileOutput = fileOutput,
                    GroupOrder = groupOrder
                });

                latestFileOutputCombined = fileOutput;
                latestTimeVideoDuration = latestTimeVideoDuration + timeForEachImage - fadeDuration;
            }

            #endregion

            #region build gif overlay

            if (_fileGifOverlay != null && _fileGifOverlay.Count > 0)
            {

                for (int i = 0; i < _fileGifOverlay.Count; i++)
                {
                    var f = ImageOverlayConfigCal(_fileGifOverlay[i]);

                    groupOrder = groupOrder + 1;

                    var outputFileWithGif = Path.Combine(_dirOutput, groupOrder + "g" + +i + "_" + _fileOutputName);

                    var gifCmd = BuildGiftOverlayCommand(latestFileOutputCombined, outputFileWithGif, f.FullPathFile, f.FromSeconds, f.Duration, f.Scale, f.X, f.Y);

                    listAllSubOrderedCmd.Add(new FfmpegCommandLine
                    {
                        GroupOrder = groupOrder,
                        FileOutput = outputFileWithGif,
                        FfmpegCommand = gifCmd,
                    });

                    latestFileOutputCombined = outputFileWithGif;
                }

            }

            #endregion

            #region build image overlay

            if (_fileImageOverlay != null && _fileImageOverlay.Count > 0)
            {

                for (int i = 0; i < _fileImageOverlay.Count; i++)
                {
                    var f = ImageOverlayConfigCal(_fileImageOverlay[i]);

                    groupOrder = groupOrder + 1;

                    var outputFileWithOverlay = Path.Combine(_dirOutput, groupOrder + "oi" + +i + "_" + _fileOutputName);

                    var overlayCmd = BuildImageOverlayCommand(latestFileOutputCombined, outputFileWithOverlay, f.FullPathFile, _videoDuration, f.FromSeconds, f.Duration, f.Scale, f.X, f.Y);

                    listAllSubOrderedCmd.Add(new FfmpegCommandLine
                    {
                        GroupOrder = groupOrder,
                        FileOutput = outputFileWithOverlay,
                        FfmpegCommand = overlayCmd,
                    });

                    latestFileOutputCombined = outputFileWithOverlay;
                }
            }

            #endregion

            #region build main command

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            var fileAudioSilence = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin/silence.mp3");
            var fileAudio = fileAudioSilence;

            if (!string.IsNullOrEmpty(_fileAudio))
            {
                fileAudio = _fileAudio;
            }

            string cmd = $"\"{ffmpegCmd}\" -y -i {latestFileOutputCombined} -t {_videoDuration} -i {fileAudio} -c copy -shortest {_fileOutput}";

            #endregion

            var cmdMain = new FfmpegCommandLine
            {
                CommandsToBeforeConvert = cmdsPrepare,
                GroupOrder = groupOrder + 1,
                FfmpegCommand = cmd,
                FileOutput = _fileOutput,
                CommandsToConvert = listAllSubOrderedCmd
            };

            return cmdMain;
        }

        /// <summary>
        /// only suport 2 file input
        /// </summary>
        /// <param name="fileInput"></param>
        /// <param name="fileOutput"></param>
        /// <param name="timeOfEachInput"></param>
        /// <param name="fadeDuration"></param>
        /// <param name="fadeMethod"> fade, wipeleft, wiperight, wipeup, wipedown, slideleft, slideright, slideup, slidedown, circlecrop, rectcrop, distance, fadeblack, fadewhite, radial, smoothleft, smoothright, smoothup, smoothdown, circleopen, circleclose, vertopen, vertclose, horzopen, horzclose, dissolve, pixelize, diagtl, diagtr, diagbl, diagbr, hlslice, hrslice, vuslice, vdslice </param>
        /// <returns></returns>
        public string BuildFfmpegCommandTransitionXFade(FileInput fileInput0, FileInput fileInput1, string fileOutput, decimal durationInput0, decimal durationInput1, decimal fadeDuration
            , string fadeMethod)
        {
            //https://trac.ffmpeg.org/wiki/Xfade
            var fileAudioSilence = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin/silence.mp3");

            if (!string.IsNullOrEmpty(_fadeMode))
            {
                fadeMethod = _fadeMode;
            }

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            string cmd = $"\"{ffmpegCmd}\" -y";
            string filterFadeIndex = "";
            string filterScaleImage = "";


            durationInput0 = Math.Round(durationInput0, 2);

            durationInput1 = Math.Round(durationInput1, 2);

            fadeDuration = Math.Round(fadeDuration, 2);

            var offset = durationInput0 - fadeDuration;

            if (fileInput0.IsVideoOrGif == false)
            {
                offset = durationInput0;
                cmd += $" -loop 1 -t {durationInput0 + fadeDuration } -i \"{fileInput0.FullPathFile}\"";
            }
            if (fileInput0.IsVideoOrGif == true)
            {
                offset = durationInput0 - fadeDuration;
                cmd += $" -i \"{fileInput0.FullPathFile}\"";
            }

            offset = Math.Round(offset, 2);

            if (offset <= 0) offset = (decimal)0.1;

            if (fileInput1.IsVideoOrGif == false)
            {
                cmd += $" -loop 1 -t {durationInput1 } -i \"{fileInput1.FullPathFile}\"";
            }
            if (fileInput1.IsVideoOrGif == true)
            {
                cmd += $" -i \"{fileInput1.FullPathFile}\"";
            }
            cmd += $" -t {durationInput0 + durationInput1} -i \"{fileAudioSilence}\"";
            //trick for exactly video duration
            //cmd += $" -t {durationInput0 + durationInput1} -i \"{fileAudioSilence}\"";

            filterScaleImage += $"[0:v]scale={_videoScale}:force_original_aspect_ratio=decrease,pad={_videoScale}:(ow-iw)/2:(oh-ih)/2,setsar=1,{_fps}[v0];";
            filterScaleImage += $"[1:v]scale={_videoScale}:force_original_aspect_ratio=decrease,pad={_videoScale}:(ow-iw)/2:(oh-ih)/2,setsar=1,{_fps}[v1];";

            filterFadeIndex += $"[v0][v1]";

            cmd += $" -filter_complex \"{filterScaleImage}{filterFadeIndex}xfade=transition={fadeMethod}:duration={fadeDuration}:offset={offset},format=yuv420p[v]\"";

            //cmd += $" -map \"[v]\" -map 2:a \"{fileOutput}\"";
            cmd += $" -map \"[v]\" -map 2:a \"{fileOutput}\"";

            while (cmd.IndexOf("\\") >= 0)
            {
                cmd = cmd.Replace("\\", "/");
            }

            return cmd;
        }

        public ImageOverlayFileConfig ImageOverlayConfigCal(ImageOverlayFileConfig f)
        {
            var arr = _videoScale.Split(':');

            var w = int.Parse(arr[0]);
            var h = int.Parse(arr[1]);
            var temp = new ImageOverlayFileConfig();
            temp.FullPathFile = f.FullPathFile;
            temp.FromSeconds = f.FromSeconds;
            temp.Duration = f.Duration;

            if (f.X == 0 || f.Y == 0)
            {
                temp.X = _rnd.Next(0, w / 2);
                temp.Y = _rnd.Next(0, h / 2);
            }
            else
            {
                temp.Y = f.Y;
                temp.X = f.X;
            }

            temp.Width = f.Width;
            temp.Height = f.Width;

            if (f.Width == 0 && f.Height == 0)
            {
                temp.Scale = _gifScaleConst[_rnd.Next(0, _gifScaleConst.Count - 1)];
            }
            else
            {
                temp.Scale = "320:240";

                if (f.Width == 0 && f.Height != 0)
                {
                    temp.Width = temp.Height * w / h;
                }
                if (f.Width != 0 && f.Height == 0)
                {
                    temp.Height = temp.Width * h / w;
                }

                temp.Scale = $"{temp.Width}:{temp.Height}";
            }

            if (temp.Scale == _videoScale)
            {
                temp.X = 0;
                temp.Y = 0;
            }

            return temp;
        }

        public string BuildGiftOverlayCommand(string fileInput, string fileOutput, string fileGiftOverlay, decimal fromSeconds, decimal duration, string scale, decimal x, decimal y)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            var displayBetween = $":enable='between(t, {fromSeconds}, {fromSeconds + duration})";
            var loop = string.Empty;

            if (duration == 0)
            {
                displayBetween = string.Empty;
            }

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            string cmd = $"\"{ffmpegCmd}\" -y -i \"{fileInput}\" -loop {duration} -i \"{fileGiftOverlay}\" -filter_complex \"[1:v]{_fps},scale={scale},setsar=1[ovrl];[0:v][ovrl]overlay = {x}:{y}{displayBetween}'\" -shortest \"{fileOutput}\"";
            //addOption(['-ignore_loop 0', '-i '+wmimage+ '','-filter_complex [0:v][1:v]overlay=10:10:shortest=1:enable="between(t,2,5)"'])

            return cmd;
        }

        public string BuildVideoOverlayCommand(string fileInput, string fileOutput, string fileGiftOverlay, decimal fromSeconds, decimal duration, string scale, decimal x, decimal y)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            var displayBetween = $":enable='between(t, {fromSeconds}, {fromSeconds + duration})";
            var loop = string.Empty;

            if (duration == 0)
            {
                displayBetween = string.Empty;
            }

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            string cmd = $"\"{ffmpegCmd}\" -y -i \"{fileInput}\" -stream_loop {duration} -i \"{fileGiftOverlay}\" -filter_complex \"[1:v]{_fps},scale={scale},setsar=1[ovrl];[0:v][ovrl]overlay = {x}:{y}{displayBetween}'\" -shortest \"{fileOutput}\"";
            //addOption(['-ignore_loop 0', '-i '+wmimage+ '','-filter_complex [0:v][1:v]overlay=10:10:shortest=1:enable="between(t,2,5)"'])

            return cmd;
        }

        public string BuildImageOverlayCommand(string fileInput, string fileOutput, string fileImageOverlay, decimal videoDuration, decimal fromSeconds, decimal duration, string scale, decimal x, decimal y)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            string cmd = $"\"{ffmpegCmd}\" -y -i \"{fileInput}\" -loop 1 -t {videoDuration} -i \"{fileImageOverlay}\" -filter_complex \"[1:v]{_fps},scale={scale},setsar=1[ovrl];[0:v][ovrl]overlay = {x}:{y}:enable='between(t, {fromSeconds}, {fromSeconds + duration})'\" \"{fileOutput}\"";
            //addOption(['-ignore_loop 0', '-i '+wmimage+ '','-filter_complex [0:v][1:v]overlay=10:10:shortest=1:enable="between(t,2,5)"'])

            return cmd;
        }
        public string BuildCmdForGiftToVideo(string fileInput, string fileOutput, decimal duration)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            var fileAudioSilence = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin/silence.mp3");

            var filterScaleImage = $"[0:v]scale={_videoScale}:force_original_aspect_ratio=decrease,pad={_videoScale}:(ow-iw)/2:(oh-ih)/2,setsar=1,{_fps}[v0]";

            string cmd = $"\"{ffmpegCmd}\" -y -stream_loop {duration} -i \"{fileInput}\" -t {duration} -i \"{fileAudioSilence}\" -filter_complex \"{filterScaleImage};[v0]format=yuv420p[v]\" -map \"[v]\"  -map 1:a -shortest \"{fileOutput}\"";

            return cmd;
        }

        public string BuildCmdForImgToVideo(string fileInput, string fileOutput, decimal duration)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            var loopInput = $" -loop 1 -t {duration} -i \"{fileInput}\"";

            var fileAudioSilence = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin/silence.mp3");

            var filterScaleImage = $"[0:v]scale={_videoScale}:force_original_aspect_ratio=decrease,pad={_videoScale}:(ow-iw)/2:(oh-ih)/2,setsar=1,{_fps}[v0]";

            string cmd = $"\"{ffmpegCmd}\" -y {loopInput} -t {duration} -i \"{fileAudioSilence}\" -filter_complex \"{filterScaleImage};[v0]format=yuv420p[v]\" -map \"[v]\" -map 1:a \"{fileOutput}\"";

            return cmd;
        }

        public string BuildTextOverlayCommand(string fileInput, string fileOutput, string text, decimal x, decimal y, string pathfont, int fontSize, string fontColor, int allowBg = 1, string bgColor = "black")
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            pathfont = pathfont.Replace("\\", "/");

            var arrLineText = text.Split('\n');

            var line = "";
            for (int i = 0; i < arrLineText.Length; i++)
            {
                string l = arrLineText[i].Trim(new[] { ' ', '\r', '\n' });

                var lineSpace = i * (fontSize / 3 + fontSize);

                line += $"drawtext=fontfile=\'{pathfont}\':text='{l}':fontcolor={fontColor}:fontsize={fontSize}:box={allowBg}:boxcolor={bgColor}@0.5:boxborderw=5: x=(w-text_w)/2: y={lineSpace}+(h-text_h)/2,";
            }
            line = line.Trim(',', ' ', '\r', '\n');
            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");
            string cmd = $"\"{ffmpegCmd}\" -y -i \"{fileInput}\" -vf \"[in]{line}[out]\" -codec:a copy \"{fileOutput}\"";

            return cmd;
        }

        public string BuildAddAudioCommand(string fileInput, string fileOutput, string fileAudio, decimal videoDuration)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            var fileAudioSilence = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin/silence.mp3");

            if (string.IsNullOrEmpty(fileAudio))
            {
                fileAudio = fileAudioSilence;
            }

            string cmd = $"\"{ffmpegCmd}\" -y -i {fileInput} -t {videoDuration} -i {fileAudio} -c copy -shortest {fileOutput}";

            return cmd;
        }

        public string BuildAudioInSpecificTime(string fileInput, string fileOutput, string fileAudio, decimal offset)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");
            string cmd = $"\"{ffmpegCmd}\" -y -i {fileInput} -itsoffset {offset} -i {fileAudio} -map 0:0 -map 1:0 -c copy {fileOutput}";

            return cmd;
            //ffmpeg - y - i a.mp4 - itsoffset 00:00:30 - i sng.m4a - map 0:0 - map 1:0 - c:v copy -preset ultrafast - async 1 out.mp4
        }

        public void SplitToRun<T>(List<T> allItems, Action<List<T>, int> doBatch, int batchSize = 2)
        {
            allItems.SplitToRun(batchSize, doBatch);
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

//[Obsolete("Please use new function ToCommandXfade ")]
//public FfmpegCommandLine ToCommand()
//{
//    if (_fileInput == null || _fileInput.Count == 0) throw new Exception("No input file. please call function AddFileInput");
//    if (_videoDuration < 1) throw new Exception("Video duration do not valid. please call function WithVideoDurationInSeconds");
//    if (_fadeDuration < 0) throw new Exception("Video duration do not valid. please call function WithFadeDurationInSeconds");
//    if (string.IsNullOrEmpty(_fileOutput)) throw new Exception("File video outputdo not valid. please call function WithFileOutput");

//    List<FfmpegCommandLine> listOf2ImageTo1Video = new List<FfmpegCommandLine>();

//    if (_fileInput.Count % 2 != 0 || _fileInput.Count == 1)
//    {
//        _fileInput.Add(_fileInput[0]);
//    }

//    var timeForEachImage = _videoDuration / _fileInput.Count;

//    var fadeDuration = _fadeDuration;
//    if (fadeDuration > timeForEachImage)
//    {
//        fadeDuration = timeForEachImage;
//    }

//    SplitToRun(_fileInput, (itms, idx) =>
//    {
//        if (itms.Count == 1) { itms.Add(itms[0]); }

//        var twoImgTo1Video = Path.Combine(_dirOutput, idx + "_" + _fileOutputName);

//        var subCmd = BuildFfmpegCommandTransitionXFade(itms[0], itms[1], twoImgTo1Video, timeForEachImage, fadeDuration, _xfadeImageConst[_rnd.Next(0, _xfadeImageConst.Count - 1)], true);

//        listOf2ImageTo1Video.Add(new FfmpegCommandLine
//        {
//            FfmpegCommand = subCmd,
//            FileOutput = twoImgTo1Video
//        });
//    });

//    string mergerVideoCmd = BuildFfmpegConcatVideo(listOf2ImageTo1Video.Select(i => i.FileOutput).ToList(), _fileOutput, timeForEachImage * 2, fadeDuration, true);

//    if (mergerVideoCmd.Length > 8000)
//    {
//        //todo: should do more smart , cause commandline limited about 8000 char

//        var list10VideoTo1Video = listOf2ImageTo1Video.Select(i => i.FileOutput).ToList();
//        List<FfmpegCommandLine> listSubConcat = new List<FfmpegCommandLine>();

//        SplitToRun(list10VideoTo1Video, (itms, idx) =>
//        {
//            var concatFile = Path.Combine(_dirOutput, idx + "_c_" + _fileOutputName);

//            var concatVideoCmd = BuildFfmpegConcatVideo(itms, concatFile, timeForEachImage * 2 * 10, fadeDuration, false);

//            listSubConcat.Add(new FfmpegCommandLine
//            {
//                FfmpegCommand = concatVideoCmd,
//                FileOutput = concatFile
//            });

//        }, 10);

//        mergerVideoCmd = BuildFfmpegConcatVideo(listSubConcat.Select(i => i.FileOutput).ToList(), _fileOutput, timeForEachImage * 2 * 10, fadeDuration, true);

//        listOf2ImageTo1Video.AddRange(listSubConcat);
//    }

//    var cmdMain = new FfmpegCommandLine
//    {
//        FfmpegCommand = mergerVideoCmd,
//        FileOutput = _fileOutput,
//        SubFileOutput = listOf2ImageTo1Video
//    };

//    return cmdMain;

//}

//string BuildFfmpegConcatVideo(List<string> filesInput, string fileOutput, decimal timeForEachInput, decimal fadeDuration
//    , bool allowAddAudio)
//{

//    timeForEachInput = Math.Round(timeForEachInput, 1);
//    fadeDuration = Math.Round(fadeDuration, 1);

//    var timeFadeOut = timeForEachInput - fadeDuration;
//    string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

//    string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");


//    string cmd = $"\"{ffmpegCmd}\" -y";

//    var filterFadeVideo = "";
//    var filterIndex = "";

//    for (int i = 0; i < filesInput.Count; i++)
//    {
//        string f = filesInput[i];
//        cmd += $" -i \"{f}\"";
//        filterFadeVideo += $"[{i}:v]scale={_videoScale}:force_original_aspect_ratio=decrease,pad={_videoScale}:(ow-iw)/2:(oh-ih)/2,setsar=1,fade=t=in:st=0:d={fadeDuration},fade=t=out:st={timeFadeOut}:d={fadeDuration}[v{i}];";
//        filterIndex += $"[v{i}]";
//    }
//    if (allowAddAudio && !string.IsNullOrEmpty(_fileAudio))
//    {
//        cmd += $" -i \"{_fileAudio}\"";
//    }

//    cmd += $" -filter_complex \"{filterFadeVideo}{filterIndex} concat=n={filesInput.Count}:v=1:a=0,format=yuv420p[v]\"";
//    if (allowAddAudio && !string.IsNullOrEmpty(_fileAudio))
//    {
//        cmd += $" -map \"[v]\" -map {filesInput.Count}:a -shortest \"{fileOutput}\"";
//    }
//    else
//    {
//        cmd += $" -map \"[v]\" -shortest \"{fileOutput}\"";
//    }

//    return cmd;
//}
