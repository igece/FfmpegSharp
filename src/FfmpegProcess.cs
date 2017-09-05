﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using FfmpegSharp.Exceptions;


namespace FfmpegSharp
{
  internal sealed class FfmpegProcess : Process
  {
    public static readonly Regex InfoRegex = new Regex(@"Input File\s*: .+\r?\nChannels\s*: (\d+)\r?\nSample Rate\s*: (\d+)\r?\nPrecision\s*: ([\s\S]+?)\r?\nDuration\s*: (\d{2}:\d{2}:\d{2}\.?\d{2}?)[\s\S]+?\r?\nFile Size\s*: (\d+\.?\d{0,2}?[k|M|G]?)\r?\nBit Rate\s*: (\d+\.?\d{0,2}?[k|M|G]?)\r?\nSample Encoding\s*: (.+)");
    public static readonly Regex ProgressRegex = new Regex(@"In:(\d{1,3}\.?\d{0,2})%\s+(\d{2}:\d{2}:\d{2}\.?\d{0,2})\s+\[(\d{2}:\d{2}:\d{2}\.?\d{0,2})\]\s+Out:(\d+\.?\d{0,2}[k|M|G]?)");
    public static readonly Regex LogRegex = new Regex(@"(FAIL|WARN|DBUG|INFO)\s(\w+):\s(.+)");


    private FfmpegProcess()
    : base()
    {
      StartInfo.ErrorDialog = false;
      StartInfo.CreateNoWindow = true;
      StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
      StartInfo.UseShellExecute = false;
      StartInfo.RedirectStandardOutput = true;
      StartInfo.RedirectStandardError = true;
      EnableRaisingEvents = true;
    }


    /// <summary>
    /// Create a new <see cref="T:FfmpegSharp.FfmpegProcess"/> instance prepared to run Ffmpeg.
    /// </summary>
    /// <returns>The Ffmpeg process instance.</returns>
    public static FfmpegProcess Create(string path)
    {
      string soxExecutable;

      if (String.IsNullOrEmpty(path))
        throw new FfmpegException("Ffmpeg path not specified");

      if (File.Exists(path))
        soxExecutable = path;
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
      ffmpegProc.StartInfo.FileName = soxExecutable;
      ffmpegProc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;

      string ffmpegPath = Path.GetDirectoryName(soxExecutable);

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
