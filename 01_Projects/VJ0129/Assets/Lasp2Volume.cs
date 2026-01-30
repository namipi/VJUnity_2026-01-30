using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Lasp;
using System.Collections.Generic;
using System.Reflection;

public class Lasp2Volume : MonoBehaviour
{
   [System.Serializable]
    public class VolumeMapping
    {
        public string label = "New Mapping";
        [Range(0, 1)] public float inputValue; // LASPのAudio Property Binderからここを制御
        public string effectTypeName = "Bloom"; // エフェクト名 (例: Bloom, Vignette)
        public string parameterName = "intensity"; // パラメータ名 (例: intensity, postExposure)
        public float minAmount = 0f;
        public float maxAmount = 1f;
    }

    [Header("Volume Target")]
    [SerializeField] private Volume _volume;
    
    [Header("Mappings")]
    [SerializeField] private List<VolumeMapping> _mappings = new List<VolumeMapping>();

    void Start()
    {
        if (_volume == null) _volume = GetComponent<Volume>();

        // 実行中に元のプロファイルアセットを書き換えないよう、コピーを作成して適用する
        if (_volume != null && _volume.profile != null)
        {
            _volume.profile = Instantiate(_volume.profile);
        }
    }

    void Update()
    {
        if (_volume == null || _volume.profile == null) return;

        foreach (var map in _mappings)
        {
            ApplyValue(map);
        }
    }

    private void ApplyValue(VolumeMapping map)
    {
        // 1. プロファイルから指定した名前のエフェクトを探す
        VolumeComponent component = null;
        foreach (var comp in _volume.profile.components)
        {
            if (comp.GetType().Name == map.effectTypeName)
            {
                component = comp;
                break;
            }
        }

        if (component == null) return;

        // 2. エフェクト内の指定したフィールド（変数）をリフレクションで取得
        FieldInfo field = component.GetType().GetField(map.parameterName, BindingFlags.Public | BindingFlags.Instance);
        if (field == null) return;

        object val = field.GetValue(component);
        float targetValue = Mathf.Lerp(map.minAmount, map.maxAmount, map.inputValue);

        // 3. 各種VolumeParameter型に対応させ、overrideStateをtrueにして値を適用
        if (val is VolumeParameter<float> fp)
        {
            fp.overrideState = true;
            fp.value = targetValue;
        }
        else if (val is ClampedFloatParameter cp)
        {
            cp.overrideState = true;
            cp.value = targetValue;
        }
        else if (val is MinFloatParameter mp)
        {
            mp.overrideState = true;
            mp.value = targetValue;
        }
        else if (val is NoInterpFloatParameter np)
        {
            np.overrideState = true;
            np.value = targetValue;
        }
    }

    // 必要に応じて外部やEventから値を流し込む用
    public void SetInputValue(int index, float value)
    {
        if (index >= 0 && index < _mappings.Count)
        {
            _mappings[index].inputValue = value;
        }
    }
}