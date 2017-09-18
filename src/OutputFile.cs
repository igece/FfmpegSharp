using System.Collections.Generic;
using System;


namespace FfmpegSharp
{
  /// <summary>
  ///  Options to be applied to an output file.
  /// </summary>
  public class OutputFile : FileOptions
  {
    public string CustomFilters { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="OutputFile"/> class.
    /// </summary>
    public OutputFile()
    : base()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="OutputFile"/> class.
    /// </summary>
    public OutputFile(string url)
    : base(url)
    {
    }


    /// <summary>
    /// Translate a <see cref="OutputFile"/> instance to a set of command arguments to be passed to Ffmpeg (adds additional command arguments to <see cref="FileOptions.ToString()"/>).
    /// </summary>
    /// <returns>String containing Ffmpeg command arguments.</returns>
    public override string ToString()
    {
      List<string> outputOptions = new List<string>(4);

      string baseStr = base.ToString();

      if (!string.IsNullOrEmpty(baseStr))
        outputOptions.Add(baseStr);

      // Filters.
      /*
      foreach (IBaseFilter filter in Filters)
				outputOptions.Add(filter.ToString());
			*/

      // Custom filters.
      outputOptions.Add(CustomFilters);

      if (!String.IsNullOrEmpty(Url))
        outputOptions.Add(Url);
      else
      {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
          outputOptions.Add("NUL");
        else
          outputOptions.Add("/dev/null");
      }

      return string.Join(" ", outputOptions);
    }
  }
}
