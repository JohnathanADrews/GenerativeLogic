
function LogicalOperator() {
    this.name = null;
    this.description = null;
    this.type = null;
    this.evaluator = null;
}
LogicalOperator.Type = {};
LogicalOperator.Type.Unary = "LogicalOperator.Type.Unary";
LogicalOperator.Type.Binary = "LogicalOperator.Type.Binary";
LogicalOperator.prototype = {
    Evaluate: function (values) {
        var t = this;
        if (t.type == LogicalOperator.Type.Binary) {
            return t.evaluator(values[0], values[1]);
        }
        if (t.type == LogicalOperator.Type.Unary) {
            return t.evaluator(values[0]);
        }
    }
};
LogicalOperator.Create = function (name, description, type, evaluator) {
    var operator = new LogicalOperator();
    operator.name = name;
    operator.description = description;
    operator.type = type;
    operator.evaluator = evaluator;
    return operator;
}
LogicalOperator.CreateUnary = function (name, description, evaluator) {
    return LogicalOperator.Create(name, description, LogicalOperator.Type.Unary, evaluator);
}
LogicalOperator.CreateBinary = function (name, description, evaluator) {
    return LogicalOperator.Create(name, description, LogicalOperator.Type.Binary, evaluator);
}

LogicalOperator.Operators = {};
LogicalOperator.Operators.UZ = LogicalOperator.CreateUnary("UZ", "Always returns zero.", a => 0);
LogicalOperator.Operators.UA = LogicalOperator.CreateUnary("UA", "Returns the operand's value.", a => a);
LogicalOperator.Operators.NOT = LogicalOperator.CreateUnary("NOT", "Returns the negated operand's value.", a => a ^ 1);
LogicalOperator.Operators.UO = LogicalOperator.CreateUnary("UO", "Always returns one.", a => 1 );
LogicalOperator.Unary = {};
(() => { var o = LogicalOperator.Operators; u = LogicalOperator.Unary; u[o.UZ.name] = o.UZ; u[o.UA.name] = o.UA; u[o.NOT.name] = o.NOT; u[o.UO.name] = o.UO; })();


LogicalOperator.Operators.BLOCK = LogicalOperator.CreateBinary("BLOCK", "Always returns 1.", (a, b) => 0);
LogicalOperator.Operators.AND = LogicalOperator.CreateBinary("AND", "", (a, b) => a & b);
LogicalOperator.Operators.ANB = LogicalOperator.CreateBinary("ANB", "", (a, b) => a & (!b));
LogicalOperator.Operators.EA = LogicalOperator.CreateBinary("EA", "", (a, b) => a);

LogicalOperator.Operators.NAB = LogicalOperator.CreateBinary("NAB", "", (a, b) => (!a) & b);
LogicalOperator.Operators.EB = LogicalOperator.CreateBinary("EB", "", (a, b) => b);
LogicalOperator.Operators.XOR = LogicalOperator.CreateBinary("XOR", "", (a, b) => a ^ b);
LogicalOperator.Operators.OR = LogicalOperator.CreateBinary("OR", "", (a, b) => a | b);

LogicalOperator.Operators.NOR = LogicalOperator.CreateBinary("NOR", "", (a, b) => (a | b) ^ 1);
LogicalOperator.Operators.XNOR = LogicalOperator.CreateBinary("XNOR", "", (a, b) => (a ^ b) ^ 1);
LogicalOperator.Operators.NB = LogicalOperator.CreateBinary("NB", "", (a, b) => b ^ 1);
LogicalOperator.Operators.AORNB = LogicalOperator.CreateBinary("AORNB", "", (a, b) => a | (b ^ 1));

LogicalOperator.Operators.NA = LogicalOperator.CreateBinary("NA", "", (a, b) => a ^ 1);
LogicalOperator.Operators.NAORB = LogicalOperator.CreateBinary("NAORB", "", (a, b) => (a ^ 1) | b);
LogicalOperator.Operators.NAND = LogicalOperator.CreateBinary("NAND", "", (a, b) => (a & b) ^ 1);
LogicalOperator.Operators.PASS = LogicalOperator.CreateBinary("PASS", "Always returns 1.", (a, b) => 1);

LogicalOperator.Binary = {};
(() => {
    var o = LogicalOperator.Operators;
    var b = LogicalOperator.Binary;
    b[o.BLOCK.name] = o.BLOCK; b[o.AND.name] = o.AND; b[o.ANB.name] = o.ANB; b[o.EA.name] = o.EA;
    b[o.NAB.name] = o.NAB; b[o.EB.name] = o.EB; b[o.XOR.name] = o.XOR; b[o.OR.name] = o.OR;
    b[o.NOR.name] = o.NOR; b[o.XNOR.name] = o.XNOR; b[o.NB.name] = o.NB; b[o.AORNB.name] = o.AORNB;
    b[o.NA.name] = o.NA; b[o.NAORB.name] = o.NAORB; b[o.NAND.name] = o.NAND; b[o.PASS.name] = o.PASS; 
})();

//name: the name of the series; 
//exactLength: the length of the series in bits;
//memberName: the name of the individual series members.
//abstractLength: the letter used to indicate an arbitrary length, i.e. 'k'
function BitSeries(name, exactLength, abstractLength, memberName, indexName) {
    this.name = name;
    this.exactLength = exactLength;
    this.values = [];
    this.abstractLength = abstractLength;
    this.memberName = memberName;
    this.indexName = indexName;
    this.provider = null;
};
BitSeries.prototype = {
    GetValue: function (index) {
        var t = this;
        if (t.provider != null) { return provider(index); }
        return this.values[index];
    },
    SetValue: function (index, value) {
        var t = this;
        while (index >= t.values.length) { t.values.push(0); }
        t.values[index] = value;
    },
    SetValues: function (values) {
        this.values = values;
    },
    SetValueProvider: function (provider) {
        this.provider = provider;
    }
};

