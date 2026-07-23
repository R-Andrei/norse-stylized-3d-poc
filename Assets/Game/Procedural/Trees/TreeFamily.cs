using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    public enum TreeFamily
    {
        [InspectorName("Common")]
        Common = 0,

        [InspectorName("Pine")]
        Pine = 1,

        [InspectorName("Twisted")]
        Twisted = 2,

        [InspectorName("Dead")]
        Dead = 3
    }

    public enum TreeReferenceRole
    {
        [InspectorName("Imported Reference")]
        ImportedReference = 0,

        [InspectorName("Procedural Comparison")]
        ProceduralComparison = 1
    }
}
