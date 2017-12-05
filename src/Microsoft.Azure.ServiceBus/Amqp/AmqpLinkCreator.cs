// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.Amqp
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Amqp;
    using Microsoft.Azure.Amqp.Framing;
    using Microsoft.Azure.ServiceBus.Primitives;

    internal abstract class AmqpLinkCreator
    {
        readonly string entityPath;
        readonly ServiceBusConnection serviceBusConnection;
        readonly Uri endpointAddress;
        readonly string[] requiredClaims;
        readonly AmqpLinkSettings amqpLinkSettings;

        protected AmqpLinkCreator(string entityPath, ServiceBusConnection serviceBusConnection, Uri endpointAddress, string[] requiredClaims, AmqpLinkSettings amqpLinkSettings, string clientId)
        {
            this.entityPath = entityPath;
            this.serviceBusConnection = serviceBusConnection;
            this.endpointAddress = endpointAddress;
            this.requiredClaims = requiredClaims;
            this.amqpLinkSettings = amqpLinkSettings;
            this.ClientId = clientId;
        }

        protected string ClientId { get; }

        public async Task<AmqpObject> CreateAndOpenAmqpLinkAsync()
        {
            var timeoutHelper = new TimeoutHelper(this.serviceBusConnection.OperationTimeout);

            MessagingEventSource.Log.AmqpGetOrCreateConnectionStart();
            var amqpConnection = await this.serviceBusConnection.ConnectionManager.GetOrCreateAsync(timeoutHelper.RemainingTime()).ConfigureAwait(false);
            MessagingEventSource.Log.AmqpGetOrCreateConnectionStop(this.entityPath, amqpConnection.ToString(), amqpConnection.State.ToString());

            var resource = this.endpointAddress.AbsoluteUri;
            MessagingEventSource.Log.AmqpSendAuthenticationTokenStart(this.endpointAddress, resource, resource, this.requiredClaims);
            MessagingEventSource.Log.AmqpSendAuthenticationTokenStop();

            AmqpSession session = null;
            try
            {
                // Create Session
                var amqpSessionSettings = new AmqpSessionSettings { Properties = new Fields() };
                session = amqpConnection.CreateSession(amqpSessionSettings);
                await session.OpenAsync(timeoutHelper.RemainingTime()).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                MessagingEventSource.Log.AmqpSessionCreationException(this.entityPath, amqpConnection, exception);
                session?.Abort();
                throw AmqpExceptionHelper.GetClientException(exception, null, session.GetInnerException());
            }

            AmqpObject link = null;
            try
            {
                // Create Link
                link = this.OnCreateAmqpLink(amqpConnection, this.amqpLinkSettings, session);
                await link.OpenAsync(timeoutHelper.RemainingTime()).ConfigureAwait(false);
                return link;
            }
            catch (Exception exception)
            {
                MessagingEventSource.Log.AmqpLinkCreationException(
                    this.entityPath,
                    session,
                    amqpConnection,
                    exception);

                session.SafeClose(exception);
                throw AmqpExceptionHelper.GetClientException(exception, null, link?.GetInnerException(), session.IsClosing());
            }
        }

        protected abstract AmqpObject OnCreateAmqpLink(AmqpConnection connection, AmqpLinkSettings linkSettings, AmqpSession amqpSession);
    }
}