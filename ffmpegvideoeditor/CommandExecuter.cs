using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class CommandExecuter
{
    public async Task<FfmpegConvertedResult> DrawOverlay(string originVideoFilePath,
    string imageOverlayFilePath, int x, int y, double fromSec, double toSec)
    {

        //ffmpeg -i input_video.mp4 -i overlay_image.png -filter_complex 
        //"[0][1]overlay=x=400:y=500:enable='between(t,3,4.5)'" -c:a copy output_video.mp4
        var savetofile = $"{originVideoFilePath}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.{getFileExt(originVideoFilePath)}";
        var cmd = $"ffmpeg -y -i \"{originVideoFilePath}\" -i \"{imageOverlayFilePath}\" -filter_complex " +
        //"\"[1]format=rgba,geq='r=255:g=255:b=255:a=alpha(0)':a=1[ov]; "+
        // $"[0][ov]overlay=x={x}:y={y}:enable='between(t,{fromSec},{toSec}):format=rgb'\" -c:a copy \"{savetofile}\"";
        $"\"[0][1]overlay=x={x}:y={y}:format=auto:enable='between(t,{fromSec},{toSec})'\" -c:v libx264 -c:a aac -movflags +faststart \"{savetofile}\"";

        return Run(cmd, savetofile);

    }
    public async Task<FfmpegConvertedResult> SaveFrame(string originVideoFilePath,
    int atMiliSec, string savetofile)
    {
        TimeSpan t = TimeSpan.FromMilliseconds(atMiliSec);

        var cmd = $"ffmpeg -i \"{originVideoFilePath}\" -ss {t.Hours.ToString("D2")}:{t.Minutes.ToString("D2")}:{t.Seconds.ToString("D2")}.{t.Microseconds.ToString("D2")} -vframes 1 -vf \"scale=iw:ih\" \"{savetofile}\"";

        return Run(cmd, savetofile);

    }

    string getFileExt(string filepath)
    {
        var idx = filepath.LastIndexOf(".");
        if (idx <= 0) return filepath;
        return filepath.Substring(idx + 1);
    }

    FfmpegConvertedResult Run(string cmdLine, string fileOutput)
    {
        try
        {
            File.Delete(fileOutput);
        }
        catch { }

        Console.WriteLine(cmdLine);

        Stopwatch sw = Stopwatch.StartNew();

        System.Diagnostics.Process cmd = new System.Diagnostics.Process();

        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        cmd.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/");
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.RedirectStandardError = true;
        cmd.StartInfo.UseShellExecute = false;
        cmd.StartInfo.FileName = "bash";
        cmd.StartInfo.StandardErrorEncoding = Encoding.UTF8;
        //cmd.StartInfo.StandardOutputEncoding = Encoding.Unicode;
        //cmd.StartInfo.StandardInputEncoding = Encoding.Unicode;
        cmd.Start();

        Console.WriteLine($"FfmpegCommandRunner Started: {fileOutput}");
        //cmd.StandardInput.WriteLine("chcp 65001");
        cmd.StandardInput.WriteLine(cmdLine);

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
        Console.WriteLine(outstring);

        Console.WriteLine(fileOutput);

        return new FfmpegConvertedResult
        {
            ConvertInMiliseconds = sw.ElapsedMilliseconds,
            Output = outstring,
            Success = isOk,
            FfmpegCmd = cmdLine,
            OutputFile = fileOutput
        };
    }
}


public class FfmpegConvertedResult
{
    /// <summary>
    /// no mater what video still rendered
    /// </summary>
    public bool Success { get; set; }

    public string Output { get; set; }

    public string OutputFile{get;set;}

    public long ConvertInMiliseconds { get; set; }
    public string FfmpegCmd { get; set; }

    public List<FfmpegConvertedResult> SubResult { get; set; }

}