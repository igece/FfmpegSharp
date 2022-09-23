using System;
using System.Collections.Generic;
using System.Globalization;


namespace FfmpegSharp
{
  /// <summary>
  /// Options that are applicable to both input and output files.
  /// </summary>
  public abstract class BaseFile
  {
    /// <summary>
    /// Input/output file URL. 
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Force file format. The format is normally auto detected for input files and guessed from the file
    /// extension for output files, so this option is not needed in most cases.
    /// </summary>
    public string Format { get; set; }

    /// <summary>
    /// Input file: Limit the duration of data read from the input file.
    /// Output file: Stop writing the output after its duration reaches this value.
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Input file: Seeks to specified position. Note that in most formats it is not possible to seek exactly,
    /// so FFmpeg will seek to the closest seek point before position. When transcoding and -accurate_seek is
    /// enabled (the default), this extra segment between the seek point and position will be decoded and discarded.
    /// When doing stream copy or when -noaccurate_seek is used, it will be preserved.
    /// Output file: Decodes but discards input until the timestamps reach position.
    /// </summary>
    public TimeSpan? Seek { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public TimeSpan? SeekFromEnd { get; set; }

    /// <summary>
    /// Custom file arguments.
    /// </summary>
    public readonly Dictionary<string, string> CustomArgs = new Dictionary<string, string>();



    protected readonly Dictionary<byte, MediaStream> streams_ = new Dictionary<byte, MediaStream>();


    /// <summary>
    /// Initializes a new instance of the <see cref="BaseFile"/> class.
    /// </summary>
    protected BaseFile(string url)
    {
      Url = url;
    }


    public void ResetStreams()
    {
      streams_.Clear();
    }


    /// <summary>
    /// Translate a <see cref="BaseFile"/> instance to a set of command arguments to be passed to Ffmpeg (invalidates <see cref="Object.ToString()"/>).
    /// </summary>
    /// <returns>String containing Ffmpeg command arguments.</returns>
    public override string ToString()
    {
      List<string> args = new List<string>();

      if (!String.IsNullOrEmpty(Format))
        args.Add("-f " + Format.ToLower());

      if (Duration.HasValue)
        args.Add("-t " + Duration.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture));

      if (Seek.HasValue)
        args.Add("-ss " + Seek.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture));

      if (SeekFromEnd.HasValue)
        args.Add("-sseof " + SeekFromEnd.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture));

      foreach (var customArg in CustomArgs)
      {
        if (string.IsNullOrEmpty(customArg.Value))
          args.Add("-" + customArg.Key);
        else
          args.Add(string.Format("-{0} {1}", customArg.Key, customArg.Value));
      }

      return string.Join(" ", args);
    }
  }
}
