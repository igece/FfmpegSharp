using System;
using System.Collections.Generic;

using FfmpegSharp.Enums;
using FfmpegSharp.Exceptions;


namespace FfmpegSharp
{
    public abstract class MediaStream
    {
        public MediaStreamType Type { get; set; }


        public byte? Id
        {
            get { return id_; }

            set
            {
                id_ = value;

                if (id_.HasValue)
                {
                    switch (Type)
                    {
                        case MediaStreamType.Audio:
                            IdStr = ":a:" + Id.Value.ToString();
                            break;

                        case MediaStreamType.Video:
                            IdStr = ":v:" + Id.Value.ToString();
                            break;

                        case MediaStreamType.Subtitle:
                            IdStr = ":s:" + Id.Value.ToString();
                            break;

                        default:
                            throw new FfmpegException("Invalid stream type");
                    }
                }

                else
                {
                    switch (Type)
                    {
                        case MediaStreamType.Audio:
                            IdStr = ":a:";
                            break;

                        case MediaStreamType.Video:
                            IdStr = ":v:";
                            break;

                        case MediaStreamType.Subtitle:
                            IdStr = ":s:";
                            break;

                        default:
                            throw new FfmpegException("Invalid stream type");
                    }
                }
            }
        }

        public string Codec { get; set; }

        public readonly Dictionary<string, string> CustomArgs = new Dictionary<string, string>();

        protected string IdStr { get; set; }


        private byte? id_;


        protected MediaStream()
        {
            Id = null;
        }


        protected MediaStream(byte id)
        {
            Id = id;
        }


        /// <summary>
        /// Translate a <see cref="MediaStream"/> instance to a set of command arguments to be passed to Ffmpeg.
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
