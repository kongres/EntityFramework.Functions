namespace EntityFramework.Functions
{
    using System;

    public static partial class Function
    {
        public static T CallNotSupported<T>(
            string methodName = null)
        {
            // System.Data.Entity.Strings.ELinq_EdmFunctionDirectCall.
            throw new NotSupportedException(
                $"Direct call to method {methodName} is not supported. This function can only be invoked from LINQ to Entities.");
        }
    }
}
