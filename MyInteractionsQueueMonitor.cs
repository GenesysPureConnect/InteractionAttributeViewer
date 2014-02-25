using ININ.InteractionClient.AddIn;
using System;
using System.Collections.Generic;

namespace BrowserAddIn
{
    public class MyInteractionsQueueMonitor : QueueMonitor
    {
        const string UrlAttribute = "Url";
        const string UrlNotSet = "Url Attribute is not set";

        private ITraceContext _traceContext;

        /// <summary>
        /// Let the interaction client know what attributes we want to retrieve the value of
        /// </summary>
        protected override IEnumerable<string> Attributes
        {
            get
            {
                return new string[] { UrlAttribute,
                                        InteractionAttributes.RemoteName,
                                        InteractionAttributes.RemoteId
                };
            }
        }
        
        protected override void InteractionAdded(IInteraction interaction)
        {
            base.InteractionAdded(interaction);
            _traceContext.Note("interaction added: " + interaction.InteractionId);

            var url = interaction.GetAttribute(UrlAttribute);

            var browser = BrowserControl.Instance;

            if (browser != null)
            {
                if (!String.IsNullOrEmpty(url))
                {
                    _traceContext.Note("Navigating to: " + url);
                    browser.NavigateToUrl(url);
                }
            }    
        }

        protected override void OnLoad(IServiceProvider serviceProvider)
        {
            base.OnLoad(serviceProvider);
            _traceContext = (ITraceContext)serviceProvider.GetService(typeof(ITraceContext));
            _traceContext.Note("My interaction queue monitor loaded...");

            base.StartMonitoring();
        }
    }
}
