using System.Collections.Generic;

namespace Ffmpeg.Core
{
    public class FfmpegCommandLine
    {
        public string FileOutput { get; set; }

        public string FfmpegCommand { get; set; }

        public List<FfmpegCommandLine> SubFileOutput { get; set; }

        //public string ConcatSubFileTxt { get; set; }

    }
}
