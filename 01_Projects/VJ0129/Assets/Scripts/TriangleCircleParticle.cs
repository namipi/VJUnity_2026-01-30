using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 三角形メッシュを3D空間に配置して回転させるパーティクルエフェクト
/// VFX Graphのようにエディタでもプレビュー可能
/// </summary>
[ExecuteAlways]
public class TriangleCircleParticle : MonoBehaviour
{
    public enum DistributionMode
    {
        Circle2D,       // 2D円形配置
        Sphere3D,       // 3D球面配置
        Helix3D,        // 3Dらせん配置
        Torus3D         // 3Dトーラス配置
    }

    [Header("配置モード")]
    [Tooltip("パーティクルの配置モード")]
    public DistributionMode distributionMode = DistributionMode.Sphere3D;

    [Header("基本設定")]
    [Tooltip("円/球の半径")]
    public float radius = 2f;

    [Tooltip("三角形の数")]
    [Range(3, 200)]
    public int triangleCount = 24;

    [Header("三角形の設定")]
    [Tooltip("三角形のサイズ")]
    public float triangleSize = 0.3f;

    [Tooltip("三角形のマテリアル")]
    public Material triangleMaterial;

    [Header("3D回転設定")]
    [Tooltip("X軸回転速度 (度/秒)")]
    public float rotationSpeedX = 15f;

    [Tooltip("Y軸回転速度 (度/秒)")]
    public float rotationSpeedY = 30f;

    [Tooltip("Z軸回転速度 (度/秒)")]
    public float rotationSpeedZ = 10f;

    [Tooltip("軌道傾斜角（度）")]
    [Range(0f, 90f)]
    public float orbitTilt = 30f;

    [Tooltip("らせんの高さ（Helixモード用）")]
    public float helixHeight = 3f;

    [Tooltip("らせんの巻き数（Helixモード用）")]
    public float helixTurns = 2f;

    [Tooltip("トーラスの管の半径（Torusモード用）")]
    public float torusTubeRadius = 0.5f;

    [Header("アニメーション設定")]
    [Tooltip("各三角形の自転速度 (度/秒)")]
    public float triangleRotationSpeed = 90f;

    [Tooltip("3D自転を有効化")]
    public bool enable3DSpin = true;

    [Tooltip("自転軸のランダム化")]
    public bool randomSpinAxis = true;

    [Tooltip("パルスアニメーションを有効化")]
    public bool enablePulse = true;

    [Tooltip("パルスの強さ")]
    public float pulseIntensity = 0.2f;

    [Tooltip("パルスの速度")]
    public float pulseSpeed = 2f;

    [Header("色設定")]
    [Tooltip("グラデーション開始色")]
    public Color startColor = Color.cyan;

    [Tooltip("グラデーション終了色")]
    public Color endColor = Color.magenta;

    [Tooltip("色のアニメーション速度")]
    public float colorAnimationSpeed = 1f;

    [Header("無重力・速度感設定")]
    [Tooltip("浮遊感のSin波揺れ")]
    public bool enableFloat = true;

    [Tooltip("浮遊の振幅")]
    [Range(0f, 2f)]
    public float floatAmplitude = 0.3f;

    [Tooltip("浮遊の速度")]
    [Range(0.1f, 5f)]
    public float floatSpeed = 1.5f;

    [Tooltip("ドリフト（方向への漂い）")]
    public bool enableDrift = true;

    [Tooltip("ドリフトの強さ")]
    [Range(0f, 1f)]
    public float driftIntensity = 0.2f;

    [Tooltip("ドリフトの速度")]
    [Range(0.1f, 3f)]
    public float driftSpeed = 0.8f;

    [Tooltip("軌道のランダム変動")]
    public bool enableOrbitVariation = true;

    [Tooltip("軌道変動の強さ")]
    [Range(0f, 1f)]
    public float orbitVariationIntensity = 0.15f;

    [Tooltip("軌道変動の速度")]
    [Range(0.1f, 5f)]
    public float orbitVariationSpeed = 2f;

    [Tooltip("奥行き揺れ")]
    public bool enableDepthMotion = true;

    [Tooltip("奥行き揺れの振幅")]
    [Range(0f, 2f)]
    public float depthAmplitude = 0.5f;

