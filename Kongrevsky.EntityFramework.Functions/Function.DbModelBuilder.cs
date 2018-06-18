namespace Kongrevsky.EntityFramework.Functions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Linq;
    using System.Reflection;

    public static partial class Function
    {
        public static void AddComplexTypesFromAssembly(this DbModelBuilder modelBuilder, Assembly assembly)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            IEnumerable<Type> complexTypes = assembly
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .Concat(Enumerable.Repeat(assembly, 1))
                .SelectMany(assemblyOrReference => assemblyOrReference.GetExportedTypes())
                .Where(type => Attribute.IsDefined(type, typeof(ComplexTypeAttribute)));
            MethodInfo complexTypeMethod = typeof(DbModelBuilder).GetMethod(nameof(modelBuilder.ComplexType));
            complexTypes.ForEach(complexType =>
                complexTypeMethod.MakeGenericMethod(complexType).Invoke(modelBuilder, null));
        }

        public static void AddFunctions<TFunctions>(
            this DbModelBuilder modelBuilder, bool addComplexTypesFromAssembly = true)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.AddFunctions(typeof(TFunctions), addComplexTypesFromAssembly);
        }

        // This overload is provided because when TFunctions is static class, AddFunctions<TFunctions> cannot be compiled.
        public static void AddFunctions(
            this DbModelBuilder modelBuilder, Type functionsType, bool addComplexTypesFromAssembly = true)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            if (functionsType == null)
            {
                throw new ArgumentNullException(nameof(functionsType));
            }

            if (addComplexTypesFromAssembly)
            {
                modelBuilder.AddComplexTypesFromAssembly(functionsType.Assembly);
            }

            modelBuilder.Conventions.Add(new FunctionConvention(functionsType));
        }
    }
}