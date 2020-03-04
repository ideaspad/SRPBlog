using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[ExecuteInEditMode]
public class TransparentAssetPipe : RenderPipelineAsset
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("SRP-Demo/03 - Create Transparent Asset Pipeline")]
    static void CreateBasicAssetPipeline()
    {
        var instance = ScriptableObject.CreateInstance<TransparentAssetPipe>();
        UnityEditor.AssetDatabase.CreateAsset(instance,
            "Assets/SRP-Demo/3-TransparentAssetPipe/TransparentAssetPipe.asset");
    }
#endif

    protected override RenderPipeline CreatePipeline()
    {
        return new TransparentAssetPipeInstance();
    }
}

public class TransparentAssetPipeInstance : RenderPipeline
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
                commandBuffer.ClearRenderTarget(true, true, Color.cyan);
                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Release();
            }
            CullingResults cullingResults = context.Cull(ref scriptableCullingParameters);

            SortingSettings sortingSettings = new SortingSettings(camera);
            DrawingSettings drawingSettings =
                new DrawingSettings(new ShaderTagId("BasicPass"), sortingSettings);
            sortingSettings.criteria = SortingCriteria.CommonOpaque;
            FilteringSettings filteringSettings = FilteringSettings.defaultValue;
            filteringSettings.renderQueueRange = RenderQueueRange.opaque;
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            context.DrawSkybox(camera);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            EndCameraRendering(context, camera);
        }

        context.Submit();
        EndFrameRendering(context, cameras);
    }
}