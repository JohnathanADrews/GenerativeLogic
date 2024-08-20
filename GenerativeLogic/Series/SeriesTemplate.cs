using GenerativeLogic.Foundation;
using GenerativeLogic.Operand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Series
{
    /// <summary>
    /// A template for constructing a series.
    /// </summary>
    public class SeriesTemplate
    {
        /// <summary>
        /// The name of the series.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A key unique to the series system that is used to identify this series.
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// The expression that governs the series.
        /// </summary>
        public LogicalExpression Expression { get; set; }

        /// <summary>
        /// Specific values that are always used (e.g. boundary values).  
        /// </summary>
        public IDictionary<int, IOperandStateProvider> Constants { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public SeriesTemplate()
        {
            this.Constants = new Dictionary<int, IOperandStateProvider>();
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>A string projection.</returns>
        public override string ToString()
        {
            return String.Format("{0} - {1} - {2}",this.Name, this.Key, this.Expression);
        }

    }
}
