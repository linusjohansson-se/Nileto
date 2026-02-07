using Microsoft.Extensions.DependencyInjection;

namespace Application.Abstractions.Messaging;

public class Sender : ISender
{
    private readonly IServiceProvider _serviceProvider;

    public Sender(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default)
    {
        // Get the concrete command type at runtime
        var commandType = command.GetType();

        // Build the handler interface type: ICommandHandler<TCommand, TResponse>
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResponse));

        // Resolve the handler from the DI container
        var handler = _serviceProvider.GetRequiredService(handlerType);

        // Get the Handle method from the handler
        var handleMethod = handlerType.GetMethod(nameof(ICommandHandler<ICommand<TResponse>, TResponse>.Handle));

        if (handleMethod is null)
            throw new InvalidOperationException(
                $"Handler for {commandType.Name} does not have a Handle method");

        // Invoke the Handle method and await the result
        var resultTask = (Task<TResponse>)handleMethod.Invoke(handler, [command, cancellationToken])!;

        return await resultTask;
    }
}