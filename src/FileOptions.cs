using System;
using System.Collections.Generic;
using System.Globalization;


namespace FfmpegSharp
{
  /// <summary>
  /// Options that are applicable to both input and output files.
  /// </summary>
  public abstract class FileOptions
  {
    /// <summary>
    /// File URL.
    /// </summary>
    public string Url { get; set; }

    public string Format { get; set; }

    public TimeSpan? Duration { get; set; }

    public TimeSpan? To { get; set; }

    public TimeSpan? Seek { get; set; }

    public TimeSpan? SeekFromEnd { get; set; }

    /// <summary>
    /// Custom format arguments.
    /// </summary>
    public string CustomArgs { get; set; }


    /// <summary>
    /// Default constructor.
    /// </summary>
    protected FileOptions()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileOptions"/> class.
    /// </summary>
    protected FileOptions(string url)
    {
      Url = url;
    }


    /// <summary>
    /// Translate a <see cref="FileOptions"/> instance to a set of command arguments to be passed to Ffmpeg (invalidates <see cref="Object.ToString()"/>).
    /// </summary>
    /// <returns>String containing Ffmpeg command arguments.</returns>
    public override string ToString()
    {
      List<string> fileOptions = new List<string>();

      if (!String.IsNullOrEmpty(Format))
        fileOptions.Add("-f " + Format.ToLower());

      if (Duration.HasValue)
        fileOptions.Add("-t " + Duration.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture));

      if (Seek.HasValue)
        fileOptions.Add("-ss " + Seek.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture));

      if (SeekFromEnd.HasValue)
        fileOptions.Add("-sseof " + SeekFromEnd.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture));

      if (!String.IsNullOrEmpty(CustomArgs))
        fileOptions.Add(CustomArgs);

      return string.Join(" ", fileOptions);
    }
  }
}
