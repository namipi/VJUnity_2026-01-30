using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// VFXGraphのプロパティを簡単に設定するためのユーティリティクラス
/// 引数一つでプロパティを設定できるパブリック関数を提供します
/// </summary>
[RequireComponent(typeof(VisualEffect))]
public class VFXPropertySetter : MonoBehaviour
{
    [Header("Property Settings")]
    [SerializeField] private string propertyName = "Value";

    [HideInInspector]
    [SerializeField] private VisualEffect vfxGraph;

    private void Reset()
    {
        // Editorでアタッチした瞬間に自動取得
        vfxGraph = GetComponent<VisualEffect>();
    }

    private void Awake()
    {
        if (vfxGraph == null)
        {
            vfxGraph = GetComponent<VisualEffect>();
        }
    }

    // ========== Float ==========
    public void SetFloat(float value)
    {
        Debug.Log($"[VFXPropertySetter] SetFloat: {propertyName} = {value}");
        if (vfxGraph != null && vfxGraph.HasFloat(propertyName))
        {
            vfxGraph.SetFloat(propertyName, value);
        }
    }

    public void SetFloatNamed(string name, float value)
    {
        Debug.Log($"[VFXPropertySetter] SetFloatNamed: {name} = {value}");
        if (vfxGraph != null && vfxGraph.HasFloat(name))
        {
            vfxGraph.SetFloat(name, value);
        }
    }

    // ========== Int ==========
    public void SetInt(int value)
    {
        if (vfxGraph != null && vfxGraph.HasInt(propertyName))
        {
            vfxGraph.SetInt(propertyName, value);
        }
    }

    public void SetIntNamed(string name, int value)
    {
        if (vfxGraph != null && vfxGraph.HasInt(name))
        {
            vfxGraph.SetInt(name, value);
        }
    }

    // ========== Bool ==========
    public void SetBool(bool value)
    {
        if (vfxGraph != null && vfxGraph.HasBool(propertyName))
        {
            vfxGraph.SetBool(propertyName, value);
        }
    }

    public void SetBoolNamed(string name, bool value)
    {
        if (vfxGraph != null && vfxGraph.HasBool(name))
        {
            vfxGraph.SetBool(name, value);
        }
    }

    // ========== Vector2 ==========
    public void SetVector2(Vector2 value)
    {
        if (vfxGraph != null && vfxGraph.HasVector2(propertyName))
        {
            vfxGraph.SetVector2(propertyName, value);
        }
    }

    public void SetVector2Named(string name, Vector2 value)
    {
        if (vfxGraph != null && vfxGraph.HasVector2(name))
        {
            vfxGraph.SetVector2(name, value);
        }
    }

    // ========== Vector3 ==========
    public void SetVector3(Vector3 value)
    {
        Debug.Log($"[VFXPropertySetter] SetVector3: {propertyName} = {value}");
        if (vfxGraph != null)
        {
            vfxGraph.SetVector3(propertyName, value);
        }
    }

    public void SetVector3Named(string name, Vector3 value)
    {
        Debug.Log($"[VFXPropertySetter] SetVector3Named: {name} = {value}");
        if (vfxGraph != null)
        {
            vfxGraph.SetVector3(name, value);
        }
    }

    // ========== Vector4 ==========
    public void SetVector4(Vector4 value)
    {
        if (vfxGraph != null && vfxGraph.HasVector4(propertyName))
        {
            vfxGraph.SetVector4(propertyName, value);
        }
    }

    public void SetVector4Named(string name, Vector4 value)
    {
        if (vfxGraph != null && vfxGraph.HasVector4(name))
        {
            vfxGraph.SetVector4(name, value);
        }
    }

    // ========== Color (Gradient) ==========
    public void SetGradient(Gradient value)
    {
        if (vfxGraph != null && vfxGraph.HasGradient(propertyName))
        {
            vfxGraph.SetGradient(propertyName, value);
        }
    }

    public void SetGradientNamed(string name, Gradient value)
    {
        if (vfxGraph != null && vfxGraph.HasGradient(name))
        {
            vfxGraph.SetGradient(name, value);
        }
    }

    // ========== Texture ==========
    public void SetTexture(Texture value)
    {
        if (vfxGraph != null && vfxGraph.HasTexture(propertyName))
        {
            vfxGraph.SetTexture(propertyName, value);
        }
    }

    public void SetTextureNamed(string name, Texture value)
    {
        if (vfxGraph != null && vfxGraph.HasTexture(name))
        {
            vfxGraph.SetTexture(name, value);
        }
    }

    // ========== Mesh ==========
    public void SetMesh(Mesh value)
    {
        if (vfxGraph != null && vfxGraph.HasMesh(propertyName))
        {
            vfxGraph.SetMesh(propertyName, value);
        }
    }

    public void SetMeshNamed(string name, Mesh value)
    {
        if (vfxGraph != null && vfxGraph.HasMesh(name))
        {
            vfxGraph.SetMesh(name, value);
        }
    }

    // ========== AnimationCurve ==========
    public void SetAnimationCurve(AnimationCurve value)
    {
        if (vfxGraph != null && vfxGraph.HasAnimationCurve(propertyName))
        {
            vfxGraph.SetAnimationCurve(propertyName, value);
        }
    }

    public void SetAnimationCurveNamed(string name, AnimationCurve value)
    {
        if (vfxGraph != null && vfxGraph.HasAnimationCurve(name))
        {
            vfxGraph.SetAnimationCurve(name, value);
        }
    }

    // ========== Matrix4x4 ==========
    public void SetMatrix4x4(Matrix4x4 value)
    {
        if (vfxGraph != null && vfxGraph.HasMatrix4x4(propertyName))
        {
            vfxGraph.SetMatrix4x4(propertyName, value);
        }
    }

    public void SetMatrix4x4Named(string name, Matrix4x4 value)
    {
        if (vfxGraph != null && vfxGraph.HasMatrix4x4(name))
        {
            vfxGraph.SetMatrix4x4(name, value);
        }
    }

    // ========== VFX Control ==========
    public void Play()
    {
        if (vfxGraph != null)
        {
            vfxGraph.Play();
        }
    }

    public void Stop()
    {
        if (vfxGraph != null)
        {
            vfxGraph.Stop();
        }
    }

    public void Reinit()
    {
        if (vfxGraph != null)
        {
            vfxGraph.Reinit();
        }
    }

    /// <summary>
    /// Event名を指定してVFXにイベントを送信
    /// </summary>
    public void SendEvent(string eventName)
    {
        if (vfxGraph != null)
        {
            vfxGraph.SendEvent(eventName);
        }
    }

    // ========== Property Name Setter ==========
    /// <summary>
    /// 操作対象のプロパティ名を変更する
    /// </summary>
    public void SetPropertyName(string name)
    {
        propertyName = name;
    }
}
