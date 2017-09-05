using System;
using FfmpegSharp;
using FfmpegSharp.Effects;
using FfmpegSharp.Exceptions;


namespace Test
{
  internal class Test
  {
    public static void Main(string[] args)
    {
      using (Ffmpeg sox = new Ffmpeg(@"D:\bin\sox\sox.exe"))
      {
        sox.OnLogMessage += (sender, e) =>
        {
          Console.WriteLine(e.LogLevel + ": " + e.Message);
        };

        sox.OnProgress += (sender, e) =>
        {
          Console.Write("Processing... {0}%   {1} {2} {3}                \r",
                        e.Progress,
                        e.Processed.ToString(@"hh\:mm\:ss\.ff"),
                        e.Remaining.ToString(@"hh\:mm\:ss\.ff"),
                        e.OutputSize);
        };

        Console.WriteLine("SoXSharp Test App\n");

        Console.WriteLine("File Information");

        MediaInfo wavInfo = sox.GetInfo("test.wav");
        Console.WriteLine(wavInfo);

        Console.WriteLine("Simple Conversion");

        sox.CustomArgs = "-V4";
        sox.Effects.Add(new VolumeEffect(6.2));
        sox.Output.Type = FileType.FLAC;
        sox.Output.Comment = "Converted using SoX & SoXSharp";

        try
        {
          sox.Process("test.wav", "test.flac");
        }
        catch (FfmpegException ex)
        {
          Console.WriteLine(ex.Message);
        }

        Console.WriteLine("\n" + sox.LastCommand);

        Console.WriteLine("\nConversion finished");
        //Console.ReadKey();
      }
    }
  }
}
