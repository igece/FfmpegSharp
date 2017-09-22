using System;
using System.Collections.Generic;
using FfmpegSharp.Exceptions;


namespace FfmpegSharp
{
  public abstract class Stream
  {
    public byte? Id
    {
      get { return id_; }

      set
      {
        id_ = value;

        if (id_.HasValue)
        {
          if (this is IVideo)
            IdStr = ":v:" + Id.Value.ToString();
          else if (this is IAudio)
            IdStr = ":a:" + Id.Value.ToString();
          else if (this is ISubtitle)
            IdStr = ":s:" + Id.Value.ToString();
          else
            throw new FfmpegException("Invalid stream type");
        }
        else
        {
          if (this is IVideo)
            IdStr = ":v";
          else if (this is IAudio)
            IdStr = ":a";
          else if (this is ISubtitle)
            IdStr = ":s";
          else
            throw new FfmpegException("Invalid stream type");
        }
      }
    }

    public string Codec { get; set; }

    public readonly Dictionary<string, string> CustomArgs = new Dictionary<string, string>();

    protected string IdStr { get; set; }


    private byte? id_;


    protected Stream()
    {
      Id = null;
    }


    protected Stream(byte id)
    {
      Id = id;
    }


    /// <summary>
    /// Translate a <see cref="Stream"/> instance to a set of command arguments to be passed to Ffmpeg.
    /// </summary>
    /// <returns>String containing Ffmpeg command arguments.</returns>
    public override string ToString()
    {
      List<string> args = new List<string>();

      if (!String.IsNullOrEmpty(Codec))
        args.Add(String.Format("-codec{0} {1}", IdStr, Codec));

      foreach (var customArg in CustomArgs)
      {
        if (string.IsNullOrEmpty(customArg.Value))
          args.Add("-" + customArg.Key);
        else
          args.Add(string.Format("-{0} {1}", customArg.Key, customArg.Value));
      }

      return string.Join(" ", args);
    }
  }
}
