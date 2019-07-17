using System;

namespace TestLib
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var sampleClass = new SampleClass { SampleAutoProp = 1 };
            Console.WriteLine(sampleClass.SampleAutoProp);
            Ololo();
        }

        private static void Ololo()
        {
            Console.WriteLine("Hello from Ololo");
            Console.ReadLine();
        }

        private static void Ololo2()
        {
        }

        private class SampleClass
        {
            public int SampleAutoProp { get; set; }
        }
    }
}