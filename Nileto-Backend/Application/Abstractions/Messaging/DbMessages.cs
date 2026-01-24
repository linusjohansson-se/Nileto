using Microsoft.EntityFrameworkCore.Metadata;

namespace Application.Abstractions.Messaging;

public sealed record InsertMessage(
    string Target,
    IReadOnlyDictionary<IProperty, object?> Values
) : IMessage
{
    public MessageKind Kind => MessageKind.Insert;
}

public sealed record UpdateMessage(
    string Target,
    IReadOnlyDictionary<IProperty, object?> Values,
    IReadOnlyDictionary<IProperty, object?> Where
) : IMessage
{
    public MessageKind Kind => MessageKind.Update;
}

public sealed record SelectMessage(
    string Target,
    IReadOnlyList<IProperty> Columns,
    IReadOnlyDictionary<IProperty, object?>? Where = null
) : IMessage
{
    public MessageKind Kind => MessageKind.Select;
}
