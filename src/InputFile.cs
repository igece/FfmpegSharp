using System.Collections.Generic;
using FfmpegSharp.Exceptions;


namespace FfmpegSharp
{
  /// <summary>
  /// Input format options.
  /// </summary>
  public class InputFile : FileOptions
  {
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
    : base(url)
    {
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

      if (!string.IsNullOrEmpty(Url))
        inputOptions.Add(Url);
      else
        throw new FfmpegException("Input file cannot be null");

      return string.Join(" ", inputOptions);
    }
  }
}
