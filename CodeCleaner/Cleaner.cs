using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Server;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Stringification;

namespace CodeCleaner
{
    public class Cleaner
    {
        private readonly Options _options;
        private readonly Logger _logger;
        private readonly string[] _filesToExcludeBySubstring;
        private readonly string[] _fileGroupExtensions = { ".aspx.cs", ".ascx.cs", ".ashx.cs", ".asmx.cs" };

        public Cleaner(Options options, Logger logger)
        {
            _options = options;
            _logger = logger;
            _filesToExcludeBySubstring = (options.FileExcludeFilter ?? string.Empty).ToLower().Split(';');
        }

        public void RunCleanup()
        {
            int typeCount = 0;
            int methodCount = 0;
            int typesDeleted = 0;
            int methodsDeleted = 0;
            string action = _options.DryRun ? "to be deleted" : "deleted";

            var methodLister = new RoslynMethodLister(_options.PathToSolution, _options.ProjectIncludeFilter, _options.ProjectExcludeFilter, _options.NamespaceFilter);

            var deletionEvaluator = new DeletionEvaluator(new CodeCleanupRepository(msg => _logger.Log(msg)), new WeawingDataProvider(_options.WeavedLibsDir, msg => _logger.Log(msg)));
            var filesToDelete = new HashSet<string>();

            methodLister.ProcessCSharpSolutionDocuments(
                (latestForkedSolution, document, root) =>
                {
                    // don't perform deletion in designer files since their content depends on parent files (like dbml or aspx). Cleaner doesn't deal with dbml or aspx/ascx content, it processes only classes
                    if (document.FilePath.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                    _logger.Log(string.Format("\r\n\r\nProcessing document {0} \r\n", document.FilePath));

                    if (_filesToExcludeBySubstring.Any(document.FilePath.ToLower().Contains))
                    {
                        _logger.Log(string.Format("Skipping processing (matches fileExcludeFilter) {0} \r\n", document.FilePath));
                        return null;
                    }

                    var typeNames = GetDocumentClassesAndStructs(document);

                    var stringifiedMethods = methodLister.GetDocumentMethodStrings(document).ToArray();
                    methodCount += stringifiedMethods.Length;
                    var methodsGroupedByType = stringifiedMethods.GroupBy(Stringifier.GetTypePart).ToArray();

                    CompilationUnitSyntax newDocumentRoot = null;
                    int typesDeletedFromDocumentCount = 0;

                    foreach (var typeName in typeNames)
                    {
                        typeCount++;
                        var typeMethods = methodsGroupedByType.FirstOrDefault(g => g.Key == typeName);
                        if (deletionEvaluator.NeedToDeleteType(typeName))
                        {
                            DeleteType(typeName, action, document, ref newDocumentRoot, typeMethods, ref typesDeleted, ref methodsDeleted);
                            typesDeletedFromDocumentCount++;
                        }
                        else
                        {
                            DeleteMethodsIfNeeded(typeMethods, deletionEvaluator, document, ref newDocumentRoot, methodLister, ref methodsDeleted);
                        }
                    }

                    // 1. delete file and dependent files if no types left in this file (there can be types other than logged, e.g. enums)
                    // 2. for --dryRun the deletion is not actually performed so the condition must be another: number of scheduled for deletion types equals to the number of types in the document
                    var allDocumentTypesCount = GetAllDocumentTypesCount(newDocumentRoot ?? (CompilationUnitSyntax)document.GetSyntaxRoot());
                    if ((!_options.DryRun && allDocumentTypesCount == 0) ||
                        (_options.DryRun && typesDeletedFromDocumentCount == allDocumentTypesCount))
                    {
                        ScheduleFilesToDelete(filesToDelete, document);
                    }

                    // if the document was modified (types or methods deleted)
                    if (newDocumentRoot != null)
                    {
                        latestForkedSolution = latestForkedSolution.UpdateDocument(document.Id, newDocumentRoot);
                        return latestForkedSolution;
                    }

                    return null;
                });

            if (!_options.DryRun)
            {
                DeleteFiles(_options.PathToSolution, methodLister, filesToDelete);
            }

            _logger.Log(string.Format("\r\n\r\n\r\n\r\nPROCESSING COMPLETED. Total type count = {0}; Types {1} = {2};. Total method count = {3}; Methods {1} = {4};\r\n", typeCount, action, typesDeleted, methodCount, methodsDeleted));
        }

        private static IEnumerable<string> GetDocumentClassesAndStructs(IDocument document)
        {
            return ListDocumentClassesAndStructs((CompilationUnitSyntax)document.GetSyntaxRoot()).Select(syntax => Stringifier.GetFullyQualifiedTypeName(new RoslynTypeInfoProvider(syntax))).ToArray();
        }

        private void DeleteType(
            string typeName,
            string action,
            IDocument document,
            ref CompilationUnitSyntax newDocumentRoot,
            IGrouping<string, string> typeMethods,
            ref int typesDeleted,
            ref int methodsDeleted)
        {
            typesDeleted++;
            methodsDeleted += typeMethods != null ? typeMethods.Count() : 0;
            if (!_options.DryRun)
            {
                // Transform the syntax tree of the document and get the root of the new tree
                newDocumentRoot = DeleteType(newDocumentRoot ?? (CompilationUnitSyntax)document.GetSyntaxRoot(), typeName);
            }
            _logger.Log(string.Format("Type {0}: {1}", action, typeName), displayTime: false);
            if (_options.Verbose)
            {
                _logger.Log(string.Format("Methods of the type {0}:", action), displayTime: false);
                if (typeMethods != null)
                {
                    foreach (var method in typeMethods)
                    {
                        _logger.Log(string.Format("-- {0}", method), displayTime: false);
                    }
                }
            }
        }

        private void DeleteMethodsIfNeeded(
            IEnumerable<string> typeMethods,
            DeletionEvaluator deletionEvaluator,
            IDocument document,
            ref CompilationUnitSyntax newDocumentRoot,
            RoslynMethodLister methodLister,
            ref int methodsDeleted)
        {
            if (typeMethods == null || _options.Act == Act.DeleteWholeTypesOnly)
            {
                return;
            }

            foreach (var method in typeMethods)
            {
                bool needToDelete = deletionEvaluator.NeedToDeleteMethod(method);
                if (needToDelete)
                {
                    methodsDeleted++;
                    if (_options.Act == Act.Delete && !_options.DryRun)
                    {
                        // Transform the syntax tree of the document and get the root of the new tree
                        newDocumentRoot = DeleteMethod(newDocumentRoot ?? (CompilationUnitSyntax)document.GetSyntaxRoot(), methodLister, deletionEvaluator, method);
                    }
                }
                else if (!deletionEvaluator.MethodFoundInWeavedLibs(method) && _options.Verbose)
                {
                    _logger.Log(string.Format("not found in dlls, skipping {0}", method), displayTime: false);
                }
                if (needToDelete || _options.Verbose)
                {
                    _logger.Log(string.Format("{0} {1}", needToDelete ? "--" : "++", method), displayTime: false);
                }
            }
        }

        private void ScheduleFilesToDelete(HashSet<string> filePathToDelete, IDocument document)
        {
            string loggedLine = document.FilePath;
            filePathToDelete.Add(document.FilePath);
            if (_fileGroupExtensions.Any(document.FilePath.EndsWith))
            {
                string mainFile = document.FilePath.TrimEndString(".cs");
                string designerFile = mainFile + ".designer.cs";
                filePathToDelete.Add(designerFile);
                filePathToDelete.Add(mainFile);
                loggedLine = _options.Verbose
                    ? string.Format("{0} + {1} + {2}", mainFile, document.FilePath, designerFile)
                    : mainFile;
            }
            LogDeletion(loggedLine);
        }

        private void LogDeletion(string filePath)
        {
            _logger.Log(string.Format("File scheduled for deletion: {0}", filePath));
        }

        private void DeleteFiles(string solutionPath,RoslynMethodLister methodLister, HashSet<string> files)
        {
            var workspace = Workspace.LoadSolution(solutionPath);
            var projects = methodLister.GetProjects(workspace);
            foreach (var project in projects)
            {
                foreach (var documentId in project.DocumentIds)
                {
                    var document = workspace.CurrentSolution.GetDocument(documentId);
                    if (files.Contains(document.FilePath))
                    {
                        workspace.RemoveDocument(documentId);
                        File.Delete(document.FilePath);

                        // special processing for aspx/ ascx and other non-c# compilable files - they are not presented in project.DocumentIds 
                        // so deleting file here and it needs to remove ref to the file from project manually
                        var mainFile = document.FilePath.TrimEndString(".cs"); // aspx or ascx
                        if (files.Contains(mainFile))
                        {
                            File.Delete(mainFile);
                        }
                    }
                }
            }
        }

        private static CompilationUnitSyntax DeleteMethod(CompilationUnitSyntax originalRoot, RoslynMethodLister methodLister, DeletionEvaluator evaluator, string method)
        {
            // needs for second and subsequent deletions from one document (cuz the document allways different).
            var currentMethodNodeFromNewRoot = methodLister.FindInDoc(originalRoot, method);
            
            // property deletion - currently implemented only whole property deletion. Separate accessor deletion requires string manipulation since there is no separate syntax node for getter/setter (only AccessorList for both)
            var methodType = currentMethodNodeFromNewRoot.MethodType;
            if ((methodType == MethodType.PropertyGetter || methodType == MethodType.PropertySetter) &&
                !NeedToDeleteWholeProperty(methodLister, evaluator, currentMethodNodeFromNewRoot, methodType))
            {
                return originalRoot;
            }
            return RemoveNode(originalRoot, currentMethodNodeFromNewRoot.Node);
        }

        private CompilationUnitSyntax DeleteType(CompilationUnitSyntax originalRoot, string typeName)
        {
            // needs for second and subsequent deletions from one document (cuz the document allways different).
            var typeNode = ListDocumentClassesAndStructs(originalRoot).FirstOrDefault(t => Stringifier.GetFullyQualifiedTypeName(new RoslynTypeInfoProvider(t)) == typeName);
            if (typeNode == null)
            {
                // this can be the case when the class declared inside other class which was deleted previously
                // this can lead to a bug when inner class is publicly visible or provides some static methods that used in other, non-deleted file. But this should be quite rare case
                _logger.Log(string.Format("Type not found in doc ({0})", typeName));
                return originalRoot;
            }
            return RemoveNode(originalRoot, typeNode);
        }

        private static CompilationUnitSyntax RemoveNode(CompilationUnitSyntax originalRoot, CommonSyntaxNode node)
        {
            return (CompilationUnitSyntax)originalRoot.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
        }


        private static IEnumerable<TypeDeclarationSyntax> ListDocumentClassesAndStructs(CompilationUnitSyntax root)
        {
            var walker = new TypeWalker();
            walker.Visit(root);
            return walker.ClassesAndStructs;
        }

        private static int GetAllDocumentTypesCount(CompilationUnitSyntax root)
        {
            var walker = new TypeWalker();
            walker.Visit(root);
            return walker.AllTypes.Count();
        }

        private static bool NeedToDeleteWholeProperty(RoslynMethodLister methooLister, DeletionEvaluator evaluator, SyntaxNodeWrapper currentMethodNodeFromNewRoot, MethodType methodType)
        {
            var prop = (PropertyDeclarationSyntax)currentMethodNodeFromNewRoot.Node;
            if (prop.HasGetter() && prop.HasSetter())
            {
                MethodType otherAccessorType = methodType == MethodType.PropertyGetter ? MethodType.PropertySetter : MethodType.PropertyGetter;
                var otherAccessorStr = methooLister.GetMethodDeclarationString(new SyntaxNodeWrapper { MethodType = otherAccessorType, Node = currentMethodNodeFromNewRoot.Node });
                bool needToDelete = evaluator.NeedToDeleteMethod(otherAccessorStr);
                return needToDelete;
            }
            return true;
        }
    }

    internal class Rewriter : SyntaxRewriter
    {
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            return base.VisitMethodDeclaration(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            return base.VisitPropertyDeclaration(node);
        }
    }
}