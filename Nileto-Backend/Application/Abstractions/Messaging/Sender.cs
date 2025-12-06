namespace Application.Abstractions.Messaging;

public class Sender : ISender
{
    public async Task<ICommandResponse> Send(ICommand command)
    {
        throw new NotImplementedException();
    }

    public async Task<IQueryResponse> Send(IQuery query)
    {
        throw new NotImplementedException();
    }
}