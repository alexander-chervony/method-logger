using System.Collections.Generic;
using System.Linq;
using Stringification;

namespace CodeCleaner
{
    public class DeletionEvaluator
    {
        private readonly ICodeCleanupRepository _repository;
        private readonly IWeawingDataProvider _weawingDataProvider;

        private HashSet<string> _loggedMethods;
        private HashSet<string> _methodsFoundInWeavedLibs;
        private HashSet<string> _nonUsedTypes;

        public DeletionEvaluator(ICodeCleanupRepository repository, IWeawingDataProvider weawingDataProvider)
        {
            _repository = repository;
            _weawingDataProvider = weawingDataProvider;
        }

        private HashSet<string> LoggedMethods { get { return _loggedMethods ?? (_loggedMethods = new HashSet<string>(_repository.GetLoggedMethods())); } }
        private HashSet<string> MethodsFoundInWeavedLibs { get { return _methodsFoundInWeavedLibs ?? (_methodsFoundInWeavedLibs = new HashSet<string>(_weawingDataProvider.GetMethodsFoundInWeavedLibs())); } }
        private HashSet<string> NonUsedTypes { get { return _nonUsedTypes ?? (_nonUsedTypes = new HashSet<string>(GetNonUsedTypes())); } }

        public bool NeedToDeleteMethod(string method)
        {
            bool called = LoggedMethods.Contains(method);
            bool foundInWeavedLibs = MethodsFoundInWeavedLibs.Contains(method);
            return !called && foundInWeavedLibs;
        }

        /// <summary>
        /// Needs to delete if:
        /// 1. ctor of the type exists in weaved libs AND invocation of this ctor is not logged -- this logic skips static helper classes that has no ctors, so next is better:
        /// ------------------------------------------------------------------ Implemented ----------------------------------------------------------------------------------------------
        /// 2. the type is weaved but there are no calls -- this is OK but it skips the case when the service created but no calls made to it so consider 3:
        /// -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// 3. no calls made except ctor - questionable that this reason is enough to delete the type - will not be implemented for now
        /// </summary>
        public bool NeedToDeleteType(string stringifiedType)
        {
            return NonUsedTypes.Contains(stringifiedType);
        }

        private IEnumerable<string> GetNonUsedTypes()
        {
            var weavedTypes = MethodsFoundInWeavedLibs.Select(Stringifier.GetTypePart).Distinct().ToArray();
            var loggedTypes = LoggedMethods.Select(Stringifier.GetTypePart).Distinct().ToArray();
            return weavedTypes.Except(loggedTypes);
        }

        public bool MethodFoundInWeavedLibs(string method)
        {
            return MethodsFoundInWeavedLibs.Contains(method);
        }
    }
}