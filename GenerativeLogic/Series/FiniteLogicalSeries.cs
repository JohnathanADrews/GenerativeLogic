using GenerativeLogic.Operand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Series
{
    /// <summary>
    /// A finite logical series based on a given parameterized series and clone function.
    /// </summary>
    public class FiniteLogicalSeries : IFiniteLogicalSeries
    {
        /// <summary>
        /// The type of this series.
        /// </summary>
        public SeriesType SeriesType { get; set; }

        /// <summary>
        /// The name of the series.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The series parameters.
        /// </summary>
        public IEnumerable<IParameter> Parameters { get { return this.series.Parameters; } }

        /// <summary>
        /// The number of indices in the series.
        /// </summary>
        public int Length { get { return this.series.Length; } }

        /// <summary>
        /// The indices of the series.
        /// </summary>
        public IEnumerable<int> Indices { get { return this.series.Indices; } }

        /// <summary>
        /// The values of the series.
        /// </summary>
        public IDictionary<int, IOperandStateProvider> Values {  get { return this.series.Values; } }

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
        public void SetInput<S>(IParameter parameter, IFiniteSeries<S> input)
        {
            this.series.SetInput<S>(parameter, input);
        }

        /// <summary>
        /// The provided series.
        /// </summary>
        private IParameterizedFiniteSeries<IOperandStateProvider> series;
       

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="series">The provided series.</param>
        public FiniteLogicalSeries(IParameterizedFiniteSeries<IOperandStateProvider> series)
        {
            this.series = series;
        }
    }
}