    [Tooltip("奥行き揺れの速度")]
    [Range(0.1f, 3f)]
    public float depthSpeed = 1.2f;

    [Tooltip("慣性・加速回転（スピルアウト感）")]
    public bool enableMomentum = false;

    [Tooltip("加速度（正で外向き、負で内向き）")]
    [Range(-2f, 2f)]
    public float radialAcceleration = 0.3f;

    [Tooltip("三角形ごとのサイズ変動")]
    public bool enableSizeVariation = true;

    [Tooltip("サイズ変動の強さ")]
    [Range(0f, 0.5f)]
    public float sizeVariationIntensity = 0.2f;

    [Tooltip("サイズ変動の速度")]
    [Range(0.1f, 5f)]
    public float sizeVariationSpeed = 1.5f;

    [Tooltip("個別の自転速度バリエーション")]
    public bool enableSpinVariation = true;

    [Tooltip("自転速度のランダム範囲")]
    [Range(0f, 2f)]
    public float spinVariationRange = 0.5f;

    [Header("エディタプレビュー設定")]
    [Tooltip("エディタでアニメーションをプレビュー")]
    public bool previewInEditor = true;

    [Tooltip("エディタでのアニメーション速度倍率")]
    [Range(0.1f, 3f)]
    public float editorTimeScale = 1f;

    // 生成された三角形オブジェクトの配列
    private GameObject[] triangles;
    private MeshRenderer[] meshRenderers;
    private MaterialPropertyBlock[] propertyBlocks;

    // 各三角形のランダムオフセット（無重力感用）
    private float[] randomOffsets;
    private float[] spinMultipliers;
    private float momentumRadius;

    // 3D用の追加変数
    private Vector3[] spinAxes;           // 各三角形のランダム自転軸
    private Vector3[] basePositions;      // 初期配置位置
    private float[] theta;                // 球面座標の緯度
    private float[] phi;                  // 球面座標の経度

    // エディタ用の変数
    private int lastTriangleCount;
    private float lastRadius;
    private float lastTriangleSize;
    private DistributionMode lastDistributionMode;
    private double editorTime;
    private double lastEditorTime;

    void OnEnable()
    {
        CreateTriangles();
        lastTriangleCount = triangleCount;
        lastRadius = radius;
        lastTriangleSize = triangleSize;

#if UNITY_EDITOR
        EditorApplication.update += EditorUpdate;
        lastEditorTime = EditorApplication.timeSinceStartup;
#endif
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= EditorUpdate;
#endif
        CleanupTriangles();
    }

#if UNITY_EDITOR
    private void EditorUpdate()
    {
        if (!previewInEditor || Application.isPlaying) return;
        
        // エディタ用のdeltaTime計算
        double currentTime = EditorApplication.timeSinceStartup;
        float deltaTime = (float)(currentTime - lastEditorTime) * editorTimeScale;
        lastEditorTime = currentTime;
        editorTime += deltaTime;
        
        // パラメータ変更を検出して再生成
        if (triangleCount != lastTriangleCount || 
            !Mathf.Approximately(radius, lastRadius) || 
            !Mathf.Approximately(triangleSize, lastTriangleSize))
        {
            CreateTriangles();
            lastTriangleCount = triangleCount;
            lastRadius = radius;
            lastTriangleSize = triangleSize;
        }
        
        AnimateTrianglesEditor(deltaTime, (float)editorTime);
        
        // シーンビューを更新
        SceneView.RepaintAll();
    }
#endif

