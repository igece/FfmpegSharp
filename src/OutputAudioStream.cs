using System.Collections.Generic;


namespace FfmpegSharp
{
  public class OutputAudioStream : OutputStream, IAudio
  {
    public OutputAudioStream()
      : base()
    {
    }


    public OutputAudioStream(byte id)
      : base(id)
    {
    }


    /// <summary>
    /// Translate a <see cref="OutputAudioStream"/> instance to a set of command arguments to be passed to Ffmpeg.
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
