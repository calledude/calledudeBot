﻿using MediatR;
using System;
using System.Threading.Tasks;

namespace calledudeBot.Services
{
    public class MessageDispatcher
    {
        private readonly IMediator _mediator;

        public MessageDispatcher(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<TResponse> SendRequest<TResponse>(IRequest<TResponse> request)
        {
            Logger.Log($"Beginning to publish a {request.GetType().Name} request" +
                $". Expecting a {typeof(TResponse).Name} response.");

            TResponse response = default;
            try
            {
                response = await _mediator.Send(request);

                Logger.Log($"Finished invoking {request.GetType().Name} handlers");
            }
            catch (Exception ex)
            {
                Logger.Log($"An exception was thrown in the MediatR adapter: {ex}");
            }

            return response;
        }

        public Task PublishAsync(INotification notification)
        {
            _ = Task.Run(async () =>
            {
                Logger.Log($"Beginning to publish a {notification.GetType().Name} message");

                try
                {
                    await _mediator.Publish(notification);
                }
                catch (Exception ex)
                {
                    Logger.Log($"An exception was thrown in the MediatR adapter: {ex}");
                }

                Logger.Log($"Finished invoking {notification.GetType().Name} handlers");
            });

            return Task.CompletedTask;
        }
    }
}
