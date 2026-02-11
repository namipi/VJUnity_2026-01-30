using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Volumeのプロパティを簡単に設定するためのユーティリティクラス
/// パブリック関数を呼び出すことでパラメータを変更できます
/// 色は HSV の色相（Hue）のみを変更可能
/// </summary>
[RequireComponent(typeof(Volume))]
public class VolumePropertySetter : MonoBehaviour
{
    [Header("Volume Reference")]
    [HideInInspector]
    [SerializeField] private Volume volume;

    [Header("Color Settings (HSV)")]
    [Tooltip("基準となるSaturation（彩度）")]
    [Range(0f, 1f)]
    [SerializeField] private float baseSaturation = 1f;

    [Tooltip("基準となるValue（明度）")]
    [Range(0f, 1f)]
    [SerializeField] private float baseValue = 1f;

    [Tooltip("基準となるAlpha")]
    [Range(0f, 1f)]
    [SerializeField] private float baseAlpha = 1f;

    [Header("Float Parameter Settings")]
    [Tooltip("操作対象のVolumeComponent名（例: Bloom, Vignette など）")]
    [SerializeField] private string targetComponentName = "";

    [Tooltip("操作対象のパラメータ名")]
    [SerializeField] private string targetParameterName = "";

    // キャッシュ
    private VolumeComponent cachedComponent;
    private System.Reflection.FieldInfo cachedFieldInfo;

    private void Reset()
    {
        // Editorでアタッチした瞬間に自動取得
        volume = GetComponent<Volume>();
    }

    private void Awake()
    {
        if (volume == null)
        {
            volume = GetComponent<Volume>();
        }
    }

    #region Hue Control (0-1 range)

    /// <summary>
    /// 色相（Hue）を設定（0～1）
    /// SaturationとValueは固定値を使用
    /// </summary>
    public void SetHue(float hue)
    {
        Color color = Color.HSVToRGB(Mathf.Clamp01(hue), baseSaturation, baseValue);
        color.a = baseAlpha;
        SetColorInternal(color);
        Debug.Log($"[VolumePropertySetter] SetHue: {hue} -> Color: {color}");
    }

    /// <summary>
    /// 色相（Hue）を設定（0～360度）
    /// </summary>
    public void SetHueDegrees(float hueDegrees)
    {
        float hue = Mathf.Clamp(hueDegrees, 0f, 360f) / 360f;
        SetHue(hue);
    }

    /// <summary>
    /// 色相、彩度、明度をまとめて指定（すべて0～1）
    /// </summary>
    public void SetHSV(float hue, float saturation, float value)
    {
        Color color = Color.HSVToRGB(
            Mathf.Clamp01(hue),
            Mathf.Clamp01(saturation),
            Mathf.Clamp01(value)
        );
        color.a = baseAlpha;
        SetColorInternal(color);
    }

    /// <summary>
    /// 彩度（Saturation）の基準値を変更
    /// </summary>
    public void SetBaseSaturation(float saturation)
    {
        baseSaturation = Mathf.Clamp01(saturation);
    }

    /// <summary>
    /// 明度（Value）の基準値を変更
    /// </summary>
    public void SetBaseValue(float value)
    {
        baseValue = Mathf.Clamp01(value);
    }

    #endregion

    #region Float Parameter Control

    /// <summary>
    /// 指定した名前のVolumeComponentのFloatパラメータを設定
    /// </summary>
    public void SetFloat(float value)
    {
        if (string.IsNullOrEmpty(targetComponentName) || string.IsNullOrEmpty(targetParameterName))
        {
            Debug.LogWarning("[VolumePropertySetter] targetComponentName or targetParameterName is not set.");
            return;
        }
        SetFloatParameter(targetComponentName, targetParameterName, value);
    }

    /// <summary>
    /// VolumeComponent名とパラメータ名を直接指定してFloat値を設定
    /// </summary>
    public void SetFloatParameter(string componentName, string parameterName, float value)
    {
        if (volume == null || volume.profile == null)
        {
            Debug.LogWarning("[VolumePropertySetter] Volume or Volume.profile is null.");
            return;
        }

        // キャッシュをチェック
        if (cachedComponent == null || cachedComponent.GetType().Name != componentName)
        {
            cachedComponent = null;
            cachedFieldInfo = null;

            foreach (var component in volume.profile.components)
            {
                if (component.GetType().Name == componentName)
                {
                    cachedComponent = component;
                    break;
                }
            }
        }

        if (cachedComponent == null)
        {
            Debug.LogWarning($"[VolumePropertySetter] VolumeComponent '{componentName}' not found.");
            return;
        }

        // フィールド情報をキャッシュ
        if (cachedFieldInfo == null || cachedFieldInfo.Name != parameterName)
        {
            cachedFieldInfo = cachedComponent.GetType().GetField(parameterName);
        }

        if (cachedFieldInfo == null)
        {
            Debug.LogWarning($"[VolumePropertySetter] Parameter '{parameterName}' not found in '{componentName}'.");
            return;
        }

        var paramValue = cachedFieldInfo.GetValue(cachedComponent);

        // ClampedFloatParameter, FloatParameter などに対応
        if (paramValue is ClampedFloatParameter clampedFloat)
        {
            clampedFloat.value = value;
            Debug.Log($"[VolumePropertySetter] {componentName}.{parameterName} = {value}");
        }
        else if (paramValue is FloatParameter floatParam)
        {
            floatParam.value = value;
            Debug.Log($"[VolumePropertySetter] {componentName}.{parameterName} = {value}");
        }
        else
        {
            Debug.LogWarning($"[VolumePropertySetter] Parameter '{parameterName}' is not a FloatParameter.");
        }
    }

