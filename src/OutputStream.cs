using System;
using System.Collections.Generic;


namespace FfmpegSharp
{
  public abstract class OutputStream : Stream
  {
    public int? MaxFrames { get; set; }


    public OutputStream()
      : base()
    {
    }


    public OutputStream(byte id)
      : base(id)
    {
    }


    /// <summary>
    /// Translate a <see cref="OutputStream"/> instance to a set of command arguments to be passed to Ffmpeg
    /// (adds additional command arguments to <see cref="Stream.ToString()"/>).
    /// </summary>
    /// <returns>String containing Ffmpeg command arguments.</returns>
    public override string ToString()
    {
      List<string> outputStreamArgs = new List<string>();

      string baseStr = base.ToString();

      if (!string.IsNullOrEmpty(baseStr))
        outputStreamArgs.Add(baseStr);

      if (MaxFrames.HasValue)
        outputStreamArgs.Add(String.Format("-frames:{0} {1}", IdStr, MaxFrames.Value));

      return string.Join(" ", outputStreamArgs);
    }
  }
}
