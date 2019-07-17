using System;
using System.Collections.Generic;
using LibWeaver;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;
using Stringification;
using TestLib.TestTypes;
using Tests.ReflectionTestInfoProviders;

namespace Tests
{
    [TestFixture]
    public class CecilStringifierTest
    {
        ///<summary>
        /// Main test method - proves that everything ok by comparing the results to the ones recieved by reflection. reflection stringification in turn is covered with unit tests
        ///</summary>
        [Test]
        public void CecilNamesShouldBeTheSameAsReflectedNames()
        {
            var cecilMethods = GetCecilTestMethods();

            var refelectionMethods = ReflectionMethodLister.GetTestMethods();

            CollectionAssert.AreEqual(cecilMethods, refelectionMethods);
        }

        [Test]
        public void ShouldStringify_ArrayArgMethod()
        {
            var actualString = GetMethodByName("ArrayArgMethod");
            Assert.That(actualString, Is.EqualTo("SomeTestType::ArrayArgMethod(string[] args)"));
        }

        [Test]
        public void ShouldStringify_ParamsArgMethod()
        {
            var actualString = GetMethodByName("ParamsArgMethod");
            Assert.That(actualString, Is.EqualTo("SomeTestType::ParamsArgMethod(params string[] pars)"));
        }

        [Test]
        public void ShouldStringify_GenericInstanceParameterMethod1()
        {
            var actualString = GetMethodByName("GenericInstanceParameterMethod1");
            Assert.That(actualString, Is.EqualTo("SomeTestType::GenericInstanceParameterMethod1(Dictionary<string,List<int>> dictList)"));
        }

        [Test]
        public void ShouldStringify_GenericInstanceParameterMethod2()
        {
            var actualString = GetMethodByName("GenericInstanceParameterMethod2");
            Assert.That(actualString, Is.EqualTo("SomeTestType::GenericInstanceParameterMethod2(Dictionary<string,int> dictList)"));
        }

        [Test]
        public void ShouldStringify_GenericInstanceParameterMethod3()
        {
            var actualString = GetMethodByName("GenericInstanceParameterMethod3");
            Assert.That(actualString, Is.EqualTo("SomeTestType::GenericInstanceParameterMethod3<TMethParam>(Dictionary<string,List<List<IEnumerable<TMethParam>>>> dictList)"));
        }

        [Test]
        public void ShouldStringify_MethodWithNullableParams()
        {
            var actualString = GetMethodByName("MethodWithNullableParams");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithNullableParams(int? par1, decimal? par2, IEnumerable<int?> list)"));
        }

        [Test]
        public void ShouldStringify_MethodWithInnerClassParam()
        {
            var actualString = GetMethodByName("MethodWithInnerClassParam");
            Assert.That(actualString, Is.EqualTo("GenericTestType<TClassParam>::MethodWithInnerClassParam(InnerClass<TClassParam> foo)"));
        }

        [Test]
        public void ShouldStringify_MethodWithOutParam()
        {
            var actualString = GetMethodByName("MethodWithOutParam");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithOutParam(string value, out byte result)"));
        }

        [Test]
        public void ShouldStringify_MethodWithRefParam()
        {
            var actualString = GetMethodByName("MethodWithRefParam");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithRefParam(ref byte result)"));
        }

        [Test]
        public void ShouldStringify_MethodWithGenericOutParam()
        {
            var actualString = GetMethodByName("MethodWithGenericOutParam");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithGenericOutParam(out byte? result)"));
        }

        [Test]
        public void ShouldStringify_MethodWithGenericRefParam()
        {
            var actualString = GetMethodByName("MethodWithGenericRefParam");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithGenericRefParam(ref byte? result)"));
        }

        [Test]
        public void ShouldStringify_MethodWithComplexGenericRefParam2()
        {
            var actualString = GetMethodByName("MethodWithComplexGenericRefParam2");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithComplexGenericRefParam2(ref Dictionary<string,IEnumerable<List<byte?>>> result)"));
        }

        [Test]
        public void ShouldStringify_ExplicitInterfaceImplementationMethod()
        {
            var actualString = GetMethodByName("ExplicitInterfaceImplementationMethod");
            Assert.That(actualString, Is.EqualTo("SomeTestType::ISomeTestInterface.ExplicitInterfaceImplementationMethod()"));
        }

