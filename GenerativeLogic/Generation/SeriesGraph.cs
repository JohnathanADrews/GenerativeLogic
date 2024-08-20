using GenerativeLogic.Operand;
using GenerativeLogic.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Generation
{
    /// <summary>
    /// A directed graph that represents the hierarchical relationship among series.
    /// </summary>
    public class SeriesGraph
    {

        /// <summary>
        /// The nodes in the graph.
        /// </summary>
        private Dictionary<int, SeriesNode> graphNodes = new Dictionary<int, SeriesNode>();

        /// <summary>
        /// Provides the connections from one node to another.
        /// </summary>
        private IParameterizedProvider<SeriesNode, IEnumerable<SeriesNode>> connector;

        /// <summary>
        /// Selects the relationship between two nodes.
        /// </summary>
        private ISelector<RelatedNodePair<SeriesNode>, DirectedGraphNodeRelation> relationSelector;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SeriesGraph()
        {
            this.connector = new ParameterizedProviderFunction<SeriesNode, IEnumerable<SeriesNode>>(input =>
            {
                //Get all of the parameter and receiver nodes, and then remove the nodes that are this node.
                var connections = new List<SeriesNode>();
                connections.AddRange(input.Parameters.Select(x => this.graphNodes[x.ParameterNodeId]).ToList());
                connections.AddRange(input.Souced.Select(x => this.graphNodes[x.ReceiverNodeId]).ToList());
                return connections.Where(x=>x.NodeId != input.NodeId).Distinct().ToList();
            });

            this.relationSelector = new SelectorFunction<RelatedNodePair<SeriesNode>, DirectedGraphNodeRelation>(pair =>
            {
                if (pair.Origin.Parameters.Select(x => x.ReceiverNodeId == pair.Origin.NodeId && x.ParameterNodeId == pair.Related.NodeId).Count() > 0)
                {
                    return DirectedGraphNodeRelation.Destination;
                }
                if (pair.Origin.Parameters.Select(x => x.ReceiverNodeId == pair.Related.NodeId && x.ParameterNodeId == pair.Origin.NodeId).Count() > 0)
                {
                    return DirectedGraphNodeRelation.Destination;
                }
                return DirectedGraphNodeRelation.NoRelation;
            });
        }

        /// <summary>
        /// Gets the series contained by the node with the given identifier.
        /// </summary>
        /// <param name="nodeId">The identifier of the node that contains the series to get.</param>
        /// <returns>The series contained by the node with the given identifier.</returns>
        public ILogicalSeries GetNodeSeries(int nodeId)
        {
            if(!this.graphNodes.ContainsKey(nodeId))
            {
                return null;
            }
            return this.graphNodes[nodeId].Series;
        }

        /// <summary>
        /// Sets the expression node based on the index.
        /// </summary>
        /// <param name="nodeId">The identifier of the node to set.</param>
        /// <param name="series">The series to set at the node.</param>
        /// <param name="preserveConnections">Iff true, the connections of the node are preserved if the node already exists.</param>
        public void SetNode(int nodeId, ILogicalSeries series, bool preserveConnections = true)
        {
            SeriesNode current = null;
            SeriesNode newNode = new SeriesNode();
            newNode.NodeId = nodeId;
            newNode.Series = series;
            if (this.graphNodes.ContainsKey(nodeId))
            {
                current = this.graphNodes[nodeId];
                this.graphNodes.Remove(nodeId);
            }
            this.graphNodes.Add(nodeId, newNode);
            if (current != null)
            {
                if (preserveConnections)
                {
                    newNode.Parameters.AddRange(current.Parameters);
                    newNode.Souced.AddRange(current.Souced);
                }
                else
                {
                    //Remove the connections in the other nodes so they are not orphaned.
                    foreach (var node in current.Parameters)
                    {
                        this.graphNodes[node.ParameterNodeId].Souced.Remove(node);
                    }
                    foreach (var node in current.Souced)
                    {
                        this.graphNodes[node.ReceiverNodeId].Parameters.Remove(node);
                    }
                }
            }
        }
        
        /// <summary>
        /// Sets the parameter relationship betweeen two nodes.
        /// </summary>
        /// <param name="parameterNodeId">The identifier of the parameter series.</param>
        /// <param name="receiverNodeId">The identifier of the series that requires the parameter.</param>
        /// <param name="parameter">The description of the parameter being filled.</param>
        public void SetParameter(int parameterNodeId, int receiverNodeId, IParameter parameter)
        {
            var parameterNode = new NodeParameter();
            parameterNode.ParameterNodeId = parameterNodeId;
            parameterNode.ReceiverNodeId = receiverNodeId;
            parameterNode.Parameter = parameter;

            this.graphNodes[parameterNodeId].Souced.Add(parameterNode);
            this.graphNodes[receiverNodeId].Parameters.Add(parameterNode);
        }

        /// <summary>
        /// Connects the series together based on the parameters and returns a graph of the series'.
        /// </summary>
        /// <returns>A graph of the connected series'.</returns>
        public DirectedGraph<SeriesNode> Build()
        {
            DirectedGraphGenerator<SeriesNode> generator = new DirectedGraphGenerator<SeriesNode>();
            var graph = generator.GenerateDirectedGraph(this.graphNodes.Values, this.connector, this.relationSelector);

            List<NodeParameter> parameters;
            ILogicalSeries receiverSeries;
            foreach(var node in this.graphNodes.Values)
            {
                //Setup the parameter relationships where the current node is the parameter.
                parameters = node.Souced.Where(x => x.ParameterNodeId == node.NodeId).ToList();
                foreach (var parameter in parameters)
                {
                    receiverSeries = this.graphNodes[parameter.ReceiverNodeId].Series;
                    receiverSeries.SetInput<IOperandStateProvider>(parameter.Parameter, node.Series);
                }
            }
            return graph;
        }

        /// <summary>
        /// A node representing the position of a kind of series.
        /// </summary>
        public class SeriesNode
        {
            /// <summary>
            /// The unique identifier of the series node.
            /// </summary>
            public int NodeId { get; set; }

            /// <summary>
            /// The series at this node.
            /// </summary>
            public ILogicalSeries Series { get; set; }

            /// <summary>
            /// The nodes that are parameters to the series of this node.
            /// </summary>
            public List<NodeParameter> Parameters { get; private set; }

            /// <summary>
            /// The nodes that the series of this node is a parameter to.
            /// </summary>
            public List<NodeParameter> Souced { get; private set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public SeriesNode()
            {
                this.Parameters = new List<NodeParameter>();
                this.Souced = new List<NodeParameter>();
            }
        }

        /// <summary>
        /// A parameter relationship between two nodes.
        /// </summary>
        public class NodeParameter
        {
            /// <summary>
            /// The node id of the parameter.
            /// </summary>
            public int ParameterNodeId { get; set; }

            /// <summary>
            /// The node id of the node accepting the parameter.
            /// </summary>
            public int ReceiverNodeId { get; set; }

            /// <summary>
            /// The parameter specification.
            /// </summary>
            public IParameter Parameter { get; set; }

        }

    }
}
