using ExternalPropertyAttributes;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using DisableIf = Sirenix.OdinInspector.DisableIfAttribute;

namespace DownBelow
{
    [Serializable, DataContract]
    [InlineProperty]
    public struct ConditionComparison : IFormattable, IComparable<ConditionComparison>
    {
        /// <summary>
        /// If this comparison can be used or not
        /// </summary>
        [HorizontalGroup]
        [DataMember]
        [HideLabel]
        [Tooltip("If this comparison can be used or not")]
        public readonly bool Enabled;
        /// <summary>
        /// Operator used for the comparison
        /// </summary>
        [HorizontalGroup]
        [DataMember]
        [HideLabel]
        [ValueDropdown("ConditionOperatorFilter")]
        [DisableIf("@!this.Enabled")]
        [Tooltip("Operator used for the comparison")]
        public readonly ConditionOperator Operator;
        /// <summary>
        /// Treshold this conditiopn must reach to be valid
        /// </summary>
        [HorizontalGroup]
        [DataMember]
        [HideLabel]
        [DisableIf("@!this.Enabled || this.Operator == ConditionOperator.always || this.Operator == ConditionOperator.none")]
        [Tooltip("Treshold this conditiopn must reach to be valid")]
        public readonly float Treshold;

        private static IEnumerable<ConditionOperator> ConditionOperatorFilter()
        {
            yield return ConditionOperator.always;
            yield return ConditionOperator.none;
            yield return ConditionOperator.less;
            yield return ConditionOperator.less_or_equal;
            yield return ConditionOperator.greater_or_equal;
            yield return ConditionOperator.greater;
            yield return ConditionOperator.equal;
            yield return ConditionOperator.different;
        }

        public ConditionComparison(bool enabled, ConditionOperator @operator, float treshold)
        {
            this.Enabled = enabled;
            this.Operator = @operator;
            this.Treshold = treshold;
        }

        /// <summary>
        /// Return <see langword="true"/> if the given value match the conditions
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Match(float value)
        {
            if (!this.Enabled)
                return true;

            switch (Operator)
            {
                case ConditionOperator.none:
                    return false;
                case ConditionOperator.less:
                    return value < this.Treshold;
                case ConditionOperator.less_or_equal:
                    return value <= this.Treshold;
                case ConditionOperator.greater:
                    return value > this.Treshold;
                case ConditionOperator.greater_or_equal:
                    return value >= this.Treshold;
                case ConditionOperator.equal:
                    return value == this.Treshold;
                case ConditionOperator.different:
                    return value != this.Treshold;
                case ConditionOperator.always:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Return the difference betwent the given value and the condition treshold depending on the condition operator
        /// <list type="bullet">
        /// <item>none: return <b>0</b></item>
        /// <item>equal: return <b>0</b> if equal, <see cref="float.MaxValue"/> otherwise</item>
        /// <item>different: return <b>0</b> if not equal, <see cref="float.MaxValue"/> otherwise</item>
        /// <item>always: return <b>0</b></item>
        /// <item>never: return <see cref="float.MaxValue"/></item>
        /// </list>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public float Difference(float value)
        {
            if (!this.Enabled)
                return 0;

            switch (Operator)
            {
                case ConditionOperator.none:
                    return 0;
                case ConditionOperator.less:
                case ConditionOperator.less_or_equal:
                    return this.Treshold - value;
                case ConditionOperator.greater:
                case ConditionOperator.greater_or_equal:
                    return value - this.Treshold;
                case ConditionOperator.equal:
                    if (value == this.Treshold)
                        return 0;
                    else
                        return float.MaxValue;
                case ConditionOperator.different:
                    if (value != this.Treshold)
                        return 0;
                    else
                        return float.MaxValue;
                case ConditionOperator.always:
                    return 0;
                default:
                    return float.MaxValue;
            }
        }

        /// <summary>
        /// Do the same as <see cref="Difference(float)"/> but don't allow negative value (set to 0)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public float DifferenceNoNegative(float value)
        {
            float result = this.Difference(value);
            if (result < 0)
                result = 0;

            return result;
        }

        public string ToString(string format)
        {
            return this.Operator.ToString() + " " + this.Treshold.ToString(format);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return this.Operator.ToString() + " " + this.Treshold.ToString(format, formatProvider);
        }

        public int CompareTo(ConditionComparison other)
        {
            if (this.Treshold == 0 || other.Treshold == 0)
                return 0;
            else if (this.Treshold != 0 && other.Treshold == 0)
                return -1;
            else if (this.Treshold == 0 || (this.Treshold > other.Treshold))
                return 1;
            else
                return -1;
        }
    }
}
