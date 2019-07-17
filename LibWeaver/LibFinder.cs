using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibWeaver
{
    internal class LibFinder
    {
        public IEnumerable<string> GetLibs(string entryDir, string[] includeFileStartsWith, string[] extensionsToProcess, string[] excludeDirs, string[] excludeDirsEndsWith, string[] excludeFileContains)
        {
            var libFiles = new List<string>();
            ProcessDir(new DirectoryInfo(entryDir), includeFileStartsWith, extensionsToProcess, excludeDirs, excludeDirsEndsWith, excludeFileContains, libFiles);
            return libFiles;
        }

        private void ProcessDir(DirectoryInfo dirInfo, string[] includeFileStartsWith, string[] extensionsToProcess, string[] excludeDirs, string[] excludeDirsEndsWith, string[] excludeFileContains, List<string> libFiles)
        {
            FileSystemInfo[] infos = dirInfo.GetFileSystemInfos();
            foreach (FileSystemInfo i in infos)
            {
                if (i is DirectoryInfo)
                {
                    var d = (DirectoryInfo)i;
                    string dirName = d.Name.ToLower();
                    if (!excludeDirs.Contains(dirName) && !excludeDirsEndsWith.Any(end => dirName.EndsWith(end, StringComparison.OrdinalIgnoreCase)))
                    {
                        ProcessDir(d, includeFileStartsWith, extensionsToProcess, excludeDirs, excludeDirsEndsWith, excludeFileContains, libFiles);
                    }
                }
                else if (i is FileInfo)
                {
                    if (extensionsToProcess.Contains(i.Extension.ToLower()) &&
                        includeFileStartsWith.Any(p => i.Name.StartsWith(p, StringComparison.OrdinalIgnoreCase)) &&
                        excludeFileContains.All(m => !i.Name.Contains(m)))
                    {
                        libFiles.Add(i.FullName);
                    }
                }
            }
        }
    }
}