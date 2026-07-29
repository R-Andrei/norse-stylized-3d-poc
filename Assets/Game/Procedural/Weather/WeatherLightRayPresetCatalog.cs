using UnityEngine;

namespace ProgrammaticStylized3D.Weather
{
    [CreateAssetMenu(
        fileName = "WeatherLightRayPresetCatalog",
        menuName = "PS3D/Weather/LightRay Preset Catalog")]
    public sealed class WeatherLightRayPresetCatalog : ScriptableObject
    {
        [SerializeField] private WeatherLightRayPreset defaultSun;
        [SerializeField] private WeatherLightRayPreset sunClear;
        [SerializeField] private WeatherLightRayPreset sunIntense;
        [SerializeField] private WeatherLightRayPreset sunHazy;
        [SerializeField] private WeatherLightRayPreset moonCold;
        [SerializeField] private WeatherLightRayPreset moonWhite;
        [SerializeField] private WeatherLightRayPreset moonSubtle;
        [SerializeField] private WeatherLightRayPreset bloodMoon;

        public WeatherLightRayPreset DefaultSun => defaultSun;
        public WeatherLightRayPreset SunClear => sunClear;
        public WeatherLightRayPreset SunIntense => sunIntense;
        public WeatherLightRayPreset SunHazy => sunHazy;
        public WeatherLightRayPreset MoonCold => moonCold;
        public WeatherLightRayPreset MoonWhite => moonWhite;
        public WeatherLightRayPreset MoonSubtle => moonSubtle;
        public WeatherLightRayPreset BloodMoon => bloodMoon;
    }
}
