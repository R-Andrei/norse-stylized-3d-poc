using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [Serializable]
    public sealed class TreeDefinition
    {
        [SerializeField]
        private TreeFamily family;

        [SerializeField]
        private string recipeIdentity;

        [SerializeField]
        private int generatorVersion;

        [SerializeField]
        private int masterSeed;

        [SerializeField]
        private Vector3 localUpAxis = Vector3.up;

        [SerializeField]
        private int trunkBranchIndex;

        [SerializeField]
        private List<TreeBranchDefinition> branches = new List<TreeBranchDefinition>();

        [SerializeField]
        private List<TreeFoliageClusterDefinition> foliageClusters =
            new List<TreeFoliageClusterDefinition>();

        [SerializeField]
        private Bounds localBounds;

        [SerializeField]
        private Vector2 footprintExtents;

        [SerializeField]
        private TreeSeedSet seedSet = new TreeSeedSet();

        [SerializeField]
        private TreeResolvedParameters resolvedParameters =
            new TreeResolvedParameters();

        [SerializeField]
        private TreeGenerationMetrics metrics = new TreeGenerationMetrics();

        [SerializeField]
        private string dependencyFingerprint;

        [SerializeField]
        private string trunkFingerprint;

        [SerializeField]
        private string branchFingerprint;

        [SerializeField]
        private string foliageGeometryFingerprint;

        [SerializeField]
        private string paletteFingerprint;

        [SerializeField]
        private string structuralFingerprint;

        [SerializeField]
        private List<string> warnings = new List<string>();

        public TreeFamily Family => family;
        public string RecipeIdentity => recipeIdentity;
        public int GeneratorVersion => generatorVersion;
        public int MasterSeed => masterSeed;
        public Vector3 LocalUpAxis => localUpAxis;
        public int TrunkBranchIndex => trunkBranchIndex;
        public IReadOnlyList<TreeBranchDefinition> Branches => branches;
        public IReadOnlyList<TreeFoliageClusterDefinition> FoliageClusters => foliageClusters;
        public Bounds LocalBounds => localBounds;
        public Vector2 FootprintExtents => footprintExtents;
        public TreeSeedSet SeedSet => seedSet;
        public TreeResolvedParameters ResolvedParameters => resolvedParameters;
        public TreeGenerationMetrics Metrics => metrics;
        public string DependencyFingerprint => dependencyFingerprint;
        public string TrunkFingerprint => trunkFingerprint;
        public string BranchFingerprint => branchFingerprint;
        public string FoliageGeometryFingerprint => foliageGeometryFingerprint;
        public string PaletteFingerprint => paletteFingerprint;
        public string StructuralFingerprint => structuralFingerprint;
        public IReadOnlyList<string> Warnings => warnings;
        public bool IsValid =>
            branches != null &&
            branches.Count > 0 &&
            trunkBranchIndex >= 0 &&
            trunkBranchIndex < branches.Count &&
            !string.IsNullOrEmpty(structuralFingerprint);

        internal void Initialize(
            TreeFamily treeFamily,
            string recipeId,
            int version,
            int seed,
            int trunkIndex,
            List<TreeBranchDefinition> branchDefinitions,
            List<TreeFoliageClusterDefinition> clusterDefinitions,
            Bounds bounds,
            Vector2 footprint,
            TreeSeedSet seeds,
            TreeResolvedParameters parameters,
            TreeGenerationMetrics generationMetrics,
            string dependencyHash,
            string trunkHash,
            string branchHash,
            string foliageHash,
            string paletteHash,
            string structuralHash,
            List<string> generationWarnings)
        {
            family = treeFamily;
            recipeIdentity = recipeId;
            generatorVersion = version;
            masterSeed = seed;
            localUpAxis = Vector3.up;
            trunkBranchIndex = trunkIndex;
            branches = branchDefinitions ?? new List<TreeBranchDefinition>();
            foliageClusters = clusterDefinitions ?? new List<TreeFoliageClusterDefinition>();
            localBounds = bounds;
            footprintExtents = footprint;
            seedSet = seeds ?? new TreeSeedSet();
            resolvedParameters = parameters ?? new TreeResolvedParameters();
            metrics = generationMetrics ?? new TreeGenerationMetrics();
            dependencyFingerprint = dependencyHash;
            trunkFingerprint = trunkHash;
            branchFingerprint = branchHash;
            foliageGeometryFingerprint = foliageHash;
            paletteFingerprint = paletteHash;
            structuralFingerprint = structuralHash;
            warnings = generationWarnings ?? new List<string>();
        }
    }

    public sealed class TreeGenerationResult
    {
        public bool Passed { get; internal set; }
        public TreeDefinition Definition { get; internal set; }
        public string Report { get; internal set; }
        public string Timestamp { get; internal set; }
    }
}
