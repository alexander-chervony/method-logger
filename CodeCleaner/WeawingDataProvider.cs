using System;
using System.Collections.Generic;
using LibWeaver;
using Stringification;

namespace CodeCleaner
{
    public interface IWeawingDataProvider
    {
        IEnumerable<string> GetMethodsFoundInWeavedLibs();
    }

    public class WeawingDataProvider : IWeawingDataProvider
    {
        private readonly string _libsPath;
        private readonly Action<string> _log;

        public WeawingDataProvider(string libsPath, Action<string> log)
        {
            _libsPath = libsPath;
            _log = log;
        }

        public IEnumerable<string> GetMethodsFoundInWeavedLibs()
        {
            _log("Started WeawingDataProvider.GetMethodsFoundInWeavedLibs()");
            var methodsFoundInWeavedLibs = new CecilMethodLister(LibWeaver.Program.GetLibsToWeave(_libsPath, new string[0])).GetAllMethods();
            _log("Ended WeawingDataProvider.GetMethodsFoundInWeavedLibs()");
            return methodsFoundInWeavedLibs;
        }
    }
}