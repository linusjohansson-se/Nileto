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
        var values = BuildValues(entity, fields);
        
        return new InsertMessage(table, values);
    }
    
    public UpdateMessage Update<TEntity>(
        TEntity entity,
        IReadOnlyDictionary<string, object?> fields,
        IReadOnlyDictionary<string, object?> where)
        where TEntity : class
    {
        var table = GetTable<TEntity>();
        var values = BuildValues(entity, fields);

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

    private IReadOnlyDictionary<IProperty, object?> BuildValues<TEntity>(
        IReadOnlyDictionary<string, object?> fields)
        where TEntity : class
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(TEntity))
                         ?? throw new InvalidOperationException(
                             $"Type {typeof(TEntity).Name} is not mapped in EF");

        var byPropName = entityType.GetProperties()
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        var result = new Dictionary<IProperty, object?>();

        foreach (var (key, providedValue) in fields)
        {
            if (!byPropName.TryGetValue(key, out var prop))
                throw new InvalidOperationException(
                    $"Field '{key}' is not a mapped property for '{entityType.ClrType.Name}'.");

            result[prop] = ConvertToProviderValue(prop, providedValue);
        }

        return result;
    }

    private static object? ConvertToProviderValue(IProperty prop, object? value)
    {
        if (value is null) return null;

        var converter = prop.GetTypeMapping().Converter;
        if (converter is null) return value;

        return converter.ConvertToProvider(value);
    }
}