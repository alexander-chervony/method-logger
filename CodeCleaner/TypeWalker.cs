using System.Collections.Generic;
using System.Linq;
using Roslyn.Compilers.CSharp;

namespace CodeCleaner
{
    internal class TypeWalker : SyntaxWalker
    {
        private readonly List<TypeDeclarationSyntax> _classesAndStructs = new List<TypeDeclarationSyntax>();
        private readonly List<BaseTypeDeclarationSyntax> _enumTypes = new List<BaseTypeDeclarationSyntax>();
        private readonly List<BaseTypeDeclarationSyntax> _interfaceTypes = new List<BaseTypeDeclarationSyntax>();

        public IEnumerable<TypeDeclarationSyntax> ClassesAndStructs { get { return _classesAndStructs; } }
        public IEnumerable<BaseTypeDeclarationSyntax> AllTypes { get { return _classesAndStructs.Concat(_interfaceTypes).Concat(_enumTypes); } }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            _classesAndStructs.Add(node);
            base.VisitStructDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _classesAndStructs.Add(node);
            base.VisitClassDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            _interfaceTypes.Add(node);
            base.VisitInterfaceDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            _enumTypes.Add(node);
            base.VisitEnumDeclaration(node);
        }
    }
}