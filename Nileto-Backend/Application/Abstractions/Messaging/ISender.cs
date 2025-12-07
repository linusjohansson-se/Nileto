using Application.Abstractions.Messaging;

namespace Application.Abstractions;

public interface ISender
{
    public Task<IResponse> Send(ICommand command);
    
    public Task<IQueryResponse> Send(IQuery query);
}