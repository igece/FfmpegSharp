using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using FfmpegSharp.Exceptions;


namespace FfmpegSharp
{
  /// <summary>
  /// Encapsulates all the information needed to launch Ffmpeg and handle its output.
  /// </summary>
  public class Ffmpeg : IDisposable
  {
    /// <summary>
    /// Provides updated progress status while FFmpeg is being executed.
    /// </summary>
    public event EventHandler<ProgressEventArgs> OnProgress = null;

    /// <summary>
    /// Location of the Ffmpeg executable to be used by the library.
    /// </summary>
    /// <value>The executable path.</value>
    public string Path { get; set; }

    /// <summary>
    /// Overwrite existing output files. If this option is set to false and any specified output
    /// file already exists, FFmpeg exeution ends.
    /// </summary>
    public bool OverwriteOutput { get; set; }

    /// <summary>
    /// Custom global arguments.
    /// </summary>
    public string CustomArgs { get; set; }

    /// <summary>
    /// Gets the full command line of the last call to Ffmpeg.
    /// </summary>
    public string LastCommand { get; private set; }


    private FfmpegProcess FfmpegProcess_ = null;
    private bool disposed_ = false;


    /// <summary>
    /// Initializes a new instance of the <see cref="T:FfmpegSharp.Sox"/> class.
    /// </summary>
    /// <param name="path">Location of the Ffmpeg executable to be used by the library.</param>
    public Ffmpeg(string path)
    {
      Path = path;
    }


    ~Ffmpeg()
    {
      Dispose(false);
    }