function BitSeriesReader(series, offset) {
    this.series = series;
    this.offset = offset;
    this.index = 0;
};
BitSeriesReader.prototype = {
    Evaluate: function () {
        var t = this;
        return t.series.GetValue(t.index + t.offset);
    },
    CreateVariableNode: function (index) {
        var t = this;
        var node = new LogicalExpressionNode();
        node.type = LogicalExpressionNode.Type.Variable;
        node.value = node.reader;
        node.name = t.series.memberName;
        node.index = index;
        return node;
    }
};


function LogicalExpressionNode() {
    this.type = null;
    this.value = null;
    this.state = 0;
    this.negation = 0;
}
LogicalExpressionNode.Type = {};
LogicalExpressionNode.Type.Operator = "LogicalExpressionNode.Type.Operator";
LogicalExpressionNode.Type.OperandVariable = "LogicalExpressionNode.Type.OperandVariable";
LogicalExpressionNode.Type.Constant = "LogicalExpressionNode.Type.Constant";
LogicalExpressionNode.Type.BitSeriesReader = "LogicalExpressionNode.Type.BitSeriesReader";
LogicalExpressionNode.prototype = {
    Evaluate: function () {
        var t = this;
        console.log("LogicalExpressionNode.Evaluate - ", t);
        if (t.type == LogicalExpressionNode.Type.Constant) { return t.state; }
        else if (t.type == LogicalExpressionNode.Type.OperandVariable) { return t.state; }
        else if (t.type == LogicalExpressionNode.Type.Operator && t.value.type == LogicalOperator.Type.Unary) {
            if (t.left != null) { t.state = t.value.evaluator(t.left.state); } 
            if (t.right != null) { t.state = t.value.evaluator(t.right.state); }           
            return t.state;
        }
        else if (t.type == LogicalExpressionNode.Type.Operator && t.value.type == LogicalOperator.Type.Binary) {
            t.state = t.value.evaluator(t.left.state, t.right.state);
            return t.state;
        }
        else if (t.type == LogicalExpressionNode.Type.BitSeriesReader) {
            t.state = t.reader.Evaluate();
            return t.state;
        }
    },
    GetNodes: function () {
        var t = this;
        var n = [];
        if (t.left != null) { n.push(t.left); }
        if (t.right != null) { n.push(t.right); }
        if (t.operand != null) { n.push(t.operand); }
        return n;
    },
    Clone: function () {
        var t = this;
        var node = new LogicalExpressionNode();
        $.d.ProjectOnto(t, node);
        if (t.negation != node.negation) {
            //console.log("LogicalExpressionNode.Clone - ", t, node);
        }
      //  console.log("LogicalExpressionNode.Clone 1- ", t.negation, node.negation);
        return node;
        //node.type = t.type;
        //node.value = t.value;
        //node.name = t.name;
        //node.index = t.index;
        //node.negation = t.negation;
        //if (t.left != null) { node.left = t.left; }
        //if (t.right != null) { node.right = t.right; }
        //return node;
    }
}

