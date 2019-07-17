using System.Collections.Generic;
using Roslyn.Compilers.CSharp;
using Stringification;

namespace CodeCleaner
{
    internal class MethodWalker : SyntaxWalker
    {
        private readonly List<SyntaxNodeWrapper> _allMembers = new List<SyntaxNodeWrapper>();
        public IEnumerable<SyntaxNodeWrapper> AllMembers { get { return _allMembers; } }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            _allMembers.Add(new SyntaxNodeWrapper { Node = node });
            base.VisitMethodDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax prop)
        {
            if (prop.HasGetter())
            {
                _allMembers.Add(
                    new SyntaxNodeWrapper
                    {
                        Node = prop,
                        MethodType = MethodType.PropertyGetter
                    });
            }

            if (prop.HasSetter())
            {
                _allMembers.Add(
                    new SyntaxNodeWrapper
                    {
                        Node = prop,
                        MethodType = MethodType.PropertySetter
                    });
            }

            base.VisitPropertyDeclaration(prop);
        }

        public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            // todo: implement property processing with this method rather than VisitPropertyDeclaration
            base.VisitAccessorDeclaration(node);
        }

        //public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
        //{
        //    base.VisitIndexerDeclaration(node);
        //}
    }

    public struct SyntaxNodeWrapper
    {
        public SyntaxNode Node;
        public MethodType MethodType;
    }
}