using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AgenticDemo.Infrastructure.Plugins;

public sealed class FileSystemPlugin(ILogger<FileSystemPlugin> logger)
{
    private readonly string _baseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AgenticDemo");

    private void EnsureDirectory()
    {
        if (!Directory.Exists(_baseDirectory)) Directory.CreateDirectory(_baseDirectory);
    }

    [KernelFunction("write_file")]
    [Description("Creates or overwrites a file with content on the desktop AgenticDemo folder")]
    public string WriteFile(
        [Description("The name of the file")] string fileName,
        [Description("The content to write")] string content)
    {
        EnsureDirectory();
        var path = Path.Combine(_baseDirectory, fileName);
        File.WriteAllText(path, content);
        logger.LogInformation("FileSystem: Wrote to {Path}", path);
        return $"Successfully wrote to {path}";
    }

    [KernelFunction("read_file")]
    [Description("Reads the content of a file from the desktop AgenticDemo folder")]
    public string ReadFile(
        [Description("The name of the file")] string fileName)
    {
        var path = Path.Combine(_baseDirectory, fileName);
        if (!File.Exists(path)) return "Error: File not found.";
        return File.ReadAllText(path);
    }

    [KernelFunction("list_files")]
    [Description("Lists all files in the desktop AgenticDemo folder")]
    public string ListFiles()
    {
        EnsureDirectory();
        var files = Directory.GetFiles(_baseDirectory).Select(Path.GetFileName);
        return "Files on Desktop/AgenticDemo: " + string.Join(", ", files);
    }
}
