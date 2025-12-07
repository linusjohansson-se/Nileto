namespace Application.Abstractions.Messaging;

public interface IMessage
{
    MessageKind Kind { get; }
    string Target { get; }
}