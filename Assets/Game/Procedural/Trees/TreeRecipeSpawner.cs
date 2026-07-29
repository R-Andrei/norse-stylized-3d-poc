using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Trees/Tree Recipe Spawner")]
    public sealed class TreeRecipeSpawner : MonoBehaviour
    {
        public const string GeneratedChildName = "GeneratedTree";

        [SerializeField]
        private TreeGenerationRecipe recipe;

        [SerializeField]
        private int spawnSeed = 7319;

        [SerializeField, HideInInspector]
        private TreeFamily referenceGrouping;

        [SerializeField, HideInInspector, Range(1, 5)]
        private int referenceVariantIndex = 1;

        [SerializeField, HideInInspector]
        private string stableSlotIdentity = string.Empty;

        [SerializeField, HideInInspector]
        private ProceduralTreeInstance generatedInstance;

        [SerializeField, HideInInspector]
        private bool lastSpawnPassed;

        [SerializeField, HideInInspector]
        private string lastSpawnTimestamp = string.Empty;

        [SerializeField, TextArea(5, 24)]
        private string lastSpawnReport = string.Empty;

        public TreeGenerationRecipe Recipe => recipe;
        public int SpawnSeed => spawnSeed;
        public TreeFamily ReferenceGrouping => referenceGrouping;
        public int ReferenceVariantIndex => referenceVariantIndex;
        public string StableSlotIdentity => stableSlotIdentity;
        public ProceduralTreeInstance GeneratedInstance => generatedInstance;
        public bool LastSpawnPassed => lastSpawnPassed;
        public string LastSpawnTimestamp => lastSpawnTimestamp;
        public string LastSpawnReport => lastSpawnReport;

        public void Configure(
            TreeGenerationRecipe sourceRecipe,
            int seed,
            TreeFamily grouping,
            int variantIndex,
            string slotIdentity)
        {
            recipe = sourceRecipe;
            spawnSeed = seed == int.MinValue ? 0 : Mathf.Abs(seed);
            referenceGrouping = grouping;
            referenceVariantIndex = Mathf.Clamp(variantIndex, 1, 5);
            stableSlotIdentity = slotIdentity ?? string.Empty;
        }

        public void SetRecipe(TreeGenerationRecipe sourceRecipe)
        {
            recipe = sourceRecipe;
        }

        public void SetSpawnSeed(int seed)
        {
            spawnSeed = seed == int.MinValue ? 0 : Mathf.Abs(seed);
        }

        public void AttachGeneratedInstance(ProceduralTreeInstance instance)
        {
            generatedInstance = instance;
        }

        public void PrepareGeneratedInstance(
            TreeGenerationLibrary meshStorageLibrary = null)
        {
            if (generatedInstance == null)
            {
                return;
            }

            generatedInstance.ConfigureRecipeOnlySpawn(
                recipe,
                spawnSeed,
                referenceGrouping,
                referenceVariantIndex,
                meshStorageLibrary);
        }

        public void RecordSpawn(
            bool passed,
            string timestamp,
            string report)
        {
            lastSpawnPassed = passed;
            lastSpawnTimestamp = timestamp ?? string.Empty;
            lastSpawnReport = report ?? string.Empty;
        }

        private void OnValidate()
        {
            if (spawnSeed == int.MinValue)
            {
                spawnSeed = 0;
            }

            referenceVariantIndex = Mathf.Clamp(referenceVariantIndex, 1, 5);
        }
    }
}
