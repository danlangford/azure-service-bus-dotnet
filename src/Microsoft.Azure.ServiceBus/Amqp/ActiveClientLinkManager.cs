// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus.Amqp
{
    using System;

    sealed class ActiveClientLinkManager
    {

        readonly string clientId;

        ActiveSendReceiveClientLink activeSendReceiveClientLink;
        ActiveRequestResponseLink activeRequestResponseClientLink;

        public ActiveClientLinkManager(string clientId)
        {
            this.clientId = clientId;
        }

        public void Close()
        {
        }

        public void SetActiveSendReceiveLink(ActiveSendReceiveClientLink sendReceiveClientLink)
        {
            this.activeSendReceiveClientLink = sendReceiveClientLink;
            this.activeSendReceiveClientLink.Link.Closed += this.OnSendReceiveLinkClosed;
        }

        void OnSendReceiveLinkClosed(object sender, EventArgs e)
        {
        }

        public void SetActiveRequestResponseLink(ActiveRequestResponseLink requestResponseLink)
        {
            this.activeRequestResponseClientLink = requestResponseLink;
            this.activeRequestResponseClientLink.Link.Closed += this.OnRequestResponseLinkClosed;
        }

        void OnRequestResponseLinkClosed(object sender, EventArgs e)
        {
        }

    }
}
