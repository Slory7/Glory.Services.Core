
#region Usings

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.IO;

#endregion

namespace Glory.Services.Core.DataCache.Parameters
{
    public class FileCacheDependency
    {
        public FileCacheDependency(string filename)
        {
            FileName = filename;
        }

        public string FileName { get; }

        internal IChangeToken GetChangeToken()
        {
            var fileInfo = new FileInfo(FileName);
            var fileProvider = new PhysicalFileProvider(fileInfo.DirectoryName);
            var changeToken = fileProvider.Watch(fileInfo.Name);
            return changeToken;
        }
    }
}