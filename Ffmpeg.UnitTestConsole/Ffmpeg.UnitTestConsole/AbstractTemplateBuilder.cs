using Ffmpeg.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ffmpeg.UnitTestConsole
{
    public class AbstractTemplateBuilder
    {
        protected List<FfmpegConvertedResult> _result = new List<FfmpegConvertedResult>();

        public FfmpegConvertedResult Run()
        {
            InternalRun();

            if (_result.Count == 0) return null;

            var latestCmd = _result.LastOrDefault();

            var addLogo = new FfmpegCommander().WithOutDuration(latestCmd.CommadExecuted.Duration)
        .WithInputFile(latestCmd.CommadExecuted.FileOutput, latestCmd.CommadExecuted.Duration)
        .WithFileOverlay(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img/logomid.png"), 0, latestCmd.CommadExecuted.Duration, "228:114", 20, 20)
        .ToCommand();

            _result.Add(addLogo.Run());

            var totalTime = _result.Select(i => i.ConvertInMiliseconds).Sum();

            CleanUp();

            latestCmd.ConvertInMiliseconds = totalTime;

            Console.WriteLine($"Converted in {totalTime} miliseconds ({totalTime/1000} seconds)");

            return latestCmd;
        }

        protected FfmpegConvertedResult Exec(FfmpegCommandLine cmd)
        {
            var r = cmd.Run();
            _result.Add(r);
            return r;
        }

        public void CleanUp()
        {
            Task.Run(() =>
            {
                for (int i1 = 0; i1 < _result.Count - 1; i1++)
                {
                    try
                    {
                        FfmpegConvertedResult i = _result[i1];
                        File.Delete(i.CommadExecuted.FileOutput);
                    }
                    catch { }

                }
            });
        }
        protected virtual void InternalRun() { }

        public string GetCommands()
        {
            return string.Join("\r\n\r\n", _result.Select(i => i.FfmpegCmd));
        }
    }
}
