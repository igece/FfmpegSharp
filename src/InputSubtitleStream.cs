using System.Collections.Generic;


namespace FfmpegSharp
{
  public class InputSubtitleStream : InputStream, ISubtitle
  {
    public InputSubtitleStream()
      : base()
    {
    }


    public InputSubtitleStream(byte id)
      : base(id)
    {
    }


    /// <summary>
    /// Translate a <see cref="InputSubtitleStream"/> instance to a set of command arguments to be passed to FFmpeg.
    /// </summary>
    /// <returns>String containing FFmpeg command arguments.</returns>
    public override string ToString()
    {
      List<string> args = new List<string>();

      string baseStr = base.ToString();

      if (!string.IsNullOrEmpty(baseStr))
        args.Add(baseStr);

      return string.Join(" ", args);
    }
  }
}