function LogicalExpression() {
    this.root = null;
    this.evaluationPlan = [];
}
LogicalExpression.prototype = {
    Evaluate: function () {
        var t = this;
        if (t.root == null) { return null; }
        var plan = TreeTraverser.CreateEvaluationPlan(TreeTraverser.LRPOrder, t.root, node => node.GetNodes(), true);
        //var results = new Map();
        //var node = null;
        //for (var i = 0; i < plan.length; i++) {
        //    node = plan[i];
        //    results.set(node, node.Evaluate());
        //}
        //return node.value.evaluator();

        //console.log("LogicalExpression.Evaluate 1 - ", plan);
        $.d.ProcessArray(plan, node => { node.Evaluate(); });
        return t.root.state;
    },
    Clone: function () {
        var t = this;
        var plan = TreeTraverser.CreateEvaluationPlan(TreeTraverser.PLROrder, t.root, node => node.GetNodes(), true);
        var copy = [];
        var index = 0;
        $.d.ProcessArray(plan, node => {
            node.id = index++;
            var cnode = node.Clone();
            copy.push(cnode);
        });
        //console.log("LogicalExpression.Clone 1 - ", plan, copy);
        $.d.ProcessArray(plan, node => {
            if (node.left != null) { copy[node.id].left = copy[node.left.id]; }
            if (node.right != null) { copy[node.id].right = copy[node.right.id]; }
        });
        $.d.ProcessArray(plan, node => { delete node.id; });
        var result = new LogicalExpression();
        result.root = copy[0];
        return result;
    },
    //series: The series to which the expression belongs.
    //index: index of series
    //constants: the constant values of the system. e.g. constants[seriesName] = [{1,1}, {2, 0} ...]
    SetSeriesIndex: function (series, index, constants) {
        var t = this;
        //if (series.name != "R") { return; }
        //console.log("LogicalExpression.SetSeriesIndex 1 - " + index + " - ", index, series, constants);
        //console.log("LogicalExpression.SetSeriesIndex 3 - " + index + " - ", series.name, constants[series.name]);
        if (constants[series.name] != null && constants[series.name][index] != null) {
            t.root = LogicalExpression.CreateConstantNode(constants[series.name][index]);
            //console.log("LogicalExpression.SetSeriesIndex 2 - " + index + " - ", t);
            return;
        }
        var plan = TreeTraverser.CreateEvaluationPlan(TreeTraverser.LRPOrder, t.root, node => node.GetNodes(), true);
        $.d.ProcessArray(plan, node => {
            if (node.type != LogicalExpressionNode.Type.BitSeriesReader) { return; }
            var nodeIndex = index + node.value.offset;
            //console.log("LogicalExpression.SetSeriesIndex 4 - " + index + " - ", nodeIndex, node);
            if (constants[node.value.series.name] != null && constants[node.value.series.name][nodeIndex] != null) {
                node.type = LogicalExpressionNode.Type.Constant;
                node.value = constants[node.value.series.name][nodeIndex];
                return;
            }
            node.type = LogicalExpressionNode.Type.Variable;
            node.name = node.value.series.memberName;
            node.series = node.value.series;
            node.value = 0;
            node.index = nodeIndex;
        });
    },

    AbsorbConstants: function () {
        var t = this;

        var start = t.Clone();
    //    console.log("LogicalExpression.AbsorbConstants 1 - ", start, t);
        //var p1 = TreeTraverser.CreateEvaluationPlan(TreeTraverser.LRPOrder, start.root, node => node.GetNodes(), true);
        //var p2 = TreeTraverser.CreateEvaluationPlan(TreeTraverser.LRPOrder, t.root, node => node.GetNodes(), true);
        //console.log("LogicalExpression.AbsorbConstants 2 - ", p1, p2);

        var plan = TreeTraverser.CreateEvaluationPlan(TreeTraverser.LRPOrder, t.root, node => node.GetNodes(), true);
        $.d.ProcessArray(plan, node => {
            if (node.left != null) { node.left.parent = node; }
            if (node.right != null) { node.right.parent = node; }
            if (node.negation != 0 && node.negation != 1) { node.negation = 0; }
        });
 //       console.log("LogicalExpression.AbsorbConstants 4 - ", plan, t.root);
        //var we = [];
        //var wef = we[123][1];
        $.d.ProcessArray(plan, node => {
            if (node.type != LogicalExpressionNode.Type.Constant) { return; }
            if (node.parent == null || node.parent.type != LogicalExpressionNode.Type.Operator) { return; }
            var value = node.value ^ node.negation;
      //      console.log("LogicalExpression.AbsorbConstants 4.1 - ", node);

            var x1 = 0;
            var x2 = 0;
       //     var other = null;
            if (node.parent.left == node) { other = node.parent.right; }
            if (node.parent.right == node) { other = node.parent.left; }
      //      console.log("LogicalExpression.AbsorbConstants 5 - ", node.parent, node, other);
            if (node.parent.left == node) {
                x1 = node.parent.value.Evaluate([value, 0]) ^ node.parent.negation;
                x2 = node.parent.value.Evaluate([value, 1]) ^ node.parent.negation;
   //             console.log("LogicalExpression.AbsorbConstants 5.1 - ", value, node.parent.negation, node.parent.value.Evaluate(value, 0), node.parent.value.Evaluate(value, 1), x1, x2, node);
            }
            else {
                x1 = node.parent.value.Evaluate([0, value]) ^ node.parent.negation;
                x2 = node.parent.value.Evaluate([1, value]) ^ node.parent.negation;
     //           console.log("LogicalExpression.AbsorbConstants 5.2 - ", value, node.parent.negation, node.parent.value.Evaluate(value, 0), node.parent.value.Evaluate(value, 1), x1, x2, node);
            }
    //        console.log("LogicalExpression.AbsorbConstants 5.8 - ", x1, x2, node);
         //   return false;

            if (x1 == x2) {
     //           console.log("LogicalExpression.AbsorbConstants 5.3 - ", node);
                $.d.ClearMembers(node.parent);
                node.parent.type = LogicalExpressionNode.Type.Constant;
                node.parent.value = x1;
            }
            else {
       //         console.log("LogicalExpression.AbsorbConstants 5.4 - ", node);
                //var grandparent = node.parent.parent;
                if (node.parent.left == node) {
       //             console.log("LogicalExpression.AbsorbConstants 5.5 - ", node);
                    $.d.ProjectOnto(node.parent.right, node.parent, true);
                   // node.right.parent = grandparent;
                }
                if (node.parent.right == node) {
                    //console.log("LogicalExpression.AbsorbConstants 5.6 - ", node, grandparent);
                    $.d.ProjectOnto(node.parent.left, node.parent, true);
                    //.parent = grandparent;
                }
                node.parent.negation ^= x1;
        //        console.log("LogicalExpression.AbsorbConstants 5.7 - ", node);
            }
        });
      //  t.root = plan[plan.length - 1];
   //     var p3 = TreeTraverser.CreateEvaluationPlan(TreeTraverser.PLROrder, t.root, node => node.GetNodes(), true);
   //     console.log("LogicalExpression.AbsorbConstants 3 - ", t.root);
    },

    GetNodes: function (nodeType, getKey) {
        var t = this;
        if (t.root == null) { return []; }
        var plan = TreeTraverser.CreateEvaluationPlan(TreeTraverser.PLROrder, t.root, node => node.GetNodes(), true);
        if (getKey == null) {
            getKey = node => node.name == null ? node.value : node.name;
        }
        var x = {};
        $.d.ProcessArray(plan, node => {
            //console.log("LogicalExpression.GetNodes 1 - ", node);
            if (node.type != nodeType) { return; }
            //console.log("LogicalExpression.GetNodes 2 - ", node);
            x[getKey(node)] = node;

        });
        return $.d.GetObjectMemberValues(x);
    },

    RenderToText: function (options) {
        return LogicalExpression.RenderToText(this, options);
    },

    GetNodesOfType: function (type) {
        var t = this;
        var result = [];
        if (t.root == null) { return result; }
        var plan = TreeTraverser.CreateEvaluationPlan(TreeTraverser.PLROrder, t.root, node => node.GetNodes(), true);
        $.d.ProcessArray(plan, node => { if (node.type == type) { result.push(node); } });
        return result;
    },

    CreateEvaluationPlan: function () {
        var t = this;
        t.evaluationPlan = TreeTraverser.CreateEvaluationPlan(TreeTraverser.LRPOrder, t.root, node => node.GetNodes(), true);
    },

    SetValues: function (values) {
        var t = this;
        $.d.ProcessArray(t.evaluationPlan, node => {
            if (node.type != LogicalExpressionNode.Type.OperandVariable) { return; }
            node.state = values[node.value.name];
        });

    },

    GetVariables: function () {
        var t = this;
        var plan = TreeTraverser.CreateEvaluationPlan(TreeTraverser.LRPOrder, t.root, node => node.GetNodes(), true);
        var variables = {};
        $.d.ProcessArray(plan, node => {
            if (node.type !== LogicalExpressionNode.Type.OperandVariable) { return; }
            variables[node.value.name] = node.value;
        });
        return variables;
    },

    RenderLogicalFunction: function (orderedVariableNames) {
        var t = this;
        if (orderedVariableNames == null) { orderedVariableNames = []; }
        var logicalFunction = new LogicalFunction();
        var variables = t.GetVariables();
        $.d.ProcessArray(orderedVariableNames, name => { logicalFunction.AddVariable(variables[name]); delete variables[name]; });
        $.d.ProcessArray($.d.GetObjectMemberNames(variables), name => { logicalFunction.AddVariable(variables[name]); });

        logicalFunction.variables = variables;

    }

}
LogicalExpression.CreateOperatorNode = function (operator) {
    var node = new LogicalExpressionNode();
    node.value = operator;
    node.type = LogicalExpressionNode.Type.Operator;
    return node;
};
LogicalExpression.CreateOperandVariableNode = function (name) {
    var node = new LogicalExpressionNode();
    node.value = { name: name, subscript: "", superscript: "" };
    node.type = LogicalExpressionNode.Type.OperandVariable;
    return node;
};
LogicalExpression.CreateConstantNode = function (value) {
    var node = new LogicalExpressionNode();
    node.value = { name: value }
    node.state = value;
    node.type = LogicalExpressionNode.Type.Constant;
    return node;
};
LogicalExpression.CreateSeriesReaderNode = function (series, offset) {
    var node = new LogicalExpressionNode();
    node.reader = new BitSeriesReader(series, offset);
    node.value = node.reader;
    node.type = LogicalExpressionNode.Type.BitSeriesReader;
    //console.log("LogicalExpression.CreateSeriesReaderNode - ", series, offset, node);
    return node;
};


