using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.Universal;

[ExecuteInEditMode]
public class OpaqueAssetPipe : RenderPipelineAsset
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("SRP-Demo/02 - Create Opaque Asset Pipeline")]
    static void CreateBasicAssetPipeline()
    {
        var instance = ScriptableObject.CreateInstance<OpaqueAssetPipe>();
        UnityEditor.AssetDatabase.CreateAsset(instance, "Assets/SRP-Demo/2-OpaqueAssetPipe/OpaqueAssetPipe.asset");
    }
#endif

    protected override RenderPipeline CreatePipeline()
    {
        return new OpaqueAssetPipeInstance();
    }
}

public class OpaqueAssetPipeInstance : RenderPipeline
{
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        BeginFrameRendering(context, cameras);

        ScriptableCullingParameters scriptableCullingParameters;
        foreach (var camera in cameras)
        {
            if (!camera.TryGetCullingParameters(out scriptableCullingParameters)) 
                continue;
            BeginCameraRendering(context, camera);
            
            context.SetupCameraProperties(camera);
            {
                CommandBuffer commandBuffer = new CommandBuffer();
                commandBuffer.ClearRenderTarget(true, true, Color.black);
                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Release();
            }

            CullingResults cullingResults = context.Cull(ref scriptableCullingParameters);
            
            DrawingSettings drawingSettings = new DrawingSettings(new ShaderTagId("BasicPass"), new SortingSettings(camera));
            FilteringSettings filteringSettings = FilteringSettings.defaultValue;
            filteringSettings.renderQueueRange = RenderQueueRange.opaque;
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            context.DrawSkybox(camera);

            EndCameraRendering(context, camera);
        }
        context.Submit();
        EndFrameRendering(context, cameras);
    }
}