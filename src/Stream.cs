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
          if (this is OutputVideoStream)
            IdStr = "v:" + Id.Value.ToString();
          else if (this is OutputAudioStream)
            IdStr = "a:" + Id.Value.ToString();
          else if (this is OutputSubtitleStream)
            IdStr = "s" + Id.Value.ToString();
          else
            throw new FfmpegException("Invalid stream type");
        }
        else
        {
          if (this is OutputVideoStream)
            IdStr = "v";
          else if (this is OutputAudioStream)
            IdStr = "a";
          else if (this is OutputSubtitleStream)
            IdStr = "s";
          else
            throw new FfmpegException("Invalid stream type");
        }
      }
    }

    public string Codec { get; set; }

    protected string IdStr { get; set; }


    private byte? id_;


    public Stream()
    {
      Id = null;
    }


    public Stream(byte id)
    {
      Id = id;
    }


    /// <summary>
    /// Translate a <see cref="Stream"/> instance to a set of command arguments to be passed to Ffmpeg.
    /// </summary>
    /// <returns>String containing Ffmpeg command arguments.</returns>
    public override string ToString()
    {
      List<string> streamArgs = new List<string>();

      if (!String.IsNullOrEmpty(Codec))
        streamArgs.Add(String.Format("-codec:{0} {1}", IdStr, Codec));

      return string.Join(" ", streamArgs);
    }
  }
}
