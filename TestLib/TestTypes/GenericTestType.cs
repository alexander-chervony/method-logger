using Ololo;

namespace TestLib.TestTypes
{
    public class GenericTestType<TClassParam   >
    {
        public class InnerClass
        {
        }

        public class InnerClassB
        {
            public void MethodWithExternalClassParam(AClass foo)
            {
            }
        }

        public void MethodWithInnerClassParam(InnerClass foo)
        {
        }

        public void MethodWithClassTypeParam(string foo, TClassParam bar)
        {
        }

        public void MethodWithMethodTypeParam<TMethParam>(string foo, TMethParam bar)
        {
        }


        public delegate TClassParam DelegateDeclaredInGenericClass();

        public static TClassParam MethodWithDelegateParamDeclaredInGenericClass(object requestItemKey, DelegateDeclaredInGenericClass function)
        {
            return default(TClassParam);
        }
    }
}