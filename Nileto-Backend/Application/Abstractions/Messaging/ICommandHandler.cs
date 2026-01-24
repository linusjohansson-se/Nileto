namespace Application.Abstractions.Messaging;

public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task<IResponse> Handle(TCommand command);
}