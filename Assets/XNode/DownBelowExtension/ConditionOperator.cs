using System;
using System.Runtime.Serialization;

namespace DownBelow
{
    [Serializable, DataContract]
    public enum ConditionOperator
    {
        [EnumMember(Value = "")]
        none = 0,

        [EnumMember(Value = "<")]
        less = 1,
        [EnumMember(Value = "<=")]
        less_or_equal = 2,
        [EnumMember(Value = ">")]
        greater = 3,
        [EnumMember(Value = ">=")]
        greater_or_equal = 4,
        [EnumMember(Value = "=")]
        set = 5,
        [EnumMember(Value = "==")]
        equal = 6,
        [EnumMember(Value = "!=")]
        different = 7,
        [EnumMember(Value = "contains")]
        contains = 8,
        [EnumMember(Value = "not_contains")]
        not_contains = 9,
        [EnumMember(Value = "always")]
        always = 10
    }
}