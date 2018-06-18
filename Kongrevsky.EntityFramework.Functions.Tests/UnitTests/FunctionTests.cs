namespace Kongrevsky.EntityFramework.Functions.Tests.UnitTests
{
    using System;
    using System.Data.Entity.Core.Objects;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Kongrevsky.EntityFramework.Functions.Tests.Examples;
    using Kongrevsky.EntityFramework.Functions.Tests.Library.Examples;
    using Xunit;

    public class AdventureWorksTests
    {
        [Fact]
        public void StoredProcedureWithSingleResultTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ObjectResult<ManagerEmployee> employees = adventureWorks.uspGetManagerEmployees(2);
                Assert.True(employees.Any());
            }
        }

        [Fact]
        public void StoreProcedureWithOutParameterTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                ObjectParameter errorLogId = new ObjectParameter("ErrorLogID", typeof(int)) { Value = 5 };
                int? rows = adventureWorks.LogError(errorLogId);
                Assert.Equal(0, errorLogId.Value);
                Assert.Equal(typeof(int), errorLogId.ParameterType);
                Assert.Equal(-1, rows);
            }
        }

        [Fact]
        public void StoreProcedureWithMultipleResultsTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                // The first type of result type: a sequence of ProductCategory objects.
                ObjectResult<ProductCategory> categories = adventureWorks.uspGetCategoryAndSubCategory(1);
                Assert.NotNull(categories.Single());
                // The second type of result type: a sequence of ProductCategory objects.
                ObjectResult<ProductSubcategory> subcategories = categories.GetNextResult<ProductSubcategory>();
                Assert.True(subcategories.Any());
            }
        }

        [Fact]
        public void ComplexTypeTableValuedFunctionTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ContactInformation> employees = adventureWorks.ufnGetContactInformation(1).Take(2);
                Assert.NotNull(employees.Single());
            }
        }

        [Fact]
        public void EntityTypeTableValuedFunctionTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<Person> persons = adventureWorks.ufnGetPersons("a").Take(2);
                Assert.True(persons.Any());
            }
        }

        [Fact]
        public void ComposedTableValuedFunctionInLinqTest()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                var employeesWithContactInformation =
                    from employee in database.Persons
                    from contactInfo in database.ufnGetContactInformation(employee.BusinessEntityID)
                    select new { employee.FirstName, contactInfo.JobTitle };
                var employeeWithContactInformation = employeesWithContactInformation.Take(1).ToList();
                Assert.Equal(employeeWithContactInformation.Count, 1);
            }
        }

        public void NonComposableScalarValuedFunctionTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                decimal? cost = adventureWorks.ufnGetProductStandardCost(999, DateTime.Now);
                Assert.NotNull(cost);
                Assert.True(cost > 1);
            }
        }

        [Fact]
        public void NonComposableScalarValuedFunctionLinqTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                try
                {
                    adventureWorks
                        .Products
                        .Where(product => product.ListPrice >= adventureWorks.ufnGetProductStandardCost(999, DateTime.Now))
                        .ToArray();
                    Assert.True(false);
                }
                catch (NotSupportedException exception)
                {
                    Trace.WriteLine(exception);
                }
            }
        }

        [Fact]
        public void ComposableScalarValuedFunctionLinqTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<Product> products = adventureWorks
                    .Products
                    .Where(product => product.ListPrice <= adventureWorks.ufnGetProductListPrice(999, DateTime.Now));
                Assert.True(products.Any());
            }
        }

        [Fact]
        public void ComposableScalarValuedFunctionTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                try
                {
                    adventureWorks.ufnGetProductListPrice(999, DateTime.Now);
                    Assert.True(false);
                }
                catch (NotSupportedException exception)
                {
                    Trace.WriteLine(exception);
                }
            }
        }

        [Fact]
        public void AggregateFunctionLinqTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                var categories = adventureWorks.ProductSubcategories
                    .GroupBy(subcategory => subcategory.ProductCategoryID)
                    .Select(category => new
                    {
                        CategoryId = category.Key,
                        SubcategoryNames = category.Select(subcategory => subcategory.Name).Concat()
                    })
                    .ToArray();
                Assert.True(categories.Length > 0);
                foreach (var category in categories)
                {
                    Assert.True(category.CategoryId > 0);
                    Assert.False(string.IsNullOrWhiteSpace(category.SubcategoryNames));
                }
            }
        }

        [Fact]
        public void BuiltInFunctionLinqTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                var categories = adventureWorks.ProductSubcategories
                    .GroupBy(subcategory => subcategory.ProductCategoryID)
                    .Select(category => new
                    {
                        CategoryId = category.Key,
                        SubcategoryNames = category.Select(subcategory => subcategory.Name.Left(4)).Concat()
                    })
                    .ToList();
                Assert.True(categories.Count > 0);
                categories.ForEach(category =>
                {
                    Assert.True(category.CategoryId > 0);
                    Assert.False(string.IsNullOrWhiteSpace(category.SubcategoryNames));
                });
            }
        }

        [Fact]
        public void NiladicFunctionLinqTest()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                var firstCategory = adventureWorks.ProductSubcategories
                    .GroupBy(subcategory => subcategory.ProductCategoryID)
                    .Select(category => new
                    {
                        CategoryId = category.Key,
                        SubcategoryNames = category.Select(subcategory => subcategory.Name.Left(4)).Concat(),
                        CurrentTimestamp = NiladicFunctions.CurrentTimestamp(),
                        CurrentUser = NiladicFunctions.CurrentUser(),
                        SessionUser = NiladicFunctions.SessionUser(),
                        SystemUser = NiladicFunctions.SystemUser(),
                        User = NiladicFunctions.User()
                    })
                    .First();
                Assert.NotNull(firstCategory);
                Assert.NotNull(firstCategory.CurrentTimestamp);
                Trace.WriteLine(DateTime.Now.Ticks);
                Trace.WriteLine(firstCategory.CurrentTimestamp.Value.Ticks);
                Assert.True(DateTime.Now >= firstCategory.CurrentTimestamp);
                Assert.Equal("dbo", firstCategory.CurrentUser, true);
                Assert.Equal("dbo", firstCategory.SessionUser, true);
                Assert.Equal($@"{Environment.UserDomainName}\{Environment.UserName}", firstCategory.SystemUser, true);
                Assert.Equal("dbo", firstCategory.User, true);
            }
        }

        [Fact]
        public void ModelDefinedFunctionInLinqTest()
        {
            using (AdventureWorks database = new AdventureWorks())
            {
                var employees = from employee in database.Persons
                                where employee.Title != null
                                let formatted = employee.FormatName()
                                select new
                                {
                                    formatted,
                                    employee
                                };
                var employeeData = employees.Take(1).ToList().FirstOrDefault();
                Assert.NotNull(employeeData);
                Assert.NotNull(employeeData.formatted);
                Assert.Equal(employeeData.employee.FormatName(), employeeData.formatted);
            }

            using (AdventureWorks database = new AdventureWorks())
            {
                var employees = from employee in database.Persons
                                where employee.Title != null
                                select new
                                {
                                    Decimal = employee.ParseDecimal(),
                                    Int32 = employee.BusinessEntityID
                                };
                var employeeData = employees.Take(1).ToList().FirstOrDefault();
                Assert.NotNull(employeeData);
                Assert.Equal(employeeData.Decimal, Convert.ToInt32(employeeData.Int32));
            }
        }
    }
}