LogicalExpression.OperatorTree = new DataTree();

LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.BLOCK.name, LogicalOperator.Operators.BLOCK);
LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.AND.name, LogicalOperator.Operators.AND);
LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.ANB.name, LogicalOperator.Operators.ANB);
LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.EA.name, LogicalOperator.Operators.EA);

LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.NAB.name, LogicalOperator.Operators.NAB);
LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.EB.name, LogicalOperator.Operators.EB);
LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.XOR.name, LogicalOperator.Operators.XOR);
LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.OR.name, LogicalOperator.Operators.OR);

LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.NOR.name, LogicalOperator.Operators.NOR);
LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.XNOR.name, LogicalOperator.Operators.XNOR);
LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.NB.name, LogicalOperator.Operators.NB);
LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.AORNB.name, LogicalOperator.Operators.AORNB);

LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.NA.name, LogicalOperator.Operators.NA);
LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.NAORB.name, LogicalOperator.Operators.NAORB);
LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.NAND.name, LogicalOperator.Operators.NAND);
LogicalExpression.OperatorTree.AddNode(LogicalOperator.Operators.PASS.name, LogicalOperator.Operators.PASS);

LogicalOperator.StandardOperators = [LogicalOperator.Operators.AND, LogicalOperator.Operators.OR];
LogicalOperator.DigitalOperators = [LogicalOperator.Operators.XOR, LogicalOperator.Operators.NAND, LogicalOperator.Operators.NOR, LogicalOperator.Operators.XNOR];
LogicalOperator.ContrivedOperators = [LogicalOperator.Operators.AORNB, LogicalOperator.Operators.NAORB, LogicalOperator.Operators.NAB, LogicalOperator.Operators.ANB];
LogicalOperator.ReductiveOperators = [LogicalOperator.Operators.EA, LogicalOperator.Operators.EB, LogicalOperator.Operators.NA, LogicalOperator.Operators.NB];
LogicalOperator.LossfulOperators = [LogicalOperator.Operators.BLOCK, LogicalOperator.Operators.PASS];
LogicalOperator.GetPrioritizedOperatorNodes = function () {
    var result = [LogicalOperator.Operators.AND, LogicalOperator.Operators.OR, /*standard*/
        LogicalOperator.Operators.XOR, LogicalOperator.Operators.NAND, LogicalOperator.Operators.NOR, LogicalOperator.Operators.XNOR, /*digital*/
        LogicalOperator.Operators.AORNB, LogicalOperator.Operators.NAORB, LogicalOperator.Operators.NAB, LogicalOperator.Operators.ANB, /*contrived*/
        LogicalOperator.Operators.EA, LogicalOperator.Operators.EB, LogicalOperator.Operators.NA, LogicalOperator.Operators.NB, /*reductive*/
        LogicalOperator.Operators.BLOCK, LogicalOperator.Operators.PASS /*lossful*/];
    return result;
};

//LogicalExpression.ParseFreeform = function (freeform) {
//    var expression = new LogicalExpression();
//    expression.root = LogicalExpression.CreateConstantNode(0);
//    return expression;
//};

LogicalExpression.IncrementValues = function (variables, values, options) {
    var o = options || { reverse: false };
    if (o.reverse) {
        for (var i = variables.length -1; i >=0; i--) {
            values[variables[i]] = values[variables[i]] ^ 1;
            if (values[variables[i]]) { return; }
        }
    }
    else {
        for (var i = 0; i < variables.length; i++) {
            values[variables[i]] = values[variables[i]] ^ 1;
            if (values[variables[i]]) { return; }
        }
    }
};

