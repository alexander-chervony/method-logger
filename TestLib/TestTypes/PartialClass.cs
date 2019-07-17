namespace TestLib.TestTypes
{
    public partial class PartialClass
    {
        partial void PartialMethodWithoutBody1();

        partial void PartialMethodWithoutBody2();
    }

    public partial class PartialClass
    {
        partial void PartialMethodWithoutBody2()
        {
        }
    }
}