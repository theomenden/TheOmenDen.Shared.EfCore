using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;
using TheOmenDen.Shared.EfCore.Converters;

namespace TheOmenDen.Shared.EfCore.Extensions;
public static class EnumerationBaseConverterExtensions
{
    public static void ConfigureEnumerationBaseModelConfigurationBuilder(this ModelConfigurationBuilder configurationBuilder)
    {
        var modelBuilder = configurationBuilder.CreateModelBuilder(null);

        var propertyTypes = modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.ClrType.GetProperties())
            .Where(p => p.PropertyType.IsDerivedFrom(typeof(EnumerationBase<,>)))
            .Select(p => p.PropertyType)
            .Distinct();

        foreach (var propertyType in propertyTypes)
        {
            var (keyType, valueType) = propertyType.GetEnumWithValueTypes(typeof(EnumerationBase<,>));

            if (keyType is null || keyType != propertyType)
            {
                continue;
            }

            var converterType = typeof(EnumerationBaseConverter<,>)
                .MakeGenericType(propertyType, keyType);

            configurationBuilder.Properties(propertyType)
                .HaveConversion(converterType);
        }
    }

    public static void ConfigureEnumerationBaseModelBuilder(this ModelBuilder modelBuilder)
    {

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType.IsDerivedFrom(typeof(EnumerationBase<,>)))
                .ToArray();


        }
    }

    private static void AddConversionToModelBuilder(ModelBuilder modelBuilder, PropertyInfo[] properties, Type entityType)
    {
        Array.ForEach(properties, (property) =>
        {
            var (keyType, enumerationValueType) = property.PropertyType.GetEnumWithValueTypes(typeof(EnumerationBase<,>));

            if (keyType is null || keyType != property.PropertyType)
            {
                return;
            }

            if (enumerationValueType is null)
            {
                return;
            }

            var converterType = typeof(EnumerationBaseConverter<,>).MakeGenericType(property.PropertyType, enumerationValueType);

            var converter = (ValueConverter)Activator.CreateInstance(converterType);

            var propertyBuilder = GetPropertyBuilder(modelBuilder, entityType, property.Name);

            if (propertyBuilder is null)
            {
                return;
            }

            propertyBuilder.HasConversion(converter);
        });
    }

    private static PropertyBuilder? GetPropertyBuilder(ModelBuilder modelBuilder, IMutableEntityType mutableEntityType, String propertyName)
    {
        var ownerships = Enumerable.Empty<IMutableForeignKey>()
            .ToList();

        var currentEntityType = mutableEntityType;

        while (currentEntityType.IsOwned())
        {
            var ownership = currentEntityType.FindOwnership();

            if (ownership is null)
            {
                return null;
            }

            ownerships.Add(ownership);

            currentEntityType = ownership.PrincipalEntityType;
        }

        var entityTypeBuilder = modelBuilder.Entity(currentEntityType.Name);

        if (ownerships.Count is 0)
        {
            return entityTypeBuilder.Property(propertyName);
        }

        var ownedNavigationBuilder = GetOwnedNavigationBuilder(entityTypeBuilder, ownerships.ToArray());

        if (ownedNavigationBuilder is null)
        {
            return null;
        }

        return ownedNavigationBuilder.Property(propertyName);
    }

    private static OwnedNavigationBuilder? GetOwnedNavigationBuilder(EntityTypeBuilder typeBuilder, IMutableForeignKey[] ownerships)
    {
        OwnedNavigationBuilder? ownedNavigationBuilder = null;

        for (var i = ownerships.Length; i >= 0; i--)
        {
            var currentOwnership = ownerships[i];

            var navigation = currentOwnership.GetNavigation(pointsToPrincipal: false);

            if(navigation is null)
            {
                ownedNavigationBuilder = currentOwnership.IsUnique
                    ? typeBuilder.OwnsOne(currentOwnership.DeclaringEntityType.Name, navigation.Name)
                    : typeBuilder.OwnsMany(currentOwnership.DeclaringEntityType.Name, navigation.Name);

                continue;
            }

            ownedNavigationBuilder = currentOwnership.IsUnique 
                ? typeBuilder.OwnsOne(currentOwnership.DeclaringEntityType.Name, navigation.Name)
                : typeBuilder.OwnsMany(currentOwnership.DeclaringEntityType.Name, navigation.Name);
        }

        return ownedNavigationBuilder;
    }
}

