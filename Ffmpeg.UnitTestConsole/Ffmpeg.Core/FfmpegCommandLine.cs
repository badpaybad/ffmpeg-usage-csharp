using System.Collections.Generic;

namespace Ffmpeg.Core
{
    public class FfmpegCommandLine
    {
        public int GroupOrder { get; set; }
        public string FileOutput { get; set; }

        public string FfmpegCommand { get; set; }

        public List<FfmpegCommandLine> SubFileOutput { get; set; }


        public bool IsValid()
        {
            if (FfmpegCommand.Length > 8000) return false;
            if (SubFileOutput == null || SubFileOutput.Count == 0) return true;

            foreach(var s in SubFileOutput)
            {
                if (s.FfmpegCommand.Length > 8000) return false;
            }

            return true;
        }

    }
}
