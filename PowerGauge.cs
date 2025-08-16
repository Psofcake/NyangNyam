using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PowerGauge : MonoBehaviour
{
    public RectTransform _gaugeMask;
    
    [NonSerialized] //인스펙터창에서 보이지 않게 함.
    public float MaxGaugeWidth; 

    private void Awake()
    {
        
        MaxGaugeWidth = _gaugeMask.sizeDelta.x;
        
        SetGaugePercent(0.8f);
    }

    public void SetGaugePercent(float normalizeValue)
    {
        float gaugeX = MaxGaugeWidth * normalizeValue;
        _gaugeMask.sizeDelta = new Vector2(gaugeX, _gaugeMask.sizeDelta.y);
    }
}
