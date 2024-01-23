# ffmpeg-usage-csharp

Combine Images into one video with audio.

You may want do slide show from images to video to easy to share

Only support run in window cause I use ffmpeg for window.


Check code usage in this class FfmpegSampleUsageRenderImagesToVideo, Template1Builder.cs

# some commadlines

ffmpeg -i 20240123_110615.mp4 -vf scale=-1:720 -c:v libx264 -crf 18 -c:a copy 3.mp4

ffmpeg -i 20240123_110615.mp4 -vf scale=-1:720 -vcodec h264 -acodec mp3 4.mp4
