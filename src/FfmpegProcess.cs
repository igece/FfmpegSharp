using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

using FfmpegSharp.Exceptions;


namespace FfmpegSharp
{
    internal sealed class FfmpegProcess : Process
    {
        public static readonly Regex ProgressRegex = new Regex(@"frame=\s*(\d+) fps=\s*(\d+[.\d]*) q=\s*(-?\d+[.\d]*) L?size=\s*(\d+|N/A)(?:kB)? time=\s*(-?\d{2,}:\d{2}:\d{2}\.?\d{0,2}) bitrate=\s*(\d+[.\d]*|N/A)\s*(?:kbits/s)?");
        public static readonly Regex LogRegex = new Regex(@"\[(\w+)\]\s(.+)");
        public static readonly Regex DurationRegEx = new Regex(@"Duration: ([^,]*), ");
        public static readonly Regex VideoMetadataRegEx = new Regex(@"(Stream\s*#[0-9]*:[0-9]*\(?[^\)]*?\)?: Video:.*)");
        public static readonly Regex VideoDetailsRegEx = new Regex(@"Video:\s*([^,]*),\s*([^,]*,?[^,]*?),?\s*(?=[0-9]*x[0-9]*)([0-9]*x[0-9]*)");


        private FfmpegProcess()
        : base()
        {
            StartInfo.ErrorDialog = false;
            StartInfo.CreateNoWindow = true;
            StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            StartInfo.UseShellExecute = false;
            StartInfo.RedirectStandardError = true;
            EnableRaisingEvents = true;
        }


        /// <summary>
        /// Create a new <see cref="T:FfmpegSharp.FfmpegProcess"/> instance prepared to run Ffmpeg.
        /// </summary>
        /// <returns>The Ffmpeg process instance.</returns>
        public static FfmpegProcess Create(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new FfmpegException("Ffmpeg path not specified");

            string ffmpegExec;

            if (File.Exists(path))
                ffmpegExec = path;
            else
                throw new FileNotFoundException("Ffmpeg executable not found");

            FfmpegProcess ffmpegProc = new FfmpegProcess();
            ffmpegProc.StartInfo.FileName = ffmpegExec;
            ffmpegProc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            ffmpegProc.StartInfo.EnvironmentVariables["AV_LOG_FORCE_NOCOLOR"] = "1";

            string ffmpegPath = Path.GetDirectoryName(ffmpegExec);

            if (!String.IsNullOrEmpty(ffmpegPath))
            {
                string pathEnv = Environment.GetEnvironmentVariable("PATH");
                pathEnv += Path.PathSeparator + ffmpegPath;
                ffmpegProc.StartInfo.EnvironmentVariables["PATH"] = pathEnv;
            }

            return ffmpegProc;
        }
    }
}
