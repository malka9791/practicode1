using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq.Expressions;

var bundleCommand = new Command("bundle", "bundle code file");
var createRsp = new Command("rsp", "rsp command");

var language = new Option<string[]>(new[] { "--language", "-l" }, "languages of files");
language.AllowMultipleArgumentsPerToken = true;
var output = new Option<FileInfo>(new[] { "--output", "-o" }, "File path and name");
var note = new Option<bool>(new[] { "--note", "-n" }, "save in this file?");
var sort = new Option<string>(new[] { "--sort", "-s" }, "sort by?");
var remove = new Option<bool>(new[] { "--remove", "-r" }, "remove empty lines");
var author = new Option<string>(new[] { "--author", "-a" }, "author by");

bundleCommand.AddOption(output);
bundleCommand.AddOption(language);
bundleCommand.AddOption(note);
bundleCommand.AddOption(sort);
bundleCommand.AddOption(remove);
bundleCommand.AddOption(author);
createRsp.AddOption(output);

bundleCommand.SetHandler((output, language, note, sort, remove, author) =>
{
    List<string> codeFiles = new List<string>();
    if (language[0] == "all")
        codeFiles.AddRange(Directory.GetFiles("\\.", "*", SearchOption.AllDirectories));
    else foreach (var l in language)
        {
            codeFiles.AddRange(Directory.GetFiles("\\.", $"*.{l}", SearchOption.AllDirectories));
        }
    if (codeFiles.Count == 0)
    {
        Console.WriteLine("ERROR: No files found to concatenate");
        return;
    }
    if (sort == "abc")
    {
        codeFiles = codeFiles.OrderBy(file => file).ToList();
    }
    else if (sort == "language")
    {
        codeFiles = codeFiles.OrderBy(file => Path.GetExtension(file)).ToList();

    }
    else { codeFiles = codeFiles.OrderBy(file => file).ToList(); }

    try
    {
        var file = File.CreateText(output.FullName);
        file.WriteLine($"// {author}");
        foreach (var f in codeFiles)
        {
            if (note)
            {
                file.WriteLine($"// path: {Path.GetFullPath(f)}");
                file.WriteLine($"// file name:{Path.GetFileName(f)}");
                file.WriteLine();
            }
            string[] lines = File.ReadAllLines(f);
            if (remove)
            {
                lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            }
            file.WriteLine(lines);
        }

    }
    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine("file invalid ");

    }
    Console.WriteLine("file was created");

}, output, language, note, sort, remove, author);

createRsp.SetHandler(() =>
{
    try
    {
        var writeRsp = new FileInfo("filename.rsp");
        Console.WriteLine("enter value for bundle command");
        using (StreamWriter writer = new StreamWriter(writeRsp.FullName))
        {
            Console.WriteLine("output file path:");
            string o = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(o))
            {
                Console.WriteLine("Errror: press output file path");
                o = Console.ReadLine();
            }
            writer.Write($"--output {o}");
            Console.WriteLine("languages file atleast1:");
            string l = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(l))
            {
                Console.WriteLine();
                l = Console.ReadLine();
            }
            writer.Write($"--language {l}");
            Console.WriteLine("enter note y/n:");
            writer.Write(Console.ReadLine().Trim().ToLower() == "y" ? $"--note {true}" : "");
            Console.WriteLine("enter remove y/n:");
            writer.Write(Console.ReadLine().Trim().ToLower() == "y" ? $"--remove {true}" : "");
            Console.WriteLine("enter kind of sort:");
            writer.Write($"--sort {Console.ReadLine()}");
            Console.WriteLine("author:");
            writer.Write($"--author {Console.ReadLine()}");
        }
        Console.WriteLine("file was created");
    }
    catch (Exception ex)
    {
        Console.WriteLine("file invalid ");

    }
});
var rootCommand = new RootCommand("Root command for file bundler CLI");

rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRsp);
rootCommand.InvokeAsync(args);