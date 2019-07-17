using System.Collections.Generic;
using System.Linq;

namespace Stringification
{
    internal class TypeNameNormalizer
    {
        private readonly Dictionary<string, string> _shortenings = new Dictionary<string, string>
        {
            { "Byte", "byte" },
            { "SByte", "sbyte" },
            { "Int32", "int" },
            { "UInt32", "uint" },
            { "Int16", "short" },
            { "UInt16", "ushort" },
            { "Int64", "long" },
            { "UInt64", "ulong" },
            { "Single", "float" },
            { "Double", "double" },
            { "Char", "char" },
            { "Boolean", "bool" },
            { "Object", "object" },
            { "String", "string" },
            { "Decimal", "decimal" }
        };

        public string Normalize(string typeName)
        {
            typeName = ReplaceNullable(typeName);
            char lastSymbol = default(char);
            // & is used by cecil (and .net) to mark reference types
            if (typeName.Last() == '?')
            {
                lastSymbol = typeName.Last();
                typeName = typeName.Substring(0, typeName.Length - 1);
            }

            typeName = UnqualifyIfNeeded(typeName);
            typeName = ShortenIfNeeded(typeName);
            typeName = typeName.RemoveWhitespace();

            if (lastSymbol != default(char))
            {
                typeName += lastSymbol;
            }
            return typeName;
        }

        private string ReplaceNullable(string typeName)
        {
            var nullables = new[] { "Nullable<", "System.Nullable<" };
            foreach (var nullable in nullables)
            {
                if (typeName.StartsWith(nullable))
                {
                    return typeName.Substring(nullable.Length, typeName.Length - (nullable.Length + 1)) + "?";
                }
            }
            return typeName;
        }

        private string UnqualifyIfNeeded(string typeName)
        {
            var dotIndex = typeName.LastIndexOf('.');
            if (dotIndex != -1)
            {
                typeName = typeName.Substring(dotIndex + 1);
            }
            return typeName;
        }

        private string ShortenIfNeeded(string typeName)
        {
            if (_shortenings.ContainsKey(typeName))
                return _shortenings[typeName];
            var arrayType = typeName.TrimEnd('[', ']');
            if (arrayType != typeName && _shortenings.ContainsKey(arrayType))
            {
                return _shortenings[arrayType] + "[]";
            }
            return typeName;
        }
    }
}