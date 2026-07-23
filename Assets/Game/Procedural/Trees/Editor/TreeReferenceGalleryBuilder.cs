using ProgrammaticStylized3D.Geometry.Ground;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    internal static class TreeReferenceGalleryBuilder
    {
        private const string CreateMenuPath =
            "GameObject/PS3D/Trees/Tree Reference Gallery (Standalone)";
        private const string GalleryObjectName = "Tree Reference Gallery";

        [MenuItem(CreateMenuPath, false, 20)]
        private static void CreateStandaloneGallery()
        {
            GeneratedGround selectedGround = ResolveGroundFromSelection();
            var galleryObject = new GameObject(GalleryObjectName);
            Undo.RegisterCreatedObjectUndo(
                galleryObject,
                "Create Standalone Tree Reference Gallery");

            var gallery = Undo.AddComponent<TreeReferenceGallery>(galleryObject);
            if (selectedGround != null)
            {
                PlaceAsGroundSibling(gallery, selectedGround, true);
            }
            Selection.activeGameObject = galleryObject;
            EditorGUIUtility.PingObject(galleryObject);
            MarkSceneDirty(galleryObject);
        }

        internal static bool AssignClosestGround(
            TreeReferenceGallery gallery,
            out string result)
        {
            result = string.Empty;
            if (gallery == null)
            {
                result = "Tree Reference Gallery is missing.";
                return false;
            }

            GeneratedGround[] grounds =
                Object.FindObjectsByType<GeneratedGround>(
                    FindObjectsInactive.Include);
            if (grounds.Length == 0)
            {
                result = "No GeneratedGround exists in the loaded scene.";
                return false;
            }

            GeneratedGround closest = null;
            float closestDistanceSquared = float.PositiveInfinity;
            Vector3 galleryPosition = gallery.transform.position;
            for (int index = 0; index < grounds.Length; index++)
            {
                GeneratedGround candidate = grounds[index];
                if (candidate == null ||
                    candidate.gameObject.scene != gallery.gameObject.scene)
                {
                    continue;
                }

                float distanceSquared =
                    (candidate.transform.position - galleryPosition).sqrMagnitude;
                if (distanceSquared >= closestDistanceSquared)
                {
                    continue;
                }

                closest = candidate;
                closestDistanceSquared = distanceSquared;
            }

            if (closest == null)
            {
                result =
                    "No GeneratedGround exists in the Tree Reference Gallery scene.";
                return false;
            }

            Undo.RecordObject(gallery, "Assign Closest Tree Reference Ground");
            gallery.SetReferenceGround(closest);
            EditorUtility.SetDirty(gallery);
            MarkSceneDirty(gallery.gameObject);
            result = $"Assigned closest Ground: {GetHierarchyPath(closest.transform)}";
            return true;
        }

        internal static bool PlaceBesideAssignedGround(
            TreeReferenceGallery gallery,
            out string result)
        {
            result = string.Empty;
            if (gallery == null)
            {
                result = "Tree Reference Gallery is missing.";
                return false;
            }

            GeneratedGround ground = gallery.ReferenceGround;
            if (ground == null)
            {
                result = "Assign a Reference Ground first.";
                return false;
            }

            if (ground.gameObject.scene != gallery.gameObject.scene)
            {
                result =
                    "The gallery and assigned Ground must be in the same scene.";
                return false;
            }

            PlaceAsGroundSibling(gallery, ground, false);
            result =
                "Placed the gallery as an independent sibling/root object beside " +
                GetHierarchyPath(ground.transform) + ".";
            return true;
        }

        private static GeneratedGround ResolveGroundFromSelection()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected != null)
            {
                GeneratedGround direct = selected.GetComponent<GeneratedGround>();
                if (direct != null)
                {
                    return direct;
                }

                GeneratedGround ancestor =
                    selected.GetComponentInParent<GeneratedGround>(true);
                if (ancestor != null)
                {
                    return ancestor;
                }
            }

            GeneratedGround[] grounds =
                Object.FindObjectsByType<GeneratedGround>(
                    FindObjectsInactive.Include);
            GeneratedGround onlyGround = null;
            for (int index = 0; index < grounds.Length; index++)
            {
                GeneratedGround candidate = grounds[index];
                if (candidate == null)
                {
                    continue;
                }

                if (onlyGround != null)
                {
                    return null;
                }

                onlyGround = candidate;
            }

            return onlyGround;
        }

        private static void PlaceAsGroundSibling(
            TreeReferenceGallery gallery,
            GeneratedGround ground,
            bool assignGround)
        {
            Transform galleryTransform = gallery.transform;
            Transform groundTransform = ground.transform;
            Transform intendedParent = groundTransform.parent;

            Undo.SetTransformParent(
                galleryTransform,
                intendedParent,
                "Place Tree Reference Gallery Beside Ground");
            Undo.RecordObject(
                galleryTransform,
                "Align Tree Reference Gallery With Ground");
            galleryTransform.SetPositionAndRotation(
                groundTransform.position,
                groundTransform.rotation);
            galleryTransform.localScale = Vector3.one;

            int targetSiblingIndex = groundTransform.GetSiblingIndex() + 1;
            galleryTransform.SetSiblingIndex(targetSiblingIndex);

            if (assignGround)
            {
                Undo.RecordObject(gallery, "Assign Tree Reference Ground");
                gallery.SetReferenceGround(ground);
                EditorUtility.SetDirty(gallery);
            }

            MarkSceneDirty(gallery.gameObject);
        }

        private static void MarkSceneDirty(GameObject gameObject)
        {
            if (gameObject != null && gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }

        internal static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return "None";
            }

            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }
}
