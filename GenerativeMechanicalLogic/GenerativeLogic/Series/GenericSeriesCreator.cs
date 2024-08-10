using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Series
{
    /// <summary>
    /// Handles creating most of the series creator methods.
    /// </summary>
    public abstract class GenericSeriesCreator
    {

        /// <summary>
        /// Used to create the various series given the governing expressions.
        /// </summary>
        /// <remarks>
        /// This encapsulates all of the logic for creating the series so we do not have to re-create it.
        /// </remarks>
        protected DynamicSeries dynamicSeries = new DynamicSeries();
        
        /// <summary>
        /// Creates an ILogicalSeries.
        /// </summary>
        /// <returns>An ILogicalSeries.</returns>
        public ILogicalSeries CreateSeries()
        {
            return this.dynamicSeries.CreateSeries();
        }

        /// <summary>
        /// Creates a finite logical series.
        /// </summary>
        /// <param name="indices">The indices in the finite series.</param>
        /// <returns>A finite logical series.</returns>
        public IFiniteLogicalSeries CreateSeries(IEnumerable<int> indices)
        {
            return this.dynamicSeries.CreateSeries(indices);
        }

        /// <summary>
        /// Creates a finite logical series.
        /// </summary>
        /// <param name="range">The range of the finite series.</param>
        /// <returns>A finite logical series.</returns>
        public IFiniteLogicalSeries CreateSeries(IRange<int> range)
        {
            return this.dynamicSeries.CreateSeries(range);
        }

        /// <summary>
        /// Gets a creator for a logical series.
        /// </summary>
        /// <returns>A creator for a logical series.</returns>
        public ICreator<ILogicalSeries> GetCreator()
        {
            return this.dynamicSeries.GetCreator();
        }

        /// <summary>
        /// Gets a creator for a finite logical series.
        /// </summary>
        /// <param name="indices">The series indices.</param>
        /// <returns>A creator for a finite logical series.</returns>
        public ICreator<IFiniteLogicalSeries> GetCreator(IEnumerable<int> indices)
        {
            return this.dynamicSeries.GetCreator(indices);
        }

        /// <summary>
        /// Gets a creator for a finite logical series.
        /// </summary>
        /// <param name="range">The series index range.</param>
        /// <returns>A creator for a finite logical series.</returns>
        public ICreator<IFiniteLogicalSeries> GetCreator(IRange<int> range)
        {
            return this.dynamicSeries.GetCreator(range);
        }

        /// <summary>
        /// Sets up the expressions for the dynamic series.
        /// </summary>
        protected abstract void SetupDynamicSeries();
    }
}
