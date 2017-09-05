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
    /// Size of all processing buffers (default is 8192).
    /// </summary>
    public uint? Buffer { get; set; }

    /// <summary>
    /// Enable parallel effects channels processing (where available).
    /// </summary>
    public bool? Multithreaded { get; set; }

    /// <summary>
    /// Output format options.
    /// </summary>
    public OutputFormatOptions Output { get; private set; }

    /// <summary>
    /// Effects to be applied.
    /// </summary>
    public List<IBaseEffect> Effects { get; private set; }

    /// <summary>
    /// Custom global arguments.
    /// </summary>
    public string CustomArgs { get; set; }

    /// <summary>
    /// Custom effects. Add here the command line arguments for any effect not currently implemented in SoXSharp.
    /// </summary>
    public string CustomEffects { get; set; }

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
      Output = new OutputFormatOptions();
      Effects = new List<IBaseEffect>();
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
    /// Spawns a new Ffmpeg process using the specified options in this instance and plays the specified file.
    /// </summary>
    /// <param name="inputFile">Audio file to be played.</param>
    public void Play(string inputFile)
    {
      Process(new InputFile[] { new InputFile(inputFile) }, "--default-device");
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance and plays the specified file.
    /// </summary>
    /// <param name="inputFile">Audio file to be played.</param>
    public void Play(InputFile inputFile)
    {
      Process(new InputFile[] { inputFile }, "--default-device");
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance and plays the specified files.
    /// </summary>
    /// <param name="inputFiles">Audio files to be played.</param>
    public void Play(string[] inputFiles)
    {
      var inputs = new List<InputFile>(inputFiles.Length);

      foreach (var inputFile in inputFiles)
        inputs.Add(new InputFile(inputFile));

      Process(inputs.ToArray(), "--default-device");
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance and plays the specified files.
    /// </summary>
    /// <param name="inputFiles">Audio files to be played.</param>
    /// <param name="combination">How to combine the input files.</param>
    public void Play(string[] inputFiles, CombinationType combination)
    {
      var inputs = new List<InputFile>(inputFiles.Length);

      foreach (var inputFile in inputFiles)
        inputs.Add(new InputFile(inputFile));

      Process(inputs.ToArray(), "--default-device", combination);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance and plays the specified files.
    /// </summary>
    /// <param name="inputFiles">Audio files to be played.</param>
    public void Play(InputFile[] inputFiles)
    {
      Process(inputFiles, "--default-device");
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance and plays the specified files.
    /// </summary>
    /// <param name="inputFiles">Audio files to be played.</param>
    /// <param name="combination">How to combine the input files.</param>
    public void Play(InputFile[] inputFiles, CombinationType combination)
    {
      Process(inputFiles, "--default-device", combination);
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
    public void Process(InputFile inputFile)
    {
      Process(new InputFile[] { inputFile }, null);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile">Audio file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(string inputFile, string outputFile)
    {
      Process(new InputFile[] { new InputFile(inputFile) }, outputFile);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile">Audio file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(InputFile inputFile, string outputFile)
    {
      Process(new InputFile[] { inputFile }, outputFile);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile1">First audio file to be processed.</param>
    /// <param name="inputFile2">Second audio file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(string inputFile1, string inputFile2, string outputFile)
    {
      Process(new InputFile[] { new InputFile(inputFile1), new InputFile(inputFile2) }, outputFile);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile1">First audio file to be processed.</param>
    /// <param name="inputFile2">Second audio file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    /// <param name="combination">How to combine the input files.</param>
    public void Process(string inputFile1, string inputFile2, string outputFile, CombinationType combination)
    {
      Process(new InputFile[] { new InputFile(inputFile1), new InputFile(inputFile2) }, outputFile, combination);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile1">First audio file to be processed.</param>
    /// <param name="inputFile2">Second audio file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(InputFile inputFile1, InputFile inputFile2, string outputFile)
    {
      Process(new InputFile[] { inputFile1, inputFile2 }, outputFile);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFile1">First audio file to be processed.</param>
    /// <param name="inputFile2">Second audio file to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    /// <param name="combination">How to combine the input files.</param>
    public void Process(InputFile inputFile1, InputFile inputFile2, string outputFile, CombinationType combination)
    {
      Process(new InputFile[] { inputFile1, inputFile2 }, outputFile, combination);
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

      Process(inputs.ToArray(), outputFile);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFiles">Audio files to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    /// <param name="combination">How to combine the input files.</param>
    public void Process(string[] inputFiles, string outputFile, CombinationType combination)
    {
      var inputs = new List<InputFile>(inputFiles.Length);

      foreach (var inputFile in inputFiles)
        inputs.Add(new InputFile(inputFile));

      Process(inputs.ToArray(), outputFile, combination);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFiles">Audio files to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    public void Process(InputFile[] inputFiles, string outputFile)
    {
      Process(inputFiles, outputFile, CombinationType.Default);
    }


    /// <summary>
    /// Spawns a new Ffmpeg process using the specified options in this instance.
    /// </summary>
    /// <param name="inputFiles">Audio files to be processed.</param>
    /// <param name="outputFile">Output file.</param>
    /// <param name="combination">How to combine the input files.</param>
    public void Process(InputFile[] inputFiles, string outputFile, CombinationType combination)
    {
      FfmpegProcess_ = FfmpegProcess.Create(Path);

      lastError_ = null;
      lastErrorSource_ = null;

      try
      {
        FfmpegProcess_.ErrorDataReceived += OnFfmpegProcessOutputReceived;
        FfmpegProcess_.OutputDataReceived += OnFfmpegProcessOutputReceived;

        List<string> args = new List<string>();

        // Global options.

        if (Buffer.HasValue)
          args.Add("--buffer " + Buffer.Value);

        if (Multithreaded.HasValue)
          args.Add(Multithreaded.Value ? "--multi-threaded" : "--single-threaded");

        if (!String.IsNullOrEmpty(CustomArgs))
          args.Add(CustomArgs);

        switch (combination)
        {
          case CombinationType.Concatenate:
            args.Add("--combine concatenate");
            break;

          case CombinationType.Merge:
            args.Add("--combine merge");
            break;

          case CombinationType.Mix:
            args.Add("--combine mix");
            break;

          case CombinationType.MixPower:
            args.Add("--combine mix-power");
            break;

          case CombinationType.Multiply:
            args.Add("--combine multiply");
            break;

          case CombinationType.Sequence:
            args.Add("--combine sequence");
            break;
        }

        args.Add("--show-progress");

        // Input options and files.

        if ((inputFiles != null) && (inputFiles.Length > 0))
        {
          foreach (InputFile inputFile in inputFiles)
            args.Add(inputFile.ToString());
        }
        else
          args.Add("--null");

        // Output options and file.

        args.Add(Output.ToString());

        if (outputFile != null)
          args.Add(outputFile);
        else
          args.Add("--null");

        // Effects.
        foreach (IBaseEffect effect in Effects)
          args.Add(effect.ToString());

        // Custom effects.
        args.Add(CustomEffects);

        FfmpegProcess_.StartInfo.Arguments = String.Join(" ", args);
        LastCommand = Path + " " + FfmpegProcess_.StartInfo.Arguments;

        try
        {
          FfmpegProcess_.Start();
          FfmpegProcess_.BeginOutputReadLine();
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
