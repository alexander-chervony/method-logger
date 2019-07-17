using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    [Description("In order to get this test working correctly solution must be updated and built. Sources must be in sync with DLLs.")]
    public class IntegrationTestsYourSln2 : IntegrationTestsBase
    {
        protected override string SolutionFile { get { return @"c:\sources\YourSln2.sln"; } }
        protected override string SolutionProjectExcludeFilter { get { return ".Test;Rtx.Logger;TestUtils;FtpExtensions.Interfaces;ActivitySyncConsole;Activity.CommentCountersUpdater;Activity.PeriodCountersUpdater"; } }
        protected override string WeaveLibrariesRootDir { get { return @"c:\\sources\\YourSln2\\bin\\Release\\"; } }
    }
}