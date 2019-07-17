using System;
using Roslyn.Compilers.CSharp;
using Stringification;

namespace CodeCleaner
{
    internal static class RoslynMethodInfoProviderFactory
    {
        internal static IMethodInfoProvider Create(SyntaxNodeWrapper n)
        {
            var syntax = n.Node as MethodDeclarationSyntax;
            if (syntax != null)
            {
                return new RoslynMethodInfoProvider(syntax);
            }

            var syntax2 = n.Node as PropertyDeclarationSyntax;
            if (syntax2 != null)
            {
                return new RoslynMethodInfoProviderForProperty(syntax2, n.MethodType);
            }

            throw new InvalidOperationException("Please define info provider for the syntax node.");
        }
    }
}