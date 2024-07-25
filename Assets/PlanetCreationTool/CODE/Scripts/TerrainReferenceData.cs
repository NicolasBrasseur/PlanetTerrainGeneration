using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 51)]
public class TerrainReferenceData : ScriptableObject
{
    public int Seed = 0;
    [SerializeField] private AnimationCurve _heightRemap;
    public float NoiseScale = 1.0f;
    [Range(1.0f, 3.0f)]
    public float NoiseGain = 1.0f;
    [Range(0.1f, 0.9f)]
    public float NoiseLacunarity = 0.5f;
    public float DetailsIntensity = 1.0f;

    public AnimationCurve GetHeightRemap()
    {
        AnimationCurve duplicatedCurve = new AnimationCurve();
        duplicatedCurve.CopyFrom(_heightRemap);
        return duplicatedCurve;
    }
}
