using GenerativeLogic.Operand;
using GenerativeLogic.Operator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Foundation
{
    /// <summary>
    /// An binary-valued expression of binary values and binary valued operators.
    /// </summary>
    public class LogicalExpression : IOperandStateProvider
    {
        /// <summary>
        /// Get the connections to this expression.
        /// </summary>
        public IEnumerable<IOperandStateProvider> Connections
        {
            get
            {
                var nodes = GetNodes<IOperandStateProvider>();
                return nodes.Select(x => (IOperandStateProvider)x.Value).ToList();
            }
        }

        /// <summary>
        /// Provides the evaluated value of this expression.
        /// </summary>
        /// <returns>The evaluated value of this expression.</returns>
        public bool Provide()
        {
            return Evaluate();
        }
        
        #region EnumsAndClasses

        /// <summary>
        /// Represents the parameter types.
        /// </summary>
        public enum ExpressionParameter
        {
            /// <summary>
            /// The node is the first parameter of the parent node.
            /// </summary>
            Parameter1,
            /// <summary>
            /// The node is the second parameter of the parent node.
            /// </summary>
            Parameter2
        }

        /// <summary>
        /// The node types for the expression. 
        /// </summary>
        public enum ExpressionNodeType
        {
            Literal,
            Operand,
            Operator,
            Expression,
            Unknown
        }

        /// <summary>
        /// Represents a single node in an expression.
        /// </summary>
        public class ExpressionNode
        {
            /// <summary>
            /// The identifier of this node.
            /// </summary>
            public int NodeID { get; set; }

            /// <summary>
            /// The identifier of the parent node.
            /// </summary>
            public int? ParentNodeId { get; set; }

            /// <summary>
            /// The parameter of this node with respect to its parent.
            /// </summary>
            public ExpressionParameter? ParameterOfParent { get; set; }

            /// <summary>
            /// The identifier of the parameter1 child node.  This is calculated by Expression; setting it has no effect.
            /// </summary>
            public int? ChildParameter1Node { get; set; }

            /// <summary>
            /// The identifier of the parameter2 child node.  This is calculated by Expression; setting it has no effect.
            /// </summary>
            public int? ChildParameter2Node { get; set; }

            /// <summary>
            /// The negation of this node.
            /// </summary>
            public INegationProvider Negation { get; set; }

            /// <summary>
            /// The value of this node.
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// The type of the node known to the expression.
            /// </summary>
            public ExpressionNodeType NodeType { get; set; }

            /// <summary>
            /// True if this node is the root.  false otherwise.
            /// </summary>
            public bool IsRoot { get; set; }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="nodeId">The identifier of this node.</param>
            /// <param name="ParentNodeId">The identifier of the parent node.</param>
            /// <param name="value">The value of this node.</param>
            /// <param name="parameterOfParent">The parameter of this node with respect to its parent.</param>
            /// <param name="negation">The negation of this node.  If true, then on evaluation, the result at the node is negated.</param>
            /// <param name="isRoot">True if this node is the root.  false otherwise.</param>
            public ExpressionNode(int nodeId, int? parentNodeId, object value, ExpressionParameter? parameterOfParent = null, INegationProvider negation = null, bool isRoot = false)
            {
                this.NodeID = nodeId;
                this.ParentNodeId = parentNodeId;
                this.ParameterOfParent = parameterOfParent;
                this.Value = value;
                this.Negation = negation == null ? NegationOption.NoNegation : negation;
                this.IsRoot = isRoot;
            }

            /// <summary>
            /// Copy Constructor.
            /// </summary>
            /// <param name="node">The node to copy.</param>
            public ExpressionNode(ExpressionNode node)
            {
                this.NodeID = node.NodeID;
                this.ParentNodeId = node.ParentNodeId;
                this.ParameterOfParent = node.ParameterOfParent;
                this.Value = node.Value;
                this.Negation = node.Negation;
                this.IsRoot = node.IsRoot;
            }

            /// <summary>
            /// A string representation of this expression.
            /// </summary>
            /// <returns>A string representation of this expression.</returns>
            public override string ToString()
            {
                return String.Format("NodeId: {0}, Root: {1}, ParentId: {2}, Parameter: {3}, Type: {4}, Value: ({5}), Negation: {6}, Child1: {7}, Child2: {8}",
                    this.NodeID,
                    this.IsRoot,
                    this.ParentNodeId,
                    this.ParameterOfParent,
                    this.Value.GetType(),
                    this.Value,
                    this.Negation,
                    this.ChildParameter1Node,
                    this.ChildParameter2Node
                    );
            }
        }

        #endregion

        #region PublicMembers

        /// <summary>
        /// The number of nodes in this expression.
        /// </summary>
        public int NodeCount { get { return this.expressionDefinition.Keys.Count; } }

        /// <summary>
        /// Gets the largest node id.
        /// </summary>
        public int MaxNodeId { get { return this.expressionDefinition.Keys.Max(); } }

        #endregion

        #region PrivateMembers

        /// <summary>
        /// A lookup to get all of the nodes of a given type.
        /// </summary>
        private Dictionary<Type, List<int>> nodeTypeLookup = new Dictionary<Type, List<int>>();

        /// <summary>
        /// Contains all of the expression nodes.
        /// </summary>
        private Dictionary<int, ExpressionNode> expressionDefinition = new Dictionary<int, ExpressionNode>();

        /// <summary>
        /// The identifier of the root node.
        /// </summary>
        private int? rootNodeId = null;

        /// <summary>
        /// The order in which to evaluate the nodes.
        /// </summary>
        private List<int> evaluationPlan = new List<int>();

        /// <summary>
        /// This becomes false every time the expression in changed and true after a build.  This prevents building before every execution.
        /// </summary>
        private bool buildIsValid = false;

        /// <summary>
        /// Tracks whether the .ToString value is valid.
        /// </summary>
        private bool printIsValid = false;

        /// <summary>
        /// The value to return in .ToString
        /// </summary>
        private string printedValue;

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public LogicalExpression()
        {
            Initialize();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="expression">The expression to copy.</param>
        public LogicalExpression(LogicalExpression expression)
        {
            Initialize();
            this.expressionDefinition = new Dictionary<int, ExpressionNode>();
            ExpressionNode newNode;
            foreach (ExpressionNode node in expression.expressionDefinition.Values)
            {
                newNode = new ExpressionNode(node);
                this.AddNode(newNode);
            }
        }

        /// <summary>
        /// Sets up the standard information for the expression.
        /// </summary>
        public void Initialize()
        {
            Invalidate();
            this.nodeTypeLookup.Add(typeof(IOperandStateProvider), new List<int>());
            this.nodeTypeLookup.Add(typeof(IOperator), new List<int>());
            this.nodeTypeLookup.Add(typeof(LogicalExpression), new List<int>());
            this.nodeTypeLookup.Add(typeof(bool), new List<int>());
        }

        /// <summary>
        /// Adds the node to the expression.
        /// </summary>
        /// <param name="node">The node to add to the expression.</param>
        public void AddNode(ExpressionNode node)
        {
            Invalidate();
            if (node.Value == null)
            {
                throw new ArgumentException("The node value cannot be null.");
            }
            if (node.IsRoot && this.rootNodeId != null)
            {
                throw new ArgumentException("The root node already exists.");
            }

            //Set the root node id if the node is the root.
            if (node.IsRoot)
            {
                this.rootNodeId = node.NodeID;
            }


            //Get the node type.
            Type type = node.Value.GetType();
            Type[] interfaces = type.GetInterfaces();
            if (interfaces.Contains(typeof(IOperandStateProvider)))
            {
                node.NodeType = ExpressionNodeType.Operand;
            }
            else if (interfaces.Contains(typeof(IOperator)))
            {
                node.NodeType = ExpressionNodeType.Operator;
            }
            else if (type == typeof(bool))
            {
                node.NodeType = ExpressionNodeType.Literal;
            }
            else if (type == typeof(LogicalExpression))
            {
                node.NodeType = ExpressionNodeType.Expression;
            }

            this.expressionDefinition.Add(node.NodeID, node);

            //Add the node to the type lookup.  If the node value implements IOperand or IOperator, it will be listed in two lists in the lookup.
            //Make sure it is not added to the same list twice by checking type != typeof(IOperand) or type != typeof(IOperator).
            if (node.NodeType == ExpressionNodeType.Operand && type != typeof(IOperandStateProvider))
            {
                this.nodeTypeLookup[typeof(IOperandStateProvider)].Add(node.NodeID);
            }
            if (node.NodeType == ExpressionNodeType.Operator && type != typeof(IOperator))
            {
                this.nodeTypeLookup[typeof(IOperator)].Add(node.NodeID);
            }
            if (!this.nodeTypeLookup.ContainsKey(type))
            {
                this.nodeTypeLookup.Add(type, new List<int>());
            }
            this.nodeTypeLookup[type].Add(node.NodeID);

            foreach (Type interfaceType in interfaces)
            {
                if (interfaceType == typeof(IOperandStateProvider) || interfaceType == typeof(IOperator))
                {
                    continue;
                }
                if (!this.nodeTypeLookup.ContainsKey(interfaceType))
                {
                    this.nodeTypeLookup.Add(interfaceType, new List<int>());
                }
                this.nodeTypeLookup[interfaceType].Add(node.NodeID);
            }

        }

        /// <summary>
        /// Removes the node from the expression.
        /// </summary>
        /// <param name="node">The node to remove from the expression.</param>
        public void RemoveNode(ExpressionNode node)
        {
            Invalidate();
            if (!this.expressionDefinition.ContainsKey(node.NodeID))
            {
                return;
            }
            //Get the current node in case the provided node is differnt.
            ExpressionNode currentNode = this.expressionDefinition[node.NodeID];
            this.expressionDefinition.Remove(node.NodeID);
            Type type = currentNode.Value.GetType();
            Type[] interfaces = type.GetInterfaces();
            this.nodeTypeLookup[type].Remove(node.NodeID);
            if (interfaces.Contains(typeof(IOperandStateProvider)) && type != typeof(IOperandStateProvider))
            {
                this.nodeTypeLookup[typeof(IOperandStateProvider)].Remove(node.NodeID);
            }
            else if (interfaces.Contains(typeof(IOperator)) && type != typeof(IOperator))
            {
                this.nodeTypeLookup[typeof(IOperator)].Remove(node.NodeID);
            }
            if (node.IsRoot)
            {
                this.rootNodeId = null;
            }

            foreach (Type interfaceType in interfaces)
            {
                if (interfaceType == typeof(IOperandStateProvider) || interfaceType == typeof(IOperator))
                {
                    continue;
                }
                this.nodeTypeLookup[interfaceType].Remove(node.NodeID);
            }

        }

        /// <summary>
        /// Sets the node in the expression.  This will add the node if it does not exist or replace an existing one.
        /// </summary>
        /// <param name="node">The node to set.</param>
        public void SetNode(ExpressionNode node)
        {
            Invalidate();
            if (!this.expressionDefinition.ContainsKey(node.NodeID))
            {
                AddNode(node);
                return;
            }
            //Remove the current node.
            if (node.NodeID == rootNodeId)
            {
                this.rootNodeId = null;
            }
            RemoveNode(this.expressionDefinition[node.NodeID]);
            AddNode(node);
        }

        /// <summary>
        /// Gets the node with the given node identifier.
        /// </summary>
        /// <param name="nodeId">The identifier of the node to get.</param>
        /// <returns>The node with the given identifier.</returns>
        public ExpressionNode GetNode(int nodeId)
        {
            return this.expressionDefinition[nodeId];
        }

        /// <summary>
        /// Gets the nodes of the given value type.
        /// </summary>
        /// <param name="type">The type of node values of the nodes to get.</param>
        /// <returns>The node that have a value of the given type.</returns>
        public IEnumerable<ExpressionNode> GetNodes(Type type)
        {
            if (!this.nodeTypeLookup.ContainsKey(type))
            {
                return new List<ExpressionNode>();
            }
            List<ExpressionNode> nodes = new List<ExpressionNode>();
            foreach (int nodeId in this.nodeTypeLookup[type])
            {
                nodes.Add(GetNode(nodeId));
            }
            return nodes;
        }

        /// <summary>
        /// Gets the nodes of the given value type.
        /// </summary>
        /// <param name="type">The type of node values of the nodes to get.</param>
        /// <returns>The node that have a value of the given type.</returns>
        public IEnumerable<ExpressionNode> GetNodes<T>()
        {
            return GetNodes(typeof(T));
        }

        /// <summary>
        /// Gets all of the nodes.
        /// </summary>
        /// <returns>All of the nodes.</returns>
        public IEnumerable<ExpressionNode> GetNodes()
        {
            List<ExpressionNode> nodes = new List<ExpressionNode>();
            foreach (ExpressionNode node in this.expressionDefinition.Values)
            {
                nodes.Add(node);
            }
            return nodes;
        }

        /// <summary>
        /// Gets the number of nodes with the given type.
        /// </summary>
        /// <typeparam name="T">The type of node to count.</typeparam>
        /// <returns>The number of nodes.</returns>
        public int GetNodeCount<T>()
        {
            Type type = typeof(T);
            if (!this.nodeTypeLookup.ContainsKey(type))
            {
                return 0;
            }
            return this.nodeTypeLookup[type].Count();
        }

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <returns>The result of the evaluation.</returns>
        public bool Evaluate()
        {
            if (!this.buildIsValid)
            {
                Build();
            }

            Dictionary<int, bool> results = new Dictionary<int, bool>();
            ExpressionNode node;

            //Set the literal nodes.
            foreach (int nodeId in this.nodeTypeLookup[typeof(bool)])
            {
                node = this.expressionDefinition[nodeId];
                results.Add(nodeId, ((bool)node.Value) ^ node.Negation.GetNegation());
            }
            //Set the operand nodes.
            foreach (int nodeId in this.nodeTypeLookup[typeof(IOperandStateProvider)])
            {
                node = this.expressionDefinition[nodeId];
                results.Add(nodeId, ((IOperandStateProvider)node.Value).Provide() ^ node.Negation.GetNegation());
            }

            //Execute the evaluation plan.
            for (int i = 0; i < this.evaluationPlan.Count; i++)
            {
                node = this.expressionDefinition[this.evaluationPlan[i]];
                results.Add(node.NodeID, ((IOperator)node.Value).Evaluate(results[(int)node.ChildParameter1Node], results[(int)node.ChildParameter2Node]) ^ node.Negation.GetNegation());
            }
            return results[(int)this.rootNodeId];
        }

        /// <summary>
        /// Sets the Expression parameter values for nodes with child nodes. 
        /// </summary>
        /// <remarks>This prepares an expression for evaluation.</remarks>
        public void Build()
        {
            SetupNodeParameters();
            CreateEvaluationPlan();
            this.buildIsValid = true;
        }

        /// <summary>
        /// Recursively integrates all nodes of type Expresssion into this expression by replacing the Expression node with its constituent nodes.
        /// </summary>
        public void IntegrateSubExpressions()
        {
            Invalidate();
            int nextNodeId;
            ExpressionNode oldNode;
            ExpressionNode newNode;
            List<int> expressionNodeIds = new List<int>();

            //If there are no Expression nodes, return.
            if (!this.nodeTypeLookup.ContainsKey(typeof(LogicalExpression)))
            {
                return;
            }

            //Process while there are Expression nodes.
            while (this.nodeTypeLookup[typeof(LogicalExpression)].Count > 0)
            {
                //Iterate through the expression nodes.
                expressionNodeIds.Clear();
                expressionNodeIds.AddRange(this.nodeTypeLookup[typeof(LogicalExpression)]);
                foreach (int nodeId in expressionNodeIds)
                {
                    nextNodeId = this.expressionDefinition.Keys.Max() + 1;
                    oldNode = this.expressionDefinition[nodeId];
                    //Get all of the nodes in the child expression.
                    foreach (ExpressionNode childNode in ((LogicalExpression)this.expressionDefinition[nodeId].Value).expressionDefinition.Values)
                    {
                        newNode = new ExpressionNode(childNode);
                        newNode.NodeID = nextNodeId + childNode.NodeID;
                        if (childNode.IsRoot)
                        {
                            newNode.Negation = NegationOption.GetFromValue(oldNode.Negation.GetNegation() ^ newNode.Negation.GetNegation());
                        }
                        if (childNode.IsRoot && oldNode.IsRoot)
                        {
                            RemoveNode(oldNode);
                        }
                        else if (childNode.IsRoot)
                        {
                            newNode.IsRoot = false;
                            newNode.ParentNodeId = oldNode.ParentNodeId;
                            newNode.ParameterOfParent = oldNode.ParameterOfParent;
                        }
                        else
                        {
                            newNode.ParentNodeId = nextNodeId + childNode.ParentNodeId;
                        }
                        this.SetNode(newNode);
                    }
                    this.RemoveNode(oldNode);
                }
            }
        }

        /// <summary>
        /// Clears all of the nodes from the expression.
        /// </summary>
        public void Clear()
        {
            Invalidate();
            List<ExpressionNode> nodes = new List<ExpressionNode>();
            nodes.AddRange(this.expressionDefinition.Values);
            foreach (ExpressionNode node in nodes)
            {
                this.RemoveNode(node);
            }
        }

        /// <summary>
        /// Sets up the parameters of nodes.
        /// </summary>
        private void SetupNodeParameters()
        {
            //Clear parent parameter nodes.
            foreach (ExpressionNode node in this.expressionDefinition.Values)
            {
                node.ChildParameter1Node = null;
                node.ChildParameter2Node = null;
            }
            //Setup the child nodes.
            foreach (ExpressionNode node in this.expressionDefinition.Values)
            {
                //The root has no parent, so continue.
                if (node.IsRoot)
                {
                    continue;
                }
                //Set the child node id based on the parameter position.
                if (node.ParameterOfParent == ExpressionParameter.Parameter1)
                {
                    this.expressionDefinition[(int)node.ParentNodeId].ChildParameter1Node = node.NodeID;
                }
                else
                {
                    this.expressionDefinition[(int)node.ParentNodeId].ChildParameter2Node = node.NodeID;
                }

            }
        }

        /// <summary>
        /// Sets the evaluation plan, which is an ordered list of nodes that the expression will be executed in.
        /// </summary>
        private void CreateEvaluationPlan()
        {
            List<int> markedNodes = new List<int>();
            ExpressionNode node;

            //Clear the old plan.
            this.evaluationPlan.Clear();

            //The leaf nodes are initially the operand and literal nodes.  The list is adjusted to include leaves from different evaluation steps and remove oned from previous steps.
            List<int> leafNodes = new List<int>();
            List<int> newLeafNodes;

            //Add the literal nodes.
            leafNodes.AddRange(this.nodeTypeLookup[typeof(bool)]);
            leafNodes.AddRange(this.nodeTypeLookup[typeof(IOperandStateProvider)]);

            //Evaluate the leaf nodes, which are changed at each iteration.  Eventually, the leaf nodes will be empty.
            while (leafNodes.Count > 0)
            {
                newLeafNodes = new List<int>();
                foreach (int nodeId in leafNodes)
                {
                    node = this.expressionDefinition[nodeId];
                    //Stop when the root node is reached.
                    if (node.IsRoot)
                    {
                        break;
                    }
                    //If the parent has not been marked, add it to the marked list.  
                    //If it has been, remove it from the marked list and add it to the new leaf node list.
                    //This indicates that both child nodes are accounted for, and that this node can be processed next in the evaluation.
                    if (!markedNodes.Contains((int)node.ParentNodeId))
                    {
                        markedNodes.Add((int)node.ParentNodeId);
                    }
                    else
                    {
                        newLeafNodes.Add((int)node.ParentNodeId);
                        markedNodes.Remove((int)node.ParentNodeId);
                    }
                }
                leafNodes = newLeafNodes;
                //Add the nodes to the execution plan.
                this.evaluationPlan.AddRange(newLeafNodes);
            }

        }

        private void Invalidate()
        {
            this.buildIsValid = false;
            this.printIsValid = false;
        }

        /// <summary>
        /// Removes all of the literal nodes in the expression by replacing the parent with an equivalent based on the operator.
        /// </summary>
        public void TrimLiteralNodes()
        {
            ExpressionNode parent;
            ExpressionNode node;
            ExpressionNode sibling;
            ExpressionNode newNode;
            IOperator binaryOperator;
            bool value;
            bool result0;
            bool result1;

            if (!this.buildIsValid)
            {
                this.Build();
            }

            IEnumerable<ExpressionNode> nodes = this.GetNodes<bool>();
            node = (nodes.Count() > 0 ? nodes.First() : null);
            while (node != null)
            {
                //If the root node is a literal then break.
                if (node.NodeID == this.rootNodeId)
                {
                    break;
                }
                parent = this.GetNode((int)node.ParentNodeId);
                binaryOperator = (IOperator)parent.Value;
                value = (bool)node.Value;
                if (node.ParameterOfParent == ExpressionParameter.Parameter1)
                {
                    result0 = binaryOperator.Evaluate(value, false) ^ parent.Negation.GetNegation();
                    result1 = binaryOperator.Evaluate(value, true) ^ parent.Negation.GetNegation();
                    //Set the new node value to the sibling in case we need it.
                    sibling = this.expressionDefinition[(int)parent.ChildParameter2Node];
                }
                else
                {
                    result0 = binaryOperator.Evaluate(false, value) ^ parent.Negation.GetNegation();
                    result1 = binaryOperator.Evaluate(true, value) ^ parent.Negation.GetNegation();
                    //Set the new node value to the sibling in case we need it.
                    sibling = this.expressionDefinition[(int)parent.ChildParameter1Node];
                }

                //If both results are 0, then replace parent with 0.
                if (!result0 && !result1)
                {
                    newNode = new ExpressionNode(parent);
                    RemoveSubExpression(parent.NodeID);
                    newNode.Value = false;
                    this.SetNode(newNode);
                }
                //If result0 is 1 and result1 is 0, then set the parent to the negated sibling.
                if (result0 && !result1)
                {
                    sibling.Negation = NegationOption.GetFromValue(sibling.Negation.GetNegation() ^ true);
                    sibling.ParentNodeId = parent.ParentNodeId;
                    sibling.ParameterOfParent = parent.ParameterOfParent;
                    sibling.IsRoot = parent.IsRoot;
                    if (!parent.IsRoot)
                    {
                        ExpressionNode grandpa = this.GetNode((int)parent.ParentNodeId);
                        if (sibling.ParameterOfParent == ExpressionParameter.Parameter1)
                        {
                            grandpa.ChildParameter1Node = sibling.NodeID;
                        }
                        else
                        {
                            grandpa.ChildParameter2Node = sibling.NodeID;
                        }
                    }
                    this.RemoveNode(node);
                    this.RemoveNode(parent);
                    this.SetNode(sibling);
                }
                //If result0 is 0 and result1 is 1, then set the parent to the sibling.
                if (!result0 && result1)
                {
                    sibling.ParentNodeId = parent.ParentNodeId;
                    sibling.ParameterOfParent = parent.ParameterOfParent;
                    sibling.IsRoot = parent.IsRoot;
                    if (!parent.IsRoot)
                    {
                        ExpressionNode grandpa = this.GetNode((int)parent.ParentNodeId);
                        if (sibling.ParameterOfParent == ExpressionParameter.Parameter1)
                        {
                            grandpa.ChildParameter1Node = sibling.NodeID;
                        }
                        else
                        {
                            grandpa.ChildParameter2Node = sibling.NodeID;
                        }
                    }
                    this.RemoveNode(node);
                    this.RemoveNode(parent);
                    this.SetNode(sibling);
                }
                //If both results are 1, then replace parent with 1.
                if (result0 && result1)
                {
                    newNode = new ExpressionNode(parent);
                    RemoveSubExpression(parent.NodeID);
                    newNode.Value = true;
                    this.SetNode(newNode);
                }

                nodes = this.GetNodes<bool>();
                node = nodes.Count() > 0 ? nodes.First() : null;
            }
        }

        /// <summary>
        /// Removes the node with the given node id from the expression tree and all of its decendents.
        /// </summary>
        /// <param name="nodeId"></param>
        public void RemoveSubExpression(int nodeId)
        {
            this.Build();
            List<ExpressionNode> nodes = new List<ExpressionNode>() { this.expressionDefinition[nodeId] };
            List<ExpressionNode> next = new List<ExpressionNode>();

            while (nodes.Count() > 0)
            {
                foreach (ExpressionNode node in nodes)
                {
                    if (node.ChildParameter1Node != null)
                    {
                        next.Add(this.expressionDefinition[(int)node.ChildParameter1Node]);
                    }
                    if (node.ChildParameter2Node != null)
                    {
                        next.Add(this.expressionDefinition[(int)node.ChildParameter2Node]);
                    }
                    this.RemoveNode(node);
                }
                nodes.Clear();
                nodes.AddRange(next);
                next.Clear();
            }
        }

        /// <summary>
        /// Gets the sibling of the node with the given identifier.
        /// </summary>
        /// <param name="nodeId">The identifier of the node.</param>
        /// <returns>The sibling of the node with the given id or null if there is no sibling.</returns>
        public ExpressionNode GetSibling(int nodeId)
        {
            if (!this.buildIsValid)
            {
                Build();
            }
            if (!this.expressionDefinition.ContainsKey(nodeId))
            {
                return null;
            }
            ExpressionNode node = this.expressionDefinition[nodeId];
            ExpressionNode parent;
            if (node.ParentNodeId != null)
            {
                parent = this.expressionDefinition[(int)node.ParentNodeId];
            }
            else
            {
                return null;
            }
            if (node.ParameterOfParent == ExpressionParameter.Parameter1)
            {
                return this.expressionDefinition[(int)parent.ChildParameter2Node];
            }
            else
            {
                return this.expressionDefinition[(int)parent.ChildParameter1Node];
            }
        }

        /// <summary>
        /// Gets the parent of the node with the given identifier.
        /// </summary>
        /// <param name="nodeId">The identifier of the node.</param>
        /// <returns>The parent of the node with the given id or null if there is no parent.</returns>
        public ExpressionNode GetParent(int nodeId)
        {
            if (!this.buildIsValid)
            {
                Build();
            }
            if (!this.expressionDefinition.ContainsKey(nodeId))
            {
                return null;
            }
            ExpressionNode node = this.expressionDefinition[nodeId];
            if (node.ParentNodeId != null)
            {
                return this.expressionDefinition[(int)node.ParentNodeId];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns true iff the node id of the given node has a value of the given type. 
        /// </summary>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <param name="nodeId">The identifier of the node to check.</param>
        /// <returns>true iff the node id of the given node has a value of the given type.</returns>
        public bool NodeIsType<T>(int nodeId)
        {
            if (!this.nodeTypeLookup.ContainsKey(typeof(T)))
            {
                return false;
            }
            return this.nodeTypeLookup[typeof(T)].Contains(nodeId);
        }

        public override string ToString()
        {
            if (this.printIsValid)
            {
                return this.printedValue;
            }
            if (!this.buildIsValid)
            {
                Build();
            }

            List<ExpressionNode> printOrder = new List<ExpressionNode>();
            if (this.rootNodeId == null)
            {
                return "(empty Expression)";
            }
            ExpressionNode node = this.expressionDefinition[(int)this.rootNodeId];
            StringBuilder result = new StringBuilder();
            HashSet<int> visitedNodes = new HashSet<int>();
            int leftCount = 0;

            while (true)
            {
                //The break condition
                if (node.IsRoot && (node.ChildParameter2Node == null || visitedNodes.Contains((int)node.ChildParameter2Node)))
                {
                    //Add the root node if it is the only node.
                    if (node.ChildParameter2Node == null)
                    {
                        if (node.Negation.GetNegation())
                        {
                            result.Append("!");
                        }
                        result.Append(node.Value);
                    }
                    break;
                }

                //If the node is a leaf node or if both of its children have been visited, add it and go up the tree.
                if (node.ChildParameter1Node == null ||
                    (visitedNodes.Contains((int)node.ChildParameter1Node) && visitedNodes.Contains((int)node.ChildParameter2Node)))
                {
                    if (node.ParameterOfParent == ExpressionParameter.Parameter1 && node.ChildParameter1Node == null)
                    {
                        for (int i = 0; i < leftCount; i++)
                        {
                            result.Append("(");
                        }
                        leftCount = 0;
                        if (node.Negation.GetNegation())
                        {
                            result.Append("!");
                        }
                        if (node.Value.GetType() == typeof(bool))
                        {
                            result.Append((bool)node.Value ? 1 : 0);
                        }
                        else
                        {
                            result.Append(node.Value);
                        }
                        result.Append(" ");
                        visitedNodes.Add(node.NodeID);
                    }
                    else if (node.ParameterOfParent == ExpressionParameter.Parameter2 && node.ChildParameter1Node == null)
                    {
                        if (node.Negation.GetNegation())
                        {
                            result.Append("!");
                        }
                        if (node.Value.GetType() == typeof(bool))
                        {
                            result.Append((bool)node.Value ? 1 : 0);
                        }
                        else
                        {
                            result.Append(node.Value);
                        }
                        result.Append(")");
                        visitedNodes.Add(node.NodeID);
                    }
                    else if (node.ParameterOfParent == ExpressionParameter.Parameter2)
                    {
                        result.Append(")");
                    }
                    else
                    {
                        result.Append(" ");
                    }

                    if (node.ParentNodeId != null)
                    {
                        node = this.expressionDefinition[(int)node.ParentNodeId];
                    }
                    continue;
                }

                //The left node hasn't been visited, go to it.
                if (node.ChildParameter1Node != null && !visitedNodes.Contains((int)node.ChildParameter1Node))
                {
                    leftCount++;
                    node = this.expressionDefinition[(int)node.ChildParameter1Node];
                    continue;
                }

                //The right node hasn't been visited, so go to it.
                if (node.ChildParameter2Node != null && !visitedNodes.Contains((int)node.ChildParameter2Node))
                {
                    result.Append(node.Value);
                    result.Append(" ");
                    visitedNodes.Add(node.NodeID);
                    node = this.expressionDefinition[(int)node.ChildParameter2Node];
                    continue;
                }
            }
            this.printedValue = result.ToString();
            this.printIsValid = true;
            return this.printedValue;
        }
    }
}
