using GenerativeLogic.Operand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Series
{
    /// <summary>
    /// A series parameter for a finite series.
    /// </summary>
    public class FiniteSeriesInputParameter
    {
        /// <summary>
        /// The parameter that the series acts as.
        /// </summary>
        public IParameter Parameter { get; set; }

        /// <summary>
        /// The finite series that is the input.
        /// </summary>
        public IFiniteSeries<IOperandStateProvider> Series { get; set; }
    }
}
