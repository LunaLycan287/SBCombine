using CommandLine;

namespace SBCombine;

public class CmdOptions
{
   [Option('d', "directory", HelpText = "Specify working directory.")]
   public string? Directory { get; set; }

   [Option('m', "main", HelpText = "Main file to put at the top. By default the first file found will be used.")]
   public string? Main { get; set; } 
   
   [Option('i', "include", HelpText = @"(Default: '**\*.cs') Include files from the list using glob pattern.")]
   public required IEnumerable<string> Include { get; set; }
   
   [Option('e', "exclude", HelpText = @"(Default: '**\obj\*', '**\bin\*', '**\Properties\AssemblyInfo.cs') Exclude files from the list using glob pattern.")]
   public  required IEnumerable<string> Exclude { get; set; }
   
   [Option('o', "output", Default = "*-Merged.sbcs", HelpText = "(Default: '*-Merged.sbcs') Sets the output filename. '*' will be replaced with the main file name.")]
   public required string Output { get; set; }
   
   [Option('u', "usings", Default = true, HelpText = "(Default: true) If common implicit usings should be added if dependent libraries are found. (This is an ever growing list, please report new ones on github)")]
   public bool Usings { get; set; }
}