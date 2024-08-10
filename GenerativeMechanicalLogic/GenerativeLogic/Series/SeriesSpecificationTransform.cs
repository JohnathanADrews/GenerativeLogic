using GenerativeLogic.Operand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Series
{
    /// <summary>
    /// Transforms a FiniteSeriesInputSpecification to a SeriesInputSpecification in order to make aspects of series creation reusable.
    /// </summary>
    public class SeriesSpecificationTransform : ITransform<FiniteSeriesInputSpecification, SeriesInputSpecification>
    {

        /// <summary>
        /// Transforms the finite series specification to series specification.
        /// </summary>
        /// <param name="input">The finite series specification.</param>
        /// <returns>The corresponding series specification.</returns>
        public SeriesInputSpecification Transform(FiniteSeriesInputSpecification input)
        {
            var seriesTransform = new FiniteSeriesToSeries<IOperandStateProvider>();
            var result = new SeriesInputSpecification();
            var parameters = new List<SeriesInputParameter>();
            result.Parameters = parameters;
            foreach (var finiteParameter in input.Parameters)
            {
                var parameter = new SeriesInputParameter();
                parameters.Add(parameter);
                parameter.Parameter = finiteParameter.Parameter;
                parameter.Series = seriesTransform.Transform(finiteParameter.Series);
            }
            return result;
        }
    }
}
