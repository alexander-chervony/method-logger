using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Stringification;
using TestLib.TestTypes;
using Tests.ReflectionTestInfoProviders;

namespace Tests
{
    [TestFixture]
    public class ReflectionStringfierTest
    {
        [Test]
        public void ShouldStringify_Overload()
        {
            AssertStringifiedCorrectly<SomeTestType>(t => t.Overload(""), "SomeTestType::Overload(string foo)");
            AssertStringifiedCorrectly<SomeTestType>(t => t.Overload("", 0), "SomeTestType::Overload(string foo, int bar)");
        }

        [Test]
        public void ShouldStringify_ArrayArgMethod()
        {
            AssertStringifiedCorrectly<SomeTestType>(t => t.ArrayArgMethod(new[] { "" }), "SomeTestType::ArrayArgMethod(string[] args)");
        }

        [Test]
        public void ShouldStringify_ParamsArgMethod()
        {
            AssertStringifiedCorrectly<SomeTestType>(t => t.ParamsArgMethod("", ""), "SomeTestType::ParamsArgMethod(params string[] pars)");
        }

        [Test]
        public void ShouldStringify_GenericInstanceParameterMethod1()
        {
            AssertStringifiedCorrectly<SomeTestType>(
                t => t.GenericInstanceParameterMethod1(new Dictionary<string, List<int>>()),
                "SomeTestType::GenericInstanceParameterMethod1(Dictionary<string,List<int>> dictList)");
        }

        [Test]
        public void ShouldStringify_GenericInstanceParameterMethod2()
        {
            AssertStringifiedCorrectly<SomeTestType>(
                t => t.GenericInstanceParameterMethod2(new Dictionary<string, int>()),
                "SomeTestType::GenericInstanceParameterMethod2(Dictionary<string,int> dictList)");
        }

        [Test]
        public void ShouldStringify_MethodWithOutParam()
        {
            byte result;
            AssertStringifiedCorrectly<SomeTestType>(
                t => t.MethodWithOutParam("", out result),
                "SomeTestType::MethodWithOutParam(string value, out byte result)");
        }

        [Test]
        public void ShouldStringify_MethodWithRefParame()
        {
            byte result = 0;
            AssertStringifiedCorrectly<SomeTestType>(
                t => t.MethodWithRefParam(ref result),
                "SomeTestType::MethodWithRefParam(ref byte result)");
        }

        [Test]
        public void ShouldStringify_MethodWithGenericOutParam()
        {
            byte? result = 0;
            AssertStringifiedCorrectly<SomeTestType>(
                t => t.MethodWithGenericOutParam(out result),
                "SomeTestType::MethodWithGenericOutParam(out byte? result)");
        }

        [Test]
        public void ShouldStringify_MethodWithGenericRefParam()
        {
            byte? result = 0;
            AssertStringifiedCorrectly<SomeTestType>(
                t => t.MethodWithGenericRefParam(ref result),
                "SomeTestType::MethodWithGenericRefParam(ref byte? result)");
        }

        [Test]
        public void ShouldStringify_MethodWithDynamicParam()
        {
            AssertStringifiedCorrectly<SomeTestType>(
                t => t.MethodWithDynamicParam(null),
                "SomeTestType::MethodWithDynamicParam(dynamic result)");
        }

        [Test]
        public void ShouldNotStringify_ExternalMethod()
        {
            var methodInfoProvider = ReflectionMethodLister.CreateTestTypesLister().GetMethodUnsafe("MoveFileEx");
            Assert.IsTrue(methodInfoProvider.IsExternal);
        }

        [Test]
        public void ShouldStringify_GenericTestType()
        {
            var methods = ReflectionMethodLister.GetTestMethods().ToArray();

            CollectionAssert.Contains(methods, "TestLib.TestTypes.GenericTestType<TClassParam>::MethodWithClassTypeParam(string foo, TClassParam bar)");
            CollectionAssert.Contains(methods, "TestLib.TestTypes.GenericTestType<TClassParam>::MethodWithMethodTypeParam<TMethParam>(string foo, TMethParam bar)");
            CollectionAssert.Contains(methods, "TestLib.TestTypes.GenericTestType<TClassParam>::MethodWithDelegateParamDeclaredInGenericClass(object requestItemKey, DelegateDeclaredInGenericClass<TClassParam> function)");
        }

        [Test]
        public void ShouldStringify_Privates()
        {
            var methods = ReflectionMethodLister.GetTestMethods().ToArray();

            CollectionAssert.Contains(methods, "TestLib.TestTypes.SomeTestType::GenericVoid<TMethParam>(string foo, TMethParam bar)");
            CollectionAssert.Contains(methods, "TestLib.TestTypes.SomeTestType::GenericEcho<TMethParam>(string foo)");
            CollectionAssert.Contains(methods, "TestLib.TestTypes.SomeTestType::PrivStat()");
            CollectionAssert.Contains(methods, "TestLib.TestTypes.SomeTestType::PrivInst()");
            // todo: prop support
            // CollectionAssert.Contains(methods, "SomeTestType::get_Prop()");
            // CollectionAssert.Contains(methods, "SomeTestType::set_Prop(long value)");
        }

        [Test]
        public void Print()
        {
            ReflectionMethodLister.GetTestMethods().Print();
        }

        private void AssertStringifiedCorrectly<TExpr>(Expression<Action<TExpr>> methodInvocation, string expectedMethodString)
        {
            MethodInfo targetMethod;
            if (methodInvocation.Body is MethodCallExpression)
            {
                var b = (MethodCallExpression)methodInvocation.Body;
                targetMethod = b.Method;
            }
            else
            {
                throw new InvalidOperationException("MethodCallExpression expected");
            }

            string actualString = Stringifier.GetMethodDeclarationString(new ReflectionMethodInfoProvider(targetMethod));

            Assert.That(actualString.TrimNamespace( "TestLib.TestTypes."), Is.EqualTo(expectedMethodString));
        }
    }
}