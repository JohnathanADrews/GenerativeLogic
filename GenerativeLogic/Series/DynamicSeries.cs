using GenerativeLogic.Foundation;
using GenerativeLogic.Helper;
using GenerativeLogic.Operand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Series
{

    /// <summary>
    /// Creates outpus series' from a set of instructions give at runtime.
    /// </summary>
    public class DynamicSeries
    {
                
        /// <summary>
        /// Specifies how to build the series system from expressions and parameters.
        /// </summary>
        public SeriesBuildSpecification BuildSpecification { get; set; }

        /// <summary>
        /// The parameters that are used as inputs for the series creation.
        /// </summary>
        public IEnumerable<IParameter> InputParameters { get; set; }

        /// <summary>
        /// The type of the series.
        /// </summary>
        public SeriesType SeriesType { get; set; }

        /// <summary>
        /// The optional wrapper for the individual operands.
        /// </summary>
        public IHomogeneousWrapper<IOperandStateProvider> OperandWrapper { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DynamicSeries()
        {
        }

        /// <summary>
        /// Creates the series given the input specification.
        /// </summary>
        /// <param name="inputSpecification">Specifies how the input series will be used.</param>
        /// <returns></returns>
        public ISeries<IOperandStateProvider> CreateSeries(SeriesInputSpecification inputSpecification)
        {
            var baseSeries = new Dictionary<int, ISeries<IOperandStateProvider>>();
            var cachedSeries = new Dictionary<int, ISeries<IOperandStateProvider>>();
            
            foreach (var template in this.BuildSpecification.Templates)
            {
                var series = new SeriesFunction<IOperandStateProvider>(index =>
                {
                    //Check for a constant.  If there is one, return it.
                    if(template.Constants.ContainsKey(index))
                    {
                        return template.Constants[index];
                    }

                    //Copy the expression.
                    var expression = new LogicalExpression(template.Expression);
                    LogicalExpression.ExpressionNode node;
                    var relationNodes = expression.GetNodes<SeriesIndexRelation>();
                    SeriesIndexRelation relationIndex;
                    SeriesInputParameter inputParameter;

                    foreach (var relationNode in relationNodes)
                    {
                        node = new LogicalExpression.ExpressionNode(relationNode);
                        relationIndex = (SeriesIndexRelation)node.Value;
                        
                        //If inputParameter is null, then the node is not an input parameter.  It should be a node for another series in the system.
                        if (relationIndex.Type == SeriesIndexRelation.RelationType.Internal)
                        {
                            node.Value = cachedSeries[relationIndex.TemplateKey].GetValue(index + relationIndex.Index);
                            expression.SetNode(node);
                        }
                        if (relationIndex.Type == SeriesIndexRelation.RelationType.Parameter)
                        {
                            inputParameter = inputSpecification.Parameters.Where(x => x.Parameter.Index == relationIndex.Parameter.Index).FirstOrDefault();
                            node.Value = inputParameter.Series.GetValue(index + relationIndex.Index);
                            expression.SetNode(node);
                        }
                    }
                    return expression;
                });

                baseSeries[template.Key] = series;
                cachedSeries.Add(template.Key, new CachedSeries<IOperandStateProvider>(series));
            }
            return cachedSeries[this.BuildSpecification.OutputKey];
        }

        /// <summary>
        /// Creates a finite series given the specified input and range.
        /// </summary>
        /// <param name="inputSpecification">Specifies how the input series will be used.</param>
        /// <param name="range">The finite range of values.</param>
        /// <returns>A finite series given the specified input and range.</returns>
        public IFiniteSeries<IOperandStateProvider> CreateSeries(FiniteSeriesInputSpecification inputSpecification, IRange<int> range)
        {
            var indices = new List<int>();
            for(int i=range.Low;i<=range.High;i++)
            {
                indices.Add(i);
            }
            return CreateSeries(inputSpecification, indices);
        }

        /// <summary>
        /// Creates a finite series given the specified input and range.
        /// </summary>
        /// <param name="inputSpecification">Specifies how the input series will be used.</param>
        /// <param name="indices">The indices in the finite series.</param>
        /// <returns>A finite series given the specified input and range.</returns>
        public IFiniteSeries<IOperandStateProvider> CreateSeries(FiniteSeriesInputSpecification inputSpecification, IEnumerable<int> indices)
        {
            var transform = new FiniteSeriesToSeries<IOperandStateProvider>();
            var specificationTransform = new SeriesSpecificationTransform();
            var seriesSpecification = specificationTransform.Transform(inputSpecification);
            var series = CreateSeries(seriesSpecification);
            var result = new FiniteSeries<IOperandStateProvider>(series, indices);
            return result;
        }

        /// <summary>
        /// Creates a logical series.
        /// </summary>
        /// <returns>A logical series based on the build specification.</returns>
        public ILogicalSeries CreateSeries()
        {
            var baseSeries = new Dictionary<int, ISeries<IOperandStateProvider>>();
            var cachedSeries = new Dictionary<int, ISeries<IOperandStateProvider>>();

            var inputSeries = new Dictionary<int, ISeries<IOperandStateProvider>>();
            var inputSeriesFuncs = new Dictionary<int, ISeries<IOperandStateProvider>>();

            foreach (var template in this.BuildSpecification.Templates)
            {
                var series = new SeriesFunction<IOperandStateProvider>(index =>
                {
                    //Check for a constant.  If there is one, return it.
                    if (template.Constants.ContainsKey(index))
                    {
                        return template.Constants[index];
                    }

                    //Copy the expression.
                    var expression = new LogicalExpression(template.Expression);
                    LogicalExpression.ExpressionNode node;
                    var relationNodes = expression.GetNodes<SeriesIndexRelation>();
                    SeriesIndexRelation relationIndex;

                    foreach (var relationNode in relationNodes)
                    {
                        node = new LogicalExpression.ExpressionNode(relationNode);
                        relationIndex = (SeriesIndexRelation)node.Value;

                        //If inputParameter is null, then the node is not an input parameter.  It should be a node for another series in the system.
                        if (relationIndex.Type == SeriesIndexRelation.RelationType.Internal)
                        {
                            node.Value = cachedSeries[relationIndex.TemplateKey].GetValue(index + relationIndex.Index);
                            expression.SetNode(node);
                        }
                        if (relationIndex.Type == SeriesIndexRelation.RelationType.Parameter)
                        {
                            node.Value = inputSeriesFuncs[relationIndex.Parameter.Index].GetValue(index + relationIndex.Index);
                            expression.SetNode(node);
                        }
                    }
                    return expression;
                });

                baseSeries[template.Key] = series;
                cachedSeries.Add(template.Key, new CachedSeries<IOperandStateProvider>(series));
            }

            var updater = new Action<IParameter, object>((info, input) =>
            {
                inputSeries[info.Index] = (ISeries<IOperandStateProvider>)input;
                if (!inputSeriesFuncs.ContainsKey(info.Index))
                {
                    inputSeriesFuncs.Add(info.Index, new SeriesFunction<IOperandStateProvider>(index =>
                    {
                        var provider = new OperandStateProvider(new ProviderFunction<bool>(() =>
                        {
                            return inputSeries[info.Index].GetValue(index).Provide();
                        }), new ProviderFunction<IConnector<IOperandStateProvider>>(() =>
                        {
                            return inputSeries[info.Index].GetValue(index);
                        }));
                        return provider;
                    }));
                }
            });
            var result = new LogicalSeries(new ParameterizedSeries<IOperandStateProvider>(cachedSeries[this.BuildSpecification.OutputKey], this.InputParameters, updater), this.SeriesType);
            return result;
        }

        /// <summary>
        /// Creates a finite logical series.
        /// </summary>
        /// <param name="range">The finite range.</param>
        /// <returns>A finite logical series based on the build specification.</returns>
        /// <returns>A finite logical series based on the build specification.</returns>
        public IFiniteLogicalSeries CreateSeries(IRange<int> range)
        {
            var indices = Conversion.RangeTransform.Transform(range);
            return CreateSeries(indices);
        }

        /// <summary>
        /// Creates a finite logical series.
        /// </summary>
        /// <returns>A finite logical series based on the build specification.</returns>
        public IFiniteLogicalSeries CreateSeries(IEnumerable<int> indices)
        {
            var baseSeries = new Dictionary<int, IFiniteSeries<IOperandStateProvider>>();
            var cachedSeries = new Dictionary<int, IFiniteSeries<IOperandStateProvider>>();

            var inputSeries = new Dictionary<int, IFiniteSeries<IOperandStateProvider>>();
            var inputSeriesFuncs = new Dictionary<int, IFiniteSeries<IOperandStateProvider>>();

            foreach (var template in this.BuildSpecification.Templates)
            {
                var series = new FiniteSeriesFunction<IOperandStateProvider>(index =>
                {
                    //Check for a constant.  If there is one, return it.
                    if (template.Constants.ContainsKey(index))
                    {
                        return template.Constants[index];
                    }

                    //Copy the expression.
                    var expression = new LogicalExpression(template.Expression);
                    LogicalExpression.ExpressionNode node;
                    var relationNodes = expression.GetNodes<SeriesIndexRelation>();
                    SeriesIndexRelation relationIndex;

                    foreach (var relationNode in relationNodes)
                    {
                        node = new LogicalExpression.ExpressionNode(relationNode);
                        relationIndex = (SeriesIndexRelation)node.Value;

                        //If inputParameter is null, then the node is not an input parameter.  It should be a node for another series in the system.
                        if (relationIndex.Type == SeriesIndexRelation.RelationType.Internal)
                        {
                            node.Value = cachedSeries[relationIndex.TemplateKey].GetValue(index + relationIndex.Index);
                            expression.SetNode(node);
                        }
                        if (relationIndex.Type == SeriesIndexRelation.RelationType.Parameter)
                        {
                            node.Value = inputSeriesFuncs[relationIndex.Parameter.Index].GetValue(index + relationIndex.Index);
                            expression.SetNode(node);
                        }
                    }
                    return expression;
                }, indices);

                baseSeries[template.Key] = series;
                cachedSeries.Add(template.Key, new CachedFiniteSeries<IOperandStateProvider>(series));
            }

            var updater = new Action<IParameter, object>((info, input) =>
            {
                inputSeries[info.Index] = (IFiniteSeries<IOperandStateProvider>)input;
                if (!inputSeriesFuncs.ContainsKey(info.Index))
                {
                    inputSeriesFuncs.Add(info.Index, new FiniteSeriesFunction<IOperandStateProvider>(index =>
                    {
                        var provider = new OperandStateProvider(new ProviderFunction<bool>(() =>
                        {
                            return inputSeries[info.Index].GetValue(index).Provide();
                        }), new ProviderFunction<IConnector<IOperandStateProvider>>(() =>
                        {
                            return inputSeries[info.Index].GetValue(index);
                        }));
                        return provider;
                    }, indices));
                }
            });
            var result = new FiniteLogicalSeries(new ParameterizedFiniteSeries<IOperandStateProvider>(cachedSeries[this.BuildSpecification.OutputKey], this.InputParameters, updater));
            result.SeriesType = this.SeriesType;
            return result;
        }

        /// <summary>
        /// Gets a creator for a logical series.
        /// </summary>
        /// <returns>A creator for a logical series.</returns>
        public ICreator<ILogicalSeries> GetCreator()
        {
            return new CreatorFunction<ILogicalSeries>(() =>
            {
                return CreateSeries();
            });
        }

        /// <summary>
        /// Gets a creator for a finite logical series.
        /// </summary>
        /// <param name="indices">The series indices.</param>
        /// <returns>A creator for a finite logical series.</returns>
        public ICreator<IFiniteLogicalSeries> GetCreator(IEnumerable<int> indices)
        {
            return new CreatorFunction<IFiniteLogicalSeries>(() =>
            {
                return CreateSeries(indices);
            });
        }

        /// <summary>
        /// Gets a creator for a finite logical series.
        /// </summary>
        /// <param name="range">The series index range.</param>
        /// <returns>A creator for a finite logical series.</returns>
        public ICreator<IFiniteLogicalSeries> GetCreator(IRange<int> range)
        {
            return new CreatorFunction<IFiniteLogicalSeries>(() =>
            {
                return CreateSeries(range);
            });
        }

    }
}
