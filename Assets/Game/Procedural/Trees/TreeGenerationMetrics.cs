using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [Serializable]
    public sealed class TreeGenerationMetrics
    {
        [SerializeField]
        private int branchCount;

        [SerializeField]
        private int primaryBranchCount;

        [SerializeField]
        private int secondaryBranchCount;

        [SerializeField]
        private int tertiaryBranchCount;

        [SerializeField]
        private int rejectedBranchCount;

        [SerializeField]
        private int deadBranchCount;

        [SerializeField]
        private int brokenBranchCount;

        [SerializeField]
        private int foliageEligibleBranchCount;

        [SerializeField]
        private int controlPointCount;

        [SerializeField]
        private int curveSampleCount;

        [SerializeField]
        private float totalBranchLength;

        [SerializeField]
        private float minimumRadius;

        [SerializeField]
        private float maximumRadius;

        [SerializeField]
        private float maximumSegmentTurnDegrees;

        [SerializeField]
        private float maximumAccumulatedTurnDegrees;

        [SerializeField]
        private float maximumArcChordRatio;

        [SerializeField]
        private int backwardProgressViolationCount;

        [SerializeField]
        private int crownEnvelopeViolationBranchCount;

        [SerializeField]
        private float maximumCrownEnvelopeOvershoot;

        [SerializeField]
        private float calibrationHeightRatio = 1f;

        [SerializeField]
        private float calibrationWidthRatio = 1f;

        [SerializeField]
        private float calibrationDepthRatio = 1f;

        [SerializeField]
        private bool calibrationWithinTolerance = true;

        [SerializeField]
        private long generationTicks;

        public int BranchCount => branchCount;
        public int PrimaryBranchCount => primaryBranchCount;
        public int SecondaryBranchCount => secondaryBranchCount;
        public int TertiaryBranchCount => tertiaryBranchCount;
        public int RejectedBranchCount => rejectedBranchCount;
        public int DeadBranchCount => deadBranchCount;
        public int BrokenBranchCount => brokenBranchCount;
        public int FoliageEligibleBranchCount => foliageEligibleBranchCount;
        public int ControlPointCount => controlPointCount;
        public int CurveSampleCount => curveSampleCount;
        public float TotalBranchLength => totalBranchLength;
        public float MinimumRadius => minimumRadius;
        public float MaximumRadius => maximumRadius;
        public float MaximumSegmentTurnDegrees => maximumSegmentTurnDegrees;
        public float MaximumAccumulatedTurnDegrees => maximumAccumulatedTurnDegrees;
        public float MaximumArcChordRatio => maximumArcChordRatio;
        public int BackwardProgressViolationCount => backwardProgressViolationCount;
        public int CrownEnvelopeViolationBranchCount =>
            crownEnvelopeViolationBranchCount;
        public float MaximumCrownEnvelopeOvershoot =>
            maximumCrownEnvelopeOvershoot;
        public float CalibrationHeightRatio => calibrationHeightRatio;
        public float CalibrationWidthRatio => calibrationWidthRatio;
        public float CalibrationDepthRatio => calibrationDepthRatio;
        public bool CalibrationWithinTolerance => calibrationWithinTolerance;
        public long GenerationTicks => generationTicks;
        public double GenerationMilliseconds => TimeSpan.FromTicks(generationTicks).TotalMilliseconds;

        internal void Initialize(
            int totalBranches,
            int primaryBranches,
            int secondaryBranches,
            int tertiaryBranches,
            int rejectedBranches,
            int deadBranches,
            int brokenBranches,
            int foliageEligibleBranches,
            int totalControlPoints,
            int totalCurveSamples,
            float totalLength,
            float smallestRadius,
            float largestRadius,
            float maxSegmentTurn,
            float maxAccumulatedTurn,
            float maxArcChord,
            int backwardViolations,
            int envelopeViolationBranches,
            float maxEnvelopeOvershoot,
            float heightRatio,
            float widthRatio,
            float depthRatio,
            bool withinTolerance,
            long elapsedTicks)
        {
            branchCount = totalBranches;
            primaryBranchCount = primaryBranches;
            secondaryBranchCount = secondaryBranches;
            tertiaryBranchCount = tertiaryBranches;
            rejectedBranchCount = rejectedBranches;
            deadBranchCount = deadBranches;
            brokenBranchCount = brokenBranches;
            foliageEligibleBranchCount = foliageEligibleBranches;
            controlPointCount = totalControlPoints;
            curveSampleCount = totalCurveSamples;
            totalBranchLength = totalLength;
            minimumRadius = smallestRadius;
            maximumRadius = largestRadius;
            maximumSegmentTurnDegrees = maxSegmentTurn;
            maximumAccumulatedTurnDegrees = maxAccumulatedTurn;
            maximumArcChordRatio = maxArcChord;
            backwardProgressViolationCount = backwardViolations;
            crownEnvelopeViolationBranchCount = envelopeViolationBranches;
            maximumCrownEnvelopeOvershoot = maxEnvelopeOvershoot;
            calibrationHeightRatio = heightRatio;
            calibrationWidthRatio = widthRatio;
            calibrationDepthRatio = depthRatio;
            calibrationWithinTolerance = withinTolerance;
            generationTicks = elapsedTicks;
        }
    }
}
