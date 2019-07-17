using System;
using System.Collections.Generic;

namespace Ololo
{
    public interface ISomeTestInterface
    {
        void ExplicitInterfaceImplementationMethod();
        void ExplicitInterfaceImplementationWithLambdaInsideMethod();
        void ExplicitInterfaceImplementationAndSameNameMethod();
    }

    public class AClass : List<CClass>
    {
        public class BClass
        {
        }
    }

    public class CClass
    {
    }
}