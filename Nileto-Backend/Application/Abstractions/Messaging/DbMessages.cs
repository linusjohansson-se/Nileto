namespace Application.Abstractions.Messaging;

public sealed record InsertMessage(
    string Target,
    IReadOnlyDictionary<string, object?> Values
) : IMessage
{
    public MessageKind Kind => MessageKind.Insert;
}

public sealed record UpdateMessage(
    string Target,
    IReadOnlyDictionary<string, object?> Values,
    IReadOnlyDictionary<string, object?> Where
) : IMessage
{
    public MessageKind Kind => MessageKind.Update;
}

public sealed record SelectMessage(
    string Target,
    IReadOnlyList<string> Columns,
    IReadOnlyDictionary<string, object?>? Where = null
) : IMessage
{
    public MessageKind Kind => MessageKind.Select;
}
