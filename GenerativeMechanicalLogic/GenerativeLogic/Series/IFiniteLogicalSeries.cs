﻿using GenerativeLogic.Operand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Series
{
    /// <summary>
    /// Represents a finite series that can be cloned and connected to other series.
    /// </summary>
    public interface IFiniteLogicalSeries : IParameterizedFiniteSeries<IOperandStateProvider>
    {
        /// <summary>
        /// The type of this series.
        /// </summary>
        SeriesType SeriesType { get; }

        /// <summary>
        /// The name of the series.
        /// </summary>
        string Name { get; set; }
    }
}
