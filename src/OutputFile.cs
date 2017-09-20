using System.Collections.Generic;
using System;
using FfmpegSharp.Exceptions;


namespace FfmpegSharp
{
  /// <summary>
  ///  Options to be applied to an output file.
  /// </summary>
  public class OutputFile : BaseFile, IOutput
  {
    public TargetType? Target { get; set; }
    public DateTime? Timestamp { get; set; }

    public OutputVideoStream Video { get; private set; }
    public OutputAudioStream Audio { get; private set; }
    public OutputSubtitleStream Subtitles { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="OutputFile"/> class.
    /// </summary>
    public OutputFile(string url)
      : base(url)
    {
      Video = new OutputVideoStream();
      Audio = new OutputAudioStream();
      Subtitles = new OutputSubtitleStream();
    }


    public void SetStream(OutputStream stream)
    {
      if (stream.Id.HasValue)
        streams_[stream.Id.Value] = stream;
      else
        throw new FfmpegException("Global stream configuration not allowed in streams list");
    }


    /// <summary>
    /// Translate a <see cref="OutputFile"/> instance to a set of command arguments to be passed to Ffmpeg (adds additional command arguments to <see cref="BaseFile.ToString()"/>).
    /// </summary>
    /// <returns>String containing Ffmpeg command arguments.</returns>
    public override string ToString()
    {
      List<string> args = new List<string>();

      string baseStr = base.ToString();

      if (!string.IsNullOrEmpty(baseStr))
        args.Add(baseStr);

      if (Target.HasValue)
      {
        switch (Target.Value)
        {
          case TargetType.Vcd: args.Add("-target vcd"); break;
          case TargetType.Svcd: args.Add("-target svcd"); break;
        }
      }

      if (Timestamp.HasValue)
        args.Add("-timestamp " + Timestamp.Value.ToString("yyyy-MM-dd HH:mm:ss.ff"));

      args.Add(Video.ToString());
      args.Add(Audio.ToString());
      args.Add(Subtitles.ToString());

      if (!String.IsNullOrEmpty(Url))
      {
        if (Url.Contains(" "))
          args.Add("\"" + Url + "\"");
        else
          args.Add(Url);
      }
      else
      {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
          args.Add("NUL");
        else
          args.Add("/dev/null");
      }

      return string.Join(" ", args);
    }
  }
}
