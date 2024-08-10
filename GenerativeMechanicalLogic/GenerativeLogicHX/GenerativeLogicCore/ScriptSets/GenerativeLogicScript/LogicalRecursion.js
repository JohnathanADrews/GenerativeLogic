
function OrderedSetDefinition() {
    this.name = "";
    this.memberName = "";
    this.minimumIndex = "";
    this.maximumIndex = "";
    this.variableIndex = "";
}
OrderedSetDefinition.prototype = {
};

function RecursiveFunction() {
    this.series = null; //BitSeries
    this.expression = new LogicalExpression();
    this.isLead = false;
    this.conditions = [];
};
RecursiveFunction.ConditionType = {};
RecursiveFunction.ConditionType.IndexValue = "RecursiveFunction.ConditionType.IndexValue";
RecursiveFunction.ConditionType.IndexBoundary = "RecursiveFunction.ConditionType.IndexBoundary";
RecursiveFunction.prototype = {

    AddIndexValue: function (index, value) {
        this.conditions.push({ type: RecursiveFunction.ConditionType.IndexValue, index: index, value: value });
    },

    AddIndexBoundary: function (index, symbol) {
        this.conditions.push({ type: RecursiveFunction.ConditionType.IndexBoundary, index: index, symbol: symbol });
    }
};


