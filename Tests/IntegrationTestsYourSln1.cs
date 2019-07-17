using System.Linq;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    [Description("In order to get this test working correctly solution must be updated and built. Sources must be in sync with DLLs.")]
    public partial class IntegrationTestsYourSln1 : IntegrationTestsBase
    {
        protected override string SolutionFile { get { return @"c:\sources\YourSln1.sln"; } }
        protected override string SolutionProjectExcludeFilter { get { return ".Test;Rtx.Logger;TestUtils;FtpExtensions.Interfaces"; } }
        protected override string WeaveLibrariesRootDir { get { return @"c:\\sources\\YourSln1\\"; } }

        [Test]
        public void MethodListsFoundByRoslynAndByCecilMustBeTheSame2()
        {
            MethodListsFoundByRoslynAndByCecilMustBeTheSameEx(
                GetWeavedMethods(namespaceFilter: "YourSln1.Web.DataAccessApi.Widgets4").WithoutCtors().ToArray(),
                GetRoslynListedMethods("YourSln1.Web.DataAccessApi.Widgets4"));
        }
    }
}