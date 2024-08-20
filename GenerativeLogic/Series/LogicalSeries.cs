using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerativeLogic.Operand;

namespace GenerativeLogic.Series
{
    /// <summary>
    /// A logical series based on a given parameterized series and clone function.
    /// </summary>
    public class LogicalSeries : ILogicalSeries
    {
        /// <summary>
        /// The series parameters.
        /// </summary>
        public IEnumerable<IParameter> Parameters { get { return this.series.Parameters; } }

        /// <summary>
        /// The type of the series.
        /// </summary>
        public SeriesType SeriesType { get; private set; }

        /// <summary>
        /// The name of the series.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the value of the series at the given index.
        /// </summary>
        /// <param name="index">The index at which to get the value.</param>
        /// <returns>The value at the given index.</returns>
        public IOperandStateProvider GetValue(int index)
        {
            return this.series.GetValue(index);
        }

        /// <summary>
        /// Sets the input series for the given parameter.
        /// </summary>
        /// <typeparam name="S">The value type of the series.</typeparam>
        /// <param name="parameter">The parameter that is being set.</param>
        /// <param name="input">The series that provides the parameter values.</param>
        public void SetInput<S>(IParameter parameter, ISeries<S> input)
        {
            this.series.SetInput<S>(parameter, input);
        }

        /// <summary>
        /// The provided series.
        /// </summary>
        private IParameterizedSeries<IOperandStateProvider> series;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="series">The provided series.</param>
        /// <param name="seriesType">The type of the series.</param>
        public LogicalSeries(IParameterizedSeries<IOperandStateProvider> series, SeriesType seriesType)
        {
            this.series = series;
            this.SeriesType = seriesType;
        }
    }
}
