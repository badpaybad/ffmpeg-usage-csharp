# ffmpeg-usage-csharp

Combine Images into one video with audio.

You may want do slide show from images to video to easy to share

Only support run in window cause I use ffmpeg for window.


Check code usage in this class FfmpegSampleUsageRenderImagesToVideo, Template1Builder.cs

# some commadlines

ffmpeg -i 20240123_110615.mp4 -vf scale=-1:720 -c:v libx264 -crf 18 -c:a copy 3.mp4

ffmpeg -i 20240123_110615.mp4 -vf scale=-1:720 -vcodec h264 -acodec mp3 4.mp4


# compatible to old device but not chrome, firefox

                    ffmpeg -y -i 3.mp4 -c:v mpeg4 -q:v 5 -c:a aac -b:a 128k -movflags +faststart 6.mp4 


`
                    ffmpeg -y -i "/home/dunp/Videos/3.mp4" -c:v libx264 -profile:v baseline -level:v 4.0 -c:a aac -movflags +faststart "/home/dunp/Videos/4.mp4"

                    Try using ffmpeg -i source.mp4 -c:a aac -c:v libx264 -profile:v high -level:v 4.1 -movflags faststart DestFile.mp4 If you are playing the video on the R-Pi you might need profile:v main or even baseline and a -level:v 4.0

                    ffmpeg -y -i "/home/dunp/Videos/3.mp4" -c:v libx264 -profile:v main -level:v 4.0 -c:a aac -movflags +faststart "/home/dunp/Videos/6.mp4"

                    ffmpeg -y -i 3.mp4 -c:v libx264 -crf 23 -profile:v baseline -level 3.0 -pix_fmt yuv420p -c:a aac -ac 2 -b:a 128k -movflags faststart 5.mp4

                    ffmpeg -y -i 3.mp4 -c:v libx264 -profile:v baseline -level 3.0 -pix_fmt yuv420p -c:a aac -strict -2 -b:a 128k 6.mp4


                    ffmpeg -y -i 3.mp4 -c:v libx264 -profile:v baseline -level 3.0 -pix_fmt yuv420p -maxrate 1M -bufsize 2M -c:a aac -b:a 128k -movflags +faststart 6.mp4


                    ffmpeg -y -i 3.mp4 -c:v mpeg4 -q:v 5 -c:a aac -b:a 128k -movflags +faststart 6.mp4 <- ok for tivi3d

                    ffmpeg -y -i 3.mp4 -c:v mpeg4 -q:v 5 -c:a aac -b:a 128k -f mp4 6.mp4


`