        [Test]
        public void ShouldStringify_ExplicitInterfaceImplementationWithLambdaInsideMethod()
        {
            var actualString = GetMethodByName("ISomeTestInterface.ExplicitInterfaceImplementationWithLambdaInsideMethod");
            Assert.That(actualString, Is.EqualTo("SomeTestType::ISomeTestInterface.ExplicitInterfaceImplementationWithLambdaInsideMethod()"));
        }

        [Test]
        public void ShouldStringify_ExplicitInterfaceImplementationAndSameNameMethod_Explicit()
        {
            var actualString = GetMethodByName("ISomeTestInterface.ExplicitInterfaceImplementationAndSameNameMethod");
            Assert.That(actualString, Is.EqualTo("SomeTestType::ISomeTestInterface.ExplicitInterfaceImplementationAndSameNameMethod()"));
        }

        [Test]
        public void ShouldStringify_ExplicitInterfaceImplementationAndSameNameMethod_SameName()
        {
            var actualString = GetMethodByName("::ExplicitInterfaceImplementationAndSameNameMethod");
            Assert.That(actualString, Is.EqualTo("SomeTestType::ExplicitInterfaceImplementationAndSameNameMethod()"));
        }

        [Test]
        public void ShouldStringify_MethodWithFullyQualifiedParam1()
        {
            var actualString = GetMethodByName("MethodWithFullyQualifiedParam1");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithFullyQualifiedParam1(ref string result)"));
        }

        [Test]
        public void ShouldStringify_MethodWithDynamicParam()
        {
            var actualString = GetMethodByName("MethodWithDynamicParam");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithDynamicParam(dynamic result)"));
        }

        [Test]
        public void ShouldStringify_EnumerableMethod()
        {
            var actualString = GetMethodByName("EnumerableMethod");
            Assert.That(actualString, Is.EqualTo("SomeTestType::EnumerableMethod()"));
        }

        [Test]
        public void ShouldStringify_MethodWithExternalClassParam()
        {
            var actualString = GetMethodByName("MethodWithExternalClassParam");
            Assert.That(actualString, Is.EqualTo("InnerClassB<TClassParam>::MethodWithExternalClassParam(AClass foo)"));
        }

        [Test]
        public void ShouldStringify_SimpleItemArrayMethod()
        {
            var actualString = GetMethodByName("SimpleItemArrayMethod");
            Assert.That(actualString, Is.EqualTo("SomeTestType::SimpleItemArrayMethod(string[] array)"));
        }

        [Test]
        public void ShouldStringify_GenericItemArrayMethod()
        {
            var actualString = GetMethodByName("GenericItemArrayMethod");
            Assert.That(actualString, Is.EqualTo("SomeTestType::GenericItemArrayMethod(KeyValuePair<string,string>[] array)"));
        }

        [Test]
        public void ShouldNotStringify_ExternalMethod()
        {
            var methodInfoProvider = CreateTestTypesLister().GetMethodUnsafe("MoveFileEx");
            Assert.IsTrue(methodInfoProvider.IsExternal);
        }

        [Test]
        public void Print()
        {
            GetCecilTestMethods().Print();
        }

        [Test]
        public void GetIlInstructions()
        {
            var method = GetMethodDefinitionByName("WeavedOpCodesContainer");
            foreach (Instruction instruction in method.Body.Instructions)
            {
                Console.WriteLine(instruction);
            }
        }

        private static string GetMethodByName(string methodSubstring)
        {
            var targetMethod = CreateTestTypesLister().GetByName(methodSubstring);
            var actualString = Stringifier.GetMethodDeclarationString(targetMethod);
            return actualString.TrimNamespace("TestLib.TestTypes.");
        }

        private static MethodDefinition GetMethodDefinitionByName(string methodSubstring)
        {
            var targetMethod = CreateAllTestTypesLister().GetByName(methodSubstring);
            return ((CecilMethodInfoProvider)targetMethod).MethodDefinition;
        }

        private static CecilMethodLister CreateTestTypesLister()
        {
            return new CecilMethodLister(
                new[] { typeof(GenericTestType<>).Assembly.Location },
                typeDefinition => typeDefinition.FullName.StartsWith("TestLib.TestTypes"));
        }

        private static CecilMethodLister CreateAllTestTypesLister()
        {
            return new CecilMethodLister(
                new[] { typeof(GenericTestType<>).Assembly.Location });
        }

        private static IEnumerable<string> GetCecilTestMethods()
        {
            return CreateTestTypesLister().GetAllMethods(methodName => methodName.EndsWith(".ctor()", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}