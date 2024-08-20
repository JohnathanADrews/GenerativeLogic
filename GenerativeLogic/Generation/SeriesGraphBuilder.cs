using GenerativeLogic.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Generation
{
    /// <summary>
    /// Builds a series directed graph, which is a collection of inter-dependent series.
    /// </summary>
    public class SeriesGraphBuilder
    {
        /// <summary>
        /// The creators for series type.
        /// </summary>
        private Dictionary<SeriesType, ICreator<ILogicalSeries>> seriesCreators = new Dictionary<SeriesType, ICreator<ILogicalSeries>>();

        /// <summary>
        /// The nodes in the graph.
        /// </summary>
        private Dictionary<int, SeriesNode> graphNodes = new Dictionary<int, SeriesNode>();        

        /// <summary>
        /// Sets the creator for a series by type.
        /// </summary>
        /// <param name="type">The type of the series to create.</param>
        /// <param name="creator">The creator for the series.</param>
        public void SetSeriesCreator(SeriesType type, ICreator<ILogicalSeries> creator)
        {
            if(this.seriesCreators.ContainsKey(type))
            {
                this.seriesCreators.Remove(type);
            }
            this.seriesCreators.Add(type, creator);
        }

        /// <summary>
        /// Sets the series node, associating it with the give node Id.
        /// </summary>
        /// <param name="nodeId">The identifier of the node.</param>
        /// <param name="seriesType">The type of series at the node.</param>
        /// <param name="preserveConnections">Iff true, the connections of the node are preserved if the node already exists.</param>
        public void SetNode(int nodeId, SeriesType seriesType, bool preserveConnections = true)
        {
            SeriesNode current = null;
            SeriesNode newNode = new SeriesNode();
            newNode.NodeId = nodeId;
            newNode.SeriesType = seriesType;
            if (this.graphNodes.ContainsKey(nodeId))
            {
                current = this.graphNodes[nodeId];
                this.graphNodes.Remove(nodeId);
            }
            this.graphNodes.Add(nodeId, newNode);
            if(current != null)
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
        /// Creates the series graph using the added node types.
        /// </summary>
        /// <returns>The series graph created using the added series types.</returns>
        public SeriesGraph CreateSeriesGraph()
        {
            var graph = new SeriesGraph();
            ICreator<ILogicalSeries> creator;
            foreach(var node in this.graphNodes.Values)
            {
                creator = this.seriesCreators[node.SeriesType];
                graph.SetNode(node.NodeId, creator.Create());
            }
            return graph;
        }

        /// <summary>
        /// A node representing the position of a kind of series.
        /// </summary>
        private class SeriesNode
        {
            /// <summary>
            /// The unique identifier of the series node.
            /// </summary>
            public int NodeId { get; set; }
            
            /// <summary>
            /// The type of series at this node.
            /// </summary>
            public SeriesType SeriesType { get; set; }

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
        private class NodeParameter
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
