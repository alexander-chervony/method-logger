using System;
using System.Collections.Generic;
using System.Linq;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Stringification;

namespace CodeCleaner
{
    public class RoslynMethodLister : IMethodLister
    {
        private readonly string _projectNameEndsWith;
        private readonly IEnumerable<string> _projectExcludeFilter;
        private readonly string _namespaceStartsWith;
        private readonly string _namespaceExcludeFilter;
        private readonly string _solutionPath;
        private readonly string[] _ignorableFileEndings = { @"\AssemblyInfo.cs", ".AssemblyAttributes.cs" };

        public RoslynMethodLister(string solutionPath, string projectNameEndsWith = null, string projectExcludeFilter = null, string namespaceStartsWith = null, string namespaceExcludeFilter = null)
        {
            _solutionPath = solutionPath;
            _projectNameEndsWith = projectNameEndsWith;
            if (projectExcludeFilter != null)
                _projectExcludeFilter = projectExcludeFilter.Split(';');
            _namespaceStartsWith = namespaceStartsWith;
            _namespaceExcludeFilter = namespaceExcludeFilter;
        }

        public IEnumerable<IMethodInfoProvider> ListAllMethods()
        {
            var methodWalker = new MethodWalker();

            ProcessCSharpSolutionDocuments(
                (latestForkedSolution, document, root) =>
                {
                    methodWalker.Visit(root);
                    return null;
                });

            return methodWalker.AllMembers.Select(RoslynMethodInfoProviderFactory.Create);
        }

        public void ProcessCSharpSolutionDocuments(Func<ISolution, IDocument, CompilationUnitSyntax, ISolution> processItem)
        {
            bool solutionModified = false;
            var workspace = Workspace.LoadSolution(_solutionPath);
            var projects = GetProjects(workspace);
            // Take a snapshot of the original solution.
            var originalSolution = workspace.CurrentSolution;
            // Declare a variable to store the intermediate solution snapshot at each step.
            ISolution latestForkedSolution = workspace.CurrentSolution;
            foreach (var project in projects)
            {
                foreach (var documentId in project.DocumentIds)
                {
                    // Look up the snapshot for the original document in the latest forked solution.
                    var document = latestForkedSolution.GetDocument(documentId);

                    if (document.LanguageServices.Language != LanguageNames.CSharp || _ignorableFileEndings.Any(end => document.FilePath.EndsWith(end, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    var root = GetDocumentRootIfNamespaceMatches(document);
                    if (root == null)
                    {
                        continue;
                    }

                    var modifiedSolution = processItem(latestForkedSolution, document, root);

                    if (modifiedSolution != null)
                    {
                        latestForkedSolution = modifiedSolution;
                        solutionModified = true;
                    }
                }
            }

            if (solutionModified)
            {
                workspace.ApplyChanges(originalSolution, latestForkedSolution);
            }
        }

        public IEnumerable<IProject> GetProjects(IWorkspace workspace)
        {
            IEnumerable<IProject> projects = workspace.CurrentSolution.Projects;
            if (!string.IsNullOrEmpty(_projectNameEndsWith))
            {
                projects = projects.Where(p => p.Name.Equals(_projectNameEndsWith));
            }
            if (_projectExcludeFilter != null)
            {
                projects = projects.Where(p => !_projectExcludeFilter.Any(e => p.Name.Contains(e)));
            }
            return projects;
        }

        public SyntaxNodeWrapper FindInDoc(CompilationUnitSyntax originalRoot, string methodString)
        {
            IEnumerable<SyntaxNodeWrapper> found =
                ListDocumentMethodSyntaxes(originalRoot).
                Where(m => GetMethodDeclarationString(m) == methodString).
                ToArray();
            if (found.Count() != 1)
            {
                throw new InvalidOperationException(methodString + " is ambigious or not found!");
            }
            return found.First();
        }

        public string GetMethodDeclarationString(SyntaxNodeWrapper m)
        {
            return Stringifier.GetMethodDeclarationString(RoslynMethodInfoProviderFactory.Create(m));
        }

        public IEnumerable<string> GetDocumentMethodStrings(IDocument document)
        {
            var root = GetDocumentRootIfNamespaceMatches(document);
            if (root == null)
            {
                return Enumerable.Empty<string>();
            }
            return ListDocumentMethodSyntaxes(root).Select(GetMethodDeclarationString).Where(m => m != null);
        }

        private static IEnumerable<SyntaxNodeWrapper> ListDocumentMethodSyntaxes(CompilationUnitSyntax root)
        {
            var methodWalker = new MethodWalker();
            methodWalker.Visit(root);
            return methodWalker.AllMembers;
        }

        private CompilationUnitSyntax GetDocumentRootIfNamespaceMatches(IDocument document)
        {
            var syntax = document.GetSyntaxTree();
            var root = (CompilationUnitSyntax)syntax.GetRoot();

            var namespaceSyntaxes = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Select(ns => ns.Name.ToString()).ToArray();

            if (!string.IsNullOrEmpty(_namespaceExcludeFilter) && namespaceSyntaxes.Any(ns => ns.StartsWith(_namespaceExcludeFilter)))
            {
                return null;
            }

            if (string.IsNullOrEmpty(_namespaceStartsWith) || namespaceSyntaxes.Any(ns => ns.StartsWith(_namespaceStartsWith)))
            {
                return root;
            }
            return null;
        }
    }
}