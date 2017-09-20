using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using FfmpegSharp.Exceptions;


namespace FfmpegSharp
{
  internal sealed class FfmpegProcess : Process
  {
    public static readonly Regex ProgressRegex = new Regex(@"frame=\s*(\d+) fps=\s*(\d+[.\d]*) q=\s*(-?\d+[.\d]*) L?size=\s*(\d+)kB time=\s*(\d{2}:\d{2}:\d{2}\.?\d{0,2}) bitrate=\s*(\d+[.\d]*)kbits/s");
    public static readonly Regex LogRegex = new Regex(@"\[(\w+)\]\s(.+)");
    public static readonly Regex DurationRegEx = new Regex(@"Duration: ([^,]*), ");
    public static readonly Regex VideoMetadataRegEx = new Regex(@"(Stream\s*#[0-9]*:[0-9]*\(?[^\)]*?\)?: Video:.*)");

    private FfmpegProcess()
    : base()
    {
      StartInfo.ErrorDialog = false;
      StartInfo.CreateNoWindow = true;
      StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
      StartInfo.UseShellExecute = false;
      //StartInfo.RedirectStandardOutput = true;
      StartInfo.RedirectStandardError = true;
      EnableRaisingEvents = true;
    }


    /// <summary>
    /// Create a new <see cref="T:FfmpegSharp.FfmpegProcess"/> instance prepared to run Ffmpeg.
    /// </summary>
    /// <returns>The Ffmpeg process instance.</returns>
    public static FfmpegProcess Create(string path)
    {
      string ffmpegExec;

      if (String.IsNullOrEmpty(path))
        throw new FfmpegException("Ffmpeg path not specified");

      if (File.Exists(path))
        ffmpegExec = path;
      else
        throw new FileNotFoundException("Ffmpeg executable not found");

      /*
      using (FfmpegProcess versionCheck = new FfmpegProcess())
      {
        versionCheck.StartInfo.RedirectStandardOutput = true;
        versionCheck.StartInfo.FileName = soxExecutable;
        versionCheck.StartInfo.Arguments = "-version";
        versionCheck.Start();

        string output = versionCheck.StandardOutput.ReadLine();

        if (versionCheck.WaitForExit(1000) == false)
          throw new TimeoutException("Cannot obtain Ffmpeg version: response timeout");

        Match versionMatch = new Regex(@"\sSoX v(\d{1,2})\.(\d{1,2})\.(\d{1,2})").Match(output);

        if (!versionMatch.Success)
          throw new FfmpegException("Cannot obtain Ffmpeg version: unable to fetch info from Sox");

        try
        {
          int majorVersion = Int32.Parse(versionMatch.Groups[1].Value);
          int minorVersion = Int32.Parse(versionMatch.Groups[2].Value);
          int fixVersion = Int32.Parse(versionMatch.Groups[3].Value);

          if ((majorVersion < 14) ||
              ((majorVersion == 14) && (minorVersion < 3)) ||
              ((majorVersion == 14) && (minorVersion == 3) && (fixVersion < 1)))
            throw new FfmpegException(versionMatch.Groups[0] + " not currently supported");
        }

        catch (Exception ex)
        {
          throw new FfmpegException("Cannot obtain Ffmpeg version", ex);
        }
      }
      */

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
