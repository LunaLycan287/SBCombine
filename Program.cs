using System.Text.Json;
using System.Text.RegularExpressions;
using CommandLine;
using Microsoft.Extensions.FileSystemGlobbing;
using SBCombine;


Parser parser = new(with => {
    with.AutoHelp = true;
    with.AutoVersion = false;
    with.CaseSensitive = false;
    with.EnableDashDash = true;
});
ParserResult<CmdOptions>? result = parser.ParseArguments<CmdOptions>(args);
if (result.Errors.Any()) {
    Console.Error.WriteLine("Could not parse commandline:" + Environment.NewLine + " " + string.Join(Environment.NewLine + " ", result.Errors));
    return;
}
CmdOptions options = result.Value;

Console.WriteLine("Params: " + JsonSerializer.Serialize(options));

List<string> includes = options.Include.ToList();
if (includes.Count == 0) {
    includes.Add(@"**\*.cs");
}
List<string> excludes = options.Exclude.ToList();
if (excludes.Count == 0) {
    excludes.Add(@"**\obj\*");
    excludes.Add(@"**\bin\*");
    excludes.Add(@"**\Properties\AssemblyInfo.cs");
}

string dir = Directory.GetCurrentDirectory();
if (options.Directory != null) {
    if (Directory.Exists(options.Directory)) {
        dir = options.Directory;
    }
    else {
        Console.Error.WriteLine($"Directory '{options.Directory}' does not exist.");
        return;
    }
}

Console.WriteLine($"Running in: {dir}");
Console.WriteLine($"Includes: {string.Join(", " , includes)}");
Console.WriteLine($"Excludes: {string.Join(", ", excludes)}");

Matcher matcher = new();
matcher.AddIncludePatterns(includes.ToArray());
matcher.AddExcludePatterns(excludes.ToArray());

List<string> files = matcher.GetResultsInFullPath(dir).ToList();
if (files.Count == 0) {
    Console.Error.WriteLine("No files matched the given parameters.");
    return;
}


if (options.Main != null) {
    Matcher mainMatcher = new();
    mainMatcher.AddInclude(options.Main);
    mainMatcher.AddExcludePatterns(options.Exclude.ToArray());
    List<string> mainOpt = (List<string>)mainMatcher.GetResultsInFullPath(dir);
    if (mainOpt.Count == 0) {
        Console.Error.WriteLine("Main file not found. Is it excluded?");
        return;
    }
    
    Console.WriteLine($"Custom Main file: {mainOpt[0].Replace(dir, ".")} ");
    files.Remove(mainOpt[0]);
    files.Insert(0, mainOpt[0]);
}
Console.WriteLine($"Files matched: {files.Aggregate("", (current, file) => current + file.Replace(dir, ".")+ ", ")} ");

string outName = options.Output.Replace("*", Path.GetFileNameWithoutExtension(files[0]));
string outFile = Path.Combine(dir, outName);

SortedSet<string> usings = [];
string outContent =
    files.Aggregate("", (current, file) => current + (GetCleanedFileContent(file, ref usings) + Environment.NewLine));

if (options.Usings) {
    AddImplicitUsings(ref usings);
}

outContent = string.Join(Environment.NewLine, usings) + Environment.NewLine + outContent;

File.WriteAllText(outFile, outContent);
Console.WriteLine("Combined " + files.Count + " files into " + outFile);
return;

static void AddImplicitUsings(ref SortedSet<string> usings) {
    List<string> newUsings = [];
    foreach (string u in usings) {
        if (u.Contains("System.Windows.Forms")) {
            newUsings.Add("using System.Drawing;");
        } else if (u.Contains("SocketIOClient;")) {
            newUsings.Add("using SocketIO.Serializer.Core;");
        }
    }

    foreach (string u in newUsings) {
        usings.Add(u);
    }
}

static string GetCleanedFileContent(string filename, ref SortedSet<string> usings) {
    List<string> cleanedLines = [];

    string[] lines = File.ReadAllLines(filename);
    bool namespaceFix = false;

    foreach (string line in lines) {
        string trimmedLine = line.Trim();

        if (trimmedLine.StartsWith("namespace") && trimmedLine.Contains(';')) {
            // Namespace for whole file does not work if it is all one file, so we need to encapsulate the contents of this file.
            cleanedLines.Add(line.Replace(";", "{"));
            namespaceFix = true;
        }
        else if (trimmedLine.StartsWith("using") && !trimmedLine.Contains('(')) {
            // This using is not needed since it is already globally used by default.
            if (!trimmedLine.Contains("Streamer.bot.Plugin.Interface")) {
                usings.Add(line);
            }
        }
        else if (CphBaseRegex().IsMatch(trimmedLine)) {
            // An option to develop code with other editors is to use "CPHInline : CPHInlineBase" which needs to be removed when copying
            cleanedLines.Add(CphBaseRegex().Replace(line, "").TrimEnd());
        }
        else if (!trimmedLine.Contains("Streamer.bot.Plugin.Interface.IInlineInvokeProxy")) {
            // An option to develop code with other editors is to use "static Streamer.bot.Plugin.Interface.IInlineInvokeProxy CPH;" which needs to be removed.
            cleanedLines.Add(line);
        }
    }

    if (namespaceFix) {
        cleanedLines.Add("}");
    }

    return string.Join(Environment.NewLine, cleanedLines);
}

internal abstract partial class Program {
    [GeneratedRegex(@"\s*\:\s*CPHInlineBase")]
    private static partial Regex CphBaseRegex();
}