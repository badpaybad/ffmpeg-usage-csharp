using Ffmpeg.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ffmpeg.Core
{
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

        public class GifFileConfig
        {
            public string FileGif;
            public int FromSeconds;
            public int Duration;
            public int Y;
            public int X;
            public int Width;
            public int Height;
            public string Scale;

        }

        List<string> _fileInput = new List<string>();

        string _fileAudio;
        string _fileOutput;
        decimal _videoDuration;
        string _videoScale = "1080:720";

        string _dirOutput;
        string _fileOutputName;
        private decimal _fadeDuration = 1;

        string _fadeMode;

        static Random _rnd = new Random();

        List<GifFileConfig> _fileGif = new List<GifFileConfig>();

        public FFmpegCommandBuilder AddFileInput(params string[] files)
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
        public FFmpegCommandBuilder AddFileGif(string file, int fromSenconds, int duration = 2, int positionX = 0, int positionY = 0, int width = 0, int height = 0)
        {
            var itm = new GifFileConfig
            {
                FileGif = file,
                FromSeconds = fromSenconds,
                Duration = duration,
                Height = height,
                Width = width,
                X = positionX,
                Y = positionY
            };

            _fileGif.Add(itm);
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

            if (_fileInput.Count % 2 != 0 || _fileInput.Count == 1)
            {
                _fileInput.Add(_fileInput[0]);
            }

            var timeForEachImage = _videoDuration / _fileInput.Count;

            var fadeDuration = _fadeDuration;
            if (fadeDuration > timeForEachImage)
            {
                fadeDuration = timeForEachImage;
            }

            List<FfmpegCommandLine> list2ImageTo1Video = new List<FfmpegCommandLine>();

            var groupOrder = 0;

            SplitToRun(_fileInput, (itms, idx) =>
            {
                var twoImgTo1Video = Path.Combine(_dirOutput, groupOrder + "img" + idx + "_" + _fileOutputName);

                var subCmd = BuildFfmpegCommandTransitionXFade(itms[0], itms[1], twoImgTo1Video, timeForEachImage, fadeDuration
                    , _xfadeImageConst[_rnd.Next(0, _xfadeImageConst.Count - 1)], true);

                list2ImageTo1Video.Add(new FfmpegCommandLine
                {
                    FfmpegCommand = subCmd,
                    FileOutput = twoImgTo1Video,
                    GroupOrder = groupOrder
                });

            }, 2);

            List<FfmpegCommandLine> listAllSubOrderedCmd = new List<FfmpegCommandLine>();

            listAllSubOrderedCmd.AddRange(list2ImageTo1Video);

            //line by line merger video into one
            groupOrder = groupOrder + 1;

            string latestFileOutputCombined = Path.Combine(_dirOutput, groupOrder + "v" + 0 + "_" + _fileOutputName);

            var subCmd = BuildFfmpegCommandTransitionXFade(list2ImageTo1Video[0].FileOutput, list2ImageTo1Video[1].FileOutput
                , latestFileOutputCombined, timeForEachImage * 2, fadeDuration
                   , _xfadeImageConst[_rnd.Next(0, _xfadeImageConst.Count - 1)], false);

            listAllSubOrderedCmd.Add(new FfmpegCommandLine
            {
                FfmpegCommand = subCmd,
                FileOutput = latestFileOutputCombined,
                GroupOrder = groupOrder
            });

            var latestTimeVideoDuration = (timeForEachImage * 2) + (timeForEachImage * 2);

            for (var i = 2; i < list2ImageTo1Video.Count; i++)
            {
                var idx = i - 1;

                var fileOutput = Path.Combine(_dirOutput, groupOrder + "v" + idx + "_" + _fileOutputName);

                subCmd = BuildFfmpegCommandTransitionXFade(latestFileOutputCombined, list2ImageTo1Video[i].FileOutput
                    , fileOutput, latestTimeVideoDuration, fadeDuration
                    , _xfadeImageConst[_rnd.Next(0, _xfadeImageConst.Count - 1)], false);

                listAllSubOrderedCmd.Add(new FfmpegCommandLine
                {
                    FfmpegCommand = subCmd,
                    FileOutput = fileOutput,
                    GroupOrder = groupOrder + idx
                });

                latestFileOutputCombined = fileOutput;
                latestTimeVideoDuration = latestTimeVideoDuration + (timeForEachImage * 2);
            }

            #region build gif additional

            if (_fileGif != null && _fileGif.Count > 0)
            {
                groupOrder = groupOrder + 1;

                for (int i = 0; i < _fileGif.Count; i++)
                {
                    var f = GifFileConfigCal(_fileGif[i]);

                    var outputFileWithGif = Path.Combine(_dirOutput, groupOrder + "g" + i + "_" + _fileOutputName);

                    var gifCmd = BuildGiftOverlayCommand(latestFileOutputCombined, outputFileWithGif, f.FileGif, f.FromSeconds, f.Duration, f.Scale, f.X, f.Y);

                    listAllSubOrderedCmd.Add(new FfmpegCommandLine
                    {
                        GroupOrder = groupOrder + i,
                        FileOutput = outputFileWithGif,
                        FfmpegCommand = gifCmd,
                    });

                    latestFileOutputCombined = outputFileWithGif;
                }

            }

            #endregion

            #region build main command

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            string cmd = $"\"{ffmpegCmd}\" -y -i {latestFileOutputCombined} -i {_fileAudio} -c copy -shortest {_fileOutput}";

            if (string.IsNullOrEmpty(_fileAudio))
            {
                cmd = $"\"{ffmpegCmd}\" -y -i {latestFileOutputCombined} -c copy -shortest {_fileOutput}";
            }

            #endregion

            var cmdMain = new FfmpegCommandLine
            {
                GroupOrder = groupOrder + 1,
                FfmpegCommand = cmd,
                FileOutput = _fileOutput,
                SubFileOutput = listAllSubOrderedCmd
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
        /// <param name="fadeMethod"></param>
        /// <returns></returns>
        string BuildFfmpegCommandTransitionXFade(string fileInput0, string fileInput1, string fileOutput, decimal timeOfEachInput, decimal fadeDuration
           , string fadeMethod, bool isImage)
        {
            //https://trac.ffmpeg.org/wiki/Xfade

            timeOfEachInput = Math.Round(timeOfEachInput, 2);
            fadeDuration = Math.Round(fadeDuration, 2);

            if (!string.IsNullOrEmpty(_fadeMode))
            {
                fadeMethod = _fadeMode;
            }

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            string cmd = $"\"{ffmpegCmd}\" -y";
            string filterFadeIndex = "";
            string filterScaleImage = "";
            var offset = timeOfEachInput - fadeDuration;

            if (isImage)
            {
                offset = timeOfEachInput;
                cmd += $" -loop 1 -t {timeOfEachInput + fadeDuration} -i \"{fileInput0}\"";
                cmd += $" -loop 1 -t {timeOfEachInput} -i \"{fileInput1}\"";
            }
            else
            {
                offset = timeOfEachInput - fadeDuration;
                cmd += $" -i \"{fileInput0}\"";
                cmd += $" -i \"{fileInput1}\"";

                if (!string.IsNullOrEmpty(_fileAudio))
                {
                    //cmd += $" -t {timeOfEachInput*2} -i \"{_fileAudio}\"";
                }
            }

            filterScaleImage += $"[0:v]scale={_videoScale}:force_original_aspect_ratio=decrease,pad={_videoScale}:(ow-iw)/2:(oh-ih)/2,setsar=1[v0];";
            filterScaleImage += $"[1:v]scale={_videoScale}:force_original_aspect_ratio=decrease,pad={_videoScale}:(ow-iw)/2:(oh-ih)/2,setsar=1[v1];";

            filterFadeIndex += $"[v0][v1]";

            cmd += $" -filter_complex \"{filterScaleImage}{filterFadeIndex}xfade=transition={fadeMethod}:duration={fadeDuration}:offset={offset},format=yuv420p[v]\"";
            if (isImage)
            {
                cmd += $" -map \"[v]\" \"{fileOutput}\"";
            }
            else
            {
                //cmd += $" -map \"[v]\" -map 2:a  \"{fileOutput}\"";
                cmd += $" -map \"[v]\" \"{fileOutput}\"";
            }

            while (cmd.IndexOf("\\") >= 0)
            {
                cmd = cmd.Replace("\\", "/");
            }

            return cmd;
        }

        public GifFileConfig GifFileConfigCal(GifFileConfig f)
        {
            var arr = _videoScale.Split(':');

            var w = int.Parse(arr[0]);
            var h = int.Parse(arr[1]);
            var temp = new GifFileConfig();
            temp.FileGif = f.FileGif;
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



        string BuildGiftOverlayCommand(string fileInput, string fileOutput, string fileGift, int fromSeconds, int duration, string scale, int x, int y)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            string cmd = $"\"{ffmpegCmd}\" -y -i \"{fileInput}\" -i \"{fileGift}\" -filter_complex \"[1:v]scale={scale},setsar=1,fade=t=in:st=0:d=1[ovrl];[0:v][ovrl]overlay = {x}:{y}:enable='between(t, {fromSeconds}, {fromSeconds + duration})'\" \"{fileOutput}\"";
            //addOption(['-ignore_loop 0', '-i '+wmimage+ '','-filter_complex [0:v][1:v]overlay=10:10:shortest=1:enable="between(t,2,5)"'])

            return cmd;
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
