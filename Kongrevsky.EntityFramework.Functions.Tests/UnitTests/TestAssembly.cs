namespace Kongrevsky.EntityFramework.Functions.Tests.UnitTests
{
    using System;
    using System.Data.SqlClient;
    using System.IO;

    public static class TestAssembly
    {
        public static void Initialize()
        {
            AppDomain.CurrentDomain.SetData(
                "DataDirectory",
                // .. in path does not work. so use new DirectoryInfo(path).FullName to remove .. in path.
                new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data")).FullName);

            using (SqlConnection connection = new SqlConnection(""))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("EXEC sys.sp_configure @configname = N'clr enabled', @configvalue = 1", connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SqlCommand command = new SqlCommand("RECONFIGURE", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
