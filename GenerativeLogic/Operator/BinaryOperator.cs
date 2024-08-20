using GenerativeLogic.Foundation;
using GenerativeLogic.Operand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenerativeLogic.Operator
{
    /// <summary>
    /// Represents all of the binary-valued binary operators.
    /// </summary>
    public class BinaryOperator : ComplexEnumeration<BinaryOperator>, IOperator
    {

        /// <summary>
        /// The unique identifier of the binary operator.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The definition of the operator.
        /// </summary>
        public Func<bool, bool, bool> OperatorFunction { get; private set; }

        /// <summary>
        /// true iff this operator is commutative.
        /// </summary>
        public bool IsCommutative { get; private set; }

        /// <summary>
        /// true if this operator is associative with itself.
        /// </summary>
        public bool IsAssociative { get; private set; }

        /// <summary>
        /// returns  ( !(a [tau] b) [and] !(b [tau] c) ) [NAORB] !(a [tau] c), where tau is this operator.
        /// </summary>
        public bool IsZeroTransitive { get; private set; }

        /// <summary>
        /// returns  ((a [tau] b) [and] (b [tau] c)) [NAORB] (a [tau] c), where tau is this operator.
        /// </summary>
        public bool IsOneTransitive { get; private set; }

        /// <summary>
        /// The operators that this operator is both left-associative and right-associative with.  
        /// tau is left-associative id (a [tau] (b [sigma] c)) == ((a [tau] b) [sigma] c)
        /// tau is right-associative if ((a [sigma] b) [tau] c) == (a [sigma] (b [tau] c))
        /// </summary>
        public IEnumerable<BinaryOperator> AssociativeWith
        {
            get
            {
                InitializeRelationships();
                return operatorAssociates[this];
            }
        }

        /// <summary>
        /// The operators that this operator is left-associative with.
        /// </summary>
        public IEnumerable<BinaryOperator> LeftAssociativeWith
        {
            get
            {
                InitializeRelationships();
                return operatorLeftAssociates[this];
            }
        }

        /// <summary>
        /// The operators that this operator is right-associative with.
        /// </summary>
        public IEnumerable<BinaryOperator> RightAssociativeWith
        {
            get
            {
                InitializeRelationships();
                return operatorRightAssociates[this];
            }
        }

        /// <summary>
        /// The operators that this operator distributes both on the left and the right.
        /// tau distributes sigma on the left if (a [sigma] (b [tau] c)) == (a [sigma] b) [tau] (a [sigma] c).
        /// tau distributes sigma on the right if ((b [tau] c) [sigma] a) == (b [sigma] a) [tau] (c [sigma] a).
        /// </summary>
        public IEnumerable<BinaryOperator> Distributes
        {
            get
            {
                InitializeRelationships();
                return operatorDistributes[this];
            }
        }

        /// <summary>
        /// The operators that this operator distributs on the left.
        /// </summary>
        public IEnumerable<BinaryOperator> LeftDistributes
        {
            get
            {
                InitializeRelationships();
                return operatorLeftDistributes[this];
            }
        }

        /// <summary>
        /// The operators that this operator distributes on the right.
        /// </summary>
        public IEnumerable<BinaryOperator> RightDistributes
        {
            get
            {
                InitializeRelationships();
                return operatorRightDistributes[this];
            }
        }

        /// <summary>
        /// The operators that this operator is distributable by both on the left and the right.  This is another view of the Distributes property.
        /// tau is distributable by sigma on the left if (a [tau] (b [sigma] c)) == (a [tau] b) [sigma] (a [tau] c).
        /// tau distributes sigma on the right if ((b [sigma] c) [tau] a) == (b [tau] a) [sigma] (c [tau] a).
        /// </summary>
        public IEnumerable<BinaryOperator> DistributedBy
        {
            get
            {
                InitializeRelationships();
                return operatorDistributedBy[this];
            }
        }

        /// <summary>
        /// The operators that distribute this operator on the left.
        /// </summary>
        public IEnumerable<BinaryOperator> LeftDistributedBy
        {
            get
            {
                InitializeRelationships();
                return operatorLeftDistributedBy[this];
            }
        }

        /// <summary>
        /// The operators that distribute this operator on the right.
        /// </summary>
        public IEnumerable<BinaryOperator> RightDistributedBy
        {
            get
            {
                InitializeRelationships();
                return operatorRightDistributedBy[this];
            }
        }


        private static bool relationshipsInitialized = false;

        private static Mutex relationshipInitializationMutex = new Mutex();

        private static Dictionary<BinaryOperator, List<BinaryOperator>> operatorAssociates = new Dictionary<BinaryOperator, List<BinaryOperator>>();

        private static Dictionary<BinaryOperator, List<BinaryOperator>> operatorLeftAssociates = new Dictionary<BinaryOperator, List<BinaryOperator>>();

        private static Dictionary<BinaryOperator, List<BinaryOperator>> operatorRightAssociates = new Dictionary<BinaryOperator, List<BinaryOperator>>();

        private static Dictionary<BinaryOperator, List<BinaryOperator>> operatorDistributes = new Dictionary<BinaryOperator, List<BinaryOperator>>();

        private static Dictionary<BinaryOperator, List<BinaryOperator>> operatorLeftDistributes = new Dictionary<BinaryOperator, List<BinaryOperator>>();

        private static Dictionary<BinaryOperator, List<BinaryOperator>> operatorRightDistributes = new Dictionary<BinaryOperator, List<BinaryOperator>>();

        private static Dictionary<BinaryOperator, List<BinaryOperator>> operatorDistributedBy = new Dictionary<BinaryOperator, List<BinaryOperator>>();

        private static Dictionary<BinaryOperator, List<BinaryOperator>> operatorLeftDistributedBy = new Dictionary<BinaryOperator, List<BinaryOperator>>();

        private static Dictionary<BinaryOperator, List<BinaryOperator>> operatorRightDistributedBy = new Dictionary<BinaryOperator, List<BinaryOperator>>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">The unique identifier of the binary operator.</param>
        /// <param name="operatorDefinition"></param>
        private BinaryOperator(int id, Func<bool, bool, bool> operatorFunction)
        {
            this.Id = id;
            this.OperatorFunction = operatorFunction;
            IdentifyRelationships();
        }

        private void IdentifyRelationships()
        {
            DetermineCommutative();
            DetermineAssociative();
        }

        private void DetermineCommutative()
        {
            LogicalExpression configurationOne = new LogicalExpression();
            LogicalExpression configurationTwo = new LogicalExpression();
            LogicalExpression.ExpressionNode node;
            VariableOperand operandA = new VariableOperand("a");
            VariableOperand operandB = new VariableOperand("b");
            OperandSet operandSet = new OperandSet();
            operandSet.AddOperand(operandA);
            operandSet.AddOperand(operandB);

            node = new LogicalExpression.ExpressionNode(0, null, this, isRoot: true);
            configurationOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, operandA, LogicalExpression.ExpressionParameter.Parameter1);
            configurationOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 0, operandB, LogicalExpression.ExpressionParameter.Parameter2);
            configurationOne.AddNode(node);

            node = new LogicalExpression.ExpressionNode(0, null, this, isRoot: true);
            configurationTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, operandA, LogicalExpression.ExpressionParameter.Parameter2);
            configurationTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 0, operandB, LogicalExpression.ExpressionParameter.Parameter1);
            configurationTwo.AddNode(node);

            bool result;
            this.IsCommutative = true;
            operandSet.ExecuteOnAllValues((index) =>
            {
                result = (configurationOne.Evaluate() == configurationTwo.Evaluate());
                this.IsCommutative = this.IsCommutative && result;
                return result;
            });

        }

        private void DetermineAssociative()
        {
            LogicalExpression configurationOne = new LogicalExpression();
            LogicalExpression configurationTwo = new LogicalExpression();
            LogicalExpression.ExpressionNode node;
            VariableOperand operandA = new VariableOperand("a");
            VariableOperand operandB = new VariableOperand("b");
            VariableOperand operandC = new VariableOperand("c");
            OperandSet operandSet = new OperandSet();
            operandSet.AddOperand(operandA);
            operandSet.AddOperand(operandB);
            operandSet.AddOperand(operandC);

            // (a [tau] (b [tau] c))
            node = new LogicalExpression.ExpressionNode(0, null, this, isRoot: true);
            configurationOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, operandA, LogicalExpression.ExpressionParameter.Parameter1);
            configurationOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 0, this, LogicalExpression.ExpressionParameter.Parameter2);
            configurationOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(3, 2, operandB, LogicalExpression.ExpressionParameter.Parameter1);
            configurationOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(4, 2, operandC, LogicalExpression.ExpressionParameter.Parameter2);
            configurationOne.AddNode(node);

            // ((a [tau] b) [tau] c)
            node = new LogicalExpression.ExpressionNode(0, null, this, isRoot: true);
            configurationTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, this, LogicalExpression.ExpressionParameter.Parameter1);
            configurationTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 1, operandA, LogicalExpression.ExpressionParameter.Parameter1);
            configurationTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(3, 1, operandB, LogicalExpression.ExpressionParameter.Parameter2);
            configurationTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(4, 0, operandC, LogicalExpression.ExpressionParameter.Parameter2);
            configurationTwo.AddNode(node);

            bool result;
            this.IsAssociative = true;
            operandSet.ExecuteOnAllValues((index) =>
            {
                result = (configurationOne.Evaluate() == configurationTwo.Evaluate());
                this.IsAssociative = this.IsAssociative && result;
                return result;
            });
        }

        private void DetermineTransitive()
        {
            /// returns  ((a [tau] b) [and] (b [tau] c)) [NAORB] (a [tau] c), where tau is this operator.
            LogicalExpression zeroTransitiveExpression = new LogicalExpression();
            LogicalExpression configurationTwo = new LogicalExpression();
            LogicalExpression.ExpressionNode node;
            VariableOperand operandA = new VariableOperand("a");
            VariableOperand operandB = new VariableOperand("b");
            VariableOperand operandC = new VariableOperand("c");
            OperandSet operandSet = new OperandSet();
            operandSet.AddOperand(operandA);
            operandSet.AddOperand(operandB);
            operandSet.AddOperand(operandC);

            // (a [tau] (b [tau] c))
            node = new LogicalExpression.ExpressionNode(0, null, BinaryOperator.NAORB, isRoot: true);
            zeroTransitiveExpression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, BinaryOperator.AND, LogicalExpression.ExpressionParameter.Parameter1);
            zeroTransitiveExpression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 0, this, LogicalExpression.ExpressionParameter.Parameter2);
            zeroTransitiveExpression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(3, 1, this, LogicalExpression.ExpressionParameter.Parameter1);
            zeroTransitiveExpression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(4, 1, this, LogicalExpression.ExpressionParameter.Parameter2);
            zeroTransitiveExpression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(5, 3, operandA, LogicalExpression.ExpressionParameter.Parameter1);
            zeroTransitiveExpression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(6, 3, operandB, LogicalExpression.ExpressionParameter.Parameter2);
            zeroTransitiveExpression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(7, 4, operandB, LogicalExpression.ExpressionParameter.Parameter1);
            zeroTransitiveExpression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(8, 4, operandC, LogicalExpression.ExpressionParameter.Parameter2);
            zeroTransitiveExpression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(9, 2, operandA, LogicalExpression.ExpressionParameter.Parameter1);
            zeroTransitiveExpression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(10, 2, operandC, LogicalExpression.ExpressionParameter.Parameter2);
            zeroTransitiveExpression.AddNode(node);

            bool result;
            this.IsZeroTransitive = true;
            this.IsOneTransitive = true;
            operandSet.ExecuteOnAllValues((index) =>
            {
                result = (zeroTransitiveExpression.Evaluate() == configurationTwo.Evaluate());
                this.IsAssociative = this.IsAssociative && result;
                return result;
            });
        }

        private static void InitializeRelationships()
        {
            if (relationshipsInitialized)
            {
                return;
            }

            relationshipInitializationMutex.WaitOne();

            if (relationshipsInitialized)
            {
                return;
            }

            InitializeAssociativity();
            InitializeDistributivity();

            relationshipsInitialized = true;
            relationshipInitializationMutex.ReleaseMutex();

        }

        private static void InitializeAssociativity()
        {
            //Initialize the dictionaries.
            foreach (BinaryOperator binaryOperator in BinaryOperator.Enumeration)
            {
                operatorAssociates.Add(binaryOperator, new List<BinaryOperator>());
                operatorLeftAssociates.Add(binaryOperator, new List<BinaryOperator>());
                operatorRightAssociates.Add(binaryOperator, new List<BinaryOperator>());
            }

            //Compute the associativity.

            LogicalExpression configurationOne = new LogicalExpression();
            LogicalExpression configurationTwo = new LogicalExpression();
            LogicalExpression.ExpressionNode node;
            VariableOperand operandA = new VariableOperand("a");
            VariableOperand operandB = new VariableOperand("b");
            VariableOperand operandC = new VariableOperand("c");
            OperandSet operandSet = new OperandSet();
            operandSet.AddOperand(operandA);
            operandSet.AddOperand(operandB);
            operandSet.AddOperand(operandC);
            VariableOperator variableOperatorLeft = new VariableOperator();
            VariableOperator variableOperatorRight = new VariableOperator();

            // (a [tau] (b [sigma] c))
            node = new LogicalExpression.ExpressionNode(0, null, variableOperatorLeft, isRoot: true);
            configurationOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, operandA, LogicalExpression.ExpressionParameter.Parameter1);
            configurationOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 0, variableOperatorRight, LogicalExpression.ExpressionParameter.Parameter2);
            configurationOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(3, 2, operandB, LogicalExpression.ExpressionParameter.Parameter1);
            configurationOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(4, 2, operandC, LogicalExpression.ExpressionParameter.Parameter2);
            configurationOne.AddNode(node);

            // ((a [tau] b) [sigma] c)
            node = new LogicalExpression.ExpressionNode(0, null, variableOperatorLeft, isRoot: true);
            configurationTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, variableOperatorRight, LogicalExpression.ExpressionParameter.Parameter1);
            configurationTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 1, operandA, LogicalExpression.ExpressionParameter.Parameter1);
            configurationTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(3, 1, operandB, LogicalExpression.ExpressionParameter.Parameter2);
            configurationTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(4, 0, operandC, LogicalExpression.ExpressionParameter.Parameter2);
            configurationTwo.AddNode(node);

            bool isAssociative;
            bool result;
            foreach (BinaryOperator binaryOperatorLeft in BinaryOperator.Enumeration)
            {
                foreach (BinaryOperator binaryOperatorRight in BinaryOperator.Enumeration)
                {
                    variableOperatorLeft.Operator = binaryOperatorLeft;
                    variableOperatorRight.Operator = binaryOperatorRight;
                    isAssociative = true;
                    operandSet.ExecuteOnAllValues((index) =>
                    {
                        result = (configurationOne.Evaluate() == configurationTwo.Evaluate());
                        isAssociative = isAssociative && result;
                        return result;
                    });
                    if (isAssociative)
                    {
                        operatorLeftAssociates[binaryOperatorLeft].Add(binaryOperatorRight);
                        operatorRightAssociates[binaryOperatorRight].Add(binaryOperatorLeft);
                    }
                }
            }

            foreach (BinaryOperator binaryOperator1 in BinaryOperator.Enumeration)
            {
                foreach (BinaryOperator binaryOperator2 in BinaryOperator.Enumeration)
                {
                    if (operatorLeftAssociates[binaryOperator1].Contains(binaryOperator2) && operatorRightAssociates[binaryOperator1].Contains(binaryOperator2))
                    {
                        operatorAssociates[binaryOperator1].Add(binaryOperator2);
                    }
                }
            }

        }

        private static void InitializeDistributivity()
        {
            //Initialize the dictionaries.
            foreach (BinaryOperator binaryOperator in BinaryOperator.Enumeration)
            {
                operatorDistributes.Add(binaryOperator, new List<BinaryOperator>());
                operatorLeftDistributes.Add(binaryOperator, new List<BinaryOperator>());
                operatorRightDistributes.Add(binaryOperator, new List<BinaryOperator>());
                operatorDistributedBy.Add(binaryOperator, new List<BinaryOperator>());
                operatorLeftDistributedBy.Add(binaryOperator, new List<BinaryOperator>());
                operatorRightDistributedBy.Add(binaryOperator, new List<BinaryOperator>());
            }

            //Compute the distributivity.

            LogicalExpression configurationLeftDisributiveOne = new LogicalExpression();
            LogicalExpression configurationLeftDisributiveTwo = new LogicalExpression();
            LogicalExpression configurationRightDisributiveOne = new LogicalExpression();
            LogicalExpression configurationRightDisributiveTwo = new LogicalExpression();
            LogicalExpression.ExpressionNode node;
            VariableOperand operandA = new VariableOperand("a");
            VariableOperand operandB = new VariableOperand("b");
            VariableOperand operandC = new VariableOperand("c");
            OperandSet operandSet = new OperandSet();
            operandSet.AddOperand(operandA);
            operandSet.AddOperand(operandB);
            operandSet.AddOperand(operandC);
            VariableOperator variableOperatorTau = new VariableOperator();
            VariableOperator variableOperatorSigma = new VariableOperator();

            //Left Distributive Form One - (a [tau] (b [sigma] c))
            node = new LogicalExpression.ExpressionNode(0, null, variableOperatorTau, isRoot: true);
            configurationLeftDisributiveOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, operandA, LogicalExpression.ExpressionParameter.Parameter1);
            configurationLeftDisributiveOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 0, variableOperatorSigma, LogicalExpression.ExpressionParameter.Parameter2);
            configurationLeftDisributiveOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(3, 2, operandB, LogicalExpression.ExpressionParameter.Parameter1);
            configurationLeftDisributiveOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(4, 2, operandC, LogicalExpression.ExpressionParameter.Parameter2);
            configurationLeftDisributiveOne.AddNode(node);

            //Left Distributive Form Two - ((a [tau] b) [sigma] (a [tau] c))
            node = new LogicalExpression.ExpressionNode(0, null, variableOperatorSigma, isRoot: true);
            configurationLeftDisributiveTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, variableOperatorTau, LogicalExpression.ExpressionParameter.Parameter1);
            configurationLeftDisributiveTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 0, variableOperatorTau, LogicalExpression.ExpressionParameter.Parameter2);
            configurationLeftDisributiveTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(3, 1, operandA, LogicalExpression.ExpressionParameter.Parameter1);
            configurationLeftDisributiveTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(4, 1, operandB, LogicalExpression.ExpressionParameter.Parameter2);
            configurationLeftDisributiveTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(5, 2, operandA, LogicalExpression.ExpressionParameter.Parameter1);
            configurationLeftDisributiveTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(6, 2, operandC, LogicalExpression.ExpressionParameter.Parameter2);
            configurationLeftDisributiveTwo.AddNode(node);

            //Right Distributive Form One - ((b [sigma] c) [tau] a)
            node = new LogicalExpression.ExpressionNode(0, null, variableOperatorTau, isRoot: true);
            configurationRightDisributiveOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, variableOperatorSigma, LogicalExpression.ExpressionParameter.Parameter1);
            configurationRightDisributiveOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 0, operandA, LogicalExpression.ExpressionParameter.Parameter2);
            configurationRightDisributiveOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(3, 1, operandB, LogicalExpression.ExpressionParameter.Parameter1);
            configurationRightDisributiveOne.AddNode(node);
            node = new LogicalExpression.ExpressionNode(4, 1, operandC, LogicalExpression.ExpressionParameter.Parameter2);
            configurationRightDisributiveOne.AddNode(node);

            //Right Distributive Form Two - ((b [tau] a) [sigma] (c [tau] a))
            node = new LogicalExpression.ExpressionNode(0, null, variableOperatorSigma, isRoot: true);
            configurationRightDisributiveTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, variableOperatorTau, LogicalExpression.ExpressionParameter.Parameter1);
            configurationRightDisributiveTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 0, variableOperatorTau, LogicalExpression.ExpressionParameter.Parameter2);
            configurationRightDisributiveTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(3, 1, operandB, LogicalExpression.ExpressionParameter.Parameter1);
            configurationRightDisributiveTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(4, 1, operandA, LogicalExpression.ExpressionParameter.Parameter2);
            configurationRightDisributiveTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(5, 2, operandC, LogicalExpression.ExpressionParameter.Parameter1);
            configurationRightDisributiveTwo.AddNode(node);
            node = new LogicalExpression.ExpressionNode(6, 2, operandA, LogicalExpression.ExpressionParameter.Parameter2);
            configurationRightDisributiveTwo.AddNode(node);

            bool isDistributive;
            bool result;
            foreach (BinaryOperator binaryOperatorTau in BinaryOperator.Enumeration)
            {
                foreach (BinaryOperator binaryOperatorSigma in BinaryOperator.Enumeration)
                {
                    variableOperatorTau.Operator = binaryOperatorTau;
                    variableOperatorSigma.Operator = binaryOperatorSigma;
                    //Left distributive.
                    isDistributive = true;
                    operandSet.ExecuteOnAllValues((index) =>
                    {
                        result = (configurationLeftDisributiveOne.Evaluate() == configurationLeftDisributiveTwo.Evaluate());
                        isDistributive = isDistributive && result;
                        return result;
                    });
                    if (isDistributive)
                    {
                        operatorLeftDistributes[binaryOperatorTau].Add(binaryOperatorSigma);
                        operatorLeftDistributedBy[binaryOperatorSigma].Add(binaryOperatorTau);
                    }
                    //Right distributive
                    isDistributive = true;
                    operandSet.ExecuteOnAllValues((index) =>
                    {
                        result = (configurationRightDisributiveOne.Evaluate() == configurationRightDisributiveTwo.Evaluate());
                        isDistributive = isDistributive && result;
                        return result;
                    });
                    if (isDistributive)
                    {
                        operatorRightDistributes[binaryOperatorTau].Add(binaryOperatorSigma);
                        operatorRightDistributedBy[binaryOperatorSigma].Add(binaryOperatorTau);
                    }
                }
            }

            foreach (BinaryOperator binaryOperator1 in BinaryOperator.Enumeration)
            {
                foreach (BinaryOperator binaryOperator2 in BinaryOperator.Enumeration)
                {
                    if (operatorLeftDistributes[binaryOperator1].Contains(binaryOperator2) && operatorRightDistributes[binaryOperator1].Contains(binaryOperator2))
                    {
                        operatorDistributes[binaryOperator1].Add(binaryOperator2);
                    }
                    if (operatorLeftDistributedBy[binaryOperator1].Contains(binaryOperator2) && operatorRightDistributedBy[binaryOperator1].Contains(binaryOperator2))
                    {
                        operatorDistributedBy[binaryOperator1].Add(binaryOperator2);
                    }
                }
            }
        }

        public bool IsAssociateOf(BinaryOperator associate)
        {
            InitializeRelationships();
            if (operatorAssociates[this].Contains(associate))
            {
                return true;
            }
            return false;
        }

        public bool IsLeftAssociateOf(BinaryOperator associate)
        {
            InitializeRelationships();
            if (operatorLeftAssociates[this].Contains(associate))
            {
                return true;
            }
            return false;
        }

        public bool IsRightAssociateOf(BinaryOperator associate)
        {
            InitializeRelationships();
            if (operatorRightAssociates[this].Contains(associate))
            {
                return true;
            }
            return false;
        }

        public bool IsDistributorOf(BinaryOperator distributee)
        {
            InitializeRelationships();
            if (operatorDistributes[this].Contains(distributee))
            {
                return true;
            }
            return false;
        }

        public bool IsLeftDistributorOf(BinaryOperator distributee)
        {
            InitializeRelationships();
            if (operatorLeftDistributes[this].Contains(distributee))
            {
                return true;
            }
            return false;
        }

        public bool IsRightDistributorOf(BinaryOperator distributee)
        {
            InitializeRelationships();
            if (operatorRightDistributes[this].Contains(distributee))
            {
                return true;
            }
            return false;
        }

        public bool IsDistributeeOf(BinaryOperator distributor)
        {
            InitializeRelationships();
            if (operatorDistributedBy[this].Contains(distributor))
            {
                return true;
            }
            return false;
        }

        public bool IsLeftDistributeeOf(BinaryOperator distributor)
        {
            InitializeRelationships();
            if (operatorLeftDistributedBy[this].Contains(distributor))
            {
                return true;
            }
            return false;
        }

        public bool IsRightDistributeeOf(BinaryOperator distributor)
        {
            InitializeRelationships();
            if (operatorRightDistributedBy[this].Contains(distributor))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the operator that has the provided output values.
        /// </summary>
        /// <param name="value0">The first output value.</param>
        /// <param name="value1">The second output value.</param>
        /// <param name="value2">The third output value.</param>
        /// <param name="value3">The fourth output value.</param>
        /// <returns>The operator that has the provided output values.</returns>
        public static BinaryOperator GetOperatorFromSignature(bool value0, bool value1, bool value2, bool value3)
        {
            if (value0)
            {
                if (value1)
                {
                    if (value2)
                    {
                        if (value3)
                        {
                            return BinaryOperator.PASS;
                        }
                        else
                        {
                            return BinaryOperator.NAND;
                        }
                    }
                    else
                    {
                        if (value3)
                        {
                            return BinaryOperator.NAORB;
                        }
                        else
                        {
                            return BinaryOperator.NA;
                        }
                    }
                }
                else
                {
                    if (value2)
                    {
                        if (value3)
                        {
                            return BinaryOperator.AORNB;
                        }
                        else
                        {
                            return BinaryOperator.NB;
                        }
                    }
                    else
                    {
                        if (value3)
                        {
                            return BinaryOperator.XNOR;
                        }
                        else
                        {
                            return BinaryOperator.NOR;
                        }
                    }
                }
            }
            else
            {
                if (value1)
                {
                    if (value2)
                    {
                        if (value3)
                        {
                            return BinaryOperator.OR;
                        }
                        else
                        {
                            return BinaryOperator.XOR;
                        }
                    }
                    else
                    {
                        if (value3)
                        {
                            return BinaryOperator.B;
                        }
                        else
                        {
                            return BinaryOperator.NAB;
                        }
                    }
                }
                else
                {
                    if (value2)
                    {
                        if (value3)
                        {
                            return BinaryOperator.A;
                        }
                        else
                        {
                            return BinaryOperator.ANB;
                        }
                    }
                    else
                    {
                        if (value3)
                        {
                            return BinaryOperator.AND;
                        }
                        else
                        {
                            return BinaryOperator.BLOCK;
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return this.EnumerationName;
        }

        #region IOperator

        public bool Evaluate(bool a, bool b)
        {
            return this.OperatorFunction(a, b);
        }

        #endregion

        /// Truth Table
        /// a b f
        /// 0 0 0
        /// 0 1 0
        /// 1 0 0
        /// 1 1 0
        public static BinaryOperator BLOCK = new BinaryOperator(1, (bool a, bool b) => { return false; });
        /// Truth Table
        /// a b f
        /// 0 0 0
        /// 0 1 0
        /// 1 0 0
        /// 1 1 1
        public static BinaryOperator AND = new BinaryOperator(2, (bool a, bool b) => { return a && b; });
        /// Truth Table
        /// a b f
        /// 0 0 0
        /// 0 1 0
        /// 1 0 1
        /// 1 1 0
        public static BinaryOperator ANB = new BinaryOperator(3, (bool a, bool b) => { return a && !b; });
        /// Truth Table
        /// a b F
        /// 0 0 0
        /// 0 1 0
        /// 1 0 1
        /// 1 1 1
        public static BinaryOperator A = new BinaryOperator(4, (bool a, bool b) => { return a; });
        /// Truth Table
        /// a b f
        /// 0 0 0
        /// 0 1 1
        /// 1 0 0
        /// 1 1 0
        public static BinaryOperator NAB = new BinaryOperator(5, (bool a, bool b) => { return !a && b; });
        /// Truth Table
        /// a b f
        /// 0 0 0
        /// 0 1 1
        /// 1 0 0
        /// 1 1 1
        public static BinaryOperator B = new BinaryOperator(6, (bool a, bool b) => { return b; });
        /// Truth Table
        /// a b f
        /// 0 0 0
        /// 0 1 1
        /// 1 0 1
        /// 1 1 0
        public static BinaryOperator XOR = new BinaryOperator(7, (bool a, bool b) => { return a ^ b; });
        /// Truth Table
        /// a b f
        /// 0 0 0
        /// 0 1 1
        /// 1 0 1
        /// 1 1 1
        public static BinaryOperator OR = new BinaryOperator(8, (bool a, bool b) => { return a || b; });
        /// Truth Table
        /// a b f
        /// 0 0 1
        /// 0 1 0
        /// 1 0 0
        /// 1 1 0
        public static BinaryOperator NOR = new BinaryOperator(9, (bool a, bool b) => { return !(a || b); });
        /// Truth Table
        /// a b f
        /// 0 0 1
        /// 0 1 0
        /// 1 0 0
        /// 1 1 1
        public static BinaryOperator XNOR = new BinaryOperator(10, (bool a, bool b) => { return !(a ^ b); });
        /// Truth Table
        /// a b f
        /// 0 0 1
        /// 0 1 0
        /// 1 0 1
        /// 1 1 0
        public static BinaryOperator NB = new BinaryOperator(11, (bool a, bool b) => { return !b; });
        /// Truth Table
        /// a b f
        /// 0 0 1
        /// 0 1 0
        /// 1 0 1
        /// 1 1 1
        public static BinaryOperator AORNB = new BinaryOperator(12, (bool a, bool b) => { return a || !b; });
        /// Truth Table
        /// a b f
        /// 0 0 1
        /// 0 1 1
        /// 1 0 0
        /// 1 1 0
        public static BinaryOperator NA = new BinaryOperator(13, (bool a, bool b) => { return !a; });
        /// Truth Table
        /// a b f
        /// 0 0 1
        /// 0 1 1
        /// 1 0 0
        /// 1 1 1
        public static BinaryOperator NAORB = new BinaryOperator(14, (bool a, bool b) => { return !a || b; });
        /// Truth Table
        /// a b f
        /// 0 0 1
        /// 0 1 1
        /// 1 0 1
        /// 1 1 0
        public static BinaryOperator NAND = new BinaryOperator(15, (bool a, bool b) => { return !(a && b); });
        /// Truth Table
        /// a b f
        /// 0 0 1
        /// 0 1 1
        /// 1 0 1
        /// 1 1 1
        public static BinaryOperator PASS = new BinaryOperator(16, (bool a, bool b) => { return true; });


    }
}
