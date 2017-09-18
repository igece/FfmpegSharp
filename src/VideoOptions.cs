using System;
using System.Collections.Generic;


namespace FfmpegSharp
{
  public class VideoOptions
  {
    public string Codec { get; set; }

    public Dictionary<int, VideoOptions> Streams;


    public VideoOptions()
    {
      Streams = new Dictionary<int, VideoOptions>();
    }


    /// <summary>
    /// Translate a <see cref="VideoOptions"/> instance to a set of command arguments to be passed to Ffmpeg.
    /// </summary>
    /// <returns>String containing Ffmpeg command arguments.</returns>
    public override string ToString()
    {
      List<string> videoOptions = new List<string>();

      return string.Join(" ", videoOptions);
    }
  }
}
