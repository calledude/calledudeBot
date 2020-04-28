using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace calledudeBot.Services
{
    public class MessageDispatcher
    {
        private readonly ILogger<MessageDispatcher> _logger;
        private readonly IMediator _mediator;

        public MessageDispatcher(ILogger<MessageDispatcher> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<TResponse> SendRequest<TResponse>(IRequest<TResponse> request)
        {
            var requestType = request.GetType().Name;

            _logger.LogInformation("Beginning to publish a {0} request. Expecting a {1} response.", requestType, typeof(TResponse).Name);

            TResponse response = default;
            try
            {
                response = await _mediator.Send(request);

                _logger.LogInformation("Finished invoking {0} handlers", requestType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception was thrown in the MediatR adapter");
            }

            return response;
        }

        public Task PublishAsync(INotification notification)
        {
            _ = Task.Run(async () =>
            {
                var notificationType = notification.GetType().Name;
                _logger.LogInformation("Beginning to publish a {0} message", notificationType);

                try
                {
                    await _mediator.Publish(notification);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An exception was thrown in the MediatR adapter");
                }

                _logger.LogInformation("Finished invoking {0} handlers", notificationType);
            });

            return Task.CompletedTask;
        }
    }
}