LogicalExpression.RenderToText = function (expression, options) {
    var o = options || { enclose: true };
    if (expression == null || expression.root == null) { return ""; }
    var text = "";
    var textProvider = node => {
        if (node.type == LogicalExpressionNode.Type.Constant) { return node.value; }
        if (node.type == LogicalExpressionNode.Type.Operator) { return node.value.name; }
        if (node.type == LogicalExpressionNode.Type.OperandVariable) { return node.value.name; }
        if (node.type == LogicalExpressionNode.Type.BitSeriesReader) { return node.value.name; }
    };

    var text = "";
    TreeTraverser.RenderExpression(expression.root, node => node.GetNodes(),
        node => { text += "("; }, node => { text += " " +  textProvider(node) + " "; }, node => { text += ")"; },
        { useLeafClosure: false, useOuterClosure: false });
    while (text.indexOf("  ") >= 0) { text = text.replace("  ", " "); }
    text = text.replace("( ", "(");
    text = text.replace(" )", ")");
    return text.trim();
};

//Returns a Math.pow(2, t.variables.length) by t.variables.length two-dimensional array of variable state values.
LogicalExpression.CreateStateSpaceValues = function (length, reverseSets) {
    var states = (1 << length);
    var result = [];
    var values = [];
    var variables = [];
    for (var i = 0; i < length; i++) { variables.push(i); }
    for (var i = 0; i < length; i++) { values.push(0); }
    for (var i = 0; i < states; i++) {
        result.push(values);
        values = $.d.CopyArray(values, v => v);
        LogicalExpression.IncrementValues(variables, values);
    }
    if (reverseSets === true) { $.d.ProcessArray(result, state => state.reverse()); }
    return result;
}

LogicalExpression.ParseToTree = function (source, options) {
    return LogicalExpression.Parse(source, options);
}

