using System.Collections.Generic;


namespace FfmpegSharp
{
    public abstract class InputMediaStream : MediaStream
    {
        protected InputMediaStream()
          : base()
        {
        }


        protected InputMediaStream(byte id)
          : base(id)
        {
        }


        /// <summary>
        /// Translate a <see cref="InputMediaStream"/> instance to a set of command arguments to be passed to Ffmpeg
        /// (adds additional command arguments to <see cref="MediaStream.ToString()"/>).
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
