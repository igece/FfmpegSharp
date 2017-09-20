using System.Collections.Generic;


namespace FfmpegSharp
{
  public abstract class OutputStream : Stream, IOutput
  {
    public int? MaxFrames { get; set; }

    public StreamDisposition? Disposition { get; set; }


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
      List<string> args = new List<string>();

      string baseStr = base.ToString();

      if (!string.IsNullOrEmpty(baseStr))
        args.Add(baseStr);

      if (Disposition.HasValue)
      {
        string dispositionStr = string.Empty;

        switch (Disposition.Value)
        {
          case StreamDisposition.Remove: dispositionStr = "0"; break;
          case StreamDisposition.Default: dispositionStr = "default"; break;
          case StreamDisposition.Dub: dispositionStr = "dub"; break;
          case StreamDisposition.Original: dispositionStr = "original"; break;
          case StreamDisposition.Comment: dispositionStr = "comment"; break;
          case StreamDisposition.Lyrics: dispositionStr = "lyrics"; break;
          case StreamDisposition.Karaoke: dispositionStr = "karaoke"; break;
          case StreamDisposition.Forced: dispositionStr = "forced"; break;
          case StreamDisposition.HearingImpaired: dispositionStr = "hearing_impaired"; break;
          case StreamDisposition.VisualImpaired: dispositionStr = "visual_impaired"; break;
          case StreamDisposition.CleanEffects: dispositionStr = "clean_effects"; break;
          case StreamDisposition.Captions: dispositionStr = "captions"; break;
          case StreamDisposition.Descriptions: dispositionStr = "descriptions"; break;
          case StreamDisposition.Metadata: dispositionStr = "metadata"; break;
        }

        args.Add(string.Format("-disposition:{0} {1}", IdStr, dispositionStr));
      }

      if (MaxFrames.HasValue)
        args.Add(string.Format("-frames:{0} {1}", IdStr, MaxFrames.Value));

      return string.Join(" ", args);
    }
  }
}
