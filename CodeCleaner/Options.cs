using System.IO;
using CommandLine;
using CommandLine.Text;

namespace CodeCleaner
{
    public enum Act
    {
        Delete,
        DeleteWholeTypesOnly
    }

    public class Options
    {
        [Option('s', "pathToSolution", Required = true, HelpText = "Solution file to process.")]
        public string PathToSolution { get; set; }

        [Option('n', "namespaceFilter", Required = false, HelpText = "Process only this namespace.")]
        public string NamespaceFilter { get; set; }

        [Option('p', "projectIncludeFilter", Required = false, HelpText = "Process only this project (project name).")]
        public string ProjectIncludeFilter { get; set; }

        [Option('e', "projectExcludeFilter", Required = false, HelpText = "Process only project that doesn't contain substring in the name. For example: '.Test;Rtx.Logger'.")]
        public string ProjectExcludeFilter { get; set; }

        [Option('f', "fileExcludeFilter", Required = false, HelpText = "Dont delete files identified by these substrings. For example: 'ModerationUserPictureList.ascx;ContentPendingFlags.ascx'.")]
        public string FileExcludeFilter { get; set; }

        private string _weavedLibsDir;
        [Option('w', "weavedLibsDir", Required = false, HelpText = "Directory with weaved libs. Methods deleted only if foud in weaved libs. If method were logged by prod DLLs then these dlls must be used during the cleanup process. Solution directory used by default for debugging simplicity.")]
        public string WeavedLibsDir
        {
            get
            {
                return _weavedLibsDir ?? new FileInfo(PathToSolution).DirectoryName;
            }
            set { _weavedLibsDir = value; }
        }

        [Option('a', "action", Required = true,
            HelpText =
                "DryRun, Delete or SelfVerificationOnly. \t\tDryRun - Just lists methods to be deleted. \t\tDelete - Deletes them.\t\tSelfVerificationOnly - Checks that all the logged methods are found in the code. This guaranties that both Ceil-based and Roslyn-based method declaration stringifications work equally and no false-positive deletion will be done."
            )]
        public Act Act { get; set; }

        [Option('d', "dryRun", Required = false)]
        public bool DryRun { get; set; }

        [Option('v', "verbose", Required = false)]
        public bool Verbose { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        public override string ToString()
        {
            return string.Format(
                "action='{0}', pathToSolution='{1}', namespaceFilter='{2}', projectIncludeFilter='{3}', projectExcludeFilter='{4}', weavedLibsDir='{5}', dryRun='{6}', verbose='{7}'",
                Act,
                PathToSolution,
                NamespaceFilter,
                ProjectIncludeFilter,
                ProjectExcludeFilter,
                WeavedLibsDir,
                DryRun,
                Verbose);
        }
    }
}