    #endregion

    #region Color Parameter Control

    /// <summary>
    /// 内部的にColorパラメータを設定
    /// VolumeComponent内のColorParameterを探して設定
    /// </summary>
    private void SetColorInternal(Color color)
    {
        if (volume == null || volume.profile == null)
        {
            Debug.LogWarning("[VolumePropertySetter] Volume or Volume.profile is null.");
            return;
        }

        // Color対応コンポーネントを探す（Vignette, ColorAdjustments など）
        foreach (var component in volume.profile.components)
        {
            SetColorParameterOnComponent(component, color);
        }
    }

    /// <summary>
    /// 指定したVolumeComponent名のColorパラメータを設定
    /// </summary>
    public void SetColorOn(string componentName, Color color)
    {
        if (volume == null || volume.profile == null) return;

        foreach (var component in volume.profile.components)
        {
            if (component.GetType().Name == componentName)
            {
                SetColorParameterOnComponent(component, color);
                return;
            }
        }
        Debug.LogWarning($"[VolumePropertySetter] VolumeComponent '{componentName}' not found.");
    }

    /// <summary>
    /// 指定したComponent名のHue（色相）のみを変更
    /// </summary>
    public void SetHueOn(string componentName, float hue)
    {
        Color color = Color.HSVToRGB(Mathf.Clamp01(hue), baseSaturation, baseValue);
        color.a = baseAlpha;
        SetColorOn(componentName, color);
    }

    private void SetColorParameterOnComponent(VolumeComponent component, Color color)
    {
        var fields = component.GetType().GetFields();
        foreach (var field in fields)
        {
            var paramValue = field.GetValue(component);
            if (paramValue is ColorParameter colorParam)
            {
                colorParam.value = color;
                Debug.Log($"[VolumePropertySetter] {component.GetType().Name}.{field.Name} = {color}");
            }
        }
    }

    #endregion

    #region Intensity Controls (Common Parameters)

    /// <summary>
    /// Bloom の Intensity を設定
    /// </summary>
    public void SetBloomIntensity(float intensity)
    {
        SetFloatParameter("Bloom", "intensity", intensity);
    }

    /// <summary>
    /// Vignette の Intensity を設定
    /// </summary>
    public void SetVignetteIntensity(float intensity)
    {
        SetFloatParameter("Vignette", "intensity", intensity);
    }

    /// <summary>
    /// ChromaticAberration の Intensity を設定
    /// </summary>
    public void SetChromaticAberrationIntensity(float intensity)
    {
        SetFloatParameter("ChromaticAberration", "intensity", intensity);
    }

    /// <summary>
    /// LensDistortion の Intensity を設定
    /// </summary>
    public void SetLensDistortionIntensity(float intensity)
    {
        SetFloatParameter("LensDistortion", "intensity", intensity);
    }

    /// <summary>
    /// FilmGrain の Intensity を設定
    /// </summary>
    public void SetFilmGrainIntensity(float intensity)
    {
        SetFloatParameter("FilmGrain", "intensity", intensity);
    }

    #endregion

    #region Volume Control

    /// <summary>
    /// Volumeの重み（Weight）を設定（0～1）
    /// </summary>
    public void SetWeight(float weight)
    {
        if (volume != null)
        {
            volume.weight = Mathf.Clamp01(weight);
            Debug.Log($"[VolumePropertySetter] Volume.weight = {weight}");
        }
    }

    /// <summary>
    /// Volumeを有効化
    /// </summary>
    public void Enable()
    {
        if (volume != null)
        {
            volume.enabled = true;
        }
    }

    /// <summary>
    /// Volumeを無効化
    /// </summary>
    public void Disable()
    {
        if (volume != null)
        {
            volume.enabled = false;
        }
    }

    /// <summary>
    /// Volumeの有効/無効を切り替え
    /// </summary>
    public void Toggle()
    {
        if (volume != null)
        {
            volume.enabled = !volume.enabled;
        }
    }

    #endregion

    #region Target Settings

    /// <summary>
    /// 操作対象のVolumeComponent名を設定
    /// </summary>
    public void SetTargetComponentName(string name)
    {
        targetComponentName = name;
        cachedComponent = null;
        cachedFieldInfo = null;
    }

    /// <summary>
    /// 操作対象のパラメータ名を設定
    /// </summary>
    public void SetTargetParameterName(string name)
    {
        targetParameterName = name;
        cachedFieldInfo = null;
    }

    #endregion
}
