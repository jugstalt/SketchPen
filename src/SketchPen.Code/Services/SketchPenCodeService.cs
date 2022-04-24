using Microsoft.Extensions.Options;
using SketchPen.Code.Extensions;

namespace SketchPen.Code.Services
{
    public class SketchPenCodeService
    {
        private readonly SketchPenCodeServiceOptions _options;

        public SketchPenCodeService(IOptionsMonitor<SketchPenCodeServiceOptions> optionsMonitor)
        {
            _options = optionsMonitor.CurrentValue;
        }

        public IEnumerable<string> GetAllFiles(string id)
        {
            var rootDirInfo = new DirectoryInfo(Path.Combine(_options.RootPath, id));
            var filesList = new List<string>();

            CollectFiles(rootDirInfo, filesList);

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

        #region Helper

        private void CollectFiles(DirectoryInfo dirInfo, List<string> filesList)
        {
            foreach (var folderDirInfo in dirInfo.GetDirectories())
            {
                CollectFiles(folderDirInfo, filesList);
            }

            foreach (var fileInfo in dirInfo.GetFiles())
            {
                filesList.Add(fileInfo.FullName.Substring(_options.RootPath.Length + 1).Replace("\\", "/"));
            }
        }

        #endregion
    }
}
