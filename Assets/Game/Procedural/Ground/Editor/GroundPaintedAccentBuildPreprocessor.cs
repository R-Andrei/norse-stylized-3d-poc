using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace ProgrammaticStylized3D.Geometry.Ground.Editor
{
    internal sealed class GroundPaintedAccentBuildPreprocessor :
        IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            GroundPaintedAccentProductionValidationReport validation =
                GroundPaintedAccentProductionValidator
                    .ValidateEnabledBuildScenes();
            if (!validation.IsValid)
            {
                throw new BuildFailedException(
                    validation.BuildSummary());
            }
        }
    }
}
