namespace Kongrevsky.EntityFramework.Functions.Tests.Examples
{
    using System.Data.Entity;

    public partial class AdventureWorks : DbContext
    {
        static AdventureWorks()
        {
            Database.SetInitializer<AdventureWorks>(null);
            // Equivalent to: Database.SetInitializer(new NullDatabaseInitializer<AdventureWorks>());
        }

        public AdventureWorks()
            : base("")
        {
        }
    }
}
