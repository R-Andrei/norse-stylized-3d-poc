using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [Serializable]
    public struct TreeCurveSample
    {
        [SerializeField]
        private Vector3 position;

        [SerializeField]
        private Vector3 tangent;

        [SerializeField]
        private Vector3 normal;

        [SerializeField]
        private Vector3 binormal;

        [SerializeField]
        private float radius;

        [SerializeField]
        private float normalizedDistance;

        public TreeCurveSample(
            Vector3 samplePosition,
            Vector3 sampleTangent,
            Vector3 sampleNormal,
            Vector3 sampleBinormal,
            float sampleRadius,
            float sampleNormalizedDistance)
        {
            position = samplePosition;
            tangent = sampleTangent;
            normal = sampleNormal;
            binormal = sampleBinormal;
            radius = sampleRadius;
            normalizedDistance = sampleNormalizedDistance;
        }

        public Vector3 Position => position;
        public Vector3 Tangent => tangent;
        public Vector3 Normal => normal;
        public Vector3 Binormal => binormal;
        public float Radius => radius;
        public float NormalizedDistance => normalizedDistance;
    }

    [Serializable]
    public sealed class TreeBranchDefinition
    {
        [SerializeField]
        private int stableBranchId;

        [SerializeField]
        private int parentBranchIndex = -1;

        [SerializeField]
        private int branchOrder;

        [SerializeField]
        private float parentAttachmentDistance;

        [SerializeField]
        private Vector3 localReferenceAxis = Vector3.right;

        [SerializeField]
        private float baseRadius;

        [SerializeField]
        private float endRadius;

        [SerializeField]
        private float stiffness = 1f;

        [SerializeField]
        private float phase;

        [SerializeField]
        private TreeBranchState state;

        [SerializeField]
        private float foliageEligibilityStart = 0.45f;

        [SerializeField]
        private float foliageEligibilityEnd = 1f;

        [SerializeField]
        private List<Vector3> controlPoints = new List<Vector3>();

        [SerializeField]
        private List<TreeCurveSample> samples = new List<TreeCurveSample>();

        public int StableBranchId => stableBranchId;
        public int ParentBranchIndex => parentBranchIndex;
        public int BranchOrder => branchOrder;
        public float ParentAttachmentDistance => parentAttachmentDistance;
        public Vector3 LocalReferenceAxis => localReferenceAxis;
        public float BaseRadius => baseRadius;
        public float EndRadius => endRadius;
        public float Stiffness => stiffness;
        public float Phase => phase;
        public TreeBranchState State => state;
        public bool IsDead => (state & TreeBranchState.Dead) != 0;
        public bool IsBroken => (state & TreeBranchState.Broken) != 0;
        public float FoliageEligibilityStart => foliageEligibilityStart;
        public float FoliageEligibilityEnd => foliageEligibilityEnd;
        public IReadOnlyList<Vector3> ControlPoints => controlPoints;
        public IReadOnlyList<TreeCurveSample> Samples => samples;

        internal void Initialize(
            int branchId,
            int parentIndex,
            int order,
            float attachmentDistance,
            Vector3 referenceAxis,
            float startRadius,
            float terminalRadius,
            float branchStiffness,
            float branchPhase,
            TreeBranchState branchState,
            float foliageStart,
            float foliageEnd,
            List<Vector3> branchControlPoints,
            List<TreeCurveSample> branchSamples)
        {
            stableBranchId = branchId;
            parentBranchIndex = parentIndex;
            branchOrder = order;
            parentAttachmentDistance = attachmentDistance;
            localReferenceAxis = referenceAxis;
            baseRadius = startRadius;
            endRadius = terminalRadius;
            stiffness = branchStiffness;
            phase = branchPhase;
            state = branchState;
            foliageEligibilityStart = foliageStart;
            foliageEligibilityEnd = foliageEnd;
            controlPoints = branchControlPoints ?? new List<Vector3>();
            samples = branchSamples ?? new List<TreeCurveSample>();
        }

        internal void ReplaceGeometry(
            List<Vector3> branchControlPoints,
            List<TreeCurveSample> branchSamples,
            float scaledBaseRadius,
            float scaledEndRadius)
        {
            controlPoints = branchControlPoints ?? new List<Vector3>();
            samples = branchSamples ?? new List<TreeCurveSample>();
            baseRadius = Mathf.Max(0.0001f, scaledBaseRadius);
            endRadius = Mathf.Max(0.0001f, scaledEndRadius);
        }

        internal void ReplaceSamples(List<TreeCurveSample> branchSamples)
        {
            samples = branchSamples ?? new List<TreeCurveSample>();
        }

        internal void ReplaceLocalReferenceAxis(Vector3 referenceAxis)
        {
            localReferenceAxis = referenceAxis.sqrMagnitude > 0.000001f
                ? referenceAxis.normalized
                : Vector3.right;
        }
    }
}
