using System;
using System.IO;

namespace Ffmpeg.Core
{
    public class FfmpegCommandRunner
    {
        public string Run()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window/ffmpeg/bin");

            System.Diagnostics.Process cmd = new System.Diagnostics.Process();
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            cmd.StartInfo.WorkingDirectory = dir;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.UseShellExecute = false;

            string ffmpegCmd = Path.Combine(dir, "ffmpeg.exe");
            var file1 = Path.Combine(dir, "1.jpg");
            var file2 = Path.Combine(dir, "2.jpg");
            var file3 = Path.Combine(dir, "3.mp3");
            var file4 = Path.Combine(dir, "4.mp4");

            string args = $" -loop 1 -t 5 -i \"{file1}\" -loop 1 -t 5 -i \"{file2}\" -i \"{file3}\" -filter_complex \"[0:v]scale = 1280:720:force_original_aspect_ratio = decrease,pad = 1280:720:(ow - iw) / 2:(oh - ih) / 2,setsar = 1,fade = t =out:st = 4:d = 1[v0];[1:v]scale = 1280:720:force_original_aspect_ratio = decrease,pad = 1280:720:(ow - iw) / 2:(oh - ih) / 2,setsar = 1,fade = t =in:st = 0:d = 1,fade = t =out:st = 4:d = 1[v1];[v0][v1]concat = n = 2:v = 1:a = 0,format = yuv420p[v]\" -map \"[v]\" -map 2:a -shortest \"{file4}\"\r\n";
            //args = "-version";

            var bat1 = Path.Combine(dir, "bat1.bat");

            cmd.StartInfo.FileName = "cmd.exe";

            //cmd.StartInfo.FileName = ffmpegCmd;
            //cmd.StartInfo.Arguments = args;

            cmd.Start();

            Console.WriteLine("FfmpegCommandRunner Started");

            //            cmd.StandardInput.WriteLine($"{ffmpegCmd} {args}");
            cmd.StandardInput.WriteLine(bat1);

            cmd.StandardInput.WriteLine("echo ##done##");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();

            var line = "";
            var allLine = "";
            while (true)
            {
                var lastChr = cmd.StandardOutput.Read();
                if (lastChr <= 0) break;

                var outputChr = cmd.StandardOutput.CurrentEncoding.GetString(new byte[] { (byte)lastChr });
                line += outputChr;
                if (outputChr.IndexOf("\n") >= 0)
                {
                    Console.Write(line);
                    allLine += line;
                    line = "";
                }
            }
            if (allLine.IndexOf("##done##") >= 0)
            {
                Console.WriteLine("\r\n------Ended---\r\n");
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
            }
            return allLine;
        }
    }
}