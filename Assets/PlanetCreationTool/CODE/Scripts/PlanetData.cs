using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 51)]
public class PlanetData : ScriptableObject
{
    [Header("Planet parameters")]
    public float PlanetSize = 1;
    public int PlanetResolution = 5;

    [Space(10)]
    [Header("Terrain shape parameters")]
    public int Seed = 0;
    public float MountainsHeight = 100.0f;
    public AnimationCurve HeightRemap;
    public float NoiseScale = 1.0f;
    public float NoiseGain = 1.0f;
    public float NoiseLacunarity = 0.5f;
    public float DetailsIntensity = 1.0f;

    [Space(10)]
    [Header("Terrain material parameters")]
    public Texture TerrainTopTexture;
    public Texture TerrainBottomTexture;
    public Color TerrainTopColor;
    public Color TerrainBottomColor;
    public float TerrainTopTextureTilling = 1.0f;
    public float TerrainBottomTextureTilling = 1.0f;
    public float TerrainTopSmoothness = 1.0f;
    public float TerrainBottomSmoothness = 1.0f;
    public float TerrainTexturesSeparationHeight = 0.5f;
    public float TerrainTexturesSeparationSmoothness = 0.0f;

    [Space(10)]
    [Header("Atmosphere parameters")]
    public bool HasAtmosphere;
    public Color AtmosphereMainColor;
    public Color AtmosphereHorizonColor;
    public float AtmosphereRadius;
    public float AtmosphereDensity;
    public float AtmosphereEdgeSmoothness;
    public float AtmosphereLightingDistance;
    public float AtmospherePlanetVisibilityModifier;

    [Space(10)]
    [Header("Ocean parameters")]
    public bool HasOcean;
    public float OceanHeight;
    public Color OceanColor;
    public Texture OceanTexture;
    public Texture OceanNormalTexture;
    public float OceanTextureTilling = 1.0f;
    public float OceanSmoothness = 0.5f;
    public float OceanMetalness = 0.0f;
    public float OceanHeightVariationIntensity = 0.0f;
    public float OceanHeightVariationFrequency = 0.0f;
    public float OceanHeightVariationSeed = 0.0f;
    public float OceanMovementSpeed = 0.0f;

    [Space(10)]
    [Header("Rings parameters")]
    public bool HasRings;
    public float RingsSize;
    public Vector3 RingsRotation;
    public float RingsWidth;
    public Color RingsColor;
    public Texture RingsTexture;
}
