using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Series
{
    /// <summary>
    /// Specifies which series will serve as parameters to another series.
    /// </summary>
    public class SeriesInputSpecification
    {

        /// <summary>
        /// The parameters to use as an input.
        /// </summary>
        public IEnumerable<SeriesInputParameter> Parameters { get; set; }

    }
}
