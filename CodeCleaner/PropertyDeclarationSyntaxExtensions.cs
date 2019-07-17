using System.Linq;
using System.Text.RegularExpressions;
using Roslyn.Compilers.CSharp;

namespace CodeCleaner
{
    public static class PropertyDeclarationSyntaxExtensions
    {
        private static readonly Regex GetterRegex = new Regex(@"^[^/]*?(\{|\s) get \s* (;|\{)", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
        private static readonly Regex SetterRegex = new Regex(@"^[^/]*?(\{|\s) set \s* (;|\{)", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);

        public static bool HasGetter(this PropertyDeclarationSyntax prop)
        {
            return GetterRegex.IsMatch(GetAccessorsString(prop));
        }

        public static bool HasSetter(this PropertyDeclarationSyntax prop)
        {
            return SetterRegex.IsMatch(GetAccessorsString(prop));
        }

        private static string GetAccessorsString(PropertyDeclarationSyntax prop)
        {
            ChildSyntaxList children = prop.ChildNodesAndTokens();
            var accessorList = children.FirstOrDefault(n => n.Kind == SyntaxKind.AccessorList);
            return accessorList != null ? accessorList.ToString() : string.Empty;
        }
    }
}