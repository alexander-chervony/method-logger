using System.Linq;
using Mono.Cecil;
using Stringification;

namespace LibWeaver
{
    public class CecilMethodInfoProvider : IMethodInfoProvider
    {
        private readonly MethodDefinition _m;

        public CecilMethodInfoProvider(MethodDefinition m)
        {
            _m = m;
        }

        public ITypeInfoProvider DeclaringType { get { return new CecilTypeInfoProvider(_m.DeclaringType); } }
        public string Name { get { return _m.Name; } }
        //public string Name { get { return _m.Name; } }
        public bool IsGenericMethodDefinition { get { return _m.HasGenericParameters; } }

        public object[] GetGenericArguments()
        {
            return _m.GenericParameters.ToArray();
        }

        public IParameterInfoProvider[] GetParameters()
        {
            return _m.Parameters.Select(p => new CecilParameterInfoProvider(p)).ToArray();
        }

        public bool IsAbstractClassOrInterfaceMember
        {
            get
            {
                return
                    ((_m.DeclaringType.IsAbstract || _m.DeclaringType.IsInterface) && !_m.DeclaringType.IsSealed) ||
                    IsAbstractClassInterfaceImplementation;
            }
        }
        public bool IsPartialMethodWithoutBody { get { return false; } }
        /// <summary>
        /// not supported
        /// </summary>
        public bool IsExternal { get { return _m.IsPInvokeImpl; } }
        public MethodType MethodType { get { return MethodType.Method; } }

        internal MethodDefinition MethodDefinition{get { return _m; }}


        /// <summary>
        /// This needs to identify this abstract class's property MetaClass.IsNew which is defined in interface IDataObject:
        /// Rtx.Core.DataLayer.Content.CommunityDal::IDataObject.get_IsNew()
        /// 
        /// The best approach would be to find abstract class in the current assembly (_m.Module) and if the method defined in this abstract class then skip.
        /// But MetaClass defined in another assembly.
        /// 
        /// todo: compare flags of _m object for this case and for explicit interface implementation for example
        /// 
        /// So hardcoded for now: 
        /// </summary>
        private bool IsAbstractClassInterfaceImplementation
        {
            get
            {
                return
                    _m.Name.EndsWith(".IDataObject.get_IsNew") ||
                    _m.Name.EndsWith(".IImport.get_IsDeleted") ||
                    _m.Name.EndsWith(".IPageCatalogueEntry.get_IsModified") ||
                    _m.Name.EndsWith(".IResource.get_IsModified") ||
                    _m.Name.EndsWith(".IModerationDataContext.SubmitChanges");
                // Mono.Cecil.MethodAttributes.Private | Mono.Cecil.MethodAttributes.Final | Mono.Cecil.MethodAttributes.Virtual | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.VtableLayoutMask  
                // (_m.IsPrivate && _m.IsFinal && _m.IsVirtual && _m.IsHideBySig /* && VtableLayoutMask - not found in flags */);
            }
        }
    }
}