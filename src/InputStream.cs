using System.Collections.Generic;


namespace FfmpegSharp
{
  public abstract class InputStream : Stream
  {
    public InputStream()
      : base()
    {
    }


    public InputStream(byte id)
      : base(id)
    {
    }


    /// <summary>
    /// Translate a <see cref="InputStream"/> instance to a set of command arguments to be passed to Ffmpeg
    /// (adds additional command arguments to <see cref="Stream.ToString()"/>).
    /// </summary>
    /// <returns>String containing Ffmpeg command arguments.</returns>
    public override string ToString()
    {
      List<string> inputStreamArgs = new List<string>();

      string baseStr = base.ToString();

      if (!string.IsNullOrEmpty(baseStr))
        inputStreamArgs.Add(baseStr);

      return string.Join(" ", inputStreamArgs);
    }
  }
}
