using System;
using FfmpegSharp;
using FfmpegSharp.Exceptions;
using System.Collections.Generic;


namespace Test
{
  internal class Test
  {
    public static void Main(string[] args)
    {
      using (Ffmpeg ffmpeg = new Ffmpeg(@"/usr/local/bin/ffmpeg"))
      {
        ffmpeg.OnProgress += (sender, eventArgs) =>
        {
          Console.Write("Processing... {0}    {1} Kb             \r",
                        eventArgs.Time.ToString(@"hh\:mm\:ss\.ff"),
                        eventArgs.Size / 1024);
        };

        Console.WriteLine("FfmpegSharp Test App\n");


        Console.WriteLine("File Information");

        MediaInfo mediaInfo = ffmpeg.GetInfo("big_buck_bunny_1080p_h264.mov");
        Console.WriteLine(mediaInfo);


        Console.WriteLine("Simple Conversion");

        try
        {
          OutputFile outFile = new OutputFile("Big Buck Bunny.mp4");
          outFile.Video.Codec = "libx264";
          outFile.Video.CustomArgs["crf"] = "25";
          outFile.Video.CustomArgs["preset"] = "veryfast";

          ffmpeg.OverwriteOutput = true;
          ffmpeg.Process("big_buck_bunny_1080p_h264.mov", outFile);
          //ffmpeg.Process("big_buck_bunny_1080p_h264.mov", "Big Buck Bunny.mp4");
        }

        catch (FfmpegException ex)
        {
          Console.WriteLine(ex.Message);
        }

        Console.WriteLine("\n" + ffmpeg.LastCommand);
        Console.WriteLine("\nConversion finished");
        //Console.ReadKey();
      }
    }
  }
}
