using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private bool IsAutomaticBirthSourcesDebugActive =>
            river != null &&
            river.FoamDebugView ==
                StylizedRiverFoamDebugView.AutomaticBirthSources;

        private void ResetAutomaticBirthDiagnosticSession()
        {
            automaticBirthDebugLiveAffectedTexels = 0;
            automaticBirthDebugReadbackAvailable = false;
            automaticBirthDebugSessionGeneration++;
        }

        private void EnsureAutomaticBirthDiagnosticResources()
        {
            if (!IsAutomaticBirthSourcesDebugActive ||
                computeShader == null ||
                fieldWidth <= 0 ||
                fieldHeight <= 0)
            {
                return;
            }

            if (automaticBirthDebugTexture == null ||
                !automaticBirthDebugTexture.IsCreated())
            {
                ReleaseTexture(ref automaticBirthDebugTexture);
                automaticBirthDebugTexture = CreateFieldTexture(
                    "PS3D_RiverFoam_AutomaticBirthDebug");
                ClearRenderTexture(automaticBirthDebugTexture);
            }

            if (automaticBirthDebugCounterBuffer == null)
            {
                automaticBirthDebugCounterBuffer = new ComputeBuffer(
                    AutomaticBirthDebugCounterCount,
                    sizeof(uint),
                    ComputeBufferType.Structured);
                System.Array.Clear(
                    automaticBirthDebugCounterReadback,
                    0,
                    automaticBirthDebugCounterReadback.Length);
                automaticBirthDebugCounterBuffer.SetData(
                    automaticBirthDebugCounterReadback);
                automaticBirthDebugResourceGeneration++;
            }
        }

        private void BeginAutomaticBirthDebugStep()
        {
            if (!IsAutomaticBirthSourcesDebugActive)
            {
                return;
            }

            EnsureAutomaticBirthDiagnosticResources();
            if (automaticBirthDebugTexture == null ||
                automaticBirthDebugCounterBuffer == null ||
                clearAutomaticBirthDebugAllKernel < 0)
            {
                return;
            }

            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetInt("_FoamRangeStart", 0);
            computeShader.SetInt("_FoamRangeCount", fieldWidth);
            computeShader.SetTexture(
                clearAutomaticBirthDebugAllKernel,
                "_FoamBirthDebugWrite",
                automaticBirthDebugTexture);
            computeShader.SetBuffer(
                clearAutomaticBirthDebugAllKernel,
                "_FoamBirthDebugCounters",
                automaticBirthDebugCounterBuffer);
            Dispatch(
                clearAutomaticBirthDebugAllKernel,
                fieldWidth,
                fieldHeight);
            automaticBirthDebugLiveAffectedTexels = 0;
            automaticBirthDebugReadbackAvailable = false;
        }

        private void EndAutomaticBirthDebugStep()
        {
            if (!IsAutomaticBirthSourcesDebugActive ||
                automaticBirthDebugCounterBuffer == null)
            {
                return;
            }

            RequestAutomaticBirthDebugReadback();
        }

        private void RequestAutomaticBirthDebugReadback()
        {
            if (automaticBirthDebugReadbackPending ||
                automaticBirthDebugCounterBuffer == null)
            {
                return;
            }

            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                automaticBirthDebugCounterBuffer.GetData(
                    automaticBirthDebugCounterReadback);
                ApplyAutomaticBirthDebugReadback(
                    automaticBirthDebugCounterReadback);
                return;
            }

            automaticBirthDebugReadbackPending = true;
            int generation = automaticBirthDebugResourceGeneration;
            int sessionGeneration = automaticBirthDebugSessionGeneration;
            ComputeBuffer requestedBuffer = automaticBirthDebugCounterBuffer;
            AsyncGPUReadback.Request(
                requestedBuffer,
                request =>
                {
                    if (this == null ||
                        generation != automaticBirthDebugResourceGeneration)
                    {
                        requestedBuffer?.Release();
                        return;
                    }

                    automaticBirthDebugReadbackPending = false;
                    if (sessionGeneration !=
                        automaticBirthDebugSessionGeneration)
                    {
                        return;
                    }
                    if (request.hasError)
                    {
                        automaticBirthDebugReadbackAvailable = false;
                        return;
                    }

                    var data = request.GetData<uint>();
                    int count = Mathf.Min(
                        data.Length,
                        automaticBirthDebugCounterReadback.Length);
                    for (int index = 0; index < count; index++)
                    {
                        automaticBirthDebugCounterReadback[index] = data[index];
                    }

                    ApplyAutomaticBirthDebugReadback(
                        automaticBirthDebugCounterReadback);
                });
        }

        private void ApplyAutomaticBirthDebugReadback(uint[] data)
        {
            if (data == null || data.Length < AutomaticBirthDebugCounterCount)
            {
                automaticBirthDebugReadbackAvailable = false;
                return;
            }

            automaticBirthDebugLiveAffectedTexels = data[0];
            automaticBirthDebugReadbackAvailable = true;
        }

        private void ReleaseAutomaticBirthDiagnosticResources()
        {
            ReleaseTexture(ref automaticBirthDebugTexture);
            automaticBirthDebugResourceGeneration++;

            if (automaticBirthDebugReadbackPending)
            {
                // The outstanding request callback owns this retired buffer
                // until the GPU copy completes.
                automaticBirthDebugCounterBuffer = null;
            }
            else
            {
                automaticBirthDebugCounterBuffer?.Release();
                automaticBirthDebugCounterBuffer = null;
            }

            automaticBirthDebugReadbackPending = false;
            automaticBirthDebugReadbackAvailable = false;
            automaticBirthDebugLiveAffectedTexels = 0;
            automaticBirthDebugActiveLastUpdate = false;
        }
    }
}
