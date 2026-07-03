using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Hidden Stage 6 runtime that owns the complete shared Foam network.
    /// Presence, Presence-weighted Remaining Life, and Presence-weighted
    /// Material Pattern are transported in one persistent state while a
    /// shared structural-resolution guidance field,
    /// GPU-only population controller, boundaries, Wake, and Impact activity organise
    /// that material into an evolving web-like tracer network.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StylizedRiver))]
    [AddComponentMenu("")]
    public sealed partial class StylizedRiverFoamRuntime : MonoBehaviour
    {

    }
}
