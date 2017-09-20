using System;
using System.Text;


namespace FfmpegSharp
{
  /// <summary>
  /// Provides information about an audio file.
  /// </summary>
  public struct MediaInfo
  {
    /// <summary>
    /// Media time length. 
    /// </summary>
    public readonly TimeSpan? Duration;


    /// <summary>
    /// Initializes a new instance of the <see cref="MediaInfo"/> class. 
    /// </summary>
    /// <param name="channels">Number of audio channels.</param>
    /// <param name="sampleRate">Audio sample rate.</param>
    /// <param name="sampleSize">Audio sample size (bits).</param>
    /// <param name="duration">Audio time length.</param>
    /// <param name="size">Audio file size</param>
    /// <param name="bitRate"></param>
    /// <param name="format">Audio format.</param>
    public MediaInfo(TimeSpan? duration)
    {
      Duration = duration;
    }


    /// <summary>
    /// Returns information about the <see cref="MediaInfo"/> instance (invalidates <see cref="Object.ToString()"/>).
    /// </summary>
    /// <returns>String containing all <see cref="MediaInfo"/> instance properties values.</returns>
    public override string ToString()
    {
      StringBuilder result = new StringBuilder();

      if (Duration.HasValue)
        result.AppendLine("Duration: " + Duration.Value.ToString(@"hh\:mm\:ss\.ff"));
      else
        result.AppendLine("Duration: N/A");

      return result.ToString();
    }
  }
}
