using System.Collections.Generic;


namespace FfmpegSharp
{
  public class OutputSubtitleStream : OutputStream, ISubtitle
  {
    public OutputSubtitleStream()
      : base()
    {
    }


    public OutputSubtitleStream(byte id)
      : base(id)
    {
    }


    /// <summary>
    /// Translate a <see cref="OutputSubtitleStream"/> instance to a set of command arguments to be passed to Ffmpeg.
    /// </summary>
    /// <returns>String containing Ffmpeg command arguments.</returns>
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