//Parses the source string to create an expression tree.
LogicalExpression.Parse = function (source, options) {
    if (options == null) { options = {}; };
    if (options.recursions == null) { options.recursions = {}; }
    if (options.variables == null) { options.variables = {}; }
    //console.log("LogicalExpression.ParseToTree 1 324f34g3erg - ", source, " - ", options);
    var operators = [];
    $.d.ProcessArray($.d.GetObjectMemberNames(LogicalOperator.Unary), name => operators.push(name));
    $.d.ProcessArray($.d.GetObjectMemberNames(LogicalOperator.Binary), name => operators.push(name));
    var tokens = TreeTraverser.TokenizeRenderedExpression(source, "(", ")", { operators: operators, leftBoundaryType: 0, rightBoundaryType: 1, operatorType: 2, otherType: 3 });
    var nodes = [];
    var nodeMaker = () => { nodes.push({ type: 2, content: [] }); return nodes[nodes.length - 1]; };
    console.log("LogicalExpression.ParseToTree 2 - ", source, " - ", tokens, LogicalOperator.Unary);

    for (var i = 0; i < tokens.length; i++) {
        var token = tokens[i];
        if (token.type == 0 || token.type == 1) {
            nodes.push({ type: token.type });
        }
        else {
            var node = i == 0 ? nodeMaker() : nodes[nodes.length - 1];
            if (node.type == 0 || node.type == 1) { node = nodeMaker(); }
            node.content.push(token);
            if (LogicalOperator.Unary[token.value] != null) { token.unaryLeft = true; }
        }
    }
    var framework = TreeTraverser.CreateExpressionTreeFramework(nodes);
    //put this into framework
    console.log("LogicalExpression.Parse - framework 1 -", framework, " - ", nodes);
    //return;
    //while (framework.length == 1 && framework[0].content == null && framework[0].nodes != null) {
    //    framework = framework[0].nodes;
    //}


    var plan = TreeTraverser.CreateEvaluationPlan(TreeTraverser.PLROrder, { nodes: framework, r: "r" }, node => node.nodes == null ? [] : node.nodes, true);
    //var index = 0;
    $.d.ProcessArray(plan, node => {
        node.upProvider = function () {
            if (node.content != null) { return node.up; }
            for (var i = node.nodes.length - 1; i >= 0; i--) {
                if (node.nodes[i].content != null) { return node.nodes[i].up; }
            }
            return node.nodes[0].upProvider();
        }
        if (node.content != null) {
            //Determine which nodes are related to other nodes.
            console.log("LogicalExpression.ParseToTree - openLeft openRight 1.1 - ", node);
            node.openLeft = node.content[0].type == 2 && node.content[0].unaryLeft !== true;
            node.openRight = node.content[node.content.length - 1].type == 2;
            $.d.ProcessArray(node.content, inode => {
                if (inode.type == 2) { inode.left = null; inode.right = null; }
            });
            //Link the operator nodes within a content to the other operator nodes or operands.
            //The top of the tree is on the right to conform to order of operations.
            $.d.ProcessIterator(0, node.content.length - 1, i => {
                if (node.content[i].type != 2) { return; }
                if (node.up == null) { node.up = node.content[i] };
                // + A + B
                if (i - 2 >= 0) { node.content[i].left = node.content[i - 2]; }
                // A + B
                else if (i - 1 >= 0) { node.content[i].left = node.content[i - 1]; }
                // Point right to a resolvable.
                if (i + 1 < node.content.length) { node.content[i].right = node.content[i + 1]; }
            });
        }
    });
    $.d.ProcessArray(plan, node => {
        if (node.nodes == null) { return; }
        for (var i = 0; i < node.nodes.length; i++) {
            if (node.nodes[i].content == null) { continue; }
            if (node.nodes[i].openRight && i < node.nodes.length - 1) {
                node.nodes[i].right = node.nodes[i + 1];
            }
            if (node.nodes[i].openLeft) {
                if (i == 1) {
                    node.nodes[i].left = node.nodes[0];
                }
                else {
                    node.nodes[i].left = node.nodes[i - 2];
                }
            }
        }
    });

    $.d.ProcessArray(plan, node => {
        if (node.openLeft || node.openRight) { node.up.nodes = []; }
        if (node.openLeft) { node.up.left = node.left.upProvider(); }
        if (node.openRight) { node.up.right = node.right.upProvider(); }
    });


    var root = null;//framework[0];
    $.d.ProcessReverseIterator(framework.length - 1, 0, i => {
        //console.log("LogicalExpression.ParseToTree - framework 1.1 - ", framework[i]);
        if (framework[i].content != null) {
            if (framework[i].openRight == true || framework[i].content.length == 1) { root = framework[i].content[framework[i].content.length - 1]; }
            else { root = framework[i].content[framework[i].content.length - 2]; }
            return false;
        }
    });
    
    //console.log("LogicalExpression.ParseToTree - framework 2  5y45yhth4th- ", framework, framework[0], root);
    plan = TreeTraverser.CreateEvaluationPlan(TreeTraverser.PLROrder, root, node => {
        node.GetNodes = () => {
            var n = [];
            if (node.left != null) { n.push(node.left); }
            if (node.right != null) { n.push(node.right); }
            return n;
        }
        return node.GetNodes();
    }, true);

    console.log("LogicalExpression.ParseToTree - framework 2.1 - ", framework);
    console.log("LogicalExpression.ParseToTree - framework 2.2 - ", plan);
    var index = 0;
    $.d.ProcessArray(plan, node => {
        node.index = index++;
        //It is a series index;
        if (node.value.indexOf("[") > 0) {
            var indexPart = node.value.substring(node.value.indexOf("[") + 1, node.value.indexOf("]")).trim();
            var indexName = indexPart;
            var offset = 0;
            if (indexPart.indexOf("-") > 0) {
                indexName = indexPart.substring(0, indexPart.indexOf("-")).trim();
                offset = parseInt(indexPart.substring(indexPart.indexOf("-")));
            }
            else if (indexPart.indexOf("+") > 0) {
                indexName = indexPart.substring(0, indexPart.indexOf("-")).trim();
                offset = parseInt(indexPart.substring(indexPart.indexOf("+")));
            }
            var seriesName = node.value.substring(0, node.value.indexOf("["));
            var series = {};
            if (options.recursions[seriesName] != null) {
                node.value = new BitSeriesReader(options.recursions[seriesName].series, offset);
            }
            else if (options.variables[seriesName] != null) {
                node.value = new BitSeriesReader(options.variables[seriesName], offset);
            }
            node.value.index = indexName;
            node.type = LogicalExpressionNode.Type.BitSeriesReader;
        }
        //It is an operator
        else if (LogicalOperator.Operators[node.value] != null) {
            //node.value = LogicalOperator.Operators[node.value];
            //node.type = LogicalExpressionNode.Type.Operator;
            $.d.ProjectOnto(LogicalExpression.CreateOperatorNode(LogicalOperator.Operators[node.value]), node);
        }
        //It is a constant
        else if (node.value == "0") {
            //node.value = LogicalExpression.CreateConstantNode(0);
            //node.type = LogicalExpressionNode.Type.Constant;
            $.d.ProjectOnto(LogicalExpression.CreateConstantNode(0), node);
        }
        else if (node.value == "1") {
            //node.value = 1;
            //node.type = LogicalExpressionNode.Type.Constant;
            $.d.ProjectOnto(LogicalExpression.CreateConstantNode(1), node);
        }
        //The node must be a variable operand
        else {
            //node.type = LogicalExpressionNode.Type.Operand;
            //console.log("LogicalExpression.ParseToTree - LogicalExpressionNode.Type.OperandVariable 1  423534tg3rgh4erg- ", node);
            $.d.ProjectOnto(LogicalExpression.CreateOperandVariableNode(node.value), node);
            //node.type = LogicalExpressionNode.Type.OperandVariable;
            //node.value = LogicalExpression.CreateOperandVariableNode(node.value);
            //node.name = node.value;
            //node.state = 0;
        }
    });

    var copy = [];
    $.d.ProcessArray(plan, node => {
        //copy.push(node.Clone());
        var cnode = new LogicalExpressionNode();
        cnode.type = node.type;
        cnode.value = node.value;
        cnode.name = node.name;
        copy.push(cnode);
    });
    //console.log("LogicalExpression.ParseToTree - framework 3 reghtrehtr - ", plan, copy);
    $.d.ProcessArray(plan, node => {
        if (node.left != null) { copy[node.index].left = copy[node.left.index]; }
        if (node.right != null) { copy[node.index].right = copy[node.right.index]; }
    });

    var expression = new LogicalExpression();
    //expression.root = root;
    expression.root = copy[0];
    console.log("LogicalExpression.ParseToTree - framework 3 reghtrehtr - ", plan, copy, expression);
    return expression;
};

LogicalExpression.ConvertSetToNatural = function (set) {
    var result = 0;
    var index = 1;
    for (var i = 0; i < set.length; i++) {
        //result += set[i] * Math.pow(2, i);
        result += set[i] * index;
        index = index << 1;
    }
    return result;
}

LogicalExpression.ConvertNumberToSet = function (natural) {
    if (natural == 0) { return [0]; }
    var set = [];
    while (natural > 0) {
        set.push(natural % 2);
        natural = Math.floor(natural / 2);
    }
    return set;
}

