﻿namespace Kongrevsky.EntityFramework.Functions.Tests.Examples
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using Kongrevsky.EntityFramework.Functions.Tests.Library.Examples;

    public partial class AdventureWorks
    {
        public const string dbo = nameof(dbo);

        public const string NameSpace = nameof(AdventureWorks);

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add functions to model.
            modelBuilder.AddFunctions<AdventureWorks>();
            modelBuilder.AddFunctions(typeof(AdventureWorksFunctions));
            modelBuilder.AddFunctions(typeof(ModelDefinedFunctions));
            modelBuilder.AddFunctions(typeof(BuiltInFunctions));
            modelBuilder.AddFunctions(typeof(NiladicFunctions));
        }

        // Defines stored procedure returning a single result type: 
        // - a ManagerEmployee sequence.
        [StoredProcedure(nameof(uspGetManagerEmployees), Schema = dbo)]
        public ObjectResult<ManagerEmployee> uspGetManagerEmployees(int? BusinessEntityID)
        {
            ObjectParameter businessEntityIdParameter = BusinessEntityID.HasValue
                ? new ObjectParameter(nameof(BusinessEntityID), BusinessEntityID)
                : new ObjectParameter(nameof(BusinessEntityID), typeof(int));

            return this.ObjectContext().ExecuteFunction<ManagerEmployee>(
                nameof(this.uspGetManagerEmployees), businessEntityIdParameter);
        }

        private const string uspLogError = nameof(uspLogError);

        // Defines stored procedure accepting an output parameter.
        // Output parameter must be ObjectParameter, with ParameterAttribute.ClrType provided.
        [StoredProcedure(uspLogError, Schema = dbo)]
        public int LogError([Parameter(DbType = "int", ClrType = typeof(int))]ObjectParameter ErrorLogID) =>
            this.ObjectContext().ExecuteFunction(uspLogError, ErrorLogID);

        // Defines stored procedure returning multiple result types: 
        // - a ProductCategory sequence.
        // - a ProductSubcategory sequence.
        [StoredProcedure(nameof(uspGetCategoryAndSubCategory), Schema = dbo)]
        [ResultType(typeof(ProductCategory))]
        [ResultType(typeof(ProductSubcategory))]
        public ObjectResult<ProductCategory> uspGetCategoryAndSubCategory(int CategoryID)
        {
            ObjectParameter categoryIdParameter = new ObjectParameter(nameof(CategoryID), CategoryID);
            return this.ObjectContext().ExecuteFunction<ProductCategory>(
                nameof(this.uspGetCategoryAndSubCategory), categoryIdParameter);
        }

        // Defines table-valued function, which must return IQueryable<T>.
        [TableValuedFunction(nameof(ufnGetContactInformation), NameSpace, Schema = dbo)]
        public IQueryable<ContactInformation> ufnGetContactInformation(
            [Parameter(DbType = "int", Name = "PersonID")]int? personId)
        {
            ObjectParameter personIdParameter = personId.HasValue
                ? new ObjectParameter("PersonID", personId)
                : new ObjectParameter("PersonID", typeof(int));

            return this.ObjectContext().CreateQuery<ContactInformation>(
                $"[{nameof(this.ufnGetContactInformation)}](@{nameof(personId)})", personIdParameter);
        }

        [TableValuedFunction(nameof(ufnGetPersons), NameSpace, Schema = dbo)]
        public IQueryable<Person> ufnGetPersons(
            [Parameter(DbType = "nvarchar", Name = "Name")]string name)
        {
            ObjectParameter nameParameter = new ObjectParameter("Name", name);

            return this.ObjectContext().CreateQuery<Person>(
                $"[{nameof(this.ufnGetPersons)}](@{nameof(name)})", nameParameter);
        }

        // Defines scalar-valued function (composable),
        // which can only be used in LINQ to Entities queries, where its body will never be executed;
        // and cannot be called directly.
        [ComposableScalarValuedFunction(nameof(ufnGetProductListPrice), Schema = dbo)]
        [return: Parameter(DbType = "money")]
        public decimal? ufnGetProductListPrice(
            [Parameter(DbType = "int")] int ProductID,
            [Parameter(DbType = "datetime")] DateTime OrderDate) =>
                Function.CallNotSupported<decimal?>();

        // Defines scalar-valued function (non-composable), 
        // which cannot be used in LINQ to Entities queries;
        // and can be called directly.
        [NonComposableScalarValuedFunction(nameof(ufnGetProductStandardCost), Schema = dbo)]
        [return: Parameter(DbType = "money")]
        public decimal? ufnGetProductStandardCost(
            [Parameter(DbType = "int")]int ProductID,
            [Parameter(DbType = "datetime")]DateTime OrderDate)
        {
            ObjectParameter productIdParameter = new ObjectParameter(nameof(ProductID), ProductID);
            ObjectParameter orderDateParameter = new ObjectParameter(nameof(OrderDate), OrderDate);
            return this.ObjectContext().ExecuteFunction<decimal?>(
                nameof(this.ufnGetProductStandardCost), productIdParameter, orderDateParameter).SingleOrDefault();
        }
    }
}