    void Start()
    {
        if (Application.isPlaying)
        {
            CreateTriangles();
        }
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            AnimateTriangles();
        }
    }

    /// <summary>
    /// 三角形メッシュを生成して3D空間に配置
    /// </summary>
    private void CreateTriangles()
    {
        // 既存の三角形を全て削除（配列参照が失われた場合も含めて子オブジェクトを全削除）
        CleanupAllTriangleChildren();

        if (triangles != null)
        {
            triangles = null;
            meshRenderers = null;
            propertyBlocks = null;
            randomOffsets = null;
            spinMultipliers = null;
        }

        triangles = new GameObject[triangleCount];
        meshRenderers = new MeshRenderer[triangleCount];
        propertyBlocks = new MaterialPropertyBlock[triangleCount];
        randomOffsets = new float[triangleCount];
        spinMultipliers = new float[triangleCount];
        spinAxes = new Vector3[triangleCount];
        basePositions = new Vector3[triangleCount];
        theta = new float[triangleCount];
        phi = new float[triangleCount];
        momentumRadius = radius;

        // ランダム値を生成（シード固定で再現性確保）
        Random.InitState(42);
        for (int i = 0; i < triangleCount; i++)
        {
            randomOffsets[i] = Random.Range(0f, Mathf.PI * 2f);
            spinMultipliers[i] = 1f + Random.Range(-spinVariationRange, spinVariationRange);

            // ランダム自転軸を生成
            if (randomSpinAxis)
            {
                spinAxes[i] = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)
                ).normalized;
            }
            else
            {
                spinAxes[i] = Vector3.forward;
            }
        }

        // デフォルトマテリアルがない場合は作成
        if (triangleMaterial == null)
        {
            triangleMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (triangleMaterial.shader == null)
            {
                triangleMaterial = new Material(Shader.Find("Standard"));
            }
        }

        for (int i = 0; i < triangleCount; i++)
        {
            // 三角形オブジェクトを作成
            GameObject triObj = new GameObject($"Triangle_{i}");
            triObj.transform.SetParent(transform);

            // 配置モードに応じた位置計算
            Vector3 position = CalculatePosition(i, triangleCount, radius);
            basePositions[i] = position;
            triObj.transform.localPosition = position;

            // 中心に向けて回転（3Dの場合は外向き）
            if (distributionMode == DistributionMode.Circle2D)
            {
                float angle = (360f / triangleCount) * i;
                triObj.transform.localRotation = Quaternion.Euler(0, 0, angle - 90f);
            }
            else
            {
                // 3Dの場合は球の中心から外向きに
                triObj.transform.localRotation = Quaternion.LookRotation(position.normalized, Vector3.up);
            }

            // メッシュを追加
            MeshFilter meshFilter = triObj.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateTriangleMesh();

            MeshRenderer meshRenderer = triObj.AddComponent<MeshRenderer>();
            meshRenderer.material = triangleMaterial;

            triangles[i] = triObj;
            meshRenderers[i] = meshRenderer;
            propertyBlocks[i] = new MaterialPropertyBlock();
        }
    }

    /// <summary>
    /// 配置モードに応じた位置を計算
    /// </summary>
    private Vector3 CalculatePosition(int index, int total, float r)
    {
        float t = (float)index / total;

        switch (distributionMode)
        {
            case DistributionMode.Circle2D:
                {
                    float angle = t * Mathf.PI * 2f;
                    return new Vector3(
                        Mathf.Cos(angle) * r,
                        Mathf.Sin(angle) * r,
                        0
                    );
                }

            case DistributionMode.Sphere3D:
                {
                    // フィボナッチ球面配置（均等分布）
                    float goldenRatio = (1f + Mathf.Sqrt(5f)) / 2f;
                    float thetaVal = 2f * Mathf.PI * index / goldenRatio;
                    float phiVal = Mathf.Acos(1f - 2f * (index + 0.5f) / total);

                    theta[index] = thetaVal;
                    phi[index] = phiVal;

                    return new Vector3(
                        Mathf.Sin(phiVal) * Mathf.Cos(thetaVal) * r,
                        Mathf.Cos(phiVal) * r,
                        Mathf.Sin(phiVal) * Mathf.Sin(thetaVal) * r
                    );
                }

            case DistributionMode.Helix3D:
                {
                    float angle = t * Mathf.PI * 2f * helixTurns;
                    float height = (t - 0.5f) * helixHeight;

                    return new Vector3(
                        Mathf.Cos(angle) * r,
                        height,
                        Mathf.Sin(angle) * r
                    );
                }

            case DistributionMode.Torus3D:
                {
                    float majorAngle = t * Mathf.PI * 2f;
                    float minorAngle = t * Mathf.PI * 2f * 8f; // 8回巻き

                    float x = (r + torusTubeRadius * Mathf.Cos(minorAngle)) * Mathf.Cos(majorAngle);
                    float y = torusTubeRadius * Mathf.Sin(minorAngle);
                    float z = (r + torusTubeRadius * Mathf.Cos(minorAngle)) * Mathf.Sin(majorAngle);

                    return new Vector3(x, y, z);
                }

            default:
                return Vector3.zero;
        }
    }

    /// <summary>
    /// 三角形のメッシュを生成
    /// </summary>
    private Mesh CreateTriangleMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Triangle";

        // 正三角形の頂点 (中心が原点)
        float height = triangleSize * Mathf.Sqrt(3) / 2f;
        Vector3[] vertices = new Vector3[3]
        {
            new Vector3(0, height * 2f / 3f, 0),           // 上頂点
            new Vector3(-triangleSize / 2f, -height / 3f, 0), // 左下
            new Vector3(triangleSize / 2f, -height / 3f, 0)   // 右下
        };

        // 両面表示のため、2つの三角形を定義
        int[] trianglesFront = new int[3] { 0, 1, 2 };
        int[] trianglesBack = new int[3] { 0, 2, 1 };
        int[] allTriangles = new int[6] { 0, 1, 2, 0, 2, 1 };

        // 法線
        Vector3[] normals = new Vector3[3]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };

        // UV座標
        Vector2[] uvs = new Vector2[3]
        {
            new Vector2(0.5f, 1f),
            new Vector2(0f, 0f),
            new Vector2(1f, 0f)
        };

        mesh.vertices = vertices;
        mesh.triangles = allTriangles;
        mesh.normals = normals;
        mesh.uv = uvs;

        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// 三角形のアニメーション更新（3D対応）
    /// </summary>
    private void AnimateTriangles()
    {
        if (triangles == null || randomOffsets == null || spinAxes == null) return;

        float deltaTime = Time.deltaTime;
        float time = Time.time;

        // 3軸回転（全体）
        Vector3 rotationDelta = new Vector3(
            rotationSpeedX * deltaTime,
            rotationSpeedY * deltaTime,
            rotationSpeedZ * deltaTime
        );
        transform.Rotate(rotationDelta, Space.Self);

        // 慣性（モメンタム）による半径変動
        if (enableMomentum)
        {
            momentumRadius += radialAcceleration * deltaTime;
            momentumRadius = Mathf.Clamp(momentumRadius, radius * 0.3f, radius * 3f);
        }
        else
        {
            momentumRadius = radius;
        }

        for (int i = 0; i < triangles.Length; i++)
        {
            if (triangles[i] == null) continue;

            float offset = randomOffsets[i];

            // 基本半径（パルス + モメンタム）
            float currentRadius = momentumRadius;
            if (enablePulse)
            {
                float pulse = 1f + Mathf.Sin(time * pulseSpeed + offset) * pulseIntensity;
                currentRadius *= pulse;
            }

            // 軌道変動
            if (enableOrbitVariation)
            {
                float orbitOffset = Mathf.Sin(time * orbitVariationSpeed + offset * 2f) * orbitVariationIntensity;
                currentRadius *= (1f + orbitOffset);
            }

            // 配置モードに応じた位置を再計算
            Vector3 basePos = CalculatePosition(i, triangleCount, currentRadius);
            float x = basePos.x;
            float y = basePos.y;
            float z = basePos.z;

            // 浮遊感
            if (enableFloat)
            {
                float floatOffset = Mathf.Sin(time * floatSpeed + offset) * floatAmplitude;
                // 3Dモードでは各軸に分散
                if (distributionMode != DistributionMode.Circle2D)
                {
                    x += Mathf.Sin(time * floatSpeed * 1.1f + offset) * floatAmplitude * 0.3f;
                    z += Mathf.Cos(time * floatSpeed * 0.9f + offset) * floatAmplitude * 0.3f;
                }
                y += floatOffset;
            }

            // ドリフト
            if (enableDrift)
            {
                float driftX = Mathf.Sin(time * driftSpeed + offset * 1.5f) * driftIntensity;
                float driftY = Mathf.Cos(time * driftSpeed * 0.7f + offset) * driftIntensity * 0.5f;
                float driftZ = Mathf.Sin(time * driftSpeed * 0.5f + offset * 0.8f) * driftIntensity * 0.3f;
                x += driftX;
                y += driftY;
                z += driftZ;
            }

            // 奥行き揺れ（3Dモードでは半径方向に）
            if (enableDepthMotion)
            {
                float depthOffset = Mathf.Sin(time * depthSpeed + offset * 0.8f) * depthAmplitude;
                if (distributionMode == DistributionMode.Circle2D)
                {
                    z += depthOffset;
                }
                else
                {
                    // 球面配置では放射方向に
                    Vector3 radialDir = basePos.normalized;
                    x += radialDir.x * depthOffset;
                    y += radialDir.y * depthOffset;
                    z += radialDir.z * depthOffset;
                }
            }

            triangles[i].transform.localPosition = new Vector3(x, y, z);

            // 3D自転
            float spinSpeed = triangleRotationSpeed;
            if (enableSpinVariation)
            {
                spinSpeed *= spinMultipliers[i];
            }

            if (enable3DSpin && randomSpinAxis)
            {
                triangles[i].transform.Rotate(spinAxes[i], spinSpeed * deltaTime, Space.Self);
            }
            else
            {
                triangles[i].transform.Rotate(Vector3.forward, spinSpeed * deltaTime, Space.Self);
            }

            // サイズ変動
            if (enableSizeVariation)
            {
                float sizeScale = 1f + Mathf.Sin(time * sizeVariationSpeed + offset) * sizeVariationIntensity;
                triangles[i].transform.localScale = Vector3.one * sizeScale;
            }

            // 色のアニメーション
            if (meshRenderers[i] != null)
            {
                float t = (Mathf.Sin(time * colorAnimationSpeed + i * (Mathf.PI * 2f / triangleCount)) + 1f) / 2f;
                Color currentColor = Color.Lerp(startColor, endColor, t);

                meshRenderers[i].GetPropertyBlock(propertyBlocks[i]);
                propertyBlocks[i].SetColor("_BaseColor", currentColor);
                propertyBlocks[i].SetColor("_Color", currentColor);
                propertyBlocks[i].SetColor("_EmissionColor", currentColor * 0.5f);
                meshRenderers[i].SetPropertyBlock(propertyBlocks[i]);
            }
        }
    }

    /// <summary>
    /// エディタ用アニメーション更新（deltaTimeとtimeを外部から受け取る）（3D対応）
    /// </summary>
    private void AnimateTrianglesEditor(float deltaTime, float time)
    {
        if (triangles == null || randomOffsets == null || spinAxes == null) return;

        // 3軸回転（全体）
        Vector3 rotationDelta = new Vector3(
            rotationSpeedX * deltaTime,
            rotationSpeedY * deltaTime,
            rotationSpeedZ * deltaTime
        );
        transform.Rotate(rotationDelta, Space.Self);

        // 慣性（モメンタム）による半径変動
        if (enableMomentum)
        {
            momentumRadius += radialAcceleration * deltaTime;
            momentumRadius = Mathf.Clamp(momentumRadius, radius * 0.3f, radius * 3f);
        }
        else
        {
            momentumRadius = radius;
        }

        for (int i = 0; i < triangles.Length; i++)
        {
            if (triangles[i] == null) continue;

            float offset = randomOffsets[i];

            // 基本半径（パルス + モメンタム）
            float currentRadius = momentumRadius;
            if (enablePulse)
            {
                float pulse = 1f + Mathf.Sin(time * pulseSpeed + offset) * pulseIntensity;
                currentRadius *= pulse;
            }

            // 軌道変動
            if (enableOrbitVariation)
            {
                float orbitOffset = Mathf.Sin(time * orbitVariationSpeed + offset * 2f) * orbitVariationIntensity;
                currentRadius *= (1f + orbitOffset);
            }

            // 配置モードに応じた位置を再計算
            Vector3 basePos = CalculatePosition(i, triangleCount, currentRadius);
            float x = basePos.x;
            float y = basePos.y;
            float z = basePos.z;

            // 浮遊感
            if (enableFloat)
            {
                float floatOffset = Mathf.Sin(time * floatSpeed + offset) * floatAmplitude;
                if (distributionMode != DistributionMode.Circle2D)
                {
                    x += Mathf.Sin(time * floatSpeed * 1.1f + offset) * floatAmplitude * 0.3f;
                    z += Mathf.Cos(time * floatSpeed * 0.9f + offset) * floatAmplitude * 0.3f;
                }
                y += floatOffset;
            }

            // ドリフト
            if (enableDrift)
            {
                float driftX = Mathf.Sin(time * driftSpeed + offset * 1.5f) * driftIntensity;
                float driftY = Mathf.Cos(time * driftSpeed * 0.7f + offset) * driftIntensity * 0.5f;
                float driftZ = Mathf.Sin(time * driftSpeed * 0.5f + offset * 0.8f) * driftIntensity * 0.3f;
                x += driftX;
                y += driftY;
                z += driftZ;
            }

            // 奥行き揺れ
            if (enableDepthMotion)
            {
                float depthOffset = Mathf.Sin(time * depthSpeed + offset * 0.8f) * depthAmplitude;
                if (distributionMode == DistributionMode.Circle2D)
                {
                    z += depthOffset;
                }
                else
                {
                    Vector3 radialDir = basePos.normalized;
                    x += radialDir.x * depthOffset;
                    y += radialDir.y * depthOffset;
                    z += radialDir.z * depthOffset;
                }
            }

            triangles[i].transform.localPosition = new Vector3(x, y, z);

            // 3D自転
            float spinSpeed = triangleRotationSpeed;
            if (enableSpinVariation)
            {
                spinSpeed *= spinMultipliers[i];
            }

            if (enable3DSpin && randomSpinAxis)
            {
                triangles[i].transform.Rotate(spinAxes[i], spinSpeed * deltaTime, Space.Self);
            }
            else
            {
                triangles[i].transform.Rotate(Vector3.forward, spinSpeed * deltaTime, Space.Self);
            }

            // サイズ変動
            if (enableSizeVariation)
            {
                float sizeScale = 1f + Mathf.Sin(time * sizeVariationSpeed + offset) * sizeVariationIntensity;
                triangles[i].transform.localScale = Vector3.one * sizeScale;
            }

            // 色のアニメーション
            if (meshRenderers[i] != null)
            {
                float t = (Mathf.Sin(time * colorAnimationSpeed + i * (Mathf.PI * 2f / triangleCount)) + 1f) / 2f;
                Color currentColor = Color.Lerp(startColor, endColor, t);

                meshRenderers[i].GetPropertyBlock(propertyBlocks[i]);
                propertyBlocks[i].SetColor("_BaseColor", currentColor);
                propertyBlocks[i].SetColor("_Color", currentColor);
                propertyBlocks[i].SetColor("_EmissionColor", currentColor * 0.5f);
                meshRenderers[i].SetPropertyBlock(propertyBlocks[i]);
            }
        }
    }

    /// <summary>
    /// 子オブジェクトの中からTriangle_で始まる全てのオブジェクトを削除
    /// 配列参照が失われても確実にクリーンアップする
    /// </summary>
    private void CleanupAllTriangleChildren()
    {
        // 逆順でイテレート（削除中にインデックスがずれないように）
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Triangle_"))
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
    }

    /// <summary>
    /// 三角形オブジェクトをクリーンアップ
    /// </summary>
    private void CleanupTriangles()
    {
        CleanupAllTriangleChildren();
        triangles = null;
        meshRenderers = null;
        propertyBlocks = null;
        randomOffsets = null;
        spinMultipliers = null;
    }

    /// <summary>
    /// パラメータ変更時に三角形を再生成
    /// </summary>
    public void RegenerateTriangles()
    {
        CreateTriangles();
    }

    /// <summary>
    /// エディタでパラメータ変更時に呼ばれる
    /// </summary>
    void OnValidate()
    {
        // 三角形が存在するかつシーン内にある場合のみ再生成を検討
        if (triangles != null && gameObject.scene.isLoaded)
        {
            // 次フレームで再生成（OnValidate内での直接操作を避ける）
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.delayCall += () =>
                {
                    if (this != null)
                    {
                        CreateTriangles();
                    }
                };
            }
            else
#endif
            {
                CreateTriangles();
            }
        }
    }

    void OnDestroy()
    {
        CleanupTriangles();
    }
}
