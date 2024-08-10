using GenerativeLogic.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Operand
{
    /// <summary>
    /// Represents a set of operands.
    /// </summary>
    public class OperandSet
    {
        #region FieldsAndProperties

        /// <summary>
        /// A dictionary of the operands indexed by name.
        /// </summary>
        public IDictionary<string, IOperand> Operands
        {
            get { return new Dictionary<string, IOperand>(this.operands); }
        }

        /// <summary>
        /// The precedence of the operands, where the zero index has the greatest precedence, and therefor changes the least frequently when values are incremented. 
        /// </summary>
        public string[] OperandPrecedence
        {
            get
            {
                if (this.operandPrecedence == null)
                {
                    this.operandPrecedence = this.operands.Keys.ToArray().Reverse().ToArray();
                }
                return this.operandPrecedence;
            }
        }

        /// <summary>
        /// The operands in the set.
        /// </summary>
        private Dictionary<string, IOperand> operands = new Dictionary<string, IOperand>();

        /// <summary>
        /// The current values assigned to the operands.
        /// </summary>
        private bool[] currentValues = null;

        /// <summary>
        /// The ordered set of operands.
        /// </summary>
        private IOperand[] operandArray = null;

        /// <summary>
        /// The precedence of the operands, where the zero index has the greatest precedence, and therefor changes the least frequently when values are incremented. 
        /// </summary>
        private string[] operandPrecedence = null;

        /// <summary>
        /// Contains a mapping from an operand to its precedence.
        /// </summary>
        private Dictionary<IOperand, int> operandPrecedenceMap = new Dictionary<IOperand, int>();

        /// <summary>
        /// An action to call when the precedence is updated.
        /// </summary>
        private Action<string[], string[]> updatePrecedenceAction = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public OperandSet()
        {

        }

        /// <summary>
        /// Constructor. Creates operands for each givem operand name.
        /// </summary>
        /// <param name="operands">The name of the operands to name.</param>
        public OperandSet(List<string> operandNames)
        {
            IOperand operand;
            foreach (string operandName in operandNames)
            {
                operand = new VariableOperand(operandName);
                this.AddOperand(operand);
            }
        }

        /// <summary>
        /// Copy Constructor.
        /// </summary>
        /// <param name="operandSet">The operandSet to copy.</param>
        public OperandSet(OperandSet operandSet)
        {
            foreach (IOperand operand in operandSet.Operands.Values)
            {
                this.AddOperand(operand);
            }

            string[] precedence = new string[operandSet.operandPrecedence.Length];
            operandSet.operandPrecedence.CopyTo(precedence, 0);
            SetOperandPrecedence(precedence);
        }

        #endregion

        #region Mutators

        /// <summary>
        /// Sets the action to call when the precedence is updated.
        /// </summary>
        /// <param name="updateAction">The action to call when the precedence is updated.  The first parameter is the old precedence and the second parameter is the new precedence.</param>
        public void SetUpdatePrecedenceAction(Action<string[], string[]> updateAction)
        {
            this.updatePrecedenceAction = updateAction;
        }

        /// <summary>
        /// Adds the operand to the set or does nothing if an operand with a matching name already exists.
        /// </summary>
        /// <param name="operand">The operand to add</param>
        public void AddOperand(IOperand operand)
        {
            if (this.operands.ContainsKey(operand.Name))
            {
                return;
            }
            this.operands.Add(operand.Name, operand);
            ResetValues();
            SetOperandPrecedence(null);
        }

        /// <summary>
        /// Adds all of the operands of the given set to this set.
        /// </summary>
        /// <param name="set">The set of operands to add.</param>
        public void AddAll(OperandSet set)
        {
            foreach (IOperand operand in set.Operands.Values)
            {
                this.AddOperand(operand);
            }
        }

        /// <summary>
        /// Removes the opoerand with the given name.
        /// </summary>
        /// <param name="operandName">The name of the operand to remove.</param>
        public void RemoveOperand(string operandName)
        {
            if (this.operands.ContainsKey(operandName))
            {
                this.operands.Remove(operandName);
                ResetValues();
                SetOperandPrecedence(null);
            }
        }

        /// <summary>
        /// Removes the provided operand.
        /// </summary>
        /// <param name="operand">The operand to remove.</param>
        public void RemoveOperand(IOperand operand)
        {
            this.RemoveOperand(operand.Name);
        }

        /// <summary>
        /// Creates the operand with the given name.
        /// </summary>
        /// <param name="operandName">The name of the operand to create.</param>
        /// <returns>The created operand.</returns>
        public IOperand CreateOperand(string operandName)
        {
            IOperand operand = new VariableOperand(operandName);
            this.operands.Add(operandName, operand);
            SetOperandPrecedence(null);
            return operand;
        }

        /// <summary>
        /// Sets the precedence of the operands. index 0 is LSB.
        /// </summary>
        /// <param name="precedence">The precedence of the operands.</param>
        public void SetOperandPrecedence(string[] precedence)
        {
            if (this.updatePrecedenceAction != null)
            {
                this.updatePrecedenceAction(this.OperandPrecedence, precedence);
            }

            if (precedence != null)
            {
                this.operandPrecedence = precedence;
            }
            else
            {
                this.operandPrecedence = this.operands.Keys.ToArray().Reverse().ToArray();
            }

            this.operandPrecedenceMap.Clear();
            for (int i = 0; i < this.operandPrecedence.Length; i++)
            {
                this.operandPrecedenceMap.Add(this.Operands[this.operandPrecedence[i]], i);
            }
        }

        /// <summary>
        /// Creates a new value array for the set and sets the values to false.
        /// </summary>
        public void ResetValues()
        {
            if (this.operands.Count > 0)
            {
                this.currentValues = new bool[this.operands.Count];
                for (int i = 0; i < this.currentValues.Length; i++)
                {
                    this.currentValues[i] = false;
                }
                foreach (IOperand operand in this.operands.Values)
                {
                    operand.Value = false;
                }
                this.operandArray = this.operands.Values.ToArray();
                for (int i = 0; i < this.operandArray.Length; i++)
                {
                    this.operandArray[i].Value = false;
                }
                if (this.operandPrecedence != null)
                {
                    string[] oldPrecedence = this.operandPrecedence;
                    this.operandPrecedence = new string[this.operands.Count];
                    int index = 0;
                    for (int i = 0; i < oldPrecedence.Length; i++)
                    {
                        if (this.operands.ContainsKey(oldPrecedence[i]))
                        {
                            this.operandPrecedence[index] = oldPrecedence[i];
                            index++;
                        }
                    }
                }
            }
            else
            {
                this.currentValues = null;
                this.operandArray = null;
                this.operandPrecedence = null;
            }
        }

        /// <summary>
        /// Gets the next incremented value for the operands and sets them.
        /// </summary>
        public void IncrementSetValue()
        {
            if (this.currentValues == null)
            {
                this.currentValues = new bool[this.operands.Count];
            }
            for (int i = 0; i < this.OperandPrecedence.Length; i++)
            {
                this.currentValues[i] = this.operands[this.OperandPrecedence[i]].Value;
            }
            Transform.Increment(this.currentValues);
            IOperand operand;
            //Assign the values based on precedence.
            for (int i = this.currentValues.Length - 1; i >= 0; i--)
            {
                operand = this.operands[this.OperandPrecedence[i]];
                operand.Value = this.currentValues[i];
            }
        }

        /// <summary>
        /// Iterates through all values of the operands and executes the action at each value, starting from an array of false values.
        /// </summary>
        /// <param name="executeFunction">The function to execute at each iteration. The int parameter is the zero-based index of the iteration.  Return false to stop iterating.</param>
        public void ExecuteOnAllValues(Func<int, bool> executeFunction)
        {
            ResetValues();
            int indices = this.GetStateSpace();
            for (int i = 0; i < indices; i++)
            {
                if (!executeFunction(i))
                {
                    return;
                }
                IncrementSetValue();
            }
        }

        /// <summary>
        /// Sets the values of the operands from the given index.
        /// </summary>
        /// <param name="index">The index to use to set the operands.</param>
        public void SetOperandValuesFromIndex(int index)
        {
            string[] precedence = this.OperandPrecedence;
            bool[] spread = Conversion.ConvertIntToBoolArray(index, precedence.Length, true);
            for (int i = 0; i < precedence.Length; i++)
            {
                this.Operands[precedence[i]].Value = spread[i];
            }
        }

        public void ReplaceOperand(IOperand oldOperand, IOperand newOperand)
        {
            int precedence = GetOperandPrecedenceIndex(oldOperand);
            this.operands.Remove(oldOperand.Name);
            this.operands.Add(newOperand.Name, newOperand);
            this.operandPrecedence[precedence] = newOperand.Name;
            this.operandPrecedenceMap.Remove(oldOperand);
            this.operandPrecedenceMap.Add(newOperand, precedence);
        }

        public IDictionary<int, OperandSet> GetNthOperandCombinations(int combinationSize)
        {
            OperandSet combination;
            Dictionary<int, OperandSet> combinations = new Dictionary<int, OperandSet>();
            IEnumerable<IEnumerable<IOperand>> operandCombinations;
            CombinationGenerator<IOperand> combinationProvider = new CombinationGenerator<IOperand>();
            operandCombinations = combinationProvider.GenerateCombinations(combinationSize, this.Operands.Values);

            int index = 0;
            foreach (IEnumerable<IOperand> enumCombination in operandCombinations)
            {
                combination = new OperandSet();
                foreach (IOperand operand in enumCombination)
                {
                    combination.AddOperand(operand);
                }
                combinations.Add(index++, combination);
            }
            return combinations;
        }

        #endregion

        #region PropertyGenerators

        /// <summary>
        /// Gets the operand with the provided name.
        /// </summary>
        /// <param name="name">The name of the operand to get.</param>
        /// <returns>The operand with the provided name.</returns>
        public IOperand GetOperand(string name)
        {
            if (!this.operands.ContainsKey(name))
            {
                return null;
            }
            return this.operands[name];
        }

        /// <summary>
        /// Gets the operand moment at the given index.
        /// </summary>
        /// <remarks>
        /// The operand moment is the value instance of the operand set at a given index as determined by the precedence.
        /// For example, if there are operands [a, b, c], there are eight possible moments,
        /// index = 0 - [false, false, false]
        /// index = 1 - [false, false, true]
        /// ...
        /// index = 7 - [true, true, true]
        /// </remarks>
        /// <param name="index">The zero-based index at which to get the moment.</param>
        /// <returns>The bool array that contains the moment values.</returns>
        public bool[] GetOperandMomentAtIndex(int index)
        {
            bool[] moment = new bool[this.operands.Count];
            for (int i = 0; i < moment.Length; i++)
            {
                moment[i] = (index & 1) == 1 ? true : false;
                index = index >> 1;
            }
            return moment;
        }

        /// <summary>
        /// Gets the zero-based index of the operand moment.
        /// </summary>
        /// <param name="moment">The moment to get the index of.</param>
        /// <returns>The index at the given moment.</returns>
        public int GetIndexAtOperandMoment(bool[] moment)
        {
            int index = 0;
            for (int i = moment.Length - 1; i >= 0; i--)
            {
                index = index << 1;
                index += moment[i] ? 1 : 0;
            }
            return index;
        }

        /// <summary>
        /// Gets the subset of operands with a name in operandNames.
        /// </summary>
        /// <param name="operandNames">The names of the operands to get.</param>
        /// <returns>A IOperandSet containing the operands with the given names.</returns>
        public OperandSet GetSubset(IEnumerable<string> operandNames)
        {
            OperandSet subset = new OperandSet();
            foreach (string operandName in operandNames)
            {
                if (this.operands.ContainsKey(operandName))
                {
                    subset.AddOperand(this.operands[operandName]);
                }
            }
            return subset;
        }

        /// <summary>
        /// Gets the subset of operands given the enumeration.
        /// </summary>
        /// <param name="operands">The operands to get a subset of.</param>
        /// <returns>The subset of operands whose names match the names of the provided operands.</returns>
        public OperandSet GetSubset(IEnumerable<IOperand> operands)
        {
            return this.GetSubset(operands.Select((operand) => { return operand.Name; }));
        }

        /// <summary>
        /// Gets the precedence of the subset of operands within the larger set.
        /// </summary>
        /// <param name="subset">The subset of operands.</param>
        /// <returns>The precedence of the subset of operands.</returns>
        public IDictionary<IOperand, int> GetSubsetPrecedence(OperandSet subset)
        {
            Dictionary<IOperand, int> subsetPrecedence = new Dictionary<IOperand, int>();
            foreach (IOperand operand in subset.operands.Values)
            {
                subsetPrecedence.Add(operand, this.operandPrecedenceMap[operand]);
            }
            return subsetPrecedence;
        }

        /// <summary>
        /// Gets the compliment of this set given another set.
        /// </summary>
        /// <param name="complimentSet">The set of operands to exclude from the result.</param>
        /// <returns>A IOperandSet that contains all operands in this set that are not in the given complimentSet.</returns>
        public OperandSet GetCompliment(OperandSet complimentSet)
        {
            OperandSet compliment = new OperandSet();
            foreach (string operandName in this.operands.Keys)
            {
                if (!complimentSet.operands.ContainsKey(operandName))
                {
                    compliment.AddOperand(this.operands[operandName]);
                }
            }
            return compliment;
        }

        /// <summary>
        /// Returns all of the operand names that are names of operands in this operand set but not in the provided list.
        /// </summary>
        /// <param name="operandNames">The names of the operands to get the compliment of.</param>
        /// <returns>All of the operand names that are names of operands in this operand set but not in the provided list.</returns>
        public IEnumerable<string> GetOperandCompliment(IEnumerable<string> operandNames)
        {
            List<string> compliment = new List<string>();
            foreach (string operandName in this.operands.Keys)
            {
                if (!operandNames.Contains(operandName))
                {
                    compliment.Add(operandName);
                }
            }
            return compliment;
        }

        /// <summary>
        /// Returns all of the operands that are operands in this operand set but not in the provided list.
        /// </summary>
        /// <param name="operandNames">The operands to get the compliment of.</param>
        /// <returns>All of the operands that are operands in this operand set but not in the provided list.</returns>
        public IEnumerable<IOperand> GetOperandCompliment(IEnumerable<IOperand> operands)
        {
            IEnumerable<string> parameterNames = operands.Select<IOperand, string>((operand) =>
            {
                return operand.Name;
            });
            List<IOperand> compliment = new List<IOperand>();
            foreach (string operandName in this.operands.Keys)
            {
                if (!parameterNames.Contains(operandName))
                {
                    compliment.Add(this.operands[operandName]);
                }
            }
            return compliment;
        }

        /// <summary>
        /// Gets the zero-based index of the precedence of the povided operator.
        /// </summary>
        /// <param name="operandName">The name of the provided operator.</param>
        /// <returns>The zero-based index of the precedence of the povided operator.</returns>
        public int GetOperandPrecedenceIndex(string operandName)
        {
            for (int i = 0; i < this.OperandPrecedence.Length; i++)
            {
                if (this.OperandPrecedence[i] == operandName)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the precedence of the provided operand.
        /// </summary>
        /// <param name="operand">The operand to get the precedence of.</param>
        /// <returns>The precedence of the provided operand.</returns>
        public int GetOperandPrecedenceIndex(IOperand operand)
        {
            return this.GetOperandPrecedenceIndex(operand.Name);
        }

        /// <summary>
        /// Creates a mapping from indices of the operand precedence of this operand set to the indices of the operand set of the provides operand set.
        /// </summary>
        /// <remarks>
        /// Unmapped indices will contain a -1.
        /// </remarks>
        /// <param name="operandSet">The operand set to compare to.</param>
        /// <returns>A mapping from indices of the operand precedence of this operand set to the indices of the operand set of the provides operand set.</returns>
        public int[] GetOperandPrecedenceMapping(OperandSet operandSet)
        {
            int[] map = new int[this.OperandPrecedence.Length];
            for (int i = 0; i < map.Length; i++)
            {
                map[i] = operandSet.GetOperandPrecedenceIndex(this.OperandPrecedence[i]);
            }
            return map;
        }

        /// <summary>
        /// Gets the operand that first appears in the given precedence.
        /// </summary>
        /// <param name="precedence">The precedence of operands to compare to.</param>
        /// <returns>The operand that has the name of the first matching name in the precedence.</returns>
        public IOperand GetFirstOperandInPrecedence(string[] precedence)
        {
            foreach (string operandName in precedence)
            {
                if (this.Operands.ContainsKey(operandName))
                {
                    return this.Operands[operandName];
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the current values of the operand set by operand precedence.
        /// </summary>
        /// <returns>the current values of the operand set.</returns>
        public bool[] GetCurrentValues()
        {
            bool[] values = new bool[this.Operands.Count];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = this.Operands[this.OperandPrecedence[i]].Value;
            }
            return values;
        }

        /// <summary>
        /// Gets the index based on the current values and the precedence.
        /// </summary>
        /// <returns>The index based on the current values and the precedence.</returns>
        public int GetCurrentIndex()
        {
            return this.GetIndexAtOperandMoment(this.GetCurrentValues());
        }

        /// <summary>
        /// Gets the number of possible states that the operands can be in.
        /// </summary>
        /// <returns>The number of possible states that the operands can be in.</returns>
        public int GetStateSpace()
        {
            return 1 << this.Operands.Count;
        }

        /// <summary>
        /// Gets the indices of this function where the moments match the given subMoment.
        /// </summary>
        /// <param name="subMoment">The moment of the subset of operands that we want to get the indices of.</param>
        /// <param name="operandSubset">The subset of operands to which the subMoment applies, in the provided precedence order.</param>
        /// <returns>The indices of this function where the moments match the given subMoment.</returns>
        /// <remarks>
        /// Suppose the function contains variables a, b, c, d, and e, and we want the [0, 0, 1] subMoment for [b, e, c].
        /// We would then have four results, one for each combination of values for a and d.
        /// The results are the indices of the function's result signature.
        /// 
        /// The subMoment values correspond to the precedence of the operandSubset, which will likely be different than that of the function's operand set.
        /// So, we need to map the indices of the subset precedence to the indices of the operands in the main set.
        /// Then, we can iterate through to get the indices of the subMoment.
        /// </remarks>
        public IEnumerable<int> GetSubMomentIndices(bool[] subMoment, OperandSet operandSubset)
        {
            List<int> indices = new List<int>();
            bool[] moment = new bool[this.Operands.Count()];

            IDictionary<IOperand, int> subsetPrecedence = this.GetSubsetPrecedence(operandSubset);
            int[] subsetPrecedenceMap = new int[subsetPrecedence.Count()];

            OperandSet compliment = this.GetCompliment(operandSubset);
            IDictionary<IOperand, int> complimentPrecedence = this.GetSubsetPrecedence(compliment);
            int[] complimentPrecedenceMap = new int[complimentPrecedence.Count()];
            bool[] complimentMoment = Utility.GetArrayOfType<bool>(compliment.Operands.Count(), false);

            //Create the mapping from the subset operands to the main set.
            foreach (IOperand operand in operandSubset.Operands.Values)
            {
                subsetPrecedenceMap[operandSubset.GetOperandPrecedenceIndex(operand.Name)] = subsetPrecedence[operand];
            }

            //Set the values in the moment for the subset operands.
            for (int i = 0; i < subMoment.Length; i++)
            {
                moment[subsetPrecedenceMap[i]] = subMoment[i];
            }

            //Create a mapping from the compliment to the main set.
            foreach (IOperand operand in compliment.Operands.Values)
            {
                complimentPrecedenceMap[compliment.GetOperandPrecedenceIndex(operand.Name)] = complimentPrecedence[operand];
            }

            //Iterate through all of the compliment moments, set the main moment, then get the index.
            for (int i = 0; i < (1 << complimentMoment.Length); i++)
            {
                for (int j = 0; j < complimentPrecedenceMap.Length; j++)
                {
                    moment[complimentPrecedenceMap[j]] = complimentMoment[j];
                }
                //Add the index at the moment.
                indices.Add(this.GetIndexAtOperandMoment(moment));
                //set the compliment moment to the next value.
                Transform.Increment(complimentMoment);
            }
            return indices;
        }

        /// <summary>
        /// Gets a precedence map from the subset to this set.
        /// </summary>
        /// <param name="subset">The subset to map the precedence from.</param>
        /// <param name="getReverse">If true, the mapping will be from the precedence of this set to the subset.</param>
        /// <returns>An int to int dictionary that serves as the map.</returns>
        public IDictionary<int, int> GetPrecedenceMap(OperandSet subset, bool getReverse = false)
        {
            Dictionary<int, int> map = new Dictionary<int, int>();
            string[] x = subset.OperandPrecedence;
            foreach (IOperand operand in subset.Operands.Values)
            {
                if (getReverse)
                {
                    map.Add(this.operandPrecedenceMap[operand], subset.operandPrecedenceMap[operand]);
                }
                else
                {
                    map.Add(subset.operandPrecedenceMap[operand], this.operandPrecedenceMap[operand]);
                }
            }
            return map;
        }

        /// <summary>
        /// Gets the moment of the subset given the provided index.
        /// </summary>
        /// <param name="subset">The subset to get the moment for.</param>
        /// <param name="index">The index on which to get the momment.</param>
        /// <param name="precedenceMap">A mapping from the subset precedence to this precedence.</param>
        /// <returns>The moment of the subset given the index.</returns>
        public bool[] GetSubsetMomentByIndex(OperandSet subset, int index, IDictionary<int, int> precedenceMap = null)
        {
            bool[] moment = this.GetOperandMomentAtIndex(index);
            bool[] subsetMoment = Utility.GetArrayOfType<bool>(subset.Operands.Count, false);
            if (precedenceMap == null)
            {
                precedenceMap = this.GetPrecedenceMap(subset);
            }
            for (int i = 0; i < subset.Operands.Count; i++)
            {
                subsetMoment[i] = moment[precedenceMap[i]];
            }
            return subsetMoment;
        }

        /// <summary>
        /// Gets the subset moments for a collection of indices.
        /// </summary>
        /// <param name="subset">The subset to get the moments for.</param>
        /// <param name="indices">The indices on which to get the moments.</param>
        /// <returns>The moments of the subset for each index.</returns>
        public IDictionary<int, bool[]> GetSubsetMomentsByIndices(OperandSet subset, IEnumerable<int> indices)
        {
            IDictionary<int, int> map = this.GetPrecedenceMap(subset);
            Dictionary<int, bool[]> result = new Dictionary<int, bool[]>();
            foreach (int index in indices)
            {
                result.Add(index, this.GetSubsetMomentByIndex(subset, index, map));
            }
            return result;
        }

        /// <summary>
        /// Sets the members in the precedence array to the relative precedence of the subset when compared to this set.
        /// </summary>
        /// <param name="subset">The subset to get the relative precedence of.</param>
        /// <param name="precedence">The array in which to place the relative precedence.</param>
        /// <param name="offset">The offset in the array at which to start adding the precedence.</param>
        /// <remarks>
        /// Suppose this set has a precedence of p w z q y x z and the subset contains q w z.
        /// The relative precedence for the subset would be w q z.
        /// Supposing the index is 2, then the precedence parameter would be ? ? w q z ..., where ? is some unknown operand name, not relevent here. 
        /// </remarks>
        public void SetSubsetRelativePrecedence(OperandSet subset, string[] precedence, int offset)
        {
            IEnumerable<IOperand> intersection = this.Operands.Values.Intersect(subset.Operands.Values);
            string[] thisPrecedence = this.OperandPrecedence;
            int j = offset;
            for (int i = 0; i < thisPrecedence.Length; i++)
            {
                if (intersection.Contains(this.Operands[thisPrecedence[i]]))
                {
                    precedence[j] = thisPrecedence[i];
                    j++;
                }
            }
        }

        /// <summary>
        /// Gets the intersection of this set and the provided set.
        /// </summary>
        /// <param name="set">The set to intersect.</param>
        /// <returns>A new operand set containing the intersection of the two sets.</returns>
        public OperandSet GetIntersection(OperandSet set)
        {
            OperandSet result = new OperandSet();
            foreach (IOperand operand in this.Operands.Values)
            {
                if (set.Operands.Values.Contains(operand))
                {
                    result.AddOperand(operand);
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the union of the two operand sets.
        /// </summary>
        /// <param name="set">The operand set to get the union of.</param>
        /// <returns>The union of the set with this set.</returns>
        public OperandSet GetUnion(OperandSet set)
        {
            OperandSet result = new OperandSet();
            foreach (IOperand operand in this.Operands.Values)
            {
                result.AddOperand(operand);
            }
            foreach (IOperand operand in set.Operands.Values)
            {
                result.AddOperand(operand);
            }

            return result;
        }

        /// <summary>
        /// Gets the number of possible moments.
        /// </summary>
        /// <returns>The number of possible moments.</returns>
        public int GetPossibleMomentValues()
        {
            return 1 << this.operands.Count();
        }

        #endregion

        #region Comparisons

        /// <summary>
        /// Tests whether this operand set is congruent to the provided set.  They are congruent if they have exactly the same operands.
        /// </summary>
        /// <param name="operandSet">The operand set to compare to.</param>
        /// <returns>true: they are congruent.  false: they are not congruent.</returns>
        public bool IsCongruent(OperandSet operandSet)
        {
            int intersectionCardinality = this.Operands.Keys.Intersect(operandSet.Operands.Keys).Count();
            if (intersectionCardinality == this.Operands.Count() && intersectionCardinality == operandSet.Operands.Count())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true iff the provided set contains all of the operands that this set contains.
        /// </summary>
        /// <param name="set">The set to compare to.</param>
        /// <returns>true iff the provided set contains all of the operands that this set contains.</returns>
        public bool IsSubsetOf(OperandSet set)
        {
            int x = set.Operands.Values.Intersect(this.Operands.Values).Count();
            int y = this.Operands.Values.Count();
            return set.Operands.Values.Intersect(this.Operands.Values).Count() == this.Operands.Values.Count();
        }

        /// <summary>
        /// Returns true iff the provided set contains all of the operands that this set contains and it contains at least one additional operand.
        /// </summary>
        /// <param name="set">The set to compare to.</param>
        /// <returns>true iff the provided set contains all of the operands that this set contains and it contains at least one additional operand.</returns>
        public bool IsProperSubsetOf(OperandSet set)
        {
            return (set.Operands.Values.Intersect(this.Operands.Values).Count() == this.Operands.Values.Count() && set.Operands.Count() > this.Operands.Count());
        }

        #endregion

        public override string ToString()
        {
            string[] precedence = this.OperandPrecedence;
            StringBuilder result = new StringBuilder(precedence.Length * 2);
            result.Append("{ ");
            for (int i = 0; i < precedence.Length; i++)
            {
                if (i > 0)
                {
                    result.Append(", ");
                }
                result.Append(precedence[i]);
            }
            result.Append(" }");
            return result.ToString();
        }

        public override bool Equals(object obj)
        {
            OperandSet compare = (OperandSet)obj;
            if (this.IsCongruent(compare))
            {
                for (int i = 0; i < this.OperandPrecedence.Length; i++)
                {
                    if (this.OperandPrecedence[i] != compare.OperandPrecedence[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
