using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using static Unity.VisualScripting.Member;

[ExecuteInEditMode]

public class HeightMapGenerator : MonoBehaviour
{
    // Public var
    public ComputeShader TextureGenerator;
    [Space(5)]
    public bool UpdateData = false;
    public bool isRiversInitialized = false;

    public enum NoiseType
    {
        Perlin = 0,
        Voronoi = 1
    }

    public enum VoronoiDistance
    {
        EuclideanSquare = 0,
        Euclidean = 1,
        Manhattan = 2,
        Chebyshev = 3
    }

    public enum VoronoiResult
    {
        Closest = 0,
        SecondClosest = 1,
        Difference = 2,
        Average = 3
    }

    [HideInInspector] public NoiseType NoiseTypeValue = NoiseType.Voronoi;
    [HideInInspector] public VoronoiDistance VoronoiDistanceValue = VoronoiDistance.Euclidean;
    [HideInInspector] public VoronoiResult VoronoiResultValue = VoronoiResult.Average;

    [HideInInspector] public Vector3 Position = Vector3.zero;
    [Space(10)]
    public int Seed = 0;

    [HideInInspector] public int TextureResolution = 2048;

    public AnimationCurve heightRemap = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
    public float NoiseScale = 300.0f;
    [HideInInspector] public int OctaveCount = 10;
    [Range(1.0f, 3.0f)]
    public float NoiseGain = 2;
    [Range(0.1f, 0.9f)]
    public float NoiseLacunarity = 0.5f;
    public float NormalIntensity = 10.0f;
    public float NormalLimitation = 1.0f;
    [HideInInspector] public float AOIntensity = 0.0f;

    [HideInInspector] public float[] RiversSourcesTempBuffer = new float[4096];

    // Private var
    private RenderTexture _generatedTexture;
    private RenderTexture _normalTexture;
    private RenderTexture _riversTexture;

    private ComputeBuffer _heightRemapData;
    private ComputeBuffer _riversSourcesData;

    private int _kernelGenerator;
    private int _kernelRiversSources;
    private int _kernelRivers;
    private int _kernelNormal;

    // Consts
    private const string TOOL_FOLDER_NAME = "PlanetCreationTool";
    private const string SAVED_MODULE_FOLDER_PATH = "/DATA/Textures";
    private const string HEIGHT_MAP_NAME = "_HeightMap";
    private const string NORMAL_MAP_NAME = "_NormalMap";
    private const string RIVERS_MAP_NAME = "_RiversMap";
    private const int RIVERS_GRID_SIZE = 64;



    private void OnEnable()
    {
#if UNITY_EDITOR
        EditorSceneManager.sceneSaving += OnSavingScene;
        AssemblyReloadEvents.beforeAssemblyReload += ReleaseBuffers;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorSceneManager.sceneSaving -= OnSavingScene;
        AssemblyReloadEvents.beforeAssemblyReload -= ReleaseBuffers;
#endif
        ReleaseBuffers();
    }

    private void Start()
    {
        isRiversInitialized = false;
        InitGeneration(this.transform.GetChild(0).GetComponent<MeshRenderer>());
        UpdateHeightMap();
    }

    void OnSavingScene(UnityEngine.SceneManagement.Scene scene, string path)
    {
        InitGeneration(this.transform.GetChild(0).GetComponent<MeshRenderer>());
        UpdateHeightMap();
    }

    private void Update()
    {
        if(UpdateData)
        {
            Debug.Log("Generate " + this.transform.name, this.transform);
            InitGeneration(this.transform.GetChild(0).GetComponent<MeshRenderer>());
            UpdateData = false;
        }

        UpdateRivers();
    }

    public void UpdateTerrain()
    {
        if(_heightRemapData == null) { InitGeneration(this.transform.GetChild(0).GetComponent<MeshRenderer>()); }
        UpdateHeightMap();
    }

