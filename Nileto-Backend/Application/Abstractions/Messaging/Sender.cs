namespace Application.Abstractions.Messaging;

public class Sender : ISender
{
    public async Task<IResponse> Send(ICommand command)
    {
        throw new NotImplementedException();
    }
}