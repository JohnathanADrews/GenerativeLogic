using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Operand
{

    /// <summary>
    /// An interface for operands.
    /// </summary>
    public interface IOperandStateProvider : IProvider<bool>, IConnector<IOperandStateProvider>
    {
    }
}