    public void InitGeneration(MeshRenderer renderer)
    {
        InitRivers(renderer);

        if (_generatedTexture != null)
            _generatedTexture.Release();
        if (_normalTexture != null)
            _normalTexture.Release();
        if (_heightRemapData != null)
            _heightRemapData.Release();

        _generatedTexture = new RenderTexture(TextureResolution, TextureResolution, 0, RenderTextureFormat.R16, RenderTextureReadWrite.Linear);
        _generatedTexture.enableRandomWrite = true;
        _generatedTexture.wrapMode = TextureWrapMode.Clamp;
        _generatedTexture.Create();

        _normalTexture = new RenderTexture(TextureResolution, TextureResolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        _normalTexture.enableRandomWrite = true;
        _normalTexture.wrapMode = TextureWrapMode.Clamp;
        _normalTexture.Create();

        _kernelGenerator = TextureGenerator.FindKernel("CSMain");
        _kernelNormal = TextureGenerator.FindKernel("CSNormal");

        _heightRemapData = new ComputeBuffer(256, 4);

        renderer.sharedMaterial.SetTexture(HEIGHT_MAP_NAME, _generatedTexture);
        renderer.sharedMaterial.SetTexture(NORMAL_MAP_NAME, _normalTexture);
    }

    private void InitRivers(MeshRenderer renderer)
    {
        if (_riversTexture != null)
            _riversTexture.Release();
        if(_riversSourcesData != null)
            _riversSourcesData.Release();

        _riversTexture = new RenderTexture(TextureResolution, TextureResolution, 0, RenderTextureFormat.R16, RenderTextureReadWrite.Linear);
        _riversTexture.enableRandomWrite = true;
        _riversTexture.filterMode = FilterMode.Point;
        _riversTexture.wrapMode = TextureWrapMode.Clamp;
        _riversTexture.Create();

        _riversSourcesData = new ComputeBuffer(RIVERS_GRID_SIZE * RIVERS_GRID_SIZE, 4);

        _kernelRiversSources = TextureGenerator.FindKernel("CSRiversSources");
        _kernelRivers = TextureGenerator.FindKernel("CSRivers");

        renderer.sharedMaterial.SetTexture(RIVERS_MAP_NAME, _riversTexture);

        isRiversInitialized = true;
    }

    private void UpdateHeightMap()
    {
        float[] tempBuffer = new float[256];

        for (int i = 0; i < 256; i++)
        {
            float t = (float)i / (float)256.0f;
            float value = heightRemap.Evaluate(t);
            tempBuffer[i] = value;
        }

        _heightRemapData.SetData(tempBuffer);


        TextureGenerator.SetFloat("NoiseScale", NoiseScale);
        TextureGenerator.SetInt("OctaveCount", OctaveCount);
        TextureGenerator.SetFloat("NoiseGain", NoiseGain);
        TextureGenerator.SetFloat("NoiseLacunarity", NoiseLacunarity);
        TextureGenerator.SetFloat("NormalIntensity", NormalIntensity);
        TextureGenerator.SetFloat("NormalLimitation", NormalLimitation);
        TextureGenerator.SetFloat("AOIntensity", AOIntensity);
        TextureGenerator.SetVector("Position", Position);
        TextureGenerator.SetFloat("Seed", Seed);

        TextureGenerator.SetInt("NoiseType", (int)NoiseTypeValue);
        TextureGenerator.SetInt("VoronoiDistance", (int)VoronoiDistanceValue);
        TextureGenerator.SetInt("VoronoiResult", (int)VoronoiResultValue);

        TextureGenerator.SetBuffer(_kernelGenerator, "HeightRemapData", _heightRemapData);
        TextureGenerator.SetTexture(_kernelGenerator, "HeightOutput", _generatedTexture);
        TextureGenerator.Dispatch(_kernelGenerator, TextureResolution / 8, TextureResolution / 8, 1);

        TextureGenerator.SetTexture(_kernelNormal, "HeightInput", _generatedTexture);
        TextureGenerator.SetTexture(_kernelNormal, "NormalOutput", _normalTexture);
        TextureGenerator.Dispatch(_kernelNormal, TextureResolution / 8, TextureResolution / 8, 1);
    }

    public void UpdateRiversSources()
    {
        InitRivers(this.transform.GetChild(0).GetComponent<MeshRenderer>());

        _riversSourcesData.SetData(RiversSourcesTempBuffer);
        TextureGenerator.SetBuffer(_kernelRiversSources, "RiversSourcesData", _riversSourcesData);
        TextureGenerator.SetTexture(_kernelRiversSources, "RiversMap", _riversTexture);

        TextureGenerator.Dispatch(_kernelRiversSources, TextureResolution / 8, TextureResolution / 8, 1);
    }

    private void UpdateRivers()
    {
        if(!isRiversInitialized) { InitRivers(this.transform.GetChild(0).GetComponent<MeshRenderer>()); }
        if(_riversTexture == null) { Debug.LogWarning("The rivers texture wasn't initalized"); return; }

        TextureGenerator.SetTexture(_kernelRivers, "RiversMap", _riversTexture);
        TextureGenerator.SetTexture(_kernelRivers, "HeightInput", _generatedTexture);
        TextureGenerator.Dispatch(_kernelRivers, TextureResolution / 8, TextureResolution / 8, 1);
    }

    public Texture2D GetHeightMap()
    {
        return RenderTextureToTexture2D(_generatedTexture, FilterMode.Bilinear);
    }

    public Texture2D GetRiversMap()
    {
        return RenderTextureToTexture2D(_riversTexture, FilterMode.Point);
    }

    public void ExportAllTextures()
    {
        ExportTexture(_generatedTexture, "ExportedHeightMap");
        ExportTexture(_normalTexture, "ExportedNormalMap");
        ExportTexture(_riversTexture, "ExportedRiversMap");
    }

    //https://stackoverflow.com/questions/44264468/convert-rendertexture-to-texture2d
    private void ExportTexture(RenderTexture source, string name)
    {
        Texture2D exportTexture = RenderTextureToTexture2D(source, FilterMode.Bilinear);

        byte[] bytes = exportTexture.EncodeToPNG();
        var dirPath = Application.dataPath + "/SaveImages/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + name + ".png", bytes);

        Debug.Log($"Exported {name} at this location : {dirPath}");
    }

    private Texture2D RenderTextureToTexture2D(RenderTexture source, FilterMode filterMode)
    {
        Texture2D targetTexture = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);
        targetTexture.filterMode = filterMode;
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = source;
        targetTexture.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        targetTexture.Apply();

        return targetTexture;
    }

    private void ReleaseBuffers()
    {
        if (_heightRemapData != null)
            _heightRemapData.Release();
        if (_riversSourcesData != null)
            _riversSourcesData.Release();
    }
}
