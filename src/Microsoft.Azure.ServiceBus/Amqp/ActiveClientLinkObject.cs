﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.Amqp
{
    using Microsoft.Azure.Amqp;
    using System;

    abstract class ActiveClientLinkObject
    {
        readonly string[] requiredClaims;

        protected ActiveClientLinkObject(AmqpObject amqpLinkObject,  Uri endpointUri, string audience, string[] requiredClaims)
        {
            this.LinkObject = amqpLinkObject;
            this.EndpointUri = endpointUri;
            this.Audience = audience;
            this.requiredClaims = requiredClaims;
        }

        public AmqpObject LinkObject { get; }

        public string Audience { get; }

        public Uri EndpointUri { get; }

        public string[] RequiredClaims => (string[])this.requiredClaims.Clone();

        public abstract AmqpConnection Connection { get; }
    }
}
