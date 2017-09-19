using System.Collections.Generic;
using FfmpegSharp.Exceptions;


namespace FfmpegSharp
{
  /// <summary>
  /// Input format options.
  /// </summary>
  public class InputFile : BaseFile
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="InputFile"/> class.
    /// </summary>
    public InputFile(string url)
    : base(url)
    {
    }


    public void SetStream(InputStream stream)
    {
      if (stream.Id.HasValue)
        streams_[stream.Id.Value] = stream;
      else
        throw new FfmpegException("Global stream configuration not allowed in streams list");
    }


    /// <summary>
    /// Translate a <see cref="InputFile"/> instance to a set of command arguments to be passed to Ffmpeg to be applied to the input URL (invalidates <see cref="object.ToString()"/>).
    /// </summary>
    /// <returns>String containing Ffmpeg command arguments.</returns>
    public override string ToString()
    {
      List<string> args = new List<string>();

      string baseStr = base.ToString();

      if (!string.IsNullOrEmpty(baseStr))
        args.Add(baseStr);

      if (!string.IsNullOrEmpty(Url))
        args.Add("-i " + Url);
      else
        throw new FfmpegException("Input file cannot be null");

      return string.Join(" ", args);
    }
  }
}
