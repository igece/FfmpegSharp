using System;


namespace FfmpegSharp.Events
{
    public class OutputDataEventArgs : EventArgs
    {
        public byte[] Data { get; private set; }


        public OutputDataEventArgs(byte[] data)
        {
            Data = data;
        }
    }
}

