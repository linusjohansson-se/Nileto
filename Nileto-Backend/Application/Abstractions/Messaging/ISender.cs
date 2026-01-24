namespace Application.Abstractions.Messaging;

public interface ISender
{
    Task<TResponse> Send<TResponse>(ICommand<TResponse> command);
}