using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
public class SoinApplyMass
{

    public async Task<string> Do(string originVideoPath, List<SoinOverlay> items)
    {
        FfmpegConvertedResult? r = null;

        List<string> filesout = new List<string>();
        foreach (var i in items)
        {

            r = await new CommandExecuter().DrawOverlay(r == null ? originVideoPath : r.OutputFile,
            i.ImageOverlayFilePath, i.X, i.Y, i.FromSeconds, i.ToSeconds);

            if (r != null) filesout.Add(r.OutputFile);

        }

        var finnal = filesout.LastOrDefault();

        foreach (var torem in filesout.Where(i => i != finnal))
        {
            try
            {
                File.Delete(torem);
            }
            catch { }
        }
        return finnal;
    }
}

public class SoinVideo
{
    public string OriginalVideoFilePath { get; set; }

    public List<SoinOverlay> Overlays { get; set; }
}

public class SoinOverlay
{
    public string ImageOverlayFilePath { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public double FromSeconds { get; set; }
    public double ToSeconds { get; set; }
}