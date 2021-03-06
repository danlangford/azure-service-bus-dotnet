﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.Amqp
{
    using System;
    using Microsoft.Azure.Amqp;
    using Microsoft.Azure.Amqp.Sasl;
    using Microsoft.Azure.Amqp.Transport;
    using Microsoft.Azure.ServiceBus.Primitives;

    internal class AmqpConnectionHelper
    {
        public static AmqpSettings CreateAmqpSettings(
            Version amqpVersion,
            bool useSslStreamSecurity,
            string sslHostName = null,
            bool useWebSockets = false,
            bool sslStreamUpgrade = false,
            System.Net.NetworkCredential networkCredential = null)
        {
            var amqpSettings = new AmqpSettings();
            if (useSslStreamSecurity && !useWebSockets && sslStreamUpgrade)
            {
                var tlsSettings = new TlsTransportSettings
                {
                    TargetHost = sslHostName
                };

                var tlsProvider = new TlsTransportProvider(tlsSettings);
                tlsProvider.Versions.Add(new AmqpVersion(amqpVersion));
                amqpSettings.TransportProviders.Add(tlsProvider);
            }

            if (networkCredential != null)
            {
                var saslTransportProvider = new SaslTransportProvider();
                saslTransportProvider.Versions.Add(new AmqpVersion(amqpVersion));
                amqpSettings.TransportProviders.Add(saslTransportProvider);

                if (networkCredential != null)
                {
                    var plainHandler = new SaslPlainHandler
                    {
                        AuthenticationIdentity = networkCredential.UserName,
                        Password = networkCredential.Password
                    };
                    saslTransportProvider.AddHandler(plainHandler);
                }
            }

            var amqpTransportProvider = new AmqpTransportProvider();
            amqpTransportProvider.Versions.Add(new AmqpVersion(amqpVersion));
            amqpSettings.TransportProviders.Add(amqpTransportProvider);

            return amqpSettings;
        }

        public static TransportSettings CreateTcpTransportSettings(
            string networkHost,
            string hostName,
            int port,
            bool useSslStreamSecurity,
            bool sslStreamUpgrade = false,
            string sslHostName = null)
        {
            var tcpTransportSettings = new TcpTransportSettings
            {
                Host = networkHost,
                Port = port < 0 ? AmqpConstants.DefaultSecurePort : port,
                ReceiveBufferSize = AmqpConstants.TransportBufferSize,
                SendBufferSize = AmqpConstants.TransportBufferSize
            };

            TransportSettings tpSettings = tcpTransportSettings;
            if (useSslStreamSecurity && !sslStreamUpgrade)
            {
                var tlsTransportSettings = new TlsTransportSettings(tcpTransportSettings)
                {
                    TargetHost = sslHostName ?? hostName
                };
                tpSettings = tlsTransportSettings;
            }

            return tpSettings;
        }

        public static TransportSettings CreateWebSocketTransportSettings(
            string networkHost,
            string hostName,
            int port)
        {
            var uriBuilder = new UriBuilder(WebSocketConstants.WebSocketSecureScheme, networkHost, port < 0 ? WebSocketConstants.WebSocketSecurePort : port, WebSocketConstants.WebSocketDefaultPath);
            var webSocketTransportSettings = new WebSocketTransportSettings
            {
                Uri = uriBuilder.Uri,
                ReceiveBufferSize = AmqpConstants.TransportBufferSize,
                SendBufferSize = AmqpConstants.TransportBufferSize
            };

            TransportSettings tpSettings = webSocketTransportSettings;
            return tpSettings;
        }

        public static AmqpConnectionSettings CreateAmqpConnectionSettings(uint maxFrameSize, string containerId, string hostName)
        {
            var connectionSettings = new AmqpConnectionSettings
            {
                MaxFrameSize = maxFrameSize,
                ContainerId = containerId,
                HostName = hostName
            };

            connectionSettings.AddProperty("product", ClientInfo.Product);
            connectionSettings.AddProperty("version", ClientInfo.Version);
            connectionSettings.AddProperty("framework", ClientInfo.Framework);
            connectionSettings.AddProperty("platform", ClientInfo.Platform);
            return connectionSettings;
        }
    }
}