using System.Collections.Generic;
using FfmpegSharp.Exceptions;
using System;


namespace FfmpegSharp
{
  /// <summary>
  /// Input format options.
  /// </summary>
  public class InputFile : BaseFile, IInput
  {
    public TimeSpan? Offset { get; set; }

    public InputVideoStream Video { get; private set; }
    public InputAudioStream Audio { get; private set; }
    public InputSubtitleStream Subtitles { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="InputFile"/> class.
    /// </summary>
    public InputFile(string url)
    : base(url)
    {
      Video = new InputVideoStream();
      Audio = new InputAudioStream();
      Subtitles = new InputSubtitleStream();
    }


    public void SetStream(InputStream stream)
    {
      if (stream.Id.HasValue)
        streams_[stream.Id.Value] = stream;
      else
        throw new FfmpegException("Global stream configuration not allowed in streams list");
    }


    /// <summary>
    /// Translate a <see cref="InputFile"/> instance to a set of command arguments to be passed to Ffmpeg to be applied to the input URL (invalidates <see cref="object.ToString()"/>).
    /// </summary>
    /// <returns>String containing Ffmpeg command arguments.</returns>
    public override string ToString()
    {
      List<string> args = new List<string>();

      string baseStr = base.ToString();

      if (!string.IsNullOrEmpty(baseStr))
        args.Add(baseStr);

      args.Add(Video.ToString());
      args.Add(Audio.ToString());
      args.Add(Subtitles.ToString());

      if (Offset.HasValue)
        args.Add("-itsoffset " + Offset.Value.TotalSeconds);

      if (!string.IsNullOrEmpty(Url))
      {
        if (Url.Contains(" "))
          args.Add("-i \"" + Url + "\"");
        else
          args.Add("-i " + Url);
      }
      else
        throw new FfmpegException("Input file cannot be null");

      return string.Join(" ", args);
    }
  }
}
