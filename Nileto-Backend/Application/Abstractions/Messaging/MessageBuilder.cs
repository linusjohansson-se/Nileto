using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Application.Abstractions.Messaging;

public class MessageBuilder
{
    private readonly DbContext _dbContext;
    public MessageBuilder(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public InsertMessage Insert<TEntity>(
        TEntity entity,
        IReadOnlyDictionary<string, object?> fields)
        where TEntity : class
    {
        var table = GetTable<TEntity>();
        var values = BuildValues(entity, table, fields);
        
        return new InsertMessage(table, values);
    }
    
    public UpdateMessage Update<TEntity>(
        TEntity entity,
        IReadOnlyDictionary<string, object?> fields,
        IReadOnlyDictionary<string, object?> where)
        where TEntity : class
    {
        var table = GetTable<TEntity>();
        var values = BuildValues(entity, table, fields);

        return new UpdateMessage(table, values, where);
    }

    public SelectMessage Select<TEntity>(
        IReadOnlyDictionary<string, object?>? filters,
        IReadOnlyList<string>? columns = null)
        where TEntity : class
    {
        var table = GetTable<TEntity>();
        var allColumns = columns ?? GetAllColumns<TEntity>(table);

        return new SelectMessage(table, allColumns, filters);
    }
    
    private string GetTable<TEntity>()
        where TEntity : class
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(TEntity))
                         ?? throw new InvalidOperationException(
                             $"Type {typeof(TEntity).Name} is not mapped in EF");

        var tableName = entityType.GetTableName();
        if (tableName is null)
            throw new InvalidOperationException(
                $"Type {typeof(TEntity).Name} is not mapped to a table");

        var schema = entityType.GetSchema();

        return schema is null
            ? tableName
            : $"{schema}.{tableName}";
    }
    
    private IReadOnlyList<string> GetAllColumns<TEntity>(string table)
        where TEntity : class
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(TEntity))
                         ?? throw new InvalidOperationException(
                             $"Type {typeof(TEntity).Name} is not mapped in EF");

        var tableName = entityType.GetTableName()
                        ?? throw new InvalidOperationException("No table name");

        var schema = entityType.GetSchema();

        var storeObject = StoreObjectIdentifier.Table(tableName, schema);

        return entityType
            .GetProperties()
            .Select(p => p.GetColumnName(storeObject))
            .Where(c => c is not null)
            .Cast<string>()
            .ToList();
    }

    private IReadOnlyDictionary<string, object?> BuildValues<TEntity>(
        TEntity entity,
        string table,
        IReadOnlyDictionary<string, object?> fields)
        where TEntity : class
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(TEntity))
                         ?? throw new InvalidOperationException(
                             $"Type {typeof(TEntity).Name} is not mapped in EF");

        var tableName = entityType.GetTableName()
                        ?? throw new InvalidOperationException("No table name");

        var schema = entityType.GetSchema();
        var storeObject = StoreObjectIdentifier.Table(tableName, schema);

        // Se till att vi kan läsa värden via Entry.Property även för shadow props
        var entry = _dbContext.Entry(entity);
        if (entry.State == EntityState.Detached)
            _dbContext.Attach(entity);

        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, providedValue) in fields)
        {
            var prop = entityType.GetProperties()
                .FirstOrDefault(p =>
                {
                    var col = p.GetColumnName(storeObject);
                    return col != null && col.Equals(key, StringComparison.OrdinalIgnoreCase);
                });

            if (prop is null)
                throw new InvalidOperationException(
                    $"Field '{key}' is not a mapped column for '{entityType.ClrType.Name}'.");

            var columnName = prop.GetColumnName(storeObject)
                             ?? throw new InvalidOperationException(
                                 $"No column mapping found for '{entityType.ClrType.Name}.{prop.Name}'.");

            // Använd värdet från dict (inkl null) om key finns, annars läs från entity
            object? valueToUse = providedValue;

            // Konvertera via EF ValueConverter (t.ex. enums, value objects)
            valueToUse = ConvertToProviderValue(prop, valueToUse);

            result[columnName] = valueToUse;
        }

        return result;
    }

    private static object? ConvertToProviderValue(IProperty prop, object? value)
    {
        if (value is null) return null;

        var converter = prop.GetValueConverter();
        if (converter is null) return value;

        return converter.ConvertToProvider(value);
    }
}