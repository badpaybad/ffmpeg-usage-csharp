﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ffmpeg.Core
{
    public class FFmpegCommandBuilder
    {
        List<string> _xfade = new List<string> {
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

        int _audioLength;

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

            var timeForEachImage = _videoDuration / _fileInput.Count;

            var fadeDuration = _fadeDuration;
            if (fadeDuration > timeForEachImage)
            {
                fadeDuration = timeForEachImage;
            }
            if (_fileInput.Count % 2 != 0 || _fileInput.Count == 1)
            {
                _fileInput.Add(_fileInput[0]);
            }


            List<FfmpegCommandLine> listSubCommand = new List<FfmpegCommandLine>();

            var counter = 1;

            List<string> listFileInput = _fileInput.ToList();

            string latestFileCombined = "";
            decimal latestTimeForEacheImage = 0;

            while (true)
            {
                List<string> listFileInputNext = new List<string>();

                var isImage = counter == 1;
                var nextTimeForEacheImage = timeForEachImage * (decimal)Math.Pow(2, counter);

                SplitToRun(listFileInput, (itms, idx) =>
                {
                    var twoImgTo1Video = Path.Combine(_dirOutput, counter + "_" + idx + "_" + _fileOutputName);

                    var subCmd = BuildFfmpegCommandImageTransitionXFade(itms, twoImgTo1Video, nextTimeForEacheImage, fadeDuration, _xfade[_rnd.Next(0, _xfade.Count - 1)], isImage);

                    listSubCommand.Add(new FfmpegCommandLine
                    {
                        FfmpegCommand = subCmd,
                        FileOutput = twoImgTo1Video
                    });

                    listFileInputNext.Add(twoImgTo1Video);
                }, 2);


                if (listFileInputNext.Count == 1)
                {
                    latestFileCombined = listFileInputNext[0];
                    latestTimeForEacheImage = nextTimeForEacheImage;
                    break;
                }

                listFileInput = listFileInputNext;
                counter++;
            }

            string mergerAudio = BuildFfmpegConcatVideo(new List<string> { latestFileCombined }
            , _fileOutput, latestTimeForEacheImage, fadeDuration, true);

            var cmdMain = new FfmpegCommandLine
            {
                FfmpegCommand = mergerAudio,
                FileOutput = _fileOutput,
                SubFileOutput = listSubCommand
            };

            return cmdMain;
        }



        public FfmpegCommandLine ToCommand()
        {
            if (_fileInput == null || _fileInput.Count == 0) throw new Exception("No input file. please call function AddFileInput");
            if (_videoDuration < 1) throw new Exception("Video duration do not valid. please call function WithVideoDurationInSeconds");
            if (_fadeDuration < 0) throw new Exception("Video duration do not valid. please call function WithFadeDurationInSeconds");
            if (string.IsNullOrEmpty(_fileOutput)) throw new Exception("File video outputdo not valid. please call function WithFileOutput");

            var timeForEachImage = _videoDuration / _fileInput.Count;

            var fadeDuration = _fadeDuration;
            if (fadeDuration > timeForEachImage)
            {
                fadeDuration = timeForEachImage;
            }

            List<FfmpegCommandLine> listOf2ImageTo1Video = new List<FfmpegCommandLine>();

            if (_fileInput.Count % 2 != 0 || _fileInput.Count == 1)
            {
                _fileInput.Add(_fileInput[0]);
            }

            SplitToRun(_fileInput, (itms, idx) =>
            {
                var twoImgTo1Video = Path.Combine(_dirOutput, idx + "_" + _fileOutputName);

                var subCmd = BuildFfmpegCommandImageTransitionXFade(itms, twoImgTo1Video, timeForEachImage, fadeDuration, _xfade[_rnd.Next(0, _xfade.Count - 1)], true);

                listOf2ImageTo1Video.Add(new FfmpegCommandLine
                {
                    FfmpegCommand = subCmd,
                    FileOutput = twoImgTo1Video
                });
            });

            string mergerVideoCmd = BuildFfmpegConcatVideo(listOf2ImageTo1Video.Select(i => i.FileOutput).ToList(), _fileOutput, timeForEachImage * 2, fadeDuration, true);

            if (mergerVideoCmd.Length > 8000)
            {
                //todo: should do more smart , cause commandline limited about 8000 char

                var list10VideoTo1Video = listOf2ImageTo1Video.Select(i => i.FileOutput).ToList();
                List<FfmpegCommandLine> listSubConcat = new List<FfmpegCommandLine>();

                SplitToRun(list10VideoTo1Video, (itms, idx) =>
                {
                    var concatFile = Path.Combine(_dirOutput, idx + "_c_" + _fileOutputName);

                    var concatVideoCmd = BuildFfmpegConcatVideo(itms, concatFile, timeForEachImage * 2 * 10, fadeDuration, false);

                    listSubConcat.Add(new FfmpegCommandLine
                    {
                        FfmpegCommand = concatVideoCmd,
                        FileOutput = concatFile
                    });

                }, 10);

                mergerVideoCmd = BuildFfmpegConcatVideo(listSubConcat.Select(i => i.FileOutput).ToList(), _fileOutput, timeForEachImage * 2 * 10, fadeDuration, true);

                listOf2ImageTo1Video.AddRange(listSubConcat);
            }

            var cmdMain = new FfmpegCommandLine
            {
                FfmpegCommand = mergerVideoCmd,
                FileOutput = _fileOutput,
                SubFileOutput = listOf2ImageTo1Video
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
           , string fadeMethod, bool isImage)
        {
            //https://trac.ffmpeg.org/wiki/Xfade

            timeForEachImage = Math.Round(timeForEachImage, 1);
            fadeDuration = Math.Round(fadeDuration, 1);

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            string cmd = $"\"{ffmpegCmd}\"";
            string filterFadeIndex = "";
            string filterScaleImage = "";
            var offset = timeForEachImage - fadeDuration;
            if (offset < 0) offset = 0;

            for (int i = 0; i < fileInput.Count; i++)
            {
                string f = fileInput[i];
                if (isImage)
                {
                    cmd += $" -loop 1 -t {timeForEachImage} -i \"{f}\"";
                }
                else
                {
                    cmd += $" -i \"{f}\"";
                }

                filterScaleImage += $"[{i}:v]scale={_videoScale}:force_original_aspect_ratio=decrease,pad={_videoScale}:(ow-iw)/2:(oh-ih)/2,setsar=1[v{i}];";

                filterFadeIndex += $"[v{i}]";
            }

            cmd += $" -filter_complex \"{filterScaleImage}{filterFadeIndex}xfade=transition={fadeMethod}:duration={fadeDuration}:offset={offset},format=yuv420p[v]\"";
            cmd += $" -map \"[v]\" \"{fileOutput}\"";

            while (cmd.IndexOf("\\") >= 0)
            {
                cmd = cmd.Replace("\\", "/");
            }

            return cmd;
        }

        public string BuildFfmpegConcatVideo(List<string> filesInput, string fileOutput, decimal timeForEachInput, decimal fadeDuration
            , bool allowAddAudio)
        {

            timeForEachInput = Math.Round(timeForEachInput, 1);
            fadeDuration = Math.Round(fadeDuration, 1);

            var timeFadeOut = timeForEachInput - fadeDuration;
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");


            string cmd = $"\"{ffmpegCmd}\"";

            var filterFadeVideo = "";
            var filterIndex = "";

            for (int i = 0; i < filesInput.Count; i++)
            {
                string f = filesInput[i];
                cmd += $" -i \"{f}\"";
                filterFadeVideo += $"[{i}:v]scale={_videoScale}:force_original_aspect_ratio=decrease,pad={_videoScale}:(ow-iw)/2:(oh-ih)/2,setsar=1,fade=t=in:st=0:d={fadeDuration},fade=t=out:st={timeFadeOut}:d={fadeDuration}[v{i}];";
                filterIndex += $"[v{i}]";
            }
            if (allowAddAudio && !string.IsNullOrEmpty(_fileAudio))
            {
                cmd += $" -i \"{_fileAudio}\"";
            }

            cmd += $" -filter_complex \"{filterFadeVideo}{filterIndex} concat=n={filesInput.Count}:v=1:a=0,format=yuv420p[v]\"";
            if (allowAddAudio && !string.IsNullOrEmpty(_fileAudio))
            {
                cmd += $" -map \"[v]\" -map {filesInput.Count}:a -shortest \"{fileOutput}\"";
            }
            else
            {
                cmd += $" -map \"[v]\" -shortest \"{fileOutput}\"";
            }

            return cmd;
        }


        string BuildGiftOverlayCommand(string fileGift, int fromSeconds)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            var frame = fromSeconds * 24;

            var fileVideo = Path.Combine(_dirOutput, "g_" + _fileOutputName);

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");

            string cmd = $"\"{ffmpegCmd}\" -y -i \"{fileVideo}\" -ignore_loop 0 -i \"{fileGift}\" -filter_complex \"[1:v]scale = {_videoScale}[ovrl];[0:v][ovrl]overlay = 0:0\" -frames:v {frame} -codec:a copy -codec:v libx264 -max_muxing_queue_size 2048 \"{_fileOutput}\"";

            return cmd;
        }

        public void SplitToRun<T>(List<T> allItems, Action<List<T>, int> doBatch, int batchSize = 2)
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
