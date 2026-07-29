using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    [CustomEditor(typeof(TreeRecipeSpawner))]
    public sealed class TreeRecipeSpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var spawner = (TreeRecipeSpawner)target;
            serializedObject.UpdateIfRequiredOrScript();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("m_Script"));
            }

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("recipe"));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("spawnSeed"));
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("referenceGrouping"));
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("referenceVariantIndex"));
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("stableSlotIdentity"));
                EditorGUILayout.ObjectField(
                    "Generated Instance",
                    spawner.GeneratedInstance,
                    typeof(ProceduralTreeInstance),
                    true);
            }

            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Explicit Spawn Actions", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(spawner.Recipe == null))
            {
                if (GUILayout.Button("Spawn/Rebuild With Current Seed"))
                {
                    Spawn(spawner, false);
                }

                if (GUILayout.Button("Randomize Seed And Spawn"))
                {
                    Undo.RecordObject(spawner, "Randomize Tree Recipe Spawn Seed");
                    spawner.SetSpawnSeed(TreeDeterministicUtility.DeriveSeed(
                        DateTime.UtcNow.Ticks,
                        spawner.GetEntityId(),
                        spawner.StableSlotIdentity,
                        "tree-recipe-spawn"));
                    Spawn(spawner, false);
                }
            }

            using (new EditorGUI.DisabledScope(spawner.GeneratedInstance == null))
            {
                if (GUILayout.Button("Clear Generated Tree"))
                {
                    Clear(spawner);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Last Spawn", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Status",
                string.IsNullOrEmpty(spawner.LastSpawnTimestamp)
                    ? "Not run"
                    : spawner.LastSpawnPassed ? "PASS" : "FAIL");
            EditorGUILayout.LabelField(
                "Timestamp",
                string.IsNullOrEmpty(spawner.LastSpawnTimestamp)
                    ? "—"
                    : spawner.LastSpawnTimestamp);
            using (new EditorGUI.DisabledScope(
                string.IsNullOrEmpty(spawner.LastSpawnReport)))
            {
                if (GUILayout.Button("Copy Last Spawn Report"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        spawner.LastSpawnReport;
                }
            }
        }

        internal static bool Spawn(
            TreeRecipeSpawner spawner,
            bool skipBark,
            out string report)
        {
            report = string.Empty;
            if (spawner == null || spawner.Recipe == null)
            {
                report = "FAIL | Spawner or recipe is missing.";
                return false;
            }

            ProceduralTreeInstance instance = EnsureGeneratedChild(spawner);
            Undo.RecordObject(spawner, "Configure Tree Recipe Spawner");
            Undo.RecordObject(instance, "Spawn Recipe Tree");
            spawner.AttachGeneratedInstance(instance);
            TreeReferenceGallery gallery =
                spawner.GetComponentInParent<TreeReferenceGallery>();
            spawner.PrepareGeneratedInstance(
                gallery != null ? gallery.GenerationLibrary : null);
            TreeGenerationResult generation = instance.GenerateStructure();
            bool passed = generation.Passed &&
                generation.Definition != null &&
                generation.Definition.IsValid;
            report = generation.Report ?? string.Empty;

            if (passed && !skipBark)
            {
                if (gallery == null)
                {
                    passed = false;
                    report +=
                        "\n\nFAIL | Bark: TREE-CONTROLS.3 spawners must be " +
                        "parented beneath a Tree Reference Gallery. General " +
                        "gameplay/editor spawner integration remains deferred.";
                }
                else if (gallery.GenerationLibrary == null)
                {
                    passed = false;
                    report +=
                        "\n\nFAIL | Bark: gallery bark-mesh storage library " +
                        "is not assigned.";
                }
                else if (TreeBarkMeshAssetBuilder.BuildOrUpdate(
                             gallery,
                             gallery.GenerationLibrary,
                             instance,
                             out _,
                             out string barkReport,
                             out string barkFailure))
                {
                    report += "\n\n" + barkReport;
                }
                else
                {
                    passed = false;
                    report += "\n\nFAIL | Bark: " + barkFailure;
                }
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            spawner.RecordSpawn(passed, timestamp, report);
            EditorUtility.SetDirty(instance);
            EditorUtility.SetDirty(spawner);
            MarkSceneDirty(spawner.gameObject);
            SceneView.RepaintAll();
            return passed;
        }

        private static void Spawn(TreeRecipeSpawner spawner, bool skipBark)
        {
            bool passed = Spawn(spawner, skipBark, out string report);
            EditorGUIUtility.systemCopyBuffer = report;
            if (passed)
            {
                Debug.Log(
                    "[TREE-CONTROLS.3] Recipe tree spawned; report copied.",
                    spawner);
            }
            else
            {
                Debug.LogError(
                    "[TREE-CONTROLS.3] Recipe tree spawn failed; report copied.",
                    spawner);
            }
        }

        internal static ProceduralTreeInstance EnsureGeneratedChild(
            TreeRecipeSpawner spawner)
        {
            ProceduralTreeInstance existing = spawner.GeneratedInstance;
            if (existing != null)
            {
                return existing;
            }

            Transform childTransform = spawner.transform.Find(
                TreeRecipeSpawner.GeneratedChildName);
            GameObject child;
            if (childTransform != null)
            {
                child = childTransform.gameObject;
            }
            else
            {
                child = new GameObject(TreeRecipeSpawner.GeneratedChildName);
                Undo.RegisterCreatedObjectUndo(
                    child,
                    "Create Recipe-Spawned Tree");
                child.transform.SetParent(spawner.transform, false);
            }

            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            ProceduralTreeInstance instance =
                child.GetComponent<ProceduralTreeInstance>();
            if (instance == null)
            {
                instance = Undo.AddComponent<ProceduralTreeInstance>(child);
            }

            spawner.AttachGeneratedInstance(instance);
            return instance;
        }

        internal static void Clear(TreeRecipeSpawner spawner)
        {
            if (spawner == null)
            {
                return;
            }

            ProceduralTreeInstance instance = spawner.GeneratedInstance;
            if (instance != null)
            {
                Undo.DestroyObjectImmediate(instance.gameObject);
            }
            else
            {
                Transform child = spawner.transform.Find(
                    TreeRecipeSpawner.GeneratedChildName);
                if (child != null)
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }

            Undo.RecordObject(spawner, "Clear Recipe-Spawned Tree");
            spawner.AttachGeneratedInstance(null);
            spawner.RecordSpawn(false, string.Empty, string.Empty);
            EditorUtility.SetDirty(spawner);
            MarkSceneDirty(spawner.gameObject);
        }

        private static void MarkSceneDirty(GameObject gameObject)
        {
            if (gameObject != null && gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }
    }
}
