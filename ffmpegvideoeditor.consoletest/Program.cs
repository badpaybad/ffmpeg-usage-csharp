using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System;
// See https://aka.ms/new-console-template for more information
/* libaom-av1: AV1 (High efficiency, open-source, new and gaining support)
                        Example: -c:v libaom-av1
*/
Console.WriteLine("Hello, World!");
string videotypes = """
                    libx264: H.264 (High efficiency, widely supported)
                        Example: -c:v libx264

                    libx265: H.265/HEVC (High efficiency, better compression than H.264)
                        Example: -c:v libx265

                    libvpx: VP8 (Open-source, widely supported in web browsers)
                        Example: -c:v libvpx

                    libvpx-vp9: VP9 (Successor to VP8, better compression)
                        Example: -c:v libvpx-vp9

                
                    mpeg4: MPEG-4 (Older,)
                        Example: -c:v mpeg4

                    mpeg2video: MPEG-2 (Older, used for DVDs)
                        Example: -c:v mpeg2video

                    h263: H.263 (Older, used in video conferencing)
                        Example: -c:v h263

                    libtheora: Theora (Open-source, older)
                        Example: -c:v libtheora

                    libxvid: Xvid (MPEG-4 Part 2, widely supported on older devices)
                        Example: -c:v libxvid

                    hevc: H.265/HEVC using FFmpeg's native encoder (similar to libx265)
                        Example: -c:v hevc

                    h264_nvenc: H.264 using NVIDIA GPU hardware acceleration
                        Example: -c:v h264_nvenc

                    hevc_nvenc: H.265/HEVC using NVIDIA GPU hardware acceleration
                        Example: -c:v hevc_nvenc

                    prores: Apple ProRes (Professional, high-quality video)
                        Example: -c:v prores

                    dnxhd: DNxHD (High-definition video)
                        Example: -c:v dnxhd
""";

var arr = videotypes.Split('\n').Where(i => !string.IsNullOrEmpty(i) && !string.IsNullOrWhiteSpace(i)
&& !i.Contains("Example:")).Select(i => i.Trim().Split(':')[0]).ToList();

var originVideoFilePath = "/home/dunp/Videos/3.mp4";

foreach (var cv in arr)
{
    Console.WriteLine(cv);
    var savetofile = $"{originVideoFilePath}-{cv}_video-13_{cv}.mp4";

    var cmd = $"ffmpeg -y -i \"{originVideoFilePath}\" -c:v {cv} -c:a aac -b:a 128k -f mp4 -movflags +faststart \"{savetofile}\"";

    var r = new CommandExecuter().Run(cmd, savetofile);

}