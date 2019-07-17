using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ololo;

namespace TestLib.TestTypes
{
    public partial class SomeTestType : SomeBaseClass, ISomeTestInterface, IEnumerable<string>
    {
        public void GenericVoid<TMethParam>(string foo, TMethParam bar)
        {
        }

        public TMethParam GenericEcho<TMethParam>(string foo)
            where TMethParam : class
        {
            return foo as TMethParam;
        }

        public string Overload(string foo)
        {
            return foo;
        }

        public string Overload(string foo, int bar)
        {
            return foo;
        }

        public void ArrayArgMethod(string[] args)
        {
        }

        public void ParamsArgMethod(params string[] pars)
        {
        }

        public void GenericInstanceParameterMethod1(Dictionary<string, List<int>> dictList)
        {
        }

        public void GenericInstanceParameterMethod2(Dictionary<string, int> dictList)
        {
        }

        public void GenericInstanceParameterMethod3<TMethParam>(Dictionary<string, List<List<IEnumerable<TMethParam>>>> dictList)
        {
        }

        public void MethodWithLambdasInside()
        {
            MethodWithActionParam(actParam => { });
        }

        public void MethodWithNullableParams(int? par1, decimal? par2, IEnumerable<int?> list)
        {
        }

        public void MethodWithActionParam(Action<object> act)
        {
        }

        public delegate void Del(object obj);

        public void MethodWithDelegateParam1(Del del)
        {
        }

        public bool MethodWithOutParam(string value, out byte result)
        {
            return byte.TryParse(value, out result);
        }

        public void MethodWithGenericOutParam(out byte? result)
        {
            result = 0;
        }

        public void MethodWithGenericRefParam(ref Nullable<byte> result)
        {
        }

        public void MethodWithComplexGenericRefParam1(ref List<Nullable<byte>> result)
        {
        }

        public void MethodWithComplexGenericRefParam2(ref Dictionary<string, IEnumerable<List<Nullable<byte>>>> result)
        {
        }

        public void MethodWithRefParam(ref byte result)
        {
        }

        public void MethodWithFullyQualifiedParam1(ref System.String result)
        {
        }

        public void MethodWithFullyQualifiedParam2(ref Ololo.AClass.BClass result)
        {
        }

        public void MethodWithFullyQualifiedParam3(ref Dictionary<Ololo.AClass, List<List<Ololo.AClass.BClass>>> result)
        {
        }

        public void MethodWithDynamicParam(dynamic result)
        {
        }

        //public void MethodWithDynamicListParam(List<dynamic> result)
        //{
        //}

        private static int PrivStat()
        {
            return 0;
        }

        private void PrivInst()
        {
        }

        void ISomeTestInterface.ExplicitInterfaceImplementationMethod()
        {
        }

        void ISomeTestInterface.ExplicitInterfaceImplementationWithLambdaInsideMethod()
        {
            if (new []{""}.Any(s => s.IsNormalized()))
            {
            }
        }

        void ISomeTestInterface.ExplicitInterfaceImplementationAndSameNameMethod()
        {
        }

        void ExplicitInterfaceImplementationAndSameNameMethod()
        {
        }

        public IEnumerable<string> EnumerableMethod()
        {
            throw new NotImplementedException();
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void SimpleItemArrayMethod(string[] array)
        {
        }

        public void GenericItemArrayMethod(KeyValuePair<string, string>[] array)
        {
        }
    }
}