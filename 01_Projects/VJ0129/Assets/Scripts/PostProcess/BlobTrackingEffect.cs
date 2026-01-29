using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Blob Tracking エフェクト
/// MainCameraにアタッチするだけで映像に反映
/// TouchDesigner の Blob Track TOP 風の効果
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class BlobTrackingEffect : MonoBehaviour
{
    [Header("基本設定")]
    [Tooltip("エフェクトの強度")]
    [Range(0f, 1f)]
    public float intensity = 1f;
    
    [Tooltip("輝度のしきい値")]
    [Range(0f, 1f)]
    public float threshold = 0.5f;
    
    [Tooltip("しきい値の滑らかさ")]
    [Range(0.001f, 0.5f)]
    public float smoothness = 0.1f;
    
    [Header("ブロブ設定")]
    [Tooltip("ブラーの半径")]
    [Range(0.1f, 10f)]
    public float blurRadius = 2f;
    
    [Tooltip("ブラーの反復回数")]
    [Range(1, 16)]
    public int iterations = 4;
    
    [Tooltip("コントラスト")]
    [Range(0.5f, 5f)]
    public float contrast = 1.5f;
    
    [Tooltip("ブロブを反転")]
    public bool invertBlob = false;
    
    [Header("色設定")]
    [Tooltip("ブロブの色")]
    public Color blobColor = Color.white;
    
    [Tooltip("背景の色")]
    public Color backgroundColor = Color.black;
    
    [Header("エッジ表示")]
    [Tooltip("エッジを表示")]
    public bool showEdges = true;
    
    [Tooltip("エッジの太さ")]
    [Range(0.5f, 5f)]
    public float edgeWidth = 1.5f;
    
    private Material material;
    private Shader shader;
    
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
    
    void OnEnable()
    {
        CreateMaterial();
    }
    
    void OnDisable()
    {
        if (material != null)
        {
            if (Application.isPlaying)
                Destroy(material);
            else
                DestroyImmediate(material);
        }
    }
    
    private void CreateMaterial()
    {
        if (shader == null)
        {
            shader = Shader.Find("Hidden/BlobTracking");
        }
        
        if (shader != null && material == null)
        {
            material = new Material(shader);
            material.hideFlags = HideFlags.HideAndDontSave;
        }
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material == null)
        {
            CreateMaterial();
        }
        
        if (material == null || intensity <= 0)
        {
            Graphics.Blit(source, destination);
            return;
        }
        
        // パラメータを設定
        material.SetFloat(ThresholdId, threshold);
        material.SetFloat(SmoothnessId, smoothness);
        material.SetFloat(BlurRadiusId, blurRadius);
        material.SetFloat(IntensityId, intensity);
        material.SetColor(BlobColorId, blobColor);
        material.SetColor(BackgroundColorId, backgroundColor);
        material.SetInt(IterationsId, iterations);
        material.SetFloat(EdgeWidthId, edgeWidth);
        material.SetFloat(ContrastId, contrast);
        material.SetFloat(InvertBlobId, invertBlob ? 1f : 0f);
        material.SetFloat(ShowEdgesId, showEdges ? 1f : 0f);
        
        // エフェクトを適用
        Graphics.Blit(source, destination, material, 0);
    }
}
