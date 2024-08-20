using GenerativeLogic.Operand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Series
{

    /// <summary>
    /// Builds a graph of series.
    /// </summary>
    public class SeriesGraphBuilder
    {

        private Dictionary<int, ICreator<ILogicalSeries>> registeredSeries;

        private List<NodeLink> links = new List<NodeLink>();


        public void SetNode(int nodeId, ICreator<ILogicalSeries> seriesCreator)
        {
            if (registeredSeries.ContainsKey(nodeId))
            {
                this.registeredSeries.Remove(nodeId);
            }
            this.registeredSeries.Add(nodeId, seriesCreator);
        }

        public void LinkNodes(int sourceNodeId, int destinationNodeId, IParameter parameter)
        {
            var link = new NodeLink();
            link.SourceNodeId = sourceNodeId;
            link.DestindationNodeId = destinationNodeId;
            link.Parameter = parameter;
            links.Add(link);
        }


        public DirectedGraph<ILogicalSeries> CreateGraph()
        {
            var nodes = new Dictionary<int, ILogicalSeries>();
            foreach(var creatorNode in this.registeredSeries)
            {
                nodes.Add(creatorNode.Key, creatorNode.Value.Create());
            }
            ILogicalSeries source;
            ILogicalSeries destination;
            foreach(var link in this.links)
            {
                source = nodes[link.SourceNodeId];
                destination = nodes[link.DestindationNodeId];
                destination.SetInput<IOperandStateProvider>(link.Parameter, source);
            }

            var connectionsProvider = new ParameterizedProviderFunction<ILogicalSeries, IEnumerable<ILogicalSeries>>(origin =>
            {
                var node = nodes.Where(x => x.Value == origin).FirstOrDefault();
                var result = new List<ILogicalSeries>();
                var indices = new List<int>();
                indices = links.Where(x => x.DestindationNodeId == node.Key).Select(x => x.SourceNodeId).ToList();
                result.AddRange(nodes.Where(x => indices.Contains(x.Key)).Select(x => x.Value).ToList());
                indices = links.Where(x => x.DestindationNodeId == node.Key).Select(x => x.SourceNodeId).ToList();
                result.AddRange(nodes.Where(x => indices.Contains(x.Key)).Select(x => x.Value).ToList());
                return result.Distinct();
            });

            var relationSelector = new SelectorFunction<RelatedNodePair<ILogicalSeries>, DirectedGraphNodeRelation>(pair =>
            {
                int originId = nodes.Where(x => x.Value == pair.Origin).FirstOrDefault().Key;
                int relatedId = nodes.Where(x => x.Value == pair.Related).FirstOrDefault().Key;

                if (this.links.Where(x => x.SourceNodeId == originId && x.DestindationNodeId == relatedId).Count() > 0)
                {
                    return DirectedGraphNodeRelation.Source;
                }

                if (this.links.Where(x => x.SourceNodeId == relatedId && x.DestindationNodeId == originId).Count() > 0)
                {
                    return DirectedGraphNodeRelation.Destination;
                }

                return DirectedGraphNodeRelation.NoRelation;
            });

            var generator = new DirectedGraphGenerator<ILogicalSeries>();
            var graph = generator.GenerateDirectedGraph(nodes.Select(x => x.Value).ToList(),
                connectionsProvider, relationSelector);

            return graph;
        }

        private class NodeLink
        {

            public int SourceNodeId { get; set; }

            public int DestindationNodeId { get; set; }

            public IParameter Parameter { get; set; }
        }



    }
}
