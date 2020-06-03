using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ffmpeg.Core
{

    public class FfmpegCommandExecuter
    {
        static FfmpegCommandExecuter()
        {
            //var ping = new FfmpegCommandRunner().InternalRun($"\"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin")}/ffmpeg.exe\" -version" ,"ffmpeg -version");

            //Console.WriteLine(ping.Output);
        }

        public FfmpegCommandResult Run(FfmpegCommandOutput cmd)
        {
            List<FfmpegCommandResult> subResult = new List<FfmpegCommandResult>();

            if (cmd.SubFileOutput != null && cmd.SubFileOutput.Count > 0)
            {
                foreach (var subCmd in cmd.SubFileOutput)
                {
                    subResult.Add(InternalRun(subCmd.FfmpegCommand, subCmd.FileOutput));
                }
            }

            var mainCmdResult = InternalRun(cmd.FfmpegCommand, cmd.FileOutput);

            mainCmdResult.SubResult = subResult;

            return mainCmdResult;
        }
        //public FfmpegCommandResult RunCmd(string cmdLine, string fileOutput)
        //{
        //    return InternalRun(cmdLine, fileOutput);
        //}

        private FfmpegCommandResult InternalRun(string cmdLine, string fileOutput)
        {
            try
            {
                File.Delete(fileOutput);
            }
            catch { }

            Stopwatch sw = Stopwatch.StartNew();
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            System.Diagnostics.Process cmd = new System.Diagnostics.Process();

            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            cmd.StartInfo.WorkingDirectory = dir;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.Start();

            Console.WriteLine("FfmpegCommandRunner Started");

            cmd.StandardInput.WriteLine(cmdLine);
            cmd.StandardInput.WriteLine("echo ##done##");

            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();

            var output = new List<string>();

            cmd.OutputDataReceived += new DataReceivedEventHandler(
                (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) output.Add(e.Data);
                });
            cmd.ErrorDataReceived += new DataReceivedEventHandler(
                (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) output.Add(e.Data);
                });

            cmd.BeginOutputReadLine();
            cmd.BeginErrorReadLine();
            cmd.WaitForExit();

            try
            {
                cmd.Close();
            }
            catch
            {
            }

            try
            {
                cmd.Kill();
            }
            catch { }

            sw.Stop();
            string outstring = string.Join("\r\n", output.ToArray());
            bool isOk = outstring.IndexOf("Error", StringComparison.OrdinalIgnoreCase) <= 0;
            if (!isOk)
            {
                Console.WriteLine("ERROR: " + fileOutput);
            }
            return new FfmpegCommandResult
            {
                ConvertInMiliseconds = sw.ElapsedMilliseconds - 1000,
                Output = outstring,
                Success = isOk,
                FfmpegCmd = cmdLine
            };
        }

        private string ReadLineByLine(StreamReader streamReader)
        {
            var line = "";
            var output = "";
            while (true)
            {
                var lastChr = streamReader.Read();
                if (lastChr <= 0) break;

                var outputChr = streamReader.CurrentEncoding.GetString(new byte[] { (byte)lastChr });
                line += outputChr;
                if (outputChr.IndexOf("\n") >= 0)
                {
                    //Console.Write(line);

                    output += line;
                    line = "";
                }
            }

            return output;
        }
    }
}

/* 
 *    //string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");
            //var file1 = Path.Combine(dir, "1.jpg");
            //var file2 = Path.Combine(dir, "2.jpg");
            //var file3 = Path.Combine(dir, "3.mp3");
            //var file4 = Path.Combine(dir, "4.mp4");
            //var bat1 = Path.Combine(dir, "bat1.bat");

            //Console.WriteLine(file1);
            //Console.WriteLine(file2);
            //Console.WriteLine(file3);
            //Console.WriteLine(file4);
            //Console.WriteLine(bat1);

            //try
            //{
            //    File.Delete(file4);
            //}
            //catch
            //{

            //}

            //string args = $" -loop 1 -t 5 -i \"{file1}1\" -loop 1 -t 5 -i \"{file2}\" -i \"{file3}\" -filter_complex \"[0:v]scale = 1280:720:force_original_aspect_ratio = decrease,pad = 1280:720:(ow - iw) / 2:(oh - ih) / 2,setsar = 1,fade = t =out:st = 4:d = 1[v0];[1:v]scale = 1280:720:force_original_aspect_ratio = decrease,pad = 1280:720:(ow - iw) / 2:(oh - ih) / 2,setsar = 1,fade = t =in:st = 0:d = 1,fade = t =out:st = 4:d = 1[v1];[v0][v1]concat = n = 2:v = 1:a = 0,format = yuv420p[v]\" -map \"[v]\" -map 2:a -shortest \"{file4}\"\r\n";
            //args = "-version";
            //string cmdLine = $"\"{ffmpegCmd}\" {args}";
 * */
