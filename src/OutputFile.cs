using System.Collections.Generic;
using System;
using FfmpegSharp.Exceptions;


namespace FfmpegSharp
{
  /// <summary>
  ///  Options to be applied to an output file.
  /// </summary>
  public class OutputFile : BaseFile
  {
    public OutputVideoStream Video { get; private set; }

    public string CustomFilters { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="OutputFile"/> class.
    /// </summary>
    public OutputFile(string url)
      : base(url)
    {
      Video = new OutputVideoStream();
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

      args.Add(Video.ToString());

      if (!String.IsNullOrEmpty(Url))
        args.Add(Url);
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
