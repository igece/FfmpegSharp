using System;


namespace FfmpegSharp
{
    /// <summary>
    /// Provides data for the <see cref="Ffmpeg.OnProgress"/> event.
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a <see cref="T:FfmpegSharp.ProgressEventArgs"/> instance with the provided values.
        /// </summary>
        /// <param name="frames">Frames currently processed.</param>
        /// <param name="fps">Processed frames per second.</param>
        /// <param name="q"></param>
        /// <param name="size">Actual size of the generated output file.</param>
        /// <param name="time">File time that has been processed.</param>
        /// <param name="bitrate">Current bitrate.</param>
        public ProgressEventArgs(UInt32 frames, double fps, double q, UInt64 size, TimeSpan time, double bitrate)
        {
            Frames = frames;
            Fps = fps;
            Q = q;
            Size = size;
            Time = time;
            Bitrate = bitrate;
            Abort = false;
        }


        public UInt32 Frames { get; private set; }

        public double Fps { get; private set; }

        public double Q { get; private set; }

        public UInt64 Size { get; private set; }

        /// <summary>
        /// File time that has been processed.
        /// </summary>
        public TimeSpan Time { get; private set; }

        public double Bitrate { get; private set; }

        /// <summary>
        /// Allows to cancel the current operation.
        /// </summary>
        /// <value><c>true</c> to cancel; otherwise, leave as <c>false</c>.</value>
        public bool Abort { get; set; }
    }
}
