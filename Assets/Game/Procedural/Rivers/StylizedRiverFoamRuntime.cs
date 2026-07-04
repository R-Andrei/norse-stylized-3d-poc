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
    /// Hidden Stage 6 runtime that owns transient Foam material, topology,
    /// event-driven birth, conservative downstream transport, and lifecycle
    /// diagnostics. Persistent state stores Presence, Presence-weighted
    /// Remaining Life, and Presence-weighted Material Pattern. Topology changes
    /// the life clock; it does not continuously create, erase, spread, or steer
    /// material. Existing Foam follows river flow plus accepted physical
    /// wake/pressure disturbance motion.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StylizedRiver))]
    [AddComponentMenu("")]
    public sealed partial class StylizedRiverFoamRuntime : MonoBehaviour
    {

    }
}
