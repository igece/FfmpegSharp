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
    public TimeSpan? To { get; set; }


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
          case TargetType.Dvd: args.Add("-target dvd"); break;
          case TargetType.Dv: args.Add("-target dv"); break;
          case TargetType.Dv50: args.Add("-target dv50"); break;
          case TargetType.PalVcd: args.Add("-target pal-vcd"); break;
          case TargetType.PalSvcd: args.Add("-target pal-svcd"); break;
          case TargetType.PalDvd: args.Add("-target pal-dvd"); break;
          case TargetType.PalDv: args.Add("-target pal-dv"); break;
          case TargetType.PalDv50: args.Add("-target pal-dv50"); break;
          case TargetType.NtscVcd: args.Add("-target ntsc-vcd"); break;
          case TargetType.NtscSvcd: args.Add("-target ntsc-svcd"); break;
          case TargetType.NtscDvd: args.Add("-target ntsc-dvd"); break;
          case TargetType.NtscDv: args.Add("-target ntsc-dv"); break;
          case TargetType.NtscDv50: args.Add("-target ntsc-dv50"); break;
          case TargetType.FilmVcd: args.Add("-target film-vcd"); break;
          case TargetType.FilmSvcd: args.Add("-target film-svcd"); break;
          case TargetType.FilmDvd: args.Add("-target film-dvd"); break;
          case TargetType.FilmDv: args.Add("-target film-dv"); break;
          case TargetType.FilmDv50: args.Add("-target film-dv50"); break;
        }
      }

      if (Timestamp.HasValue)
        args.Add("-timestamp " + Timestamp.Value.ToString("yyyy-MM-dd HH:mm:ss.ff"));

      if (To.HasValue)
        args.Add("-to " + To.Value.TotalSeconds);

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
        if ((Environment.OSVersion.Platform == PlatformID.Win32NT) ||
            (Environment.OSVersion.Platform == PlatformID.Win32Windows) ||
            (Environment.OSVersion.Platform == PlatformID.Win32S) ||
            (Environment.OSVersion.Platform == PlatformID.WinCE))
          args.Add("NUL");
        else
          args.Add("/dev/null");
      }

      return string.Join(" ", args);
    }
  }
}
