https://superuser.com/questions/833232/create-video-with-5-images-with-fadein-out-effect-in-ffmpeg/834035

ffmpeg -loop 1 -t 5 -i 1.jpg -loop 1 -t 5 -i 2.jpg -i 3.mp3 -filter_complex "[0:v]scale=1280:720:force_original_aspect_ratio=decrease,pad=1280:720:(ow-iw)/2:(oh-ih)/2,setsar=1,fade=t=out:st=4:d=1[v0]; [1:v]scale=1280:720:force_original_aspect_ratio=decrease,pad=1280:720:(ow-iw)/2:(oh-ih)/2,setsar=1,fade=t=in:st=0:d=1,fade=t=out:st=4:d=1[v1];  [v0][v1]concat=n=2:v=1:a=0,format=yuv420p[v]" -map "[v]" -map 2:a -shortest out.mp4


ffmpeg -i audio.mp3 -i image1.png -i image2.png -filter_complex \
"[0:a]showwaves=s=1920x1080:mode=line[fg]; \
 [1:v][fg]overlay=0:270,drawtext=fontsize=50:fontcolor=white:fontfile=/Windows/Fonts/impact.ttf:text='Planet Money Podcast on NPR - A/B Split Testing':x=(w-text_w)/2:y=200[bg]; \
 [bg][2:v]overlay=10:10,format=yuv420p[outv]" \
-map "[outv]" -map 0:a -c:v libx264 -c:a copy -movflags +faststart -shortest out.mp4

ffmpeg -i "concat:input1.mp4|input2.mp4|input3.mp4" -c copy output.mp4

ffmpeg -i opening.mkv -i episode.mkv -i ending.mkv \
  -filter_complex "[0:v] [0:a] [1:v] [1:a] [2:v] [2:a] concat=n=3:v=1:a=1 [v] [a]" \
  -map "[v]" -map "[a]" output.mkv


  Heh, interesting task. So I think solution is

ffmpeg -i 1.ts -i 2.ts -filter_complex "[0:v][1:v]overlay=x='if(lte(-w+(t)*100,w/2),-w+(t)*100,w/2)':y=0[out]" -map '[out]' -y out.mp4
This filter graph moves second picture from left to right until it reaches half of the screen (w/2). So all you need to modify is w/2 in this expression. The same for some static stop point (100 pixels):

ffmpeg -i 1.ts -i 2.ts -filter_complex "[0:v][1:v]overlay=x='if(lte(-w+(t)*100,100),-w+(t)*100,100)':y=0[out]" -map '[out]' -y out.mp4
Hope it helps.

ffmpeg -y -i xxx.mp4 -ignore_loop 0 -i xxx.gif -filter_complex "[1:v]scale=1080:1920[ovrl];[0:v][ovrl]overlay=0:0" -frames:v 900 -codec:a copy -codec:v libx264 -max_muxing_queue_size 2048 video.mp4