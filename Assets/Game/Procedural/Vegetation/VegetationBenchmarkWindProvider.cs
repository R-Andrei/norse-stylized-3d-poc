using System;
using ProgrammaticStylized3D.Weather;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation
{
    /// <summary>
    /// Migration-only compatibility component for the existing benchmark scene.
    /// The rejected vegetation-owned analytical wind implementation has been removed.
    /// New scenes should use WeatherWindDomain directly.
    /// </summary>
    [Obsolete("Use ProgrammaticStylized3D.Weather.WeatherWindDomain.")]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public sealed class VegetationBenchmarkWindProvider : WeatherWindDomain
    {
    }
}
