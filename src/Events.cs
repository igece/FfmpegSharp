using System;


namespace FfmpegSharp
{
  /// <summary>
  /// Provides data for the <see cref="Ffmpeg.OnLogMessage"/> event. 
  /// </summary>
  public class LogMessageEventArgs : EventArgs
  {
    /// <summary>
    /// Initializes a <see cref="T:FfmpegSharp.LogMessageEventArgs"/> instance with the provided values.
    /// </summary>
    /// <param name="logLevel">Message severity.</param>
    /// <param name="source">Message source.</param>
    /// <param name="message">Message text.</param>
    public LogMessageEventArgs(LogLevelType logLevel, string source, string message)
    {
      LogLevel = logLevel;
      Source = source;
      Message = message;
    }

    /// <summary>
    /// Message severity.
    /// </summary>
    public LogLevelType LogLevel { get; private set; }

    /// <summary>
    /// Ffmpeg logger module.
    /// </summary>
    public string Source { get; private set; }

    /// <summary>
    /// Message text.
    /// </summary>
    public string Message { get; private set; }
  }


  /// <summary>
  /// Provides data for the <see cref="Ffmpeg.OnProgress"/> event.
  /// </summary>
  public class ProgressEventArgs : EventArgs
  {
    /// <summary>
    /// Initializes a <see cref="T:FfmpegSharp.ProgressEventArgs"/> instance with the provided values.
    /// </summary>
    /// <param name="progress">The actual progress value, from 0 to 100.</param>
    /// <param name="processed">File time that has been processed, based on file total duration.</param>
    /// <param name="remaining">File time pending to be processed, based on file total duration.</param>
    /// <param name="outputSize">Actual size of the generated output file.</param>
    public ProgressEventArgs(UInt32 frames, UInt16 fps, double q, UInt64 size, TimeSpan time, double bitrate)
    {
      Frames = frames;
      Fps = fps;
      Q = q;
      Size = size;
      Time = time;
      Bitrate = bitrate;
      Abort = false;
    }


    public UInt32 Frames { get; private set; }

    public UInt16 Fps { get; private set; }

    public double Q { get; private set; }

    public UInt64 Size { get; private set; }

    /// <summary>
    /// File time that has been processed.
    /// </summary>
    public TimeSpan Time { get; private set; }

    public double Bitrate { get; private set; }

    /// <summary>
    /// Allows to cancel the current operation.
    /// </summary>
    /// <value><c>true</c> to cancel; otherwise, leave as <c>false</c>.</value>
    public bool Abort { get; set; }
  }
}
