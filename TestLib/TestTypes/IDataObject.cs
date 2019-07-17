namespace TestLib.TestTypes
{
    public interface IDataObject
    {
        bool IsNew { get; }
    }

    public interface IDataInterface : IDataObject
    {
    }

    public abstract class SomeBaseClass : SomeBaseClass0
    {
    }

    public abstract class SomeBaseClass0
    {
        public bool IsNew { get { return true; } }
    }

}