using Application.Abstractions.Messaging;

namespace Application.Abstractions;

public interface ISender
{
    public Task<ICommandResponse> Send(ICommand command);
    
    public Task<IQueryResponse> Send(IQuery query);
}