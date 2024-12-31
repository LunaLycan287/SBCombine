using CommandLine;

namespace SBCombine;

public class CmdOptions
{
   [Option('d', "directory", HelpText = "Specify working directory.")]
   public string? Directory { get; set; }

   [Option('m', "main", HelpText = "Main file to put at the top. By default the first file found will be used.")]
   public string? Main { get; set; } 
   
   [Option('i', "include", HelpText = "Include files from the list using glob pattern. If this is not set all *.cs files in the directory and any sub-directory will be used.")]
   public required IEnumerable<string> Include { get; set; }
   
   [Option('e', "exclude", HelpText = @"Exclude files from the list using glob pattern. By default all files in '**\obj\*', '**\bin\*' and '**\Properties\AssemblyInfo.cs' are excluded. If you add excludes remember to add them back.")]
   public  required IEnumerable<string> Exclude { get; set; }
   
   [Option('o', "output", Default = "*-Merged.sbcs", HelpText = "Sets the output filename. '*' will be replaced with the main file name. By default it is '*-Merged.sbcs'.")]
   public required string Output { get; set; }
   
   [Option('u', "usings", Default = true, HelpText = "If common implicit usings should be added if dependent libraries are found. Default = true. (This is an ever growing list, please report new ones on github)")]
   public bool Usings { get; set; }
}