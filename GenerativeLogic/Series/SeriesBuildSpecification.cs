using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Series
{
    /// <summary>
    /// Specifies how to build a series.
    /// </summary>
    public class SeriesBuildSpecification
    {
        /// <summary>
        /// The templates for creating the component series'.
        /// </summary>
        public IEnumerable<SeriesTemplate> Templates { get; set; }

        /// <summary>
        /// The key of the output series.
        /// </summary>
        public int OutputKey { get; set; }

    }
}
