using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
    /// Provides updated progress status while Ffmpeg is being executed.
    /// </summary>
    public event EventHandler<ProgressEventArgs> OnProgress = null;

    /// <summary>
    /// Occurs when Ffmpeg generates any non-FAIL log message.
    /// </summary>
    public event EventHandler<LogMessageEventArgs> OnLogMessage = null;

    /// <summary>
    /// Location of the Ffmpeg executable to be used by the library.
    /// </summary>
    /// <value>The executable path.</value>
    public string Path { get; set; }

    /// <summary>
    /// Output format options.
    /// </summary>
    public OutputFile Output { get; private set; }

    /// <summary>
    /// Custom global arguments.
    /// </summary>
    public string CustomArgs { get; set; }

    /// <summary>
    /// Gets the full command line of the last call to Ffmpeg.
    /// </summary>
    public string LastCommand { get; private set; }


    private FfmpegProcess FfmpegProcess_ = null;
    private string lastError_ = null;
    private string lastErrorSource_ = null;
    private bool disposed_ = false;


    /// <summary>
    /// Initializes a new instance of the <see cref="T:FfmpegSharp.Sox"/> class.
    /// </summary>
    /// <param name="path">Location of the Ffmpeg executable to be used by the library.</param>
    public Ffmpeg(string path)
    {
      Output = new OutputFile();
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

      return new MediaInfo();

      /*

      FfmpegProcess_ = FfmpegProcess.Create(Path);

      lastError_ = null;
      lastErrorSource_ = null;

      try
      {
        FfmpegProcess_.StartInfo.RedirectStandardOutput = true;
        FfmpegProcess_.StartInfo.Arguments = "--info " + inputFile;
        FfmpegProcess_.Start();

        LastCommand = Path + " " + FfmpegProcess_.StartInfo.Arguments;

        string output = FfmpegProcess_.StandardOutput.ReadToEnd();

        if (String.IsNullOrEmpty(output))
          output = FfmpegProcess_.StandardError.ReadToEnd();

        if (FfmpegProcess_.WaitForExit(10000) == false)
          throw new TimeoutException("Ffmpeg response timeout");

        CheckForLogMessage(output);

        if (output != null)
        {
          Match matchInfo = FfmpegProcess.InfoRegex.Match(output);

          if (matchInfo.Success)
          {
            try
            {
              UInt16 channels = Convert.ToUInt16(double.Parse(matchInfo.Groups[1].Value, CultureInfo.InvariantCulture));
              UInt32 sampleRate = Convert.ToUInt32(double.Parse(matchInfo.Groups[2].Value, CultureInfo.InvariantCulture));
              UInt16 sampleSize = Convert.ToUInt16(double.Parse(new string(matchInfo.Groups[3].Value.Where(Char.IsDigit).ToArray()), CultureInfo.InvariantCulture));
              TimeSpan duration = TimeSpan.ParseExact(matchInfo.Groups[4].Value, @"hh\:mm\:ss\.ff", CultureInfo.InvariantCulture);
              UInt64 size = FormattedSize.ToUInt64(matchInfo.Groups[5].Value);
              UInt32 bitRate = FormattedSize.ToUInt32(matchInfo.Groups[6].Value);
              string encoding = matchInfo.Groups[7].Value;

              return new MediaInfo(channels, sampleRate, sampleSize, duration, size, bitRate, encoding);
            }

            catch (Exception ex)
            {
              throw new FfmpegException("Cannot parse Ffmpeg output", ex);
            }
          }
        }

        throw new FfmpegException("Unexpected output from Ffmpeg");
      }

      finally
      {
        if (FfmpegProcess_ != null)
        {
          FfmpegProcess_.Dispose();
          FfmpegProcess_ = null;
        }
      }
      */
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance and record audio the specified file.
    /// </summary>
    /// <param name="outputFile">Audio file to be recorded.</param>
    public void Record(string outputFile)
    {
      Process("--default-device", outputFile);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile">Audio file to be processed.</param>
    public void Process(string inputFile)
    {
      Process(new InputFile[] { new InputFile(inputFile) }, null);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile">Audio file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(string inputFile, string outputFile)
    {
      Process(new InputFile[] { new InputFile(inputFile) }, new OutputFile[] { new OutputFile(outputFile) });
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile">Audio file to be processed.</param>
    public void Process(InputFile inputFile)
    {
      Process(new InputFile[] { inputFile }, null);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile">Audio file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(InputFile inputFile, OutputFile outputFile)
    {
      Process(new InputFile[] { inputFile }, new OutputFile[] { outputFile });
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile">Audio file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(InputFile inputFile, string outputFile)
    {
      Process(new InputFile[] { inputFile }, new OutputFile[] { new OutputFile(outputFile) });
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile1">First audio file to be processed.</param>
    /// <param name="inputFile2">Second audio file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(string inputFile1, string inputFile2, string outputFile)
    {
      Process(new InputFile[] { new InputFile(inputFile1), new InputFile(inputFile2) }, new OutputFile[] { new OutputFile(outputFile) });
    }



    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile1">First audio file to be processed.</param>
    /// <param name="inputFile2">Second audio file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(InputFile inputFile1, InputFile inputFile2, string outputFile)
    {
      Process(new InputFile[] { inputFile1, inputFile2 }, new OutputFile[] { new OutputFile(outputFile) });
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFiles">Audio files to be processed.</param>
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
    /// <param name="inputFiles">Audio files to be processed.</param>
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
    /// <param name="inputFiles">Audio files to be processed.</param>
    /// <param name="outputFiles">Output files.</param>
    public void Process(InputFile[] inputFiles, OutputFile[] outputFiles)
    {
      FfmpegProcess_ = FfmpegProcess.Create(Path);

      lastError_ = null;
      lastErrorSource_ = null;

      try
      {
        FfmpegProcess_.ErrorDataReceived += OnFfmpegProcessOutputReceived;
        FfmpegProcess_.OutputDataReceived += OnFfmpegProcessOutputReceived;

        List<string> args = new List<string>();

        args.Add("-hide_banner");

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
          //FfmpegProcess_.BeginOutputReadLine();
          FfmpegProcess_.BeginErrorReadLine();
          FfmpegProcess_.WaitForExit();
        }

        catch (Exception ex)
        {
          throw new FfmpegException("Cannot spawn Ffmpeg process", ex);
        }

        if (!String.IsNullOrEmpty(lastError_))
        {
          if (String.IsNullOrEmpty(lastErrorSource_))
            throw new FfmpegException(lastError_);

          switch (lastErrorSource_)
          {
            case "getopt":
              throw new FfmpegException("Invalid parameter: " + lastError_);

            default:
              throw new FfmpegException("Processing error: " + lastError_);
          }
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


    private void OnFfmpegProcessOutputReceived(object sender, DataReceivedEventArgs received)
    {
      if (received.Data != null)
      {
        if (OnProgress != null)
        {
          Match matchProgress = FfmpegProcess.ProgressRegex.Match(received.Data);

          if (matchProgress.Success)
          {
            try
            {
              UInt16 progress = Convert.ToUInt16(double.Parse(matchProgress.Groups[1].Value, CultureInfo.InvariantCulture));
              TimeSpan processed = TimeSpan.ParseExact(matchProgress.Groups[2].Value, @"hh\:mm\:ss\.ff", CultureInfo.InvariantCulture);
              TimeSpan remaining = TimeSpan.ParseExact(matchProgress.Groups[3].Value, @"hh\:mm\:ss\.ff", CultureInfo.InvariantCulture);
              UInt64 outputSize = FormattedSize.ToUInt64(matchProgress.Groups[4].Value);

              ProgressEventArgs eventArgs = new ProgressEventArgs(progress, processed, remaining, outputSize);
              OnProgress(sender, eventArgs);

              if (eventArgs.Abort)
                Abort();

              return;
            }

            catch (OverflowException)
            {
              // Ffmpeg v14.3.1 (at least) sometimes report invalid time values (i.e. 06:31:60.00).
              // Just ignore this progress update.
              return;
            }

            catch (Exception ex)
            {
              throw new FfmpegException("Unexpected output from Ffmpeg", ex);
            }
          }
        }

        CheckForLogMessage(received.Data);
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


    protected bool CheckForLogMessage(string data)
    {
      if (string.IsNullOrEmpty(data))
        return false;

      Match logMatch = FfmpegProcess.LogRegex.Match(data);

      if (logMatch.Success)
      {
        string logLevel = logMatch.Groups[1].Value;
        string source = logMatch.Groups[2].Value;
        string message = logMatch.Groups[3].Value;

        if ("DBUG".Equals(logLevel) && (OnLogMessage != null))
          OnLogMessage(this, new LogMessageEventArgs(LogLevelType.Debug, source, message));

        if ("INFO".Equals(logLevel) && (OnLogMessage != null))
          OnLogMessage(this, new LogMessageEventArgs(LogLevelType.Info, source, message));

        if ("WARN".Equals(logLevel) && (OnLogMessage != null))
          OnLogMessage(this, new LogMessageEventArgs(LogLevelType.Warning, source, message));

        else if ("FAIL".Equals(logLevel))
        {
          if (String.IsNullOrEmpty(lastError_))
            lastError_ = message;

          if (String.IsNullOrEmpty(lastErrorSource_))
            lastErrorSource_ = source;
        }

        return true;
      }

      return false;
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
