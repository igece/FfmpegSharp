using System.Collections.Generic;


namespace FfmpegSharp
{
    public class InputAudioStream : InputMediaStream
    {
        public InputAudioStream()
          : base()
        {
        }


        public InputAudioStream(byte id)
          : base(id)
        {
        }


        /// <summary>
        /// Translate a <see cref="InputAudioStream"/> instance to a set of command arguments to be passed to FFmpeg.
        /// </summary>
        /// <returns>String containing FFmpeg command arguments.</returns>
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
