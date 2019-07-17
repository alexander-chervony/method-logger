namespace TestLib.TestTypes
{
    public partial class SomeTestType
    {
        private string _propWithBackingField;

        public long LongProp { get; set; }
        public static long StatProp { get; set; }

        public string PropWithBackingField
        {
            get { return _propWithBackingField; }
            set { _propWithBackingField = value; }
        }

        public string PropWithPrivateEmptySetter
        {
            get
            {
                return _propWithBackingField;
            }

            private set { }
        }
    }
}