using System.Collections.Generic;


namespace FfmpegSharp
{
  /// <summary>
  /// Input format options.
  /// </summary>
  public class InputFile : FormatOptions
  {
    /// <summary>
    /// Input file URL.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///Force input file format. The format is normally auto detected for input files, so this option is not needed in most cases.
    /// </summary>
    public string Format { get; set; }

    /// <summary>
    /// Number of times input stream shall be looped. Loop 0 means no loop, loop -1 means infinite loop.
    /// </summary>
    public int? Loop { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="InputFile"/> class.
    /// </summary>
    public InputFile()
    : base()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="InputFile"/> class.
    /// </summary>
    public InputFile(string url)
    : base()
    {
      Url = url;
    }


    /// <summary>
    /// Translate a <see cref="InputFile"/> instance to a set of command arguments to be passed to Ffmpeg to be applied to the input URL (invalidates <see cref="object.ToString()"/>).
    /// </summary>
    /// <returns>String containing Ffmpeg command arguments.</returns>
    public override string ToString()
    {
      List<string> inputOptions = new List<string>(4);

      string baseStr = base.ToString();

      if (!string.IsNullOrEmpty(baseStr))
        inputOptions.Add(baseStr);

      if (Volume.HasValue)
        inputOptions.Add("--volume " + Volume.Value);

      if (IgnoreLength.HasValue && (IgnoreLength.Value == true))
        inputOptions.Add("--ignore-length");

      if (FileName != null)
        inputOptions.Add(FileName);
      else
        inputOptions.Add("--null");

      return string.Join(" ", inputOptions);
    }
  }
}
