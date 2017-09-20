using System;


namespace FfmpegSharp.Exceptions
{
  /// <summary>
  /// Generic Ffmpeg exception.
  /// </summary> 
  public class FfmpegFatalErrorException : FfmpegException
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:FfmpegSharp.FfmpegException"/> class with its message string set
    /// to a default message.
    /// </summary>
    public FfmpegFatalErrorException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:FfmpegSharp.FfmpegException"/> class with a specified message.
    /// </summary>
    /// <param name="message">The exception's message.</param>
    public FfmpegFatalErrorException(string source, string message)
      : base(message)
    {
      Source = source;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:FfmpegSharp.FfmpegException"/> class with a specified message and a
    /// reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The exception's message.</param>
    /// <param name="inner">Exception that caused it.</param>
    public FfmpegFatalErrorException(string source, string message, Exception inner)
      : base(message, inner)
    {
      Source = source;
    }
  }
}
