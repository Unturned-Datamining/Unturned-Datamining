using UnityEngine;

namespace SDG.Unturned;

public enum ENPCLogicType
{
    [InspectorName("Invalid")]
    NONE,
    [InspectorName("< Less Than")]
    LESS_THAN,
    [InspectorName("≤ Less Than or Equal")]
    LESS_THAN_OR_EQUAL_TO,
    [InspectorName("= Equal")]
    EQUAL,
    [InspectorName("≠ Not Equal")]
    NOT_EQUAL,
    [InspectorName("≥ Greater Than or Equal")]
    GREATER_THAN_OR_EQUAL_TO,
    [InspectorName("> Greater Than")]
    GREATER_THAN
}
