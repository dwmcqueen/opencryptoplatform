using System;
using System.Collections.Generic;
using System.Text;
using Arbiter;
using CommonSupport;

namespace ForexPlatform
{
    /// <summary>
    /// Helper delegate.
    /// </summary>
    public delegate void IntegrationAdapterUpdateDelegate(IIntegrationAdapter adapter);

    /// <summary>
    /// Base interface for integration adapters.
    /// </summary>
    public interface IIntegrationAdapter : IArbiterClient, IOperational, IDisposable
    {
        bool IsStarted { get; }

        /// <summary>
        /// 
        /// </summary>
        event IntegrationAdapterUpdateDelegate PersistenceDataUpdateEvent;

        /// <summary>
        /// 
        /// </summary>
        bool Start(out string operationResultMessage);

        /// <summary>
        /// 
        /// </summary>
        bool Stop(out string operationResultMessage);
    }
}
