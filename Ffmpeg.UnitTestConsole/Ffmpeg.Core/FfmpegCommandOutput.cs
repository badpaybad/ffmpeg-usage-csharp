using System.Collections.Generic;

namespace Ffmpeg.Core
{
    public class FfmpegCommandOutput
    {
        public string FileOutput { get; set; }

        public string FfmpegCommand { get; set; }

        public List<FfmpegCommandOutput> SubFileOutput { get; set; }

        //public string ConcatSubFileTxt { get; set; }

    }
}
