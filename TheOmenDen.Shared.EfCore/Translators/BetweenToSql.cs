namespace TheOmenDen.Shared.EfCore.Translators;


internal sealed class BetweenToSql
{
    // C# => value.IsBetween(lowerBound, upperBound)
    // Sql => Where value Between lowerBound and upperBound

    // C# => value.IsBetweenExclusiveBounds(lowerBound, upperBound)
    // Sql => Where value is < upperBound && value > lowerBound


    // dbSet.Where(c => c.IsBetween(1,5));
    // dbSet.IsBetween(c => 1, c=> 5).Where(c => c.Name == name);
    [DbFunction(name: "Between", schema: "dbo")]
    public bool Between<T>(T value, Func<T, bool> betweenFunc)
    where T : IComparable<T>
    {
        var sb = StringBuilderPoolFactory<T>.Exists(nameof(T)) ?
            StringBuilderPoolFactory<T>.Get(nameof(T))
                : StringBuilderPoolFactory<T>.Create(nameof(T));

        //var databaseName = GetDatabaseName();
        //var schemaName = GetSchemaName();
        //var tableName = GetTableName();


        sb.AppendLine("");

        return betweenFunc?.Invoke(value) ?? false;
    }
}