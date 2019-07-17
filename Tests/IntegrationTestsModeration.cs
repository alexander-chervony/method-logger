using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    [Description("In order to get this test working correctly solution must be updated and built. Sources must be in sync with DLLs.")]
    public class IntegrationTestsModeration : IntegrationTestsBase
    {
        protected override string SolutionFile { get { return @"c:\MTV-FLUX\Moderation\trunk\Solution\ModerationSite.sln"; } }
        protected override string SolutionProjectExcludeFilter { get { return ".Test;Rtx.Logger;TestUtils;FtpExtensions.Interfaces"; } }
        protected override string WeaveLibrariesRootDir { get { return @"c:\\MTV-FLUX\\Moderation\\trunk\\"; } }
    }
}