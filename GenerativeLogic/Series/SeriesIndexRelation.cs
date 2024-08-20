using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Series
{
    /// <summary>
    /// Relates a series parameter to index that will be used in the expression.
    /// </summary>
    /// <example>
    /// If the series r is defined as r[i] = a[i] XOR r[i-1], then the index relating r to itself if -1.
    /// </example>
    public class SeriesIndexRelation
    {
        /// <summary>
        /// The type of relation that the series has.
        /// </summary>
        public enum RelationType
        {
            /// <summary>
            /// The series is a parameter.
            /// </summary>
            Parameter = 1,
            /// <summary>
            /// The seris is internal to the system.
            /// </summary>
            Internal = 2
        }

        /// <summary>
        /// The series is a parameter.
        /// </summary>
        public RelationType Type { get; set; }

        /// <summary>
        /// The parameter that the series acts as.
        /// </summary>
        public IParameter Parameter { get; set; }

        /// <summary>
        /// The index that relates the parameter to the series.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The key of the template to use for series within the system.
        /// </summary>
        public int TemplateKey { get; set; }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>A string representation of the index relation.</returns>
        public override string ToString()
        {
            return String.Format("({0}:{1})", this.Parameter == null ? null : this.Parameter.Name, Index);
        }
    }
}
