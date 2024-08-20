using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Series
{
    public class SeriesType
    {

        /// <summary>
        /// A unique identifier for the series.
        /// </summary>
        public Guid SeriesKey { get; set; }

        /// <summary>
        /// The name of the series.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description of the series.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Any additional information about the series.  This may or may not be encoded.
        /// </summary>
        public string Meta { get; set; }

        /// <summary>
        /// override equals using the series key.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>true iff the key match.</returns>
        public override bool Equals(object obj)
        {
            if(obj.GetType() != typeof(SeriesType))
            {
                return false;
            }
            return this.SeriesKey == ((SeriesType)obj).SeriesKey;
        }
    }
}
