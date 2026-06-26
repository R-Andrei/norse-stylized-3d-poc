using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public enum StylizedRiverFoamStyle
    {
        Subtle,
        Flowing,
        Lively,
        Custom
    }

    public enum StylizedRiverFoamQuality
    {
        Low,
        Medium,
        High
    }

    // Retained only so scenes and prefabs that once referenced the legacy
    // public Foam component do not become Missing Script. The canonical Stage
    // 6 system is owned by StylizedRiver and StylizedRiverFoamRuntime.
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StylizedRiver))]
    public sealed class StylizedRiverFoamSimulation : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private bool migrationWarningIssued;

        public StylizedRiverFoamStyle Style => StylizedRiverFoamStyle.Subtle;
        public StylizedRiverFoamQuality Quality => StylizedRiverFoamQuality.Low;
        public bool HasStateTexture => false;
        public Vector2Int StateTextureSize => Vector2Int.zero;
        public RenderTexture StateTexture => null;

        private void OnEnable()
        {
            enabled = false;

            if (migrationWarningIssued)
            {
                return;
            }

            migrationWarningIssued = true;
            Debug.LogWarning(
                $"Legacy StylizedRiverFoamSimulation remains attached to '{name}'. Remove this component; Stage 6 Foam is now configured on StylizedRiver and simulated by its hidden runtime.",
                this);
        }

        public void ApplyStyleDefaults()
        {
        }

        public void RefreshBinding()
        {
        }

        public void ResetSimulation()
        {
        }

        public void NotifyRiverChanged()
        {
        }
    }
}
