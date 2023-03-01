using Microsoft.Extensions.Options;
using SketchPen.Code.Extensions;
using SketchPen.Plot;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Debugging;
using SketchPen.Plot.Services;

namespace SketchPen.Code.Services;

public class SketchPenCodeService
{
    private readonly CompilerService _compiler;
    private readonly SketchPenCodeServiceOptions _options;

    public SketchPenCodeService(CompilerService compiler,
                                IOptions<SketchPenCodeServiceOptions> options)
    {
        _compiler = compiler;
        _options = options.Value;
    }

    public IEnumerable<string> GetAllFiles(string id)
    {
        var rootDirInfo = new DirectoryInfo(Path.Combine(_options.RootPath, id));
        var filesList = new List<string>();

        CollectFiles(rootDirInfo, filesList);

        return filesList;
    }

    public IEnumerable<string> GetGlobals(string id)
    {
        var rootDirInfo = new DirectoryInfo(Path.Combine(_options.RootPath, id));
        var filesList = new List<string>();

        foreach (var fileInfo in rootDirInfo.GetFiles("_*.globals"))
        {
            filesList.Add(fileInfo.FullName.ToRelativeFilePath(rootDirInfo.FullName));
        }

        return filesList;
    }

    async public Task<string> GetFileContent(string route)
    {
        var fileInfo = new FileInfo(Path.Combine(_options.RootPath, route));

        if (!fileInfo.HasAllowedExtension())
        {
            throw new Exception($"Not allowed file extension: {fileInfo.Extension}");
        }

        if (!fileInfo.Exists)
        {
            throw new Exception($"File {route} not exists");
        }

        return await File.ReadAllTextAsync(fileInfo.FullName);
    }

    async public Task SetFileContent(string route, string content)
    {
        var fileInfo = new FileInfo(Path.Combine(_options.RootPath, route));

        if (!fileInfo.HasAllowedExtension())
        {
            throw new Exception($"Not allowed file extension: {fileInfo.Extension}");
        }

        await File.WriteAllTextAsync(fileInfo.FullName, content);
    }

    async public Task<string> CreateFile(string id, string filename)
    {
        if (!filename.IsValidFilename())
        {
            throw new Exception($"Filename contains invalid characters: {filename}");
        }

        var fileInfo = new FileInfo(Path.Combine(_options.RootPath, id, filename));

        if (fileInfo.Exists)
        {
            throw new Exception($"{filename} already exists");
        }
        if (!fileInfo.HasAllowedExtension())
        {
            throw new Exception($"Not allowed file extension: {fileInfo.Extension}");
        }

        await File.WriteAllTextAsync(fileInfo.FullName, $"// {filename}{Environment.NewLine}");

        return await GetFileContent($"{id}/{filename}");
    }

    public bool DeleteFile(string id, string filename)
    {
        var fileInfo = new FileInfo(Path.Combine(_options.RootPath, id, filename));

        if (!fileInfo.Exists)
        {
            throw new Exception($"{filename} not exists");
        }

        fileInfo.Delete();

        return true;
    }

    public EditorFileType GetEditorFileType(string route)
    {
        var fileExt = route?
                        .Split('/')
                        .Last()
                        .Split(".")
                        .Last()
                        .ToLower();

        return fileExt switch
        {
            "sp" => EditorFileType.Code,
            "spt" => EditorFileType.Template,
            "globals" => EditorFileType.Globals,
            _ => EditorFileType.Unknown
        };
    }

    async public Task<IEnumerable<string>> GetGlobalVariableNames(string id)
    {
        string globalsFile = Path.Combine(_options.RootPath, id, "_.globals");
        if (!File.Exists(globalsFile))
        {
            return Array.Empty<string>();
        }

        var code = _compiler.PreCompile(globalsFile);
        var commands = _compiler.Compile(await File.ReadAllTextAsync(globalsFile));

        //using (var plotContext = new DebugPlotContext())
        //{
        //    plotContext.Init(0, 0);

        //    foreach (var command in commands)
        //    {
        //        command.Execute(plotContext);
        //    }

        //    return plotContext
        //        .Globals
        //        .Keys
        //        .Distinct()
        //        .Order();
        //}

        return Array.Empty<string>();
    }

    #region Helper

    private void CollectFiles(DirectoryInfo dirInfo, List<string> filesList)
    {
        foreach (var folderDirInfo in dirInfo.GetDirectories())
        {
            CollectFiles(folderDirInfo, filesList);
        }

        foreach (var fileInfo in dirInfo.GetFiles())
        {
            var relFilePath = fileInfo.FullName.ToRelativeFilePath(_options.RootPath);

            if (relFilePath.StartsWith("_") &&
                relFilePath.EndsWith(".globals"))
            {
                continue;
            }

            filesList.Add(relFilePath);
        }
    }

    #endregion
}
