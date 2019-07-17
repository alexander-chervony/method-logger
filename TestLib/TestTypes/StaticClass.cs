using System.Runtime.InteropServices;

namespace TestLib.TestTypes
{
    public static class StaticClass
    {
        public static void MethodOlolo()
        {
        }

        public static class FilesUtilityNativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool MoveFileEx(string
            lpExistingFileName, string lpNewFileName, int dwFlags);

            public const int MOVEFILE_REPLACE_EXISTING = 0x1;
        }
    }

    public interface IPubi
    {
        void InterfMethod();
    }

    public struct TestStruct
    {
        public int Member;
    }
}