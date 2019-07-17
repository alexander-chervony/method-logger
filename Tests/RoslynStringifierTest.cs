using System;
using System.Linq;
using CodeCleaner;
using NUnit.Framework;
using Stringification;
using Tests.ReflectionTestInfoProviders;

namespace Tests
{
    [TestFixture]
    public class RoslynStringifierTest
    {
        private static readonly RoslynMethodLister RoslynMethodLister = new RoslynMethodLister(@"c:\sources\MethodLogger\MethodLogger.sln", projectNameEndsWith: "TestLib", namespaceStartsWith: "TestLib.TestTypes");

        ///<summary>
        /// Main test method - proves that everything ok by comparing the results to the ones recieved by reflection. reflection stringification in turn is covered with unit tests
        ///</summary>
        [Test]
        public void RoslynNamesShouldBeTheSameAsReflectedNames()
        {
            var roslynMethods = RoslynMethodLister.GetAllMethods().ToArray();

            var refelectionMethods = ReflectionMethodLister.GetTestMethods().ToArray();

            var notFound = refelectionMethods.Except(roslynMethods).ToArray();

            Console.WriteLine("roslynMethods = {0}, refelectionMethods = {1}, notFound = {2}\r\n", roslynMethods.Count(), refelectionMethods.Count(), notFound.Count());

            Console.WriteLine("List of not foud methods:");
            foreach (var nf in notFound)
            {
                Console.WriteLine("\r\n" + nf);
            }

            CollectionAssert.AreEqual(roslynMethods, refelectionMethods);
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
        public void ShouldStringify_MethodWithLambdas()
        {
            var actualString = GetMethodByName("MethodWithLambdasInside");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithLambdasInside()"));
        }

        [Test]
        public void ShouldStringify_MethodWithActionParam()
        {
            var actualString = GetMethodByName("MethodWithActionParam");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithActionParam(Action<object> act)"));
        }

        [Test]
        public void ShouldStringify_MethodWithDelegateParam1()
        {
            var actualString = GetMethodByName("MethodWithDelegateParam1");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithDelegateParam1(Del del)"));
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
        public void ShouldStringify_MethodWithRefParame()
        {
            var actualString = GetMethodByName("MethodWithRefParam");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithRefParam(ref byte result)"));
        }

        [Test]
        public void ShouldStringify_ExplicitInterfaceImplementationMethod()
        {
            var actualString = GetMethodByName("ExplicitInterfaceImplementationMethod");
            Assert.That(actualString, Is.EqualTo("SomeTestType::ISomeTestInterface.ExplicitInterfaceImplementationMethod()"));
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
        public void ShouldStringify_MethodWithComplexGenericRefParam1()
        {
            var actualString = GetMethodByName("MethodWithComplexGenericRefParam1");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithComplexGenericRefParam1(ref List<byte?> result)"));
        }

        [Test]
        public void ShouldStringify_MethodWithComplexGenericRefParam2()
        {
            var actualString = GetMethodByName("MethodWithComplexGenericRefParam2");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithComplexGenericRefParam2(ref Dictionary<string,IEnumerable<List<byte?>>> result)"));
        }

        [Test]
        public void ShouldStringify_MethodWithFullyQualifiedParam1()
        {
            var actualString = GetMethodByName("MethodWithFullyQualifiedParam1");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithFullyQualifiedParam1(ref string result)"));
        }

        [Test]
        public void ShouldStringify_MethodWithFullyQualifiedParam2()
        {
            var actualString = GetMethodByName("MethodWithFullyQualifiedParam2");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithFullyQualifiedParam2(ref BClass result)"));
        }

        [Test]
        public void ShouldStringify_MethodWithFullyQualifiedParam3()
        {
            var actualString = GetMethodByName("MethodWithFullyQualifiedParam3");
            Assert.That(actualString, Is.EqualTo("SomeTestType::MethodWithFullyQualifiedParam3(ref Dictionary<AClass,List<List<BClass>>> result)"));
        }

        [Test]
        public void ShouldStringify_MethodWithDelegateParamDeclaredInGenericClass()
        {
            var actualString = GetMethodByName("MethodWithDelegateParamDeclaredInGenericClass");
            Assert.That(actualString, Is.EqualTo("GenericTestType<TClassParam>::MethodWithDelegateParamDeclaredInGenericClass(object requestItemKey, DelegateDeclaredInGenericClass<TClassParam> function)"));
        }

        [Test]
        public void ShouldStringify_MethodWithExternalClassParam()
        {
            var actualString = GetMethodByName("MethodWithExternalClassParam");
            Assert.That(actualString, Is.EqualTo("InnerClassB<TClassParam>::MethodWithExternalClassParam(AClass foo)"));
        }

        [Test]
        public void Print()
        {
            RoslynMethodLister.PrintAllMethods();
        }

        private static string GetMethodByName(string nameSubstr)
        {
            var targetMethod = RoslynMethodLister.GetByName(nameSubstr);
            var actualString = Stringifier.GetMethodDeclarationString(targetMethod);
            return actualString.TrimNamespace("TestLib.TestTypes.");
        }
    }
}