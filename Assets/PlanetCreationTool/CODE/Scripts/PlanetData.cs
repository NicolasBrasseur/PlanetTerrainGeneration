using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 51)]
public class PlanetData : ScriptableObject
{
    [Header("Planet parameters")]
    public float PlanetSize = 60;
    public int PlanetResolution = 200;

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
    public Texture TerrainTexture01;
    public Texture TerrainTexture02;
    public Texture TerrainTexture03;
    public Texture TerrainTexture04;
    public Color TerrainTextureColor01 = Color.white;
    public Color TerrainTextureColor02 = Color.white;
    public Color TerrainTextureColor03 = Color.white;
    public Color TerrainTextureColor04 = Color.white;
    public float TerrainTextureTilling01 = 1.0f;
    public float TerrainTextureTilling02 = 1.0f;
    public float TerrainTextureTilling03 = 1.0f;
    public float TerrainTextureTilling04 = 1.0f;
    public float TerrainTextureSmoothness01 = 0.0f;
    public float TerrainTextureSmoothness02 = 0.0f;
    public float TerrainTextureSmoothness03 = 0.0f;
    public float TerrainTextureSmoothness04 = 0.0f;
    public float TerrainTexturesSeparationSmoothness = 0.0f;
    public float TerrainTexture02Height = 0.0f;
    public float TerrainTexture03Height = 0.0f;
    public float TerrainTexture04Height = 0.0f;

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
    public Texture2D OceanTexture;
    public Texture2D OceanNormalTexture;
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

    [Space(10)]
    [Header("Rivers parameters")]
    public List<Vector2> RiversSources;
    public float RiversTransparency;
    public float RiversEdgeSmoothness;
    public float RiversErosionPower;
    public float RiversErosionSmoothness;
    public float RiversBedWidth;
    public Texture2D RiversTexture;
    public float RiversTextureTilling;
    public Color RiversColor;
}
