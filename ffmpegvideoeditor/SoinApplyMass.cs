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

    public async Task<List<SoinVideo>> Parse(string filepath)
    {
        string alltext = string.Empty;

        using (var sw = new StreamReader(filepath))
        {
            alltext = await sw.ReadToEndAsync();
        }
        var splchar = new[] { '\r', ' ', '\n', '\t' };
        var lines = alltext.Split('\n').Select(i => i.Trim(splchar)).Where(i => !i.Contains("duong dan file", StringComparison.OrdinalIgnoreCase)).ToList();

        List<SoinVideo> allline = new List<SoinVideo>();

        foreach (var l in lines)
        {
            // Console.WriteLine(l);
            var arr = l.Split(',').Select(i => i.Trim(splchar)).ToArray();
            // 
            if (arr.Length != 6)
            {
                // Console.WriteLine($"Not found: {arr.Length}");
                // Console.WriteLine(string.Join(" , ", arr));
                continue;
            }

            //             " duong dan file ", from second, to second, red or blue, left pix, top pix
            // /work/datatemp/SOIN/[CSIP] Course 1/Bài 0 - Tổng quan khóa học.mp4 , 21.5, 25.5, red, 467,969 
            var existed = allline.FirstOrDefault(i => i.OriginalVideoFilePath == arr[0]);
            if (existed == null)
            {
                existed = new SoinVideo
                {
                    Overlays = new List<SoinOverlay> { }
                };

                allline.Add(existed);
            }
            existed.OriginalVideoFilePath = arr[0];
            var fileoverlay = getOverlay(arr[3]);
            if (string.IsNullOrEmpty(fileoverlay))
            {
                Console.WriteLine("NOT FOUND overlay for: "+ arr[3]);
                continue;
            }
            existed.Overlays.Add(new SoinOverlay
            {
                FromSeconds = double.Parse(arr[1]),
                ToSeconds = double.Parse(arr[2]),
                ImageOverlayFilePath = fileoverlay,
                X = int.Parse(arr[4]),
                Y = int.Parse(arr[5])
            });


            Console.WriteLine(string.Join(" , ", arr));
        }

        return allline;
    }

    string getOverlay(string type)
    {
        if (type.Contains("bluesmall", StringComparison.OrdinalIgnoreCase))
        {
            return "/work/datatemp/SOIN/[CSIP] Course 1/bluesmall.png";
        }
        if (type.Contains("blue", StringComparison.OrdinalIgnoreCase))
        {
            return "/work/datatemp/SOIN/[CSIP] Course 1/blue.png";
        }
        if (type.Contains("redsmall", StringComparison.OrdinalIgnoreCase))
        {
            return "/work/datatemp/SOIN/[CSIP] Course 1/red.png";
        }
        if (type.Contains("red", StringComparison.OrdinalIgnoreCase))
        {
            return "/work/datatemp/SOIN/[CSIP] Course 1/red.png";
        }
        return "";
    }


    public async Task<string> Do(string originVideoPath, List<SoinOverlay> items)
    {
        FfmpegConvertedResult? r = null;

        Console.WriteLine($"{originVideoPath} -> {items.Count}");

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