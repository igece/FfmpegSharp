using System.Collections.Generic;


namespace FfmpegSharp
{
  public abstract class InputStream : Stream, IInput
  {
    protected InputStream()
      : base()
    {
    }


    protected InputStream(byte id)
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
      List<string> args = new List<string>();

      string baseStr = base.ToString();

      if (!string.IsNullOrEmpty(baseStr))
        args.Add(baseStr);

      return string.Join(" ", args);
    }
  }
}
