using System;

namespace Ffmpeg.Core
{
    public class FFmpegCommandBuilder
    {
        public string ToCommand()
        {
            return "ffmpeg -loop 1 -t 5 -i 1.jpg -loop 1 -t 5 -i 2.jpg -i 3.mp3 -filter_complex \"[0:v]scale = 1280:720:force_original_aspect_ratio = decrease,pad = 1280:720:(ow - iw) / 2:(oh - ih) / 2,setsar = 1,fade = t =out:st = 4:d = 1[v0];[1:v]scale = 1280:720:force_original_aspect_ratio = decrease,pad = 1280:720:(ow - iw) / 2:(oh - ih) / 2,setsar = 1,fade = t =in:st = 0:d = 1,fade = t =out:st = 4:d = 1[v1];[v0][v1]concat = n = 2:v = 1:a = 0,format = yuv420p[v]\" -map \"[v]\" -map 2:a -shortest out.mp4";
        }
    }

    public class FfmpegCommandRunner
    {

    }
}