    /// <summary>
    /// Releases all resource used by the <see cref="T:FfmpegSharp.Sox"/> object.
    /// </summary>
    /// <remarks>Call <see cref="T:Sox.Dispose"/> when you are finished using the <see cref="T:FfmpegSharp.Sox"/> instance. This
    /// <see cref="T:Sox.Dispose"/> method leaves the <see cref="T:FfmpegSharp.Sox"/> instance in an unusable state. After calling
    /// it, you must release all references to the instance so the garbage
    /// collector can reclaim the memory that it was occupying.</remarks>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Gets information about the given file. 
    /// </summary>
    /// <returns>File information as a <see cref="MediaInfo"/> instance.</returns>
    /// <param name="inputFile">Input file.</param>
    public MediaInfo GetInfo(string inputFile)
    {
      if (!File.Exists(inputFile))
        throw new FileNotFoundException("File not found: " + inputFile);

      using (FfmpegProcess ffmpeg = FfmpegProcess.Create(Path))
      {
        ffmpeg.StartInfo.RedirectStandardError = true;
        ffmpeg.StartInfo.Arguments = "-hide_banner -i " + inputFile;
        ffmpeg.Start();

        LastCommand = Path + " " + ffmpeg.StartInfo.Arguments;

        string output = ffmpeg.StandardError.ReadToEnd();

        if (ffmpeg.WaitForExit(10000) == false)
          throw new TimeoutException("FFmpeg response timeout");

        if (output != null)
        {
          TimeSpan duration = TimeSpan.MinValue;

          Match matchDuration = FfmpegProcess.DurationRegEx.Match(output);
          //Match matchVideoMetadata = FfmpegProcess.VideoMetadataRegEx.Match(output);

          if (!matchDuration.Success)
            throw new FfmpegException("Unexpected output from FFmpeg");

          TimeSpan.TryParseExact(matchDuration.Groups[1].Value, @"hh\:mm\:ss\.ff", CultureInfo.InvariantCulture, out duration);

          /*
          if (matchVideoMetadata.Success)
          {
            Match matchVideoDetails = FfmpegProcess.VideoDetailsRegEx.Match(matchVideoMetadata.Groups[1].Value);

            if (matchVideoDetails.Success)
            {
              
            }
          }
          */

          return new MediaInfo(duration);
        }

        throw new FfmpegException("Unexpected output from FFmpeg");
      }
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance and starts recording from the record audio the specified file.
    /// </summary>
    /// <param name="inputDevice">Input </param>
    /// <param name="outputFile">Media file to be recorded.</param>
    public void Record(string inputDevice, string outputFile)
    {
      InputFile device = new InputFile(inputDevice);

      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32S:

          device.Format = "dshow";
          break;

        case PlatformID.MacOSX:

          device.Format = "avfoundation";
          break;

        case PlatformID.Unix:

          device.Format = "x11grab";
          break;
      }

      Process(device, outputFile);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile">Media file to be processed.</param>
    public void Process(string inputFile)
    {
      Process(new InputFile[] { new InputFile(inputFile) }, null);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile">Media file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(string inputFile, string outputFile)
    {
      Process(new InputFile[] { new InputFile(inputFile) }, new OutputFile[] { new OutputFile(outputFile) });
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile">Media file to be processed.</param>
    public void Process(InputFile inputFile)
    {
      Process(new InputFile[] { inputFile }, null);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile">Media file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(InputFile inputFile, OutputFile outputFile)
    {
      Process(new InputFile[] { inputFile }, new OutputFile[] { outputFile });
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile">Media file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(InputFile inputFile, string outputFile)
    {
      Process(new InputFile[] { inputFile }, new OutputFile[] { new OutputFile(outputFile) });
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile">Media file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(string inputFile, OutputFile outputFile)
    {
      Process(new InputFile[] { new InputFile(inputFile) }, new OutputFile[] { outputFile });
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile1">First media file to be processed.</param>
    /// <param name="inputFile2">Second media file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(string inputFile1, string inputFile2, string outputFile)
    {
      Process(new InputFile[] { new InputFile(inputFile1), new InputFile(inputFile2) }, new OutputFile[] { new OutputFile(outputFile) });
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile1">First media file to be processed.</param>
    /// <param name="inputFile2">Second media file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(InputFile inputFile1, InputFile inputFile2, string outputFile)
    {
      Process(new InputFile[] { inputFile1, inputFile2 }, new OutputFile[] { new OutputFile(outputFile) });
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFiles">Media files to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(string[] inputFiles, string outputFile)
    {
      var inputs = new List<InputFile>(inputFiles.Length);

      foreach (var inputFile in inputFiles)
        inputs.Add(new InputFile(inputFile));

      Process(inputs.ToArray(), new OutputFile[] { new OutputFile(outputFile) });
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFiles">Media files to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(string[] inputFiles, OutputFile outputFile)
    {
      var inputs = new List<InputFile>(inputFiles.Length);

      foreach (var inputFile in inputFiles)
        inputs.Add(new InputFile(inputFile));

      Process(inputs.ToArray(), new OutputFile[] { outputFile });
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFiles">Media files to be processed.</param>
    /// <param name="outputFiles">Output files.</param>
    public void Process(InputFile[] inputFiles, OutputFile[] outputFiles)
    {
      FfmpegProcess_ = FfmpegProcess.Create(Path);

      try
      {
        FfmpegProcess_.ErrorDataReceived += OnFfmpegOutputReceived;

        List<string> args = new List<string>();

        args.Add("-hide_banner");
        args.Add("-nostdin");
        args.Add("-stats");
        args.Add("-loglevel fatal");

        // Avoid FFmpeg asking for overwrite if a specified output file already exists.

        args.Add(OverwriteOutput ? "-y" : "-n");

        // Global options.

        if (!String.IsNullOrEmpty(CustomArgs))
          args.Add(CustomArgs);

        // Input options and files.

        if ((inputFiles != null) && (inputFiles.Length > 0))
        {
          foreach (InputFile inputFile in inputFiles)
            args.Add(inputFile.ToString());
        }
        else
          throw new FfmpegException("No input files specified");

        // Output options and files.

        if ((outputFiles != null) && (outputFiles.Length > 0))
        {
          foreach (OutputFile outputFile in outputFiles)
            args.Add(outputFile.ToString());
        }
        else
        {
          if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            args.Add("NUL");
          else
            args.Add("/dev/null");
        }

        FfmpegProcess_.StartInfo.Arguments = String.Join(" ", args);
        LastCommand = Path + " " + FfmpegProcess_.StartInfo.Arguments;

        try
        {
          FfmpegProcess_.Start();
          FfmpegProcess_.BeginErrorReadLine();
          FfmpegProcess_.WaitForExit();
        }

        catch (Exception ex)
        {
          throw new FfmpegException("Cannot spawn FFmpeg process", ex);
        }
      }

      finally
      {
        if (FfmpegProcess_ != null)
        {
          FfmpegProcess_.Dispose();
          FfmpegProcess_ = null;
        }
      }
    }


    private void OnFfmpegOutputReceived(object sender, DataReceivedEventArgs received)
    {
      if (received.Data != null)
      {
        Match matchProgress = FfmpegProcess.ProgressRegex.Match(received.Data);

        // As FFmpeg is always executed with the '-stats' and '-loglevel fatal' arguments,
        // any message not matching the stats pattern is a fatal error.

        if (!matchProgress.Success)
          ParseFatalError(received.Data);

        if (OnProgress != null)
        {
          UInt32 frames = 0;
          double fps = 0.0;
          double q = 0.0;
          UInt64 size = 0;
          TimeSpan time = TimeSpan.Zero;
          double bitrate = 0.0;

          try
          {
            UInt32.TryParse(matchProgress.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out frames);
            double.TryParse(matchProgress.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out fps);
            double.TryParse(matchProgress.Groups[3].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out q);
            UInt64.TryParse(matchProgress.Groups[4].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out size);
            TimeSpan.TryParseExact(matchProgress.Groups[5].Value, @"hh\:mm\:ss\.ff", CultureInfo.InvariantCulture, out time);
            double.TryParse(matchProgress.Groups[6].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out bitrate);

            size *= 1024;
            bitrate *= 1000;

            ProgressEventArgs progressEventArgs = new ProgressEventArgs(frames, fps, q, size, time, bitrate);
            OnProgress(sender, progressEventArgs);

            if (progressEventArgs.Abort)
              Abort();

            return;
          }

          catch (Exception ex)
          {
            throw new FfmpegException("Unexpected output from FFmpeg", ex);
          }
        }
      }
    }


    /// <summary>
    /// Kills the Ffmpeg process.
    /// </summary>
    public void Abort()
    {
      if ((FfmpegProcess_ != null) && !FfmpegProcess_.HasExited)
      {
        try
        {
          FfmpegProcess_.Kill();
        }

        finally
        {
          FfmpegProcess_.Dispose();
          FfmpegProcess_ = null;
        }
      }
    }


    protected void ParseFatalError(string data)
    {
      if (string.IsNullOrEmpty(data))
        return;

      Match logMatch = FfmpegProcess.LogRegex.Match(data);

      string source = logMatch.Success ? logMatch.Groups[1].Value : "ffmpeg";
      string message = logMatch.Success ? logMatch.Groups[2].Value : data;

      if (!String.IsNullOrEmpty(message))
      {
        if (String.IsNullOrEmpty(source))
          throw new FfmpegFatalErrorException("ffmpeg", message);

        throw new FfmpegFatalErrorException(source, message);
      }
    }


    protected virtual void Dispose(bool disposing)
    {
      if (disposed_)
        return;

      if (disposing)
        Abort();

      disposed_ = true;
    }
  }
}