function RecursionSystem() {
    this.lead = null;
    this.variables = [];
    this.functions = [];
    this.exactLength = 0;
    this.abstractLength = null;
    this.indexName = "";
    this.processOrder = [];
    this.setupLength = 0;
    this.injector = null;

    //[k][j][i] := [processOrder][repetition][recursion]
    this.expressionGroups = [];
    this.renderedRepetitions = [];
    this.renderedGroups = [];
    this.simplifiedGroups = [];

    //[k][i] := [processOrder][recursion]
    this.constants = [];
};
RecursionSystem.prototype = {

    //variables: e.g. [ { name: "a", values: [] } ]
    ProcessRepetitions: function (length, repetitions, variables) {
        var t = this;
        //console.log("FrigidRecursionSystem.ProcessRepetitions - system - ", t);
        //console.log("FrigidRecursionSystem.ProcessRepetitions - variables - ", variables[0].values);
        if (t.processOrder == null || t.processOrder.length <= 0) { t.SetupProcessOrder();  }
        if (t.setupLength < length) { t.SetupExpressions(length); }
        var intermediate = {};
        var injector = null;
        var namedVariables = {};
        $.d.ProcessArray(t.variables, variable => { namedVariables[variable.name] = variable; });
        $.d.ProcessArray(variables, variable => {
            intermediate[variable.name] = variable.values;
            if (namedVariables[variable.name].isInjector == true) { injector = intermediate[variable.name]; }
        });

        $.d.ProcessArray(t.processOrder, recursion => { intermediate[recursion.series.name] = []; });

        //console.log("FrigidRecursionSystem.ProcessRepetitions - intermediate, injector - ", intermediate, " - ", injector, " - ", t.processOrder);

        //Process the repetitions one at a time.
        $.d.ProcessIterator(1, repetitions, i => {
            if (i > 1) {
                $.d.ProcessIterator(0, length - 1, j => {
                    injector[j] = intermediate[t.lead.series.name][j];
                });
            }

            //Process each recursion.
            $.d.ProcessArray(t.processOrder, recursion => {
                //Process the expression at each index of the recursion.
                $.d.ProcessIterator(1, length, j => {
                    intermediate[recursion.series.name][j - 1] = 1;
                    if (recursion.constants[j] != null) {
                        intermediate[recursion.series.name][j - 1] = recursion.constants[j];
                        return;
                    }
                    var evaluator = [];

                    $.d.ProcessIterator(0, recursion.evaluator.length - 1, k => {
                        if (recursion.evaluator[k].type === LogicalExpressionNode.Type.Operator) {
                            evaluator[k] = LogicalExpression.CreateOperatorNode(recursion.evaluator[k].value);
                        }
                        else if (recursion.evaluator[k].type === LogicalExpressionNode.Type.BitSeriesReader) {
                            var node = recursion.evaluator[k].value;
                            evaluator[k] = LogicalExpression.CreateConstantNode(intermediate[node.series.name][j + node.offset - 1]);
                        }
                        else if (recursion.evaluator[k].type === LogicalExpressionNode.Type.Constant) {
                            evaluator[k] = recursion.evaluator[k].value;
                        }
                    });
                    $.d.ProcessIterator(0, recursion.evaluator.length - 1, k => {
                        if (recursion.evaluator[k].type === LogicalExpressionNode.Type.Operator) {
                            var node = evaluator[k];
                            var source = recursion.evaluator[k];
                            node.nodes = [];
                            node.nodes.push(evaluator[source.leftId]);
                            node.nodes.push(evaluator[source.rightId]);
                        }
                    });
                    for (var k = evaluator.length - 1; k >= 0; k--) {
                        if (evaluator[k].type === LogicalExpressionNode.Type.Operator) { evaluator[k].Evaluate(); }
                    }
                    intermediate[recursion.series.name][j - 1] = evaluator[0].value;
                });
            });
        });

        var result = [];
        $.d.ProcessArray(t.processOrder, recursion => {
            result.push({ name: recursion.series.name, values: intermediate[recursion.series.name] });
        });
        return intermediate;

    },

    SetupProcessOrder: function () {
        var t = this;
        t.processOrder = [];
        $.d.ProcessArray(t.functions, recursion => { if (!recursion.isLead) { t.processOrder.push(recursion); } });
        t.processOrder.push(t.lead);
    },

    SetupExpressions: function (length) {
        var t = this;

        $.d.ProcessArray(t.functions, recursion => {
            recursion.constants = [];
            var plan = TreeTraverser.CreateEvaluationPlan(TreeTraverser.PLROrder, recursion.expression.root, node => node.GetNodes(), true);
            $.d.ProcessIterator(0, plan.length - 1, i => { plan[i].id = i; });
            $.d.ProcessArray(plan, node => {
                if (node.left != null) { node.leftId = node.left.id; }
                if (node.right != null) { node.rightId = node.right.id; }
            });
            //console.log("FrigidRecursionSystem.SetupExpressions - ", recursion.series.name, " - ", plan, " - ", length);
            recursion.evaluator = plan;

            $.d.ProcessIterator(1, length, i => {
                $.d.ProcessIterator(0, recursion.conditions.length - 1, j => {
                    if (recursion.conditions[j].index == i && recursion.conditions[j].type === RecursiveFunction.ConditionType.IndexValue) {
                        recursion.constants[i] = recursion.conditions[j].value;
                        return false;
                    }
                });
            });
        });
    },

    GetExpressionGroup: function (seriesName, bitPosition, groupIndex) {
        var t = this;
        if (t.expressionGroups.length == 0) {
            t.SetupProcessOrder();
            t.constants = {};
            $.d.ProcessArray(t.processOrder, recursion => {
                t.expressionGroups.push([]);
                t.constants[recursion.series.name] = [];
                $.d.ProcessArray(recursion.conditions, condition => {
                    if (condition.type != RecursiveFunction.ConditionType.IndexValue) { return; }
                    t.constants[recursion.series.name][condition.index] = condition.value;
                });
            });
        }

        if (groupIndex == 0) {
            if (t.expressionGroups[0][0] == null) {
                $.d.ProcessIterator(0, t.processOrder.length - 1, k => {
                    t.expressionGroups[k][0] = [];
                    t.expressionGroups[k][0][0] = null;
                });
            }
            $.d.ProcessIterator(t.expressionGroups[0].length, bitPosition, i => {
                $.d.ProcessIterator(0, t.processOrder.length - 1, k => {
                    var expression = new LogicalExpression();
                    expression.root = LogicalExpression.CreateSeriesReaderNode(t.injector, i);
                    expression.SetSeriesIndex(t.injector, 0, t.constants);
                    t.expressionGroups[k][0][i] = expression;
                });
            });
        }

        $.d.ProcessIterator(t.expressionGroups[0].length, groupIndex, j => {
            $.d.ProcessIterator(0, t.processOrder.length - 1, k => {
                if (t.expressionGroups[k][j] == null) {
                    t.expressionGroups[k][j] = [];
                    t.expressionGroups[k][j][0] = null;
                }
            });
        });

        $.d.ProcessIterator(0, t.processOrder.length - 1, k => {
            var recursion = t.processOrder[k];
            $.d.ProcessIterator(1, groupIndex, j => {
                $.d.ProcessIterator(t.expressionGroups[k][j].length, bitPosition, i => {
                    var copy = recursion.expression.Clone();
                    copy.SetSeriesIndex(recursion.series, bitPosition, t.constants);
                    t.expressionGroups[k][j][i] = copy;
                });
            });
        });
        var index = 0;
        $.d.ProcessIterator(0, t.processOrder.length - 1, i => {
            if (t.processOrder[i].series.name == seriesName) { index = i; return false; }
            return;
        });
   //    console.log("RecursionSystem.GetExpressionGroup - ", t.expressionGroups);
        return t.expressionGroups[index][groupIndex][bitPosition].Clone();
    },

    GetRenderedRepetition: function (seriesName, bitPosition, groupIndex) {
        var t = this;

        var index = 0;
        $.d.ProcessIterator(0, t.processOrder.length - 1, i => {
            if (t.processOrder[i].series.name == seriesName) { index = i; return false; }
            return;
        });
        if (t.renderedRepetitions[index] != null && t.renderedRepetitions[index][groupIndex] != null && t.renderedRepetitions[index][groupIndex][bitPosition] != null) {
            return t.renderedRepetitions[index][groupIndex][bitPosition].Clone();
        }

        //Initialization
        if (t.renderedRepetitions.length == 0) {
            t.SetupProcessOrder();
            $.d.ProcessArray(t.processOrder, recursion => { t.renderedRepetitions.push([]); });
            $.d.ProcessIterator(0, groupIndex - 1, j => { t.renderedRepetitions[k][0] = []; });
        }
        if (t.renderedRepetitions[0][0] == null) {
            $.d.ProcessIterator(0, t.processOrder.length - 1, k => {
                t.renderedRepetitions[k][0] = [null];
                $.d.ProcessIterator(1, bitPosition, i => { t.renderedRepetitions[k][0][i] = t.GetExpressionGroup(seriesName, i, 0); });
            });
        }

        var recursions = {};
        $.d.ProcessIterator(0, t.processOrder.length - 1, i => { recursions[t.processOrder[i].series.memberName] = i; });

        $.d.ProcessIterator(0, groupIndex, j => {
            $.d.ProcessIterator(0, t.processOrder.length - 1, k => { if (t.renderedRepetitions[k][j] == null) { t.renderedRepetitions[k][j] = []; } });
            $.d.ProcessIterator(1, bitPosition, i => {
                $.d.ProcessIterator(0, t.processOrder.length - 1, k => {
                    if (t.renderedRepetitions[k][j][i] != null) { return; }
                    var expression = t.GetExpressionGroup(t.processOrder[k].series.name, i, j);
                    t.renderedRepetitions[k][j][i] = expression;
                    if (j == 0) { return; }
                    var plan = TreeTraverser.CreateEvaluationPlan(TreeTraverser.PLROrder, expression.root, node => { return node.GetNodes(); }, true);
                    $.d.ProcessArray(plan, node => {
                        if (node.type != LogicalExpressionNode.Type.Variable) { return; }
                        if (node.name == t.injector.memberName) {
                            var rendering = t.renderedRepetitions[t.renderedRepetitions.length - 1];
                            var copy = rendering[j - 1][node.index].Clone();
                            var pairs = $.d.GetObjectMemberNameValuePairs(copy.root);
                            $.d.ProcessArray(pairs, pair => { node[pair.name] = pair.value; });
                        }
                        if (recursions[node.name] != null) {
                            var rendering = t.renderedRepetitions[recursions[node.name]];
                            var copy = rendering[j][node.index].Clone();
                            var pairs = $.d.GetObjectMemberNameValuePairs(copy.root);
                            $.d.ProcessArray(pairs, pair => { node[pair.name] = pair.value; });
                        }

                    });
                });
            });
        });

        return t.renderedRepetitions[index][groupIndex][bitPosition].Clone();
    },

    GetRenderedExpression: function (seriesName, bitPosition, groupIndex) {
        var t = this;

        var index = 0;
        $.d.ProcessIterator(0, t.processOrder.length - 1, i => {
            if (t.processOrder[i].series.name == seriesName) { index = i; return false; }
            return;
        });
        if (t.renderedGroups[index] != null && t.renderedGroups[index][groupIndex] != null && t.renderedGroups[index][groupIndex][bitPosition] != null) {
            return t.renderedGroups[index][groupIndex][bitPosition].Clone();
        }

        //Initialization
        if (t.renderedGroups.length == 0) {
            t.SetupProcessOrder();
            $.d.ProcessArray(t.processOrder, recursion => { t.renderedGroups.push([]); });
            $.d.ProcessIterator(0, groupIndex - 1, j => { t.renderedGroups[k][0] = []; });
        }
        if (t.renderedGroups[0][0] == null) {
            $.d.ProcessIterator(0, t.processOrder.length - 1, k => {
                t.renderedGroups[k][0] = [null];
                $.d.ProcessIterator(1, bitPosition, i => { t.renderedGroups[k][0][i] = t.GetExpressionGroup(seriesName, i, 0); });
            });
        }

        var recursions = {};
        $.d.ProcessIterator(0, t.processOrder.length - 1, i => { recursions[t.processOrder[i].series.memberName] = i; });

        $.d.ProcessIterator(0, groupIndex, j => {
            $.d.ProcessIterator(0, t.processOrder.length - 1, k => { if (t.renderedGroups[k][j] == null) { t.renderedGroups[k][j] = []; } });
            $.d.ProcessIterator(1, bitPosition, i => {
                $.d.ProcessIterator(0, t.processOrder.length - 1, k => {
                    if (t.renderedGroups[k][j][i] != null) { return; }
                    var expression = t.GetExpressionGroup(t.processOrder[k].series.name, i, j);
                    t.renderedGroups[k][j][i] = expression;
                    if (j == 0) { return; }
                    if (j > 1) {
                        expression = t.renderedGroups[k][j - 1][i].Clone();
                        t.renderedGroups[k][j][i] = expression;
                    }
                    var plan = TreeTraverser.CreateEvaluationPlan(TreeTraverser.PLROrder, expression.root, node => { return node.GetNodes(); }, true);
                    $.d.ProcessArray(plan, node => {
                        if (node.type != LogicalExpressionNode.Type.Variable) { return; }
                        if (node.name == t.injector.memberName && j > 1) {
                            var rendering = t.renderedGroups[t.renderedGroups.length - 1];
                            var copy = rendering[j - 1][node.index].Clone();
                            var negation = node.negation;
                            $.d.ProjectOnto(copy.root, node);
                            node.negation ^= negation;
                            //var pairs = $.d.GetObjectMemberNameValuePairs(copy.root);
                            //$.d.ProcessArray(pairs, pair => { node[pair.name] = pair.value; });
                        }
                        if (recursions[node.name] != null) {
                            var rendering = t.renderedGroups[recursions[node.name]];
                            var copy = rendering[j][node.index].Clone();
                            var pairs = $.d.GetObjectMemberNameValuePairs(copy.root);
                            $.d.ProcessArray(pairs, pair => { node[pair.name] = pair.value; });
                        }

                    });
                  //  if (k == 1 && j == 1 && i == 1) {
                    //    console.log("RecursionSystem.GetRenderedExpression 123.1 - AbsorbConstants - ", k, j, i, expression.Clone());
                        expression.AbsorbConstants();
                   //     console.log("RecursionSystem.GetRenderedExpression 123.2 - AbsorbConstants - ", k, j, i, expression, t.renderedGroups[k][j][i]);
                   //     console.log("RecursionSystem.GetRenderedExpression 123.3 - AbsorbConstants - ", k, j, i, t.renderedGroups[k][j][i].Clone());
                        //var x = t.renderedGroups[11][j][i][123];
                 //   }
                });
            });
        });
        return t.renderedGroups[index][groupIndex][bitPosition].Clone();
    }

};
//Parses a free-form string.
// e.g. "F!f, R!r, A!a, k@i, f[i] = a[i] XOR b[i] XOR r[i], r[i] = a[i-1] AND b[i-1] OR ( (a[i-1] XOR b[i-1]) AND r[i-1] ), i > 1, r[1] = 1"
RecursionSystem.ParseFreeform = function (freeform) {

    //freeform = "@F!f, R!r, A#a, B#b, k*i, f[i] := { (a[i] XOR b[i]) XOR r[i];  i >= 1 }, r[i] := { (a[i-1] AND b[i-1]) OR ( (a[i-1] XOR b[i-1]) AND r[i-1] ); i > 1; r[1] = 1}";
    // examples
    // "f[i] = a[i] XOR r[i]"
    // "f[i, j] = a[i] XOR r[i-1, j]"
    // "f[i] = a[i] XOR b[i] XOR r[i]"
    // "r[i] = a[i-1] AND b[i-1] OR ( (a[i-1] XOR b[i-1]) AND r[i-1] ), i > 1, r[1] = 1"

    //incrementorLead.code.Parse("@F!f, R!r, A!a, k/i, f[i] := { a[i] XOR b[i] XOR r[i];  %i >= 1 }, r[i] := { a[i-1] AND b[i-1] OR ( (a[i-1] XOR b[i-1]) AND r[i-1] ); i > 1; r[1] = 1}" );

    var system = new RecursionSystem();

    var parts = freeform.split(",");
    var formatted = [];
    $.d.ProcessArray(parts, part => {
        formatted.push(part.trim());
    });

    $.d.ProcessArray(formatted, part => {
        if (part.indexOf("*") > 0) {
            var divisions = part.split("*");
            system.abstractLength = divisions[0].trim();
            system.indexName = divisions[1].trim();
            if (divisions.length >= 3) {
                system.exactLength = parseInt(divisions[2]);
            }
            return false;
        }
    });

    var functionParts = [];
    $.d.ProcessArray(formatted, part => {
        if (part.indexOf("!") > 0) { functionParts.push(part); }
    });

    var recursions = {};
    $.d.ProcessArray(functionParts, part => {
        var divisions = part.split("!");
        var recursion = new RecursiveFunction();
        if (divisions[0].indexOf("@") == 0) {
            system.lead = recursion;
            recursion.isLead = true;
        }
        recursion.series = new BitSeries(divisions[0].replace("@", ""), system.exactLength, system.abstractLength, divisions[1], system.indexName);
        system.functions.push(recursion);
        recursions[recursion.series.memberName.trim()] = recursion;
    });

    var variableParts = [];
    $.d.ProcessArray(formatted, part => {
        if (part.indexOf("#") > 0) { variableParts.push(part); }
    });
    $.d.ProcessArray(variableParts, part => {
        var divisions = part.split("#");
        var isInjector = false;
        if (divisions[0].indexOf("@") >= 0) {
            divisions[0] = divisions[0].replace("@", "");
            isInjector = true;
        }
        var series = new BitSeries(divisions[0], system.exactLength, system.abstractLength, divisions[1], system.indexName);
        series.isInjector = isInjector;
        system.variables.push(series);
        if (series.isInjector === true) { system.injector = series; }
    });

    var definitionParts = [];
    $.d.ProcessArray(parts, part => {
        if (part.indexOf(":=") < 0) { return }
        var divisions = part.split(":=");
        var recursionPart = divisions[0];
        var recursionName = recursionPart.substring(0, recursionPart.indexOf("[")).trim();
        //var recursionIndex = recursionPart.substring(recursionPart.indexOf("[") + 1, recursionPart.indexOf("]"));
        divisions = divisions[1].replace("{", "").replace("}", "").split(";");
        var variables = {};
        $.d.ProcessArray(system.variables, variable => { variables[variable.memberName] = variable });

        //console.log("RecursionSystem.ParseFreeform  1s - ", divisions[0], " - ", { recursions: recursions, variables: variables });
        recursions[recursionName].expression = LogicalExpression.ParseToTree(divisions[0], { recursions: recursions, variables: variables });
        //recursions[recursionName].expression = LogicalExpression.ParseFreeform(divisions[0]);
        //console.log("RecursionSystem.ParseFreeform  1s - ", divisions[0], " - ", { recursions: recursions, variables: variables });

        //for (var i = 1; i < divisions.length; i++) {
        $.d.ProcessIterator(1, divisions.length - 1, i => {
            if (divisions[i].indexOf("[") < 0) { //index value
                var symbols = ["<=", ">=", "!=", "<", ">", "="];
                $.d.ProcessArray(symbols, symbol => {
                    var symbolIndex = divisions[i].indexOf(symbol);
                    if (symbolIndex >= 0) {
                        var symbol = divisions[i].substring(symbolIndex, symbolIndex + symbol.length);
                        var rangeIndex = parseInt(divisions[i].substring(symbolIndex + symbol.length + 1));
                        recursions[recursionName].AddIndexBoundary(rangeIndex, symbol);
                        return false;
                    }
                });
            }
            else { //index boundary
                var indexName = parseInt(divisions[i].substring(divisions[i].indexOf("[") + 1, divisions[i].indexOf("]")).trim());
                var indexvalue = parseInt(divisions[i].substring(divisions[i].indexOf("=") + 1).trim());
                recursions[recursionName].AddIndexValue(indexName, indexvalue);
            }
        });
    });
    //console.log("RecursionSystem.ParseFreeform - system - ", system);

    //$.d.ProcessArray(system.functions, recursion => {        
    //    recursion.expression.root = LogicalExpression.CreateConstantNode(0);
    //});

    return system;

    //AddIndexValue: function (index, value) {
    //    this.conditions.push({ type: RecursiveFunction.ConditionType.IndexValue, index: index, value: value });
    //},

    //AddIndexBoundary: function (index, symbol) {
    //    this.conditions.push({ type: RecursiveFunction.ConditionType.IndexBoundary, index: index, symbol: symbol });
    //}
};