function LogicalExpressionEquation() {
    this.expression = new LogicalExpression();
    this.name = "f";
    this.subscript = "";
    this.superscript = "";
}
LogicalExpressionEquation.prototype = {   
    Parse: function (text) {
        //We want to preserve the pointer ot this.
        var equation = LogicalExpressionEquation.Parse(text);
        $.d.ProjectOnto(equation, this, false);
    }
}
LogicalExpressionEquation.Parse = function (text) {
    //console.log("LogicalExpressionEquation.Parse - ", text);
    var equation = new LogicalExpressionEquation();
    var equalsIndex = text.indexOf("=");
    if (equalsIndex < 0) {
        //console.log("LogicalExpressionEquation.Parse 1 - ", text);
        equation.expression = LogicalExpression.Parse(text);
        //console.log("LogicalExpressionEquation.Parse 3 - ", text);
        return equation;
    }
    //console.log("LogicalExpressionEquation.Parse 2 - ", text);
    //console.log("LogicalExpressionEquation.Parse 2.1 - ", text.substring(equalsIndex + 1));

    equation.expression = LogicalExpression.Parse(text.substring(equalsIndex + 1));
    var namePart = text.substring(0, equalsIndex);

    //console.log("LogicalExpressionEquation.Parse 3 - ", namePart, " - ", text.substring(equalsIndex + 1), " - " ,equation);
    var openIndex = namePart.indexOf("[");
    if (openIndex >= 0) { equation.name = namePart.substring(0, openIndex); }
    else {
        equation.name = namePart.trim();
        return equation;
    }
    //console.log("LogicalExpressionEquation.Parse 3.1 - ", openIndex);

    equation.name = namePart.substring(0, openIndex);
    var closeIndex = namePart.indexOf("]");
    if (closeIndex < 0) { return; }
    equation.subscript = namePart.substring(openIndex + 1, closeIndex);
    openIndex = namePart.indexOf(closeIndex + 1, "[");
    if (openIndex < 0) { return; }
    closeIndex = namePart.indexOf(openIndex + 1, "]");
    equation.superscript = namePart.substring(openIndex + 1, closeIndex);
    //console.log("LogicalExpressionEquation.Parse 4 - ", text, " - ", equation);
    return equation;
}


