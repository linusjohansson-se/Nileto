using Microsoft.EntityFrameworkCore;

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
        IDictionary<string, object?> fields)
        where TEntity : class
    {
        var table = GetTable<TEntity>();
        var values = BuildValues(entity, table, fields);
        
        return new InsertMessage(table, values);
    }
    
    public UpdateMessage Update<TEntity>(
        TEntity entity,
        IDictionary<string, object?> fields,
        IDictionary<string, object?> where)
        where TEntity : class
    {
        var table = GetTable<TEntity>();
        var values = BuildValues(entity, table, fields);

        return new UpdateMessage(table, values, where);
    }

    public SelectMessage Select<TEntity>(
        IDictionary<string, object?> filters,
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

}