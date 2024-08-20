using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Operand
{
    /// <summary>
    /// An operand state provider from various constructor-injected providers.
    /// </summary>
    public class OperandStateProvider : IOperandStateProvider
    {
        /// <summary>
        /// Provides the value.
        /// </summary>
        /// <returns>The value</returns>
        public bool Provide()
        {
            return this.provider.Provide();
        }

        /// <summary>
        /// Provides the connections to this provider.
        /// </summary>
        public IEnumerable<IOperandStateProvider> Connections { get { return this.connectionsProvider.Provide().Connections; }}

        /// <summary>
        /// The provider that will provide the value.
        /// </summary>
        private IProvider<bool> provider;
        
        /// <summary>
        /// Provides the connections to this provider.
        /// </summary>
        private IProvider<IConnector<IOperandStateProvider>> connectionsProvider;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">The provider that will provide the value.</param>
        /// <param name="connectionsProvider">Provides the opernad connections.</param>
        public OperandStateProvider(IProvider<bool> provider, IProvider<IConnector<IOperandStateProvider>> connectionsProvider)
        {
            this.provider = provider;
            this.connectionsProvider = connectionsProvider;
        }

    }
}