function LogicalFunction() {
    //Variables must be ordered such that the last variable is at the left of the truth table.
    //a b c | v     variables -> [c, b, a]
    //0 0 0 | v1    values -> [v1, v2, v3, v4, v5, v6, v7, v8] - bit order must be maintained. 
    //0 0 1 | v2
    //0 1 0 | v3
    //0 1 1 | v4
    //1 0 0 | v5
    //1 0 1 | v6
    //1 1 0 | v7
    //1 0 1 | v8
    this.variables = []; //LogicalExpressionNode.Type.OperandVariable
    this.values = [];
    this.name = "f";
    this.subscript = "";
    this.superscript = "";
}
LogicalFunction.DisplayType = {};
LogicalFunction.DisplayType.Bit = "LogicalFunction.DisplayType.Bit";
LogicalFunction.DisplayType.Octal = "LogicalFunction.DisplayType.Octal";
LogicalFunction.DisplayType.Hexadecimal = "LogicalFunction.DisplayType.Hexadecimal";
LogicalFunction.BitValue = {};
LogicalFunction.BitValue.Zero = 0;
LogicalFunction.BitValue.One = 1;
LogicalFunction.BitDisplay= {};
LogicalFunction.BitDisplay.Zero = "0";
LogicalFunction.BitDisplay.One = "1";
LogicalFunction.prototype = {

    AddVariable: function (variable) {
        this.variables.push(variable);
    },

    SetVariables: function (variables) {
        this.variables = variables;
    },

    GetVariables: function () {
        //console.log("LogicalFunction.GetVariables  - ", this.variables);
        return this.variables;
    },

    Parse: function (text, options) {
        var logicalFunction = LogicalFunction.Parse(text, options);
        $.d.ProjectOnto(logicalFunction, this, false);
        //console.log("LogicalFunction.Parse x 1 - ", text, options, logicalFunction, this);
    },

    RenderToText: function (options) {
        var o = { display: LogicalFunction.DisplayType.Bit, breakCount: 0, breakValue: "," };
        $.d.ProjectOnto(options, o, false);
        var t = this;
        t.SetupValues();

        var parts = [t.name];
        if (t.subscript == null) { t.subscript = ""; }
        if (t.superscript == null) { t.superscript = ""; }
        t.subscript.trim();
        t.superscript.trim();
        if (t.subscript != "") {
            if (t.superscript == "") { parts.push("[" + t.subscript + "]"); }
            else { parts.push("[" + t.subscript + "][" + t.superscript + "]"); }
        }
        else if (t.superscript != "") { parts.push("[][" + t.superscript + "]"); }

        if (t.variables == null || t.variables.length == 0) {
            parts.push(" = {} []");
            return parts.join("");
        }

        parts.push(" = {");
        $.d.ProcessArray(t.variables, variable => parts.push(variable, ","));
        parts.pop();
        parts.push("}[");

        var index = 0;
        var upper = t.values.length - 1;
        var bits = [];
        $.d.Iterate(0, upper, i => {
            bits.push(t.values[i] == LogicalFunction.BitValue.Zero ? LogicalFunction.BitDisplay.Zero : LogicalFunction.BitDisplay.One);
            if (o.breakCount > 0) {
                index++;
                if (i != upper && index == o.breakCount) {
                    bits.push(o.breakValue);
                    index = 0;
                }
            }
        });
        parts.push(bits.join(""));
        parts.push("]");
        return parts.join("");
    },

    RenderValueSetToText: function (options) {
        var o = { breakCount: 0, breakValue: "," };
        $.d.ProjectOnto(options, o, false);
        var t = this;
        var index = 0;
        var upper = t.values.length - 1;
        var bits = ["["];
        $.d.Iterate(0, upper, i => {
            bits.push(t.values[i] == LogicalFunction.BitValue.Zero ? LogicalFunction.BitDisplay.Zero : LogicalFunction.BitDisplay.One);
            if (o.breakCount > 0) {
                index++;
                if (i != upper && index == o.breakCount) {
                    bits.push(o.breakValue);
                    index = 0;
                }
            }
        });
        bits.push("]");
        return bits.join("");
    },

    SetupValues: function (defaultValue) {
        var t = this;
        var count = (1 << t.variables.length);
        if (defaultValue !== LogicalFunction.BitValue.One) { defaultValue = LogicalFunction.BitValue.Zero; }
        //console.log("LogicalFunction.SetupValues - ", t.variables.length, count, t.values.length, t);
        $.d.ProcessIterator(0, t.values.length - count - 1, i => { t.values.pop() });
        $.d.ProcessIterator(t.values.length, count - 1, i => { t.values.push(defaultValue); });
        //while (t.values.length > count) { t.values.pop(); }
        //while (t.values.length < count) { t.values.push(LogicalFunction.BitValue.Zero); }
    },

    SetValueByAddress: function (address, value) {
        var t = this;
        var index = LogicalExpression.ConvertSetToNatural(address);
        if (index >= t.values.length) { t.SetupValues(); }
        t.values[index] = value;
    },

    SetValue: function (index, value) {
        var t = this;
        if (index >= t.values.length) { t.SetupValues(); }
        t.values[index] = value;
    },

    SetValueByVariables: function (variables, value) {
        var t = this;
        var index = 0;
        $.d.ProcessArray(t.variables, variable => {
            index = index << 1;
            index += variables[variable];
        });
        t.SetValue(index, value);
    },

    GetValue: function (values) {
        var t = this;
        var index = 0;
        $.d.ProcessIterator(0, t.variables.length - 1, i => {
            index = index << 1;
            index += values[t.variables[i]];
        });
        return t.values[index];
    },

    GetValueByVariables: function (variables) {
        var t = this;
        var values = {};
        $.d.ProcessArray(t.variables, variable => { values[variable] = variables[variable]; });
        //console.log("LogicalFunction.GetValueByVariables 1 - ", t.variables, variables, values, t.GetValue(values), t.values, index);
        return t.GetValue(values);
    },

    //variables: The variables factored on the left of the operator.
    //opertaor: The operator on which to factor the function.
    Factor: function (variables, operator, count) {
        var t = this;
        t.SetupValues();
        var result = { hasFactor: false, factors: { left: null, right: null } };
        if (operator == LogicalOperator.Binary.BLOCK || operator == LogicalOperator.Binary.PASS) {
            var checkBit = operator == LogicalOperator.Binary.BLOCK ? LogicalFunction.BitValue.Zero : LogicalFunction.BitValue.One;
            result.hasFactor = $.d.Iterate(0, t.values.length - 1, i => { if (t.values[i] != checkBit) { return false; } });
            if (result.hasFactor) {
                result.factors.left = new LogicalFunction();
                $.d.ProjectOnto({ name: "left", variables: variables }, result.factors.left, false);
                result.factors.SetupValues(LogicalFunction.BitValue.Zero);
                result.factors.right = new LogicalFunction();
                $.d.ProjectOnto({ name: "right", variables: [] }, result.factors.right, false);
                $.d.ProcessArray(t.variables, variable => { if (variables[variable] == null) { result.factors.right.variables.push(variable); } });
                result.factors.SetupValues(LogicalFunction.BitValue.Zero);
            }
            return result;
        }
        if (operator == LogicalOperator.Binary.XOR) {
            result.hasFactor = $.d.Iterate(0, t.values.length - 1, i => { if (t.values[i] != LogicalFunction.BitValue.One) { return false; } });
            if (result.hasFactor) {
                result.factors.left = new LogicalFunction();
                $.d.ProjectOnto({ name: "left", variables: variables }, result.factors.left, false);
                result.factors.SetupValues(LogicalFunction.BitValue.Zero);
                result.factors.right = new LogicalFunction();
                $.d.ProjectOnto({ name: "right", variables: [] }, result.factors.right, false);
                $.d.ProcessArray(t.variables, variable => { if (variables[variable] == null) { result.factors.right.variables.push(variable); } });
                result.factors.SetupValues(LogicalFunction.BitValue.Zero);
            }
            return result;
        }

    }

}
LogicalFunction.Parse = function (text, options) {
    var o = { display: LogicalFunction.DisplayType.Bit };
    $.d.ProjectOnto(options, o, false);
    var logicalFunction = new LogicalFunction;
    logicalFunction.name = "";
    logicalFunction.variables = [];
    logicalFunction.values = [];
    text = text.trim();
    var index = text.indexOf("=");
    var nextIndex = 0;
    //console.log("LogicalFunction.Parse 1 - ", text);
    if (index > 0) {
        var parts  = text.substring(0, index).trim().split("[");
        logicalFunction.name = parts[0];
        if (parts.length >= 2) {
            logicalFunction.subscript = parts[1];
            logicalFunction.subscript = logicalFunction.subscript.replace("]", "");
        }
        if (parts.length >= 3) {
            logicalFunction.superscript = parts[2];
            logicalFunction.superscript = logicalFunction.superscript.replace("]", "");
        }
    }
    index = text.indexOf("{");
    nextIndex = text.indexOf("}", index);
    if (index > 0 && nextIndex > index) {
        var variableText = text.substring(index + 1, nextIndex);
        $.d.ProcessArray(variableText.split(","), variable => {
            variable = variable.trim();
            if (variable != "") { logicalFunction.variables.push(variable); }
        });
    }
    index = text.indexOf("[", nextIndex) + 1;
    nextIndex = text.indexOf("]", index);
    $.d.ProcessIterator(index, nextIndex, i => {
        if (text[i] == LogicalFunction.BitDisplay.Zero) { logicalFunction.values.push(LogicalFunction.BitValue.Zero); return; }
        if (text[i] == LogicalFunction.BitDisplay.One) { logicalFunction.values.push(LogicalFunction.BitValue.One); return;  }
    });
    //console.log("LogicalFunction.Parse 2 - ", logicalFunction.values);
    logicalFunction.SetupValues();
    //console.log("LogicalFunction.Parse 3 - ", logicalFunction, logicalFunction.values);
    return logicalFunction;
}

