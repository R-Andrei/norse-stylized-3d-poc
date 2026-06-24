using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry
{
    /// <summary>
    /// Event-driven registry for active procedural geometry sources.
    /// Consumers can cheaply reject unrelated sources and derive cached data
    /// only when geometry appears, disappears, or actually changes.
    /// </summary>
    public static class GeneratedGeometryRegistry
    {
        private static readonly HashSet<IGeneratedGeometrySource> sources =
            new();
        private static readonly Dictionary<IGeneratedGeometrySource, Action>
            changeHandlers = new();

        public static event Action<IGeneratedGeometrySource> SourceAdded;
        public static event Action<IGeneratedGeometrySource> SourceRemoved;
        public static event Action<IGeneratedGeometrySource> SourceChanged;

        public static IReadOnlyCollection<IGeneratedGeometrySource> Sources =>
            sources;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntimeState()
        {
            foreach (KeyValuePair<IGeneratedGeometrySource, Action> pair
                     in changeHandlers)
            {
                if (pair.Key == null ||
                    (pair.Key is UnityEngine.Object unityObject &&
                     unityObject == null))
                {
                    continue;
                }

                pair.Key.GeometryChanged -= pair.Value;
            }

            sources.Clear();
            changeHandlers.Clear();
            SourceAdded = null;
            SourceRemoved = null;
            SourceChanged = null;
        }

        public static bool Contains(IGeneratedGeometrySource source)
        {
            return source != null && sources.Contains(source);
        }

        public static void Register(IGeneratedGeometrySource source)
        {
            if (source == null || !sources.Add(source))
            {
                return;
            }

            Action changeHandler = () => NotifyChanged(source);
            changeHandlers[source] = changeHandler;
            source.GeometryChanged += changeHandler;
            SourceAdded?.Invoke(source);
        }

        public static void Unregister(IGeneratedGeometrySource source)
        {
            if (source == null || !sources.Remove(source))
            {
                return;
            }
            

            if (changeHandlers.TryGetValue(
                    source,
                    out Action changeHandler))
            {
                source.GeometryChanged -= changeHandler;
                changeHandlers.Remove(source);
            }

            SourceRemoved?.Invoke(source);
        }

        public static void NotifyChanged(IGeneratedGeometrySource source)
        {
            if (source == null || !sources.Contains(source))
            {
                return;
            }

            SourceChanged?.Invoke(source);
        }

        public static void CopySourcesTo(
            List<IGeneratedGeometrySource> target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            target.Clear();
            target.AddRange(sources);
        }
    }
}
