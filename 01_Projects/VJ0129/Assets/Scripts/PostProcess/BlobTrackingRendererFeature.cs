using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

/// <summary>
/// Blob Tracking Renderer Feature（URP RenderGraph対応）
/// Camera にアタッチされた BlobTrackingEffect の設定を使用
/// </summary>
public class BlobTrackingRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public Shader shader;
    }
    
    public Settings settings = new Settings();
    
    private BlobTrackingRenderPass renderPass;
    private Material material;
    
    public override void Create()
    {
        if (settings.shader == null)
        {
            settings.shader = Shader.Find("Hidden/BlobTracking");
        }
        
        if (settings.shader != null)
        {
            material = CoreUtils.CreateEngineMaterial(settings.shader);
            renderPass = new BlobTrackingRenderPass(material, settings.renderPassEvent);
        }
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material == null || renderPass == null) return;
        
        // カメラにアタッチされた BlobTrackingEffect を探す
        var camera = renderingData.cameraData.camera;
        var effect = camera.GetComponent<BlobTrackingEffect>();
        
        if (effect != null && effect.enabled && effect.intensity > 0)
        {
            renderPass.Setup(effect);
            renderer.EnqueuePass(renderPass);
        }
    }
    
    protected override void Dispose(bool disposing)
    {
        if (material != null)
        {
            CoreUtils.Destroy(material);
        }
        renderPass?.Dispose();
    }
}

/// <summary>
/// Blob Tracking レンダーパス（RenderGraph API対応）
/// </summary>
public class BlobTrackingRenderPass : ScriptableRenderPass
{
    private Material material;
    private BlobTrackingEffect effect;
    
    // シェーダープロパティID
    private static readonly int ThresholdId = Shader.PropertyToID("_Threshold");
    private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");
    private static readonly int BlurRadiusId = Shader.PropertyToID("_BlurRadius");
    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
    private static readonly int BlobColorId = Shader.PropertyToID("_BlobColor");
    private static readonly int BackgroundColorId = Shader.PropertyToID("_BackgroundColor");
    private static readonly int IterationsId = Shader.PropertyToID("_Iterations");
    private static readonly int EdgeWidthId = Shader.PropertyToID("_EdgeWidth");
    private static readonly int ContrastId = Shader.PropertyToID("_Contrast");
    private static readonly int InvertBlobId = Shader.PropertyToID("_InvertBlob");
    private static readonly int ShowEdgesId = Shader.PropertyToID("_ShowEdges");
    
    public BlobTrackingRenderPass(Material material, RenderPassEvent renderPassEvent)
    {
        this.material = material;
        this.renderPassEvent = renderPassEvent;
        requiresIntermediateTexture = true;
    }
    
    public void Setup(BlobTrackingEffect effect)
    {
        this.effect = effect;
    }
    
    private void UpdateMaterialProperties()
    {
        if (material == null || effect == null) return;
        
        material.SetFloat(ThresholdId, effect.threshold);
        material.SetFloat(SmoothnessId, effect.smoothness);
        material.SetFloat(BlurRadiusId, effect.blurRadius);
        material.SetFloat(IntensityId, effect.intensity);
        material.SetColor(BlobColorId, effect.blobColor);
        material.SetColor(BackgroundColorId, effect.backgroundColor);
        material.SetInt(IterationsId, effect.iterations);
        material.SetFloat(EdgeWidthId, effect.edgeWidth);
        material.SetFloat(ContrastId, effect.contrast);
        material.SetFloat(InvertBlobId, effect.invertBlob ? 1f : 0f);
        material.SetFloat(ShowEdgesId, effect.showEdges ? 1f : 0f);
    }
    
    private class PassData
    {
        public TextureHandle source;
        public TextureHandle destination;
        public Material material;
    }
    
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (material == null || effect == null) return;
        
        UpdateMaterialProperties();
        
        var resourceData = frameData.Get<UniversalResourceData>();
        
        if (resourceData.isActiveTargetBackBuffer)
            return;
        
        var source = resourceData.activeColorTexture;
        
        var destinationDesc = renderGraph.GetTextureDesc(source);
        destinationDesc.name = "_BlobTrackingTexture";
        destinationDesc.clearBuffer = false;
        
        TextureHandle destination = renderGraph.CreateTexture(destinationDesc);
        
        // Pass 1: エフェクト適用
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("BlobTracking Pass", out var passData))
        {
            passData.source = source;
            passData.destination = destination;
            passData.material = material;
            
            builder.UseTexture(source, AccessFlags.Read);
            builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
            
            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
            });
        }
        
        // Pass 2: コピーバック
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("BlobTracking CopyBack", out var passData))
        {
            passData.source = destination;
            
            builder.UseTexture(destination, AccessFlags.Read);
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
            
            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
            });
        }
    }
    
    [System.Obsolete("Use RecordRenderGraph instead.")]
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // レガシー互換用
    }
    
    public void Dispose()
    {
        // クリーンアップ
    }
}
