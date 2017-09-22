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
    /// File URL.
    /// </summary>
    public string Url { get; set; }

    public string Format { get; set; }

    public TimeSpan? Duration { get; set; }

    public TimeSpan? Seek { get; set; }

    public TimeSpan? SeekFromEnd { get; set; }

    /// <summary>
    /// Custom file arguments.
    /// </summary>
    public readonly Dictionary<string, string> CustomArgs = new Dictionary<string, string>();



    protected readonly Dictionary<byte, Stream> streams_ = new Dictionary<byte, Stream>();


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
