using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [Serializable]
    public sealed class TreeFoliageClusterDefinition
    {
        [SerializeField]
        private int stableClusterId;

        [SerializeField]
        private int parentBranchIndex = -1;

        [SerializeField]
        private float parentBranchDistance;

        [SerializeField]
        private Vector3 position;

        [SerializeField]
        private Quaternion orientation = Quaternion.identity;

        [SerializeField]
        private Vector3 extent = Vector3.one;

        [SerializeField]
        private int requestedCardCount;

        [SerializeField]
        private float density;

        [SerializeField]
        private float stiffness;

        [SerializeField]
        private float phase;

        public int StableClusterId => stableClusterId;
        public int ParentBranchIndex => parentBranchIndex;
        public float ParentBranchDistance => parentBranchDistance;
        public Vector3 Position => position;
        public Quaternion Orientation => orientation;
        public Vector3 Extent => extent;
        public int RequestedCardCount => requestedCardCount;
        public float Density => density;
        public float Stiffness => stiffness;
        public float Phase => phase;
    }
}
