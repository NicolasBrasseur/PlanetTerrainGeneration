using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Rendering;

public class PlanetGenerationTool : EditorWindow
{
    #region Variables setup

    //Publics
    public List<Vector2> RiversSources = new List<Vector2>();

    //Privates
    private GUIContent[] _mainMenuContents;
    private string _planetName;
    private string[] _toolTips;

    private int _mainMenuIndex = 0;

    private Vector2 _scrollPosition;
    private Vector3 _planetSpawnCoordinates;

    private GUIStyle _centeredStyle;
    private GUIStyle _boldCenteredStyle;

    private Material _selectedTerrainMaterial;
    private Material _selectedAtmosphereMaterial;
    private Material _selectedOceanMaterial;
    private Material _selectedRingsMaterial;
    private GameObject _selectedPlanet;
    private GameObject _defaultPlanetPrefab;
    private Transform _selectedTerrainParent;
    private Transform _selectedAtmosphere;
    private Transform _selectedOcean;
    private Transform _selectedRings;

    private PlanetShapeGenerator _planetShapeGenerator;
    private PlanetDataContainer _planetDataContainer;
    private HeightMapGenerator _heightMapGenerator;
    private PlanetData _importedPlanetData;

    private bool _allowNavigation;
    private bool _showInvalidNameError;

    //Planet parameters
    private float _planetSize = 1;
    private float _displayedSize = 1;
    private int _planetResolution = 5;

    //Terrain shape parameters
    private int _seed = 0;
    private float _mountainsHeight = 100.0f;
    private AnimationCurve _heightRemap;
    private float _noiseScale = 1.0f;
    private float _noiseGain = 1.0f;
    private float _noiseLacunarity = 0.5f;
    private float _detailsIntensity = 1.0f;

    //Terrain material parameters
    private Texture _terrainTexture01;
    private Texture _terrainTexture02;
    private Texture _terrainTexture03;
    private Texture _terrainTexture04;
    private Color _terrainTextureColor01 = Color.white;
    private Color _terrainTextureColor02 = Color.white;
    private Color _terrainTextureColor03 = Color.white;
    private Color _terrainTextureColor04 = Color.white;
    private float _terrainTextureTilling01 = 1.0f;
    private float _terrainTextureTilling02 = 1.0f;
    private float _terrainTextureTilling03 = 1.0f;
    private float _terrainTextureTilling04 = 1.0f;
    private float _terrainTextureSmoothness01 = 0.0f;
    private float _terrainTextureSmoothness02 = 0.0f;
    private float _terrainTextureSmoothness03 = 0.0f;
    private float _terrainTextureSmoothness04 = 0.0f;
    private float _terrainTexturesSeparationSmoothness = 0.0f;
    private float _terrainTexture02Height = 0.0f;
    private float _terrainTexture03Height = 0.0f;
    private float _terrainTexture04Height = 0.0f;

    //Atmosphere parameters
    private bool _hasAtmosphere;
    private Color _atmosphereMainColor;
    private Color _atmosphereHorizonColor;
    private float _atmosphereRadius;
    private float _atmosphereDensity;
    private float _atmosphereEdgeSmoothness;
    private float _atmosphereLightingDistance;
    private float _atmospherePlanetVisibilityModifier;

    //Oceans parameters
    private bool _hasOcean;
    private float _oceanHeight = 0;
    private Color _oceanColor;
    private Texture _oceanTexture;
    private Texture _oceanNormalTexture;
    private float _oceanTextureTilling = 1.0f;
    private float _oceanSmoothness = 0.5f;
    private float _oceanMetalness = 0.0f;
    private float _oceanHeightVariationIntensity = 0.0f;
    private float _oceanHeightVariationFrequency = 0.0f;
    private float _oceanHeightVariationSeed = 0.0f;
    private float _oceanMovementSpeed = 0.0f;

    //Rings parameters
    private bool _hasRings;
    private float _ringsSize = 1.0f;
    private Vector3 _ringsRotation = Vector3.zero;
    private float _ringsWidth = 1.0f;
    private Color _ringsColor;
    private Texture _ringsTexture;

    //Consts
    private const int TOP_MARGIN = 20;
    private const int BOTTOM_MARGIN = 30;
    private const int LEFT_AND_RIGHT_MARGINS = 10;

    private const int SMALL_SPACE = 10;
    private const int BIG_SPACE = 20;
    private const int ERROR_MESSAGE_HEIGHT = 20;

    private const int CREATION_TAB_INDEX = 0;
    private const int ATMOSPHERE_TAB_INDEX = 1;
    private const int RINGS_TAB_INDEX = 2;
    private const int TERRAIN_TAB_INDEX = 3;
    private const int OCEAN_TAB_INDEX = 4;
    private const int RIVERS_TAB_INDEX = 5;

    private const float ATMOSPHERE_MIN_SIZE = 50.0f;
    private const float OCEAN_REFERENCE_HEIGHT = 0.0f;
    private const float RINGS_REFERENCE_SIZE = 8.0f;

    private const string PLANET_TAG = "Planet";
    private const string TERRAIN_PARENT_NAME = "Terrain";
    private const string ATMOPHERE_PARENT_NAME = "Atmosphere";
    private const string OCEAN_PARENT_NAME = "Ocean";
    private const string RINGS_PARENT_NAME = "Rings";

    private const string TERRAIN_SHADER_NAME = "TerrainDisplacement";
    private const string TERRAIN_REFERENCE_MATERIAL_PATH = "References/TerrainDisplacementReference";
    private const string ATMOSPHERE_REFERENCE_MATERIAL_PATH = "References/AtmosphereReference";
    private const string OCEAN_REFERENCE_MATERIAL_PATH = "References/OceanReference";
    private const string RINGS_REFERENCE_MATERIAL_PATH = "References/RingsReference";

    private const string TERRAIN_REFERENCE_DATA_PATH = "References/TerrainReferenceData";
    private const string TOOL_FOLDER_NAME = "PlanetCreationTool";
    private const string SAVED_DATA_FOLDER_PATH = "/DATA/PlanetsSavedData";
    private const string OCEAN_SPHERE_PATH = "Meshs/Sphere";
    private const string RINGS_MESH_PATH = "Meshs/RingsBothSide";

    private const int RIVERS_WINDOW_GRID_SIZE = 64;


    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        InitiateVariables();
        CreateNewTag(PLANET_TAG);
    }

    private void OnDisable()
    {
        SaveData();
        AssetDatabase.SaveAssets();
    }

    private void OnSelectionChange()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null) 
        {
            SaveData();
            ResetSelection();
            return;  
        }

        if (selectedObject.CompareTag(PLANET_TAG))
        {
            SaveData();
            UpdateSelection(selectedObject);
            SceneView.FrameLastActiveSceneView();
        }
    }

    #endregion

    #region Custom Methods

    void InitiateVariables()
    {
        _mainMenuIndex = 0;
        _planetSpawnCoordinates = Vector3.zero;
        _planetName = null;
        _selectedPlanet = null;
        _allowNavigation = false;

        _mainMenuContents = new GUIContent[]
        {
            new GUIContent(" Creation and Importation", EditorGUIUtility.IconContent("CustomTool").image),
            new GUIContent(" Atmosphere", EditorGUIUtility.IconContent("CloudConnect").image),
            new GUIContent(" Rings", EditorGUIUtility.IconContent("DotFrameDotted").image),
            new GUIContent(" Terrain", EditorGUIUtility.IconContent("TerrainInspector.TerrainToolSplat").image),
            new GUIContent(" Ocean", EditorGUIUtility.IconContent("d_TreeEditor.Wind").image),
            new GUIContent(" Rivers", EditorGUIUtility.IconContent("d_TreeEditor.Wind").image)
        };

        _toolTips = new string[]
        {
            "Enter the name of the planet. \nThis name will also be used for the GameObject's name.",
            "Set the size of the planet. \nThe size must be bigger than 0.",
            "Set the mesh resolution of the planet \nThe resolution must be bigger than 0.",
            "Set the spawn coordinates in world-space for the planet."
        };

        if (_defaultPlanetPrefab == null) _defaultPlanetPrefab = (GameObject)Resources.Load("Prefabs/DefaultPlanet");

        Camera mainCamera = Camera.main;
        mainCamera.depthTextureMode = DepthTextureMode.Depth;
    }

    void ResetSelection()
    {
        _selectedPlanet = null;
        _allowNavigation = false;
        _mainMenuIndex = CREATION_TAB_INDEX;
        Repaint();
    }

    void UpdateSelection(GameObject newSelectedPlanet)
    {
        _selectedPlanet = newSelectedPlanet;
        _allowNavigation = true;
        _mainMenuIndex = TERRAIN_TAB_INDEX;
        GetGenerationComponents();
        GetParameters();
        Repaint();
    }

    // Create new tag if it doesn't already exist
    //https://gamedev.stackexchange.com/questions/135649/programmatically-creating-new-tags-in-unity
    void CreateNewTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Adding a Tag
        string s = tagName;

        // First check if it is not already present
        bool found = false;

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(s)) { found = true; break; }
        }

        // if not found, add it
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
            n.stringValue = s;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    // Create a new planet based on input parameters
    void CreateNewPlanet()
    {
        _selectedPlanet = new GameObject();
        _selectedPlanet.name = _planetName;
        _selectedPlanet.tag = PLANET_TAG;

        GetGenerationComponents();
        GeneratePlanetMesh();
        GenerateInitialTerrain();
        GenerateInitialAtmosphere();
        GenerateInitialOcean();
        GenerateInitialRings();
        ChangeObjectSelection();

        _selectedPlanet.transform.position = _planetSpawnCoordinates;
        //SceneView.FrameLastActiveSceneView();
        CreateDataObject();



        void GeneratePlanetMesh()
        {
            _planetShapeGenerator.GenerateInitialMesh(_selectedTerrainParent.gameObject, _planetSize, _planetResolution);
            _displayedSize = _planetSize;
        }

        void GenerateInitialTerrain()
        {
            Material referenceMaterial = Resources.Load<Material>(TERRAIN_REFERENCE_MATERIAL_PATH);
            Material terrainMaterial = new Material(referenceMaterial);
            _selectedTerrainMaterial = terrainMaterial;

            ApplyMaterials();
            ApplyTerrainDefaultParameters();

            _heightMapGenerator.InitGeneration(_selectedTerrainParent.GetChild(0).gameObject.GetComponent<MeshRenderer>());


            void ApplyMaterials()
            {
                for (int i = 0; i < _selectedTerrainParent.childCount; i++)
                {
                    MeshRenderer planetFaceRenderer = _selectedTerrainParent.GetChild(i).gameObject.GetComponent<MeshRenderer>();
                    planetFaceRenderer.material = terrainMaterial;
                }
            }
        }

        void GenerateInitialAtmosphere()
        {
            _hasAtmosphere = true;

            Material referenceMaterial = Resources.Load<Material>(ATMOSPHERE_REFERENCE_MATERIAL_PATH);
            Material atmosphereMaterial = new Material(referenceMaterial);
            _selectedAtmosphereMaterial = atmosphereMaterial;

            float scaleValue = Mathf.Max(_planetSize * 3, ATMOSPHERE_MIN_SIZE);
            _selectedAtmosphere.transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
            MeshRenderer atmosphereRenderer = _selectedAtmosphere.gameObject.AddComponent<MeshRenderer>();
            MeshFilter atmosphereFilter = _selectedAtmosphere.gameObject.AddComponent<MeshFilter>();
            atmosphereFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            atmosphereRenderer.material = atmosphereMaterial;

            ApplyAtmosphereDefaultParameters();
        }

        void GenerateInitialOcean()
        {
            _hasOcean = true;

            Material referenceMaterial = Resources.Load<Material>(OCEAN_REFERENCE_MATERIAL_PATH);
            Material oceanMaterial = new Material(referenceMaterial);
            _selectedOceanMaterial = oceanMaterial;

            float scaleValue = _planetSize + _oceanHeight;
            _selectedOcean.transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
            MeshRenderer oceanRenderer = _selectedOcean.gameObject.AddComponent<MeshRenderer>();
            MeshFilter oceanFilter = _selectedOcean.gameObject.AddComponent<MeshFilter>();
            oceanFilter.mesh = Resources.Load<Mesh>(OCEAN_SPHERE_PATH);
            oceanRenderer.material = oceanMaterial;

            ApplyOceanDefaultParameters();
        }

        void GenerateInitialRings()
        {
            _hasRings = true;

            Material referenceMaterial = Resources.Load<Material>(RINGS_REFERENCE_MATERIAL_PATH);
            Material ringsMaterial = new Material(referenceMaterial);
            _selectedRingsMaterial = ringsMaterial;

            float scaleValue = _planetSize + _ringsSize;
            _selectedRings.transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
            MeshRenderer ringsRenderer = _selectedRings.gameObject.AddComponent<MeshRenderer>();
            MeshFilter ringsFilter = _selectedRings.gameObject.AddComponent<MeshFilter>();
            ringsFilter.mesh = Resources.Load<Mesh>(RINGS_MESH_PATH);
            ringsRenderer.material = ringsMaterial;

            ApplyRingsDefaultParameters();
        }

        void ChangeObjectSelection()
        {
            Object[] selection = new Object[1];
            selection[0] = _selectedPlanet;
            Selection.objects = selection;
        }

        void CreateDataObject()
        {
            PlanetData newPlanetData = ScriptableObject.CreateInstance<PlanetData>();
            EditorUtility.SetDirty(newPlanetData);

            string pathToSelf = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            string pathToTool = pathToSelf.Substring(0, (pathToSelf.LastIndexOf($"{TOOL_FOLDER_NAME}") + TOOL_FOLDER_NAME.Length));
            string savePath = $"{pathToTool}{SAVED_DATA_FOLDER_PATH}/{_selectedPlanet.transform.name}_data.asset";

            AssetDatabase.CreateAsset(newPlanetData, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _planetDataContainer.PlanetData = newPlanetData;

            SaveData();

            GUIUtility.ExitGUI();
        }
    }

    void GetGenerationComponents()
    {
        if (!_selectedPlanet.TryGetComponent<PlanetShapeGenerator>(out _planetShapeGenerator))
        {
            _planetShapeGenerator = _selectedPlanet.AddComponent<PlanetShapeGenerator>();
        }

        if (!_selectedPlanet.TryGetComponent<PlanetDataContainer>(out _planetDataContainer))
        {
            _planetDataContainer = _selectedPlanet.AddComponent<PlanetDataContainer>();
        }

        _selectedTerrainParent = FindParent(TERRAIN_PARENT_NAME).transform;
        ComputeShader heightMapComputeShader = Resources.Load<ComputeShader>("ComputeHeightMapGenerator");

        if (!_selectedTerrainParent.TryGetComponent<HeightMapGenerator>(out _heightMapGenerator))
        {
            _heightMapGenerator = _selectedTerrainParent.gameObject.AddComponent<HeightMapGenerator>();
            _heightMapGenerator.TextureGenerator = heightMapComputeShader;
            ApplyShapeDefaultParameters();
        }

        if(_selectedTerrainParent.childCount > 0) { _selectedTerrainMaterial = _selectedTerrainParent.GetChild(0).gameObject.GetComponent<MeshRenderer>().sharedMaterial; }

        _selectedAtmosphere = FindParent(ATMOPHERE_PARENT_NAME).transform;
        MeshRenderer atmosphereRenderer;
        if(_selectedAtmosphere.TryGetComponent<MeshRenderer>(out atmosphereRenderer))
        {
            _selectedAtmosphereMaterial = atmosphereRenderer.sharedMaterial;
        }

        _selectedOcean = FindParent(OCEAN_PARENT_NAME).transform;
        MeshRenderer oceanRenderer;
        if(_selectedOcean.TryGetComponent<MeshRenderer>(out oceanRenderer))
        {
            _selectedOceanMaterial = oceanRenderer.sharedMaterial;
        }

        _selectedRings = FindParent(RINGS_PARENT_NAME).transform;
        MeshRenderer ringsRenderer;
        if(_selectedRings.TryGetComponent<MeshRenderer>(out ringsRenderer))
        {
            _selectedRingsMaterial = ringsRenderer.sharedMaterial;
        }
    }

    GameObject FindParent(string parentName)
    {
        for (int i = 0; i < _selectedPlanet.transform.childCount; i++)
        {
            Transform child = _selectedPlanet.transform.GetChild(i);
            if (child.name == parentName)
            {
                return child.gameObject;
            }
        }

        GameObject newParent = new GameObject(parentName);
        newParent.transform.position = _selectedPlanet.transform.position;
        newParent.transform.parent = _selectedPlanet.transform;
        return newParent;
    }

    void GetParameters()
    {
        if (!_heightMapGenerator || !_selectedTerrainMaterial) return;
        PlanetData planetData = _planetDataContainer.PlanetData;
        if(planetData == null) return;

        _planetSize = planetData.PlanetSize;
        _displayedSize = _planetSize;
        _planetResolution = planetData.PlanetResolution;

        _seed = _heightMapGenerator.Seed;
        _heightRemap = _heightMapGenerator.heightRemap;
        _noiseScale = _heightMapGenerator.NoiseScale;
        _noiseGain = _heightMapGenerator.NoiseGain;
        _noiseLacunarity = _heightMapGenerator.NoiseLacunarity;
        _detailsIntensity = _heightMapGenerator.NormalIntensity;

        _mountainsHeight = _selectedTerrainMaterial.GetFloat("_DisplacementIntensity");
        _terrainTexture01 = _selectedTerrainMaterial.GetTexture("_Texture01");
        _terrainTexture02 = _selectedTerrainMaterial.GetTexture("_Texture02");
        _terrainTexture03 = _selectedTerrainMaterial.GetTexture("_Texture03");
        _terrainTexture04 = _selectedTerrainMaterial.GetTexture("_Texture04");
        _terrainTextureColor01 = _selectedTerrainMaterial.GetColor("_Color01");
        _terrainTextureColor02 = _selectedTerrainMaterial.GetColor("_Color02");
        _terrainTextureColor03 = _selectedTerrainMaterial.GetColor("_Color03");
        _terrainTextureColor04 = _selectedTerrainMaterial.GetColor("_Color04");
        _terrainTextureTilling01 = _selectedTerrainMaterial.GetFloat("_TillingTexture01");
        _terrainTextureTilling02 = _selectedTerrainMaterial.GetFloat("_TillingTexture02");
        _terrainTextureTilling03 = _selectedTerrainMaterial.GetFloat("_TillingTexture03");
        _terrainTextureTilling04 = _selectedTerrainMaterial.GetFloat("_TillingTexture04");
        _terrainTextureSmoothness01 = _selectedTerrainMaterial.GetFloat("_SmoothnessTexture01");
        _terrainTextureSmoothness01 = _selectedTerrainMaterial.GetFloat("_SmoothnessTexture02");
        _terrainTextureSmoothness01 = _selectedTerrainMaterial.GetFloat("_SmoothnessTexture03");
        _terrainTextureSmoothness01 = _selectedTerrainMaterial.GetFloat("_SmoothnessTexture04");
        _terrainTexturesSeparationSmoothness = _selectedTerrainMaterial.GetFloat("_TextureSeparationSmoothness");
        _terrainTexture02Height = _selectedTerrainMaterial.GetFloat("_Texture02Height");
        _terrainTexture03Height = _selectedTerrainMaterial.GetFloat("_Texture03Height");
        _terrainTexture04Height = _selectedTerrainMaterial.GetFloat("_Texture04Height");

        _hasAtmosphere = planetData.HasAtmosphere;
        _atmosphereMainColor = _selectedAtmosphereMaterial.GetColor("_BaseColor");
        _atmosphereHorizonColor = _selectedAtmosphereMaterial.GetColor("_HorizonColor");
        _atmosphereRadius = _selectedAtmosphereMaterial.GetFloat("_Radius") - _displayedSize;
        _atmosphereDensity = _selectedAtmosphereMaterial.GetFloat("_Density");
        _atmosphereEdgeSmoothness = _selectedAtmosphereMaterial.GetFloat("_DensityPower");
        _atmospherePlanetVisibilityModifier = _selectedAtmosphereMaterial.GetFloat("_PlanetVisibility");
        _atmosphereLightingDistance = _selectedAtmosphereMaterial.GetFloat("_LightingRadius");

        _hasOcean = planetData.HasOcean;
        _oceanHeight = planetData.OceanHeight;
        _oceanColor = _selectedOceanMaterial.GetColor("_Color");
        _oceanTexture = _selectedOceanMaterial.GetTexture("_BaseColor");
        _oceanNormalTexture = _selectedOceanMaterial.GetTexture("_Normal");
        _oceanTextureTilling = _selectedOceanMaterial.GetFloat("_WaterTextureTilling");
        _oceanSmoothness = _selectedOceanMaterial.GetFloat("_Smoothness");
        _oceanMetalness = _selectedOceanMaterial.GetFloat("_Metalness");
        _oceanHeightVariationIntensity = _selectedOceanMaterial.GetFloat("_OceanHeightVariation");
        _oceanHeightVariationFrequency = _selectedOceanMaterial.GetFloat("_HeightNoiseScale");
        _oceanHeightVariationSeed = _selectedOceanMaterial.GetFloat("_Seed");
        _oceanMovementSpeed = _selectedOceanMaterial.GetFloat("_WaterMovementSpeed");

        _hasRings = planetData.HasRings;
        _ringsSize = planetData.RingsSize;
        _ringsRotation = planetData.RingsRotation;
        _ringsWidth = _selectedRingsMaterial.GetFloat("_Width");
        _ringsColor = _selectedRingsMaterial.GetColor("_Color");
        _ringsTexture = _selectedRingsMaterial.GetTexture("_MainTex");
    }

    void ApplyShapeDefaultParameters()
    {
        TerrainReferenceData referenceData = Resources.Load<TerrainReferenceData>(TERRAIN_REFERENCE_DATA_PATH);

        _seed = referenceData.Seed;
        _heightRemap = referenceData.GetHeightRemap();
        _noiseScale = referenceData.NoiseScale;
        _noiseGain = referenceData.NoiseGain;
        _noiseLacunarity = referenceData.NoiseLacunarity;
        _detailsIntensity = referenceData.DetailsIntensity;
    }

    void ApplyTerrainDefaultParameters()
    {
        Material referenceMaterial = Resources.Load<Material>(TERRAIN_REFERENCE_MATERIAL_PATH);

        _mountainsHeight = referenceMaterial.GetFloat("_DisplacementIntensity");
        _terrainTexture01 = referenceMaterial.GetTexture("_Texture01");
        _terrainTexture02 = referenceMaterial.GetTexture("_Texture02");
        _terrainTexture03 = referenceMaterial.GetTexture("_Texture03");
        _terrainTexture04 = referenceMaterial.GetTexture("_Texture04");
        _terrainTextureColor01 = referenceMaterial.GetColor("_Color01");
        _terrainTextureColor02 = referenceMaterial.GetColor("_Color02");
        _terrainTextureColor03 = referenceMaterial.GetColor("_Color03");
        _terrainTextureColor04 = referenceMaterial.GetColor("_Color04");
        _terrainTextureTilling01 = referenceMaterial.GetFloat("_TillingTexture01");
        _terrainTextureTilling02 = referenceMaterial.GetFloat("_TillingTexture02");
        _terrainTextureTilling03 = referenceMaterial.GetFloat("_TillingTexture03");
        _terrainTextureTilling04 = referenceMaterial.GetFloat("_TillingTexture04");
        _terrainTextureSmoothness01 = referenceMaterial.GetFloat("_SmoothnessTexture01");
        _terrainTextureSmoothness01 = referenceMaterial.GetFloat("_SmoothnessTexture02");
        _terrainTextureSmoothness01 = referenceMaterial.GetFloat("_SmoothnessTexture03");
        _terrainTextureSmoothness01 = referenceMaterial.GetFloat("_SmoothnessTexture04");
        _terrainTexturesSeparationSmoothness = referenceMaterial.GetFloat("_TextureSeparationSmoothness");
        _terrainTexture02Height = referenceMaterial.GetFloat("_Texture02Height");
        _terrainTexture03Height = referenceMaterial.GetFloat("_Texture03Height");
        _terrainTexture04Height = referenceMaterial.GetFloat("_Texture04Height");
    }

    void ApplyAtmosphereDefaultParameters()
    {
        Material referenceMaterial = Resources.Load<Material>(ATMOSPHERE_REFERENCE_MATERIAL_PATH);

        _hasAtmosphere = true;
        _atmosphereMainColor = referenceMaterial.GetColor("_BaseColor");
        _atmosphereHorizonColor = referenceMaterial.GetColor("_HorizonColor");
        _atmosphereRadius = referenceMaterial.GetFloat("_Radius");
        _atmosphereDensity = referenceMaterial.GetFloat("_Density");
        _atmosphereEdgeSmoothness = referenceMaterial.GetFloat("_DensityPower");
        _atmospherePlanetVisibilityModifier = referenceMaterial.GetFloat("_PlanetVisibility");
        _atmosphereLightingDistance = referenceMaterial.GetFloat("_LightingRadius");
    }

    void ApplyOceanDefaultParameters()
    {
        Material referenceMaterial = Resources.Load<Material>(OCEAN_REFERENCE_MATERIAL_PATH);

        _hasOcean = true;
        _oceanHeight = OCEAN_REFERENCE_HEIGHT;
        _oceanColor = referenceMaterial.GetColor("_Color");
        _oceanTexture = referenceMaterial.GetTexture("_BaseColor");
        _oceanNormalTexture = referenceMaterial.GetTexture("_Normal");
        _oceanTextureTilling = referenceMaterial.GetFloat("_WaterTextureTilling");
        _oceanSmoothness = referenceMaterial.GetFloat("_Smoothness");
        _oceanMetalness = referenceMaterial.GetFloat("_Metalness");
        _oceanHeightVariationIntensity = referenceMaterial.GetFloat("_OceanHeightVariation");
        _oceanHeightVariationFrequency = referenceMaterial.GetFloat("_HeightNoiseScale");
        _oceanHeightVariationSeed = referenceMaterial.GetFloat("_Seed");
        _oceanMovementSpeed = referenceMaterial.GetFloat("_WaterMovementSpeed");
    }

    void ApplyRingsDefaultParameters()
    {
        Material referenceMaterial = Resources.Load<Material>(RINGS_REFERENCE_MATERIAL_PATH);

        _hasRings = true;
        _ringsSize = RINGS_REFERENCE_SIZE;
        _ringsRotation = Vector3.zero;
        _ringsWidth = referenceMaterial.GetFloat("_Width");
        _ringsColor = referenceMaterial.GetColor("_Color");
        _ringsTexture = referenceMaterial.GetTexture("_MainTex");
    }

    void SaveData()
    {
        if(_selectedPlanet == null) {  return; }

        PlanetData planetData = _planetDataContainer.PlanetData;
        if(planetData == null) { return; }

        planetData.PlanetSize = _displayedSize;
        planetData.PlanetResolution = _planetResolution;

        planetData.Seed = _seed;
        planetData.MountainsHeight = _mountainsHeight;
        planetData.HeightRemap = _heightRemap;
        planetData.NoiseScale = _noiseScale;
        planetData.NoiseGain = _noiseGain;
        planetData.NoiseLacunarity = _noiseLacunarity;
        planetData.DetailsIntensity = _detailsIntensity;

        planetData.TerrainTexture01 = _terrainTexture01;
        planetData.TerrainTexture02 = _terrainTexture02;
        planetData.TerrainTexture03 = _terrainTexture03;
        planetData.TerrainTexture04 = _terrainTexture04;
        planetData.TerrainTextureColor01 = _terrainTextureColor01;
        planetData.TerrainTextureColor02 = _terrainTextureColor02;
        planetData.TerrainTextureColor03 = _terrainTextureColor03;
        planetData.TerrainTextureColor04 = _terrainTextureColor04;
        planetData.TerrainTextureTilling01 = _terrainTextureTilling01;
        planetData.TerrainTextureTilling02 = _terrainTextureTilling02;
        planetData.TerrainTextureTilling03 = _terrainTextureTilling03;
        planetData.TerrainTextureTilling04 = _terrainTextureTilling04;
        planetData.TerrainTextureSmoothness01 = _terrainTextureSmoothness01;
        planetData.TerrainTextureSmoothness02 = _terrainTextureSmoothness02;
        planetData.TerrainTextureSmoothness03 = _terrainTextureSmoothness03;
        planetData.TerrainTextureSmoothness04 = _terrainTextureSmoothness04;
        planetData.TerrainTexturesSeparationSmoothness = _terrainTexturesSeparationSmoothness;
        planetData.TerrainTexture02Height = _terrainTexture02Height;
        planetData.TerrainTexture03Height = _terrainTexture03Height;
        planetData.TerrainTexture04Height = _terrainTexture04Height;

        planetData.HasAtmosphere = _hasAtmosphere;
        planetData.AtmosphereMainColor = _atmosphereMainColor;
        planetData.AtmosphereHorizonColor = _atmosphereHorizonColor;
        planetData.AtmosphereRadius = _atmosphereRadius;
        planetData.AtmosphereDensity = _atmosphereDensity;
        planetData.AtmosphereEdgeSmoothness = _atmosphereEdgeSmoothness;
        planetData.AtmosphereLightingDistance = _atmosphereLightingDistance;
        planetData.AtmospherePlanetVisibilityModifier = _atmospherePlanetVisibilityModifier;

        planetData.HasOcean = _hasOcean;
        planetData.OceanHeight = _oceanHeight;
        planetData.OceanColor = _oceanColor;
        planetData.OceanTexture = _oceanTexture;
        planetData.OceanNormalTexture = _oceanNormalTexture;
        planetData.OceanTextureTilling = _oceanTextureTilling;
        planetData.OceanSmoothness = _oceanSmoothness;
        planetData.OceanMetalness = _oceanMetalness;
        planetData.OceanHeightVariationIntensity = _oceanHeightVariationIntensity;
        planetData.OceanHeightVariationFrequency = _oceanHeightVariationFrequency;
        planetData.OceanHeightVariationSeed = _oceanHeightVariationSeed;
        planetData.OceanMovementSpeed = _oceanMovementSpeed;

        planetData.HasRings = _hasRings;
        planetData.RingsSize = _ringsSize;
        planetData.RingsRotation = _ringsRotation;
        planetData.RingsWidth = _ringsWidth;
        planetData.RingsColor = _ringsColor;
        planetData.RingsTexture = _ringsTexture;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(planetData);
        AssetDatabase.SaveAssets();
#endif
    }

    void ImportPlanetData()
    {
        _planetSize = _importedPlanetData.PlanetSize;
        _displayedSize = _planetSize;
        _planetResolution = _importedPlanetData.PlanetResolution;

        _planetShapeGenerator.UpdatePlanetShape(_selectedTerrainParent.gameObject, _planetSize, _planetResolution);

        _seed = _importedPlanetData.Seed;
        _heightRemap = _importedPlanetData.HeightRemap;
        _noiseScale = _importedPlanetData.NoiseScale;
        _noiseGain = _importedPlanetData.NoiseGain;
        _noiseLacunarity = _importedPlanetData.NoiseLacunarity;
        _detailsIntensity = _importedPlanetData.DetailsIntensity;

        _mountainsHeight = _importedPlanetData.MountainsHeight;
        _terrainTexture01 = _importedPlanetData.TerrainTexture01;
        _terrainTexture02 = _importedPlanetData.TerrainTexture02;
        _terrainTexture03 = _importedPlanetData.TerrainTexture03;
        _terrainTexture04 = _importedPlanetData.TerrainTexture04;
        _terrainTextureColor01 = _importedPlanetData.TerrainTextureColor01;
        _terrainTextureColor02 = _importedPlanetData.TerrainTextureColor02;
        _terrainTextureColor03 = _importedPlanetData.TerrainTextureColor03;
        _terrainTextureColor04 = _importedPlanetData.TerrainTextureColor04;
        _terrainTextureTilling01 = _importedPlanetData.TerrainTextureTilling01;
        _terrainTextureTilling02 = _importedPlanetData.TerrainTextureTilling02;
        _terrainTextureTilling03 = _importedPlanetData.TerrainTextureTilling03;
        _terrainTextureTilling04 = _importedPlanetData.TerrainTextureTilling04;
        _terrainTextureSmoothness01 = _importedPlanetData.TerrainTextureSmoothness01;
        _terrainTextureSmoothness02 = _importedPlanetData.TerrainTextureSmoothness02;
        _terrainTextureSmoothness03 = _importedPlanetData.TerrainTextureSmoothness03;
        _terrainTextureSmoothness04 = _importedPlanetData.TerrainTextureSmoothness04;
        _terrainTexturesSeparationSmoothness = _importedPlanetData.TerrainTexturesSeparationSmoothness;
        _terrainTexture02Height = _importedPlanetData.TerrainTexture02Height;
        _terrainTexture03Height = _importedPlanetData.TerrainTexture03Height;
        _terrainTexture04Height = _importedPlanetData.TerrainTexture04Height;

        _hasAtmosphere = _importedPlanetData.HasAtmosphere;
        _atmosphereMainColor = _importedPlanetData.AtmosphereMainColor;
        _atmosphereHorizonColor = _importedPlanetData.AtmosphereHorizonColor;
        _atmosphereRadius = _importedPlanetData.AtmosphereRadius;
        _atmosphereDensity = _importedPlanetData.AtmosphereDensity;
        _atmosphereEdgeSmoothness = _importedPlanetData.AtmosphereEdgeSmoothness;
        _atmospherePlanetVisibilityModifier = _importedPlanetData.AtmospherePlanetVisibilityModifier;
        _atmosphereLightingDistance = _importedPlanetData.AtmosphereLightingDistance;

        _hasOcean = _importedPlanetData.HasOcean;
        _oceanHeight = _importedPlanetData.OceanHeight;
        _oceanColor = _importedPlanetData.OceanColor;
        _oceanTexture = _importedPlanetData.OceanTexture;
        _oceanNormalTexture = _importedPlanetData.OceanNormalTexture;
        _oceanTextureTilling = _importedPlanetData.OceanTextureTilling;
        _oceanSmoothness = _importedPlanetData.OceanSmoothness;
        _oceanMetalness = _importedPlanetData.OceanMetalness;
        _oceanHeightVariationIntensity = _importedPlanetData.OceanHeightVariationIntensity;
        _oceanHeightVariationFrequency = _importedPlanetData.OceanHeightVariationFrequency;
        _oceanHeightVariationSeed = _importedPlanetData.OceanHeightVariationSeed;
        _oceanMovementSpeed = _importedPlanetData.OceanMovementSpeed;

        _hasRings = _importedPlanetData.HasRings;
        _ringsSize = _importedPlanetData.RingsSize;
        _ringsRotation = _importedPlanetData.RingsRotation;
        _ringsWidth = _importedPlanetData.RingsWidth;
        _ringsColor = _importedPlanetData.RingsColor;
        _ringsTexture = _importedPlanetData.RingsTexture;
    }

    public void ValidateRiversSources(SelectOnMapWindow selectionWindow = null)
    {
        float[] tempBuffer = new float[RIVERS_WINDOW_GRID_SIZE * RIVERS_WINDOW_GRID_SIZE];

        foreach(Vector2 sourcePosition in RiversSources)
        {
            tempBuffer[(int)sourcePosition.x + (int)sourcePosition.y * RIVERS_WINDOW_GRID_SIZE] = 1;
        }

        _heightMapGenerator.RiversSourcesTempBuffer = tempBuffer;
        _heightMapGenerator.UpdateRiversSources();

        if (selectionWindow != null) { selectionWindow.RiversMap = _heightMapGenerator.GetRiversMap(); }
    }

    public void UpdateRiversVisualization(SelectOnMapWindow selectionWindow)
    {
        selectionWindow.RiversMap = _heightMapGenerator.GetRiversMap();
    }

    #endregion


    //---------------------------------- [ Custom Editor Window ] ----------------------------------

    #region CustomEditorWindow

    [MenuItem("Tools/PlanetCreation _F1")]
    public static void ShowWindow()
    {
        EditorWindow window = EditorWindow.GetWindow(typeof(PlanetGenerationTool));
        window.titleContent = new GUIContent("PlanetCreationTool");
        window.Show();
    }

    private void OnGUI()
    {
        CreatedGUIStyles();

        ScrollAndMarginsStart();
        {
            DisplayModuleName();

            GUILayout.Space(TOP_MARGIN);

            MainMenuSelection();

            TabContent();
        }
        ScrollAndMarginsEnd();

        SetParameters();


        void CreatedGUIStyles()
        {
            _centeredStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            _boldCenteredStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            _boldCenteredStyle.fontStyle = FontStyle.Bold;
            _boldCenteredStyle.fontSize = 15;
        }

        void ScrollAndMarginsStart()
        {
            Rect area = new Rect(Vector2.zero, position.size);
            RectOffset padding = new RectOffset(LEFT_AND_RIGHT_MARGINS, LEFT_AND_RIGHT_MARGINS, TOP_MARGIN, BOTTOM_MARGIN);
            area = padding.Remove(area);
            GUILayout.BeginArea(area);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        }

        void ScrollAndMarginsEnd()
        {
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void DisplayModuleName()
        {
            GUIContent label;
            if (_selectedPlanet == null) { label = new GUIContent(" No planet selected", EditorGUIUtility.IconContent("d_scenepicking_notpickable_hover").image); }
            else { label = new GUIContent($" Selected planet : {_selectedPlanet.transform.name}", EditorGUIUtility.IconContent("d_PreMatSphere").image); }

            GUI.contentColor = Color.cyan;
            EditorGUILayout.LabelField(label, _boldCenteredStyle);
            GUI.contentColor = Color.white;
        }

        void MainMenuSelection()
        {
            if(!_allowNavigation) { GUI.enabled = false; }

            _mainMenuIndex = GUILayout.SelectionGrid(_mainMenuIndex, _mainMenuContents, Mathf.CeilToInt(_mainMenuContents.Length/2.0f));

            GUI.enabled = true;
        }

        void TabContent()
        {
            if(_mainMenuIndex == CREATION_TAB_INDEX)
            {
                CreationTab();
            }

            else if(_mainMenuIndex == ATMOSPHERE_TAB_INDEX)
            {
                AtmosphereTab();
            }

            else if(_mainMenuIndex == RINGS_TAB_INDEX)
            {
                RingsTab();
            }

            else if(_mainMenuIndex == TERRAIN_TAB_INDEX)
            {
                TerrainTab();
            }

            else if(_mainMenuIndex == OCEAN_TAB_INDEX)
            {
                OceanTab();
            }

            else if (_mainMenuIndex == RIVERS_TAB_INDEX)
            {
                RiversTab();
            }


            void CreationTab()
            {
                GUILayout.Space(BIG_SPACE);
                DrawUILine();
                GUILayout.Space(SMALL_SPACE);
                SectionTitle("Create new planet");

                GUILayout.Space(SMALL_SPACE);

                if(_showInvalidNameError)
                {
                    GUI.contentColor = Color.red;
                    EditorGUILayout.LabelField(new GUIContent(" Invalid name : Blank, already used or with special characters", EditorGUIUtility.IconContent("d_Invalid").image), _centeredStyle, GUILayout.MinHeight(ERROR_MESSAGE_HEIGHT));
                    GUI.contentColor = Color.white;
                }

                _planetName = EditorGUILayout.TextField(new GUIContent("Planet name :", _toolTips[0]), _planetName);

                _planetSize = EditorGUILayout.FloatField(new GUIContent("Planet size :", _toolTips[1]), _planetSize);
                if(_planetSize <= 0) { _planetSize = 1; }
                if(_planetSize > 60) { _planetSize = 60; } // Temporary size limit

                _planetResolution = EditorGUILayout.IntField(new GUIContent("Planet resolution :", _toolTips[2]), _planetResolution);
                if(_planetResolution <= 0) { _planetResolution = 1; }

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Spawn coordinates :", _toolTips[3]));
                _planetSpawnCoordinates = EditorGUILayout.Vector3Field("", _planetSpawnCoordinates);
                GUILayout.EndHorizontal();
                GUILayout.Space(BIG_SPACE);

                if (GUILayout.Button(new GUIContent(" Create new planet", EditorGUIUtility.IconContent("CreateAddNew").image)))
                {
                    if(_planetName == null || _planetName == string.Empty) { _showInvalidNameError = true; }
                    else
                    {
                        CreateNewPlanet();
                        ResetTextField(_planetName);

                        _displayedSize = _planetSize;
                        _showInvalidNameError = false;
                        _allowNavigation = true;
                        _mainMenuIndex = TERRAIN_TAB_INDEX;
                    }
                }

                GUILayout.Space(BIG_SPACE);

                DrawUILine();
                EditorGUILayout.LabelField("OR", _boldCenteredStyle);
                DrawUILine();

                GUILayout.Space(SMALL_SPACE);

                GUI.backgroundColor = Color.white;
                SectionTitle("Import saved planet data");

                GUILayout.Space(BIG_SPACE);

                GUILayout.BeginHorizontal();
                _importedPlanetData = EditorGUILayout.ObjectField(_importedPlanetData, typeof(PlanetData), false) as PlanetData;
                if (GUILayout.Button(new GUIContent(" Import", EditorGUIUtility.IconContent("d_Import").image)))
                {
                    if(_importedPlanetData != null)
                    {
                        ImportPlanetData();
                    }
                }
                GUILayout.EndHorizontal();

                GUI.backgroundColor = Color.grey;

            }

            void AtmosphereTab()
            {
                GUILayout.Space(BIG_SPACE);
                DrawUILine();
                GUILayout.Space(SMALL_SPACE);
                SectionTitle("Atmosphere parameters");

                GUILayout.Space(SMALL_SPACE);

                _hasAtmosphere = EditorGUILayout.Toggle("Has atmosphere :", _hasAtmosphere);
                GUILayout.Space(SMALL_SPACE);

                if (_hasAtmosphere )
                {
                    _atmosphereMainColor = EditorGUILayout.ColorField(new GUIContent("Main color :"), _atmosphereMainColor, true, false, true);
                    _atmosphereHorizonColor = EditorGUILayout.ColorField(new GUIContent("Horizon color :"), _atmosphereHorizonColor, true, false, true);
                    _atmosphereRadius = EditorGUILayout.FloatField("Radius", _atmosphereRadius);
                    if (_atmosphereRadius < 0) { _atmosphereRadius = 0; }
                    _atmosphereDensity = EditorGUILayout.FloatField("Density :", _atmosphereDensity);
                    if (_atmosphereDensity < 0) { _atmosphereDensity = 0; }
                    _atmosphereEdgeSmoothness = EditorGUILayout.FloatField("Edge smoothness :", _atmosphereEdgeSmoothness);
                    if (_atmosphereEdgeSmoothness < 0) { _atmosphereEdgeSmoothness = 0; }
                    _atmospherePlanetVisibilityModifier = EditorGUILayout.Slider("Planet visibility modifier :", _atmospherePlanetVisibilityModifier, -70.0f, 70.0f);
                    _atmosphereLightingDistance = EditorGUILayout.Slider("Lighting distance :", _atmosphereLightingDistance, -2.0f, 3.0f);
                }

                GUILayout.Space(BIG_SPACE);
                DrawUILine();
                GUILayout.Space(SMALL_SPACE);

                if (GUILayout.Button("Reset to default"))
                {
                    ApplyAtmosphereDefaultParameters();
                }
            }

            void RingsTab()
            {
                GUILayout.Space(BIG_SPACE);
                DrawUILine();
                GUILayout.Space(SMALL_SPACE);
                SectionTitle("Rings parameters");

                GUILayout.Space(SMALL_SPACE);

                _hasRings = EditorGUILayout.Toggle("Has rings :", _hasRings);
                GUILayout.Space(SMALL_SPACE);

                if (_hasRings)
                {
                    _ringsSize = EditorGUILayout.FloatField("Size :", _ringsSize);
                    if (_ringsSize < 0) { _ringsSize = 0; }
                    _ringsWidth = EditorGUILayout.Slider("Width :", _ringsWidth, 0.0f, 1.0f);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("Rotation :"));
                    _ringsRotation = EditorGUILayout.Vector3Field("", _ringsRotation);
                    GUILayout.EndHorizontal();
                    _ringsColor = EditorGUILayout.ColorField(new GUIContent("Color :"), _ringsColor, true, false, true);
                    _ringsTexture = (Texture2D)EditorGUILayout.ObjectField("Texture :", _ringsTexture, typeof(Texture2D), false);
                }

                GUILayout.Space(BIG_SPACE);
                DrawUILine();
                GUILayout.Space(SMALL_SPACE);

                if (GUILayout.Button("Reset to default"))
                {
                    ApplyRingsDefaultParameters();
                }
            }

            void TerrainTab()
            {
                GUILayout.Space(BIG_SPACE);
                DrawUILine();
                GUILayout.Space(SMALL_SPACE);
                SectionTitle("Planet parameters");

                GUILayout.Space(SMALL_SPACE);

                // Temporary size limit message
                EditorGUILayout.LabelField(new GUIContent(" Size is currently limited to 60", EditorGUIUtility.IconContent("d_Invalid").image), _centeredStyle, GUILayout.MinHeight(ERROR_MESSAGE_HEIGHT));


                _planetSize = EditorGUILayout.FloatField("Size :", _planetSize);
                if(_planetSize > 60) { _planetSize = 60; } // Temporary size limit
                _planetResolution = EditorGUILayout.IntField("Resolution :", _planetResolution);

                GUILayout.Space(SMALL_SPACE);

                if (GUILayout.Button("Update"))
                {
                    _planetShapeGenerator.UpdatePlanetShape(_selectedTerrainParent.gameObject, _planetSize, _planetResolution);
                    float scaleValue = Mathf.Max(_planetSize * 3, ATMOSPHERE_MIN_SIZE);
                    _selectedAtmosphere.transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
                    _displayedSize = _planetSize;
                }

                GUILayout.Space(BIG_SPACE);
                DrawUILine();
                GUILayout.Space(SMALL_SPACE);
                SectionTitle("Terrain shape parameters");
                GUILayout.Space(SMALL_SPACE);

                _seed = EditorGUILayout.IntField("Seed :", _seed);
                _mountainsHeight = EditorGUILayout.FloatField("Mountains height :", _mountainsHeight);
                _heightRemap = EditorGUILayout.CurveField("Height remap :", _heightRemap);
                _noiseScale = EditorGUILayout.FloatField("Noise scale :", _noiseScale);
                _noiseGain = EditorGUILayout.Slider("Noise gain :", _noiseGain, 1.0f, 3.0f);
                _noiseLacunarity = EditorGUILayout.Slider("Noise lacunarity :", _noiseLacunarity, 0.1f, 0.9f);
                _detailsIntensity = EditorGUILayout.FloatField("Details intensity :", _detailsIntensity);

                GUILayout.Space(BIG_SPACE);
                DrawUILine();
                GUILayout.Space(SMALL_SPACE);
                SectionTitle("Terrain material parameters");
                GUILayout.Space(SMALL_SPACE);

                _terrainTextureColor01 = EditorGUILayout.ColorField("Terrain 1 color :", _terrainTextureColor01);
                _terrainTextureColor02 = EditorGUILayout.ColorField("Terrain 2 color :", _terrainTextureColor02);
                _terrainTextureColor03 = EditorGUILayout.ColorField("Terrain 3 color :", _terrainTextureColor03);
                _terrainTextureColor04 = EditorGUILayout.ColorField("Terrain 4 color :", _terrainTextureColor04);
                _terrainTexture01 = (Texture2D)EditorGUILayout.ObjectField("Terrain 1 texture :", _terrainTexture01, typeof(Texture2D), false);
                _terrainTexture02 = (Texture2D)EditorGUILayout.ObjectField("Terrain 2 texture :", _terrainTexture02, typeof(Texture2D), false);
                _terrainTexture03 = (Texture2D)EditorGUILayout.ObjectField("Terrain 3 texture :", _terrainTexture03, typeof(Texture2D), false);
                _terrainTexture04 = (Texture2D)EditorGUILayout.ObjectField("Terrain 4 texture :", _terrainTexture04, typeof(Texture2D), false);
                _terrainTextureTilling01 = EditorGUILayout.FloatField("Terrain 1 texture tilling :", _terrainTextureTilling01);
                _terrainTextureTilling02 = EditorGUILayout.FloatField("Terrain 2 texture tilling :", _terrainTextureTilling02);
                _terrainTextureTilling03 = EditorGUILayout.FloatField("Terrain 3 texture tilling :", _terrainTextureTilling03);
                _terrainTextureTilling04 = EditorGUILayout.FloatField("Terrain 4 texture tilling :", _terrainTextureTilling04);
                _terrainTextureSmoothness01 = EditorGUILayout.Slider("Terrain 1 smoothness :", _terrainTextureSmoothness01, 0.0f, 1.0f);
                _terrainTextureSmoothness02 = EditorGUILayout.Slider("Terrain 2 smoothness :", _terrainTextureSmoothness02, 0.0f, 1.0f);
                _terrainTextureSmoothness03 = EditorGUILayout.Slider("Terrain 3 smoothness :", _terrainTextureSmoothness03, 0.0f, 1.0f);
                _terrainTextureSmoothness04 = EditorGUILayout.Slider("Terrain 4 smoothness :", _terrainTextureSmoothness04, 0.0f, 1.0f);
                _terrainTexturesSeparationSmoothness = EditorGUILayout.Slider("Texture separation smoothness :", _terrainTexturesSeparationSmoothness, 0.0f, 1.0f);
                _terrainTexture02Height = EditorGUILayout.Slider("Terrain 2 starting height :", _terrainTexture02Height, 0.0f, 1.0f);
                _terrainTexture03Height = EditorGUILayout.Slider("Terrain 3 starting height :", _terrainTexture03Height, 0.0f, 1.0f);
                _terrainTexture04Height = EditorGUILayout.Slider("Terrain 4 starting height :", _terrainTexture04Height, 0.0f, 1.0f);

                GUILayout.Space(BIG_SPACE);
                DrawUILine();
                GUILayout.Space(SMALL_SPACE);

                if (GUILayout.Button("Reset to default"))
                {
                    ApplyShapeDefaultParameters();
                    ApplyTerrainDefaultParameters();
                }

                if (GUILayout.Button("Export textures"))
                {
                    _heightMapGenerator.ExportAllTextures();
                }
            }

            void OceanTab()
            {
                GUILayout.Space(BIG_SPACE);
                DrawUILine();
                GUILayout.Space(SMALL_SPACE);
                SectionTitle("Ocean parameters");

                GUILayout.Space(SMALL_SPACE);

                _hasOcean = EditorGUILayout.Toggle("Has ocean :", _hasOcean);
                GUILayout.Space(SMALL_SPACE);

                if (_hasOcean)
                {
                    _oceanHeight = EditorGUILayout.FloatField("Water level :", _oceanHeight);
                    _oceanColor = EditorGUILayout.ColorField(new GUIContent("Color :"), _oceanColor, true, false, true);
                    _oceanTexture = (Texture2D)EditorGUILayout.ObjectField("Base color texture :", _oceanTexture, typeof(Texture2D), false);
                    _oceanNormalTexture = (Texture2D)EditorGUILayout.ObjectField("Normal texture :", _oceanNormalTexture, typeof(Texture2D), false);
                    _oceanTextureTilling = EditorGUILayout.FloatField("Texture tilling :", _oceanTextureTilling);
                    _oceanSmoothness = EditorGUILayout.Slider("Smoothness :", _oceanSmoothness, 0.0f, 1.0f);
                    _oceanMetalness = EditorGUILayout.Slider("Metalness :", _oceanMetalness, 0.0f, 1.0f);
                    _oceanHeightVariationIntensity = EditorGUILayout.FloatField("Height variation intensity :", _oceanHeightVariationIntensity);
                    if (_oceanHeightVariationIntensity < 0) { _oceanHeightVariationIntensity = 0; }
                    _oceanHeightVariationFrequency = EditorGUILayout.FloatField("Height variation frequency :", _oceanHeightVariationFrequency);
                    if (_oceanHeightVariationFrequency < 0) { _oceanHeightVariationFrequency = 0; }
                    _oceanHeightVariationSeed = EditorGUILayout.FloatField("Height variation seed :", _oceanHeightVariationSeed);
                    _oceanMovementSpeed = EditorGUILayout.FloatField("Movement speed :", _oceanMovementSpeed);
                }

                GUILayout.Space(BIG_SPACE);
                DrawUILine();
                GUILayout.Space(SMALL_SPACE);

                if (GUILayout.Button("Reset to default"))
                {
                    ApplyOceanDefaultParameters();
                }
            }

            void RiversTab()
            {
                GUILayout.Space(BIG_SPACE);
                DrawUILine();
                GUILayout.Space(SMALL_SPACE);
                SectionTitle("Rivers sources list");

                GUILayout.Space(SMALL_SPACE);

                if (GUILayout.Button("Modify"))
                {
                    SelectOnMapWindow selectionWindow = (SelectOnMapWindow)EditorWindow.GetWindow(typeof(SelectOnMapWindow), true, "Selection Window", true);
                    selectionWindow.minSize = new Vector2(535f, 632f);
                    selectionWindow.LinkedWindow = this;
                    selectionWindow.LinkedWindowScript = this;
                    selectionWindow.HeightMap = _heightMapGenerator.GetHeightMap();
                    selectionWindow.RiversMap = _heightMapGenerator.GetRiversMap();
                    selectionWindow.SetupGrid();

                    selectionWindow.Show();
                }

                GUILayout.Space(SMALL_SPACE);
                DrawUILine();

                if(RiversSources.Count > 0)
                {
                    for (int i = 0; i < RiversSources.Count; i++)
                    {
                        EditorGUILayout.LabelField($"• River {i + 1} - x : {RiversSources[i].x} / y : {RiversSources[i].y}");
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("• There is no rivers source in the list, press the Modify button to add a new river source");
                }

                DrawUILine();
                GUILayout.Space(SMALL_SPACE);

                if (GUILayout.Button("Clear all"))
                {
                    RiversSources.Clear();
                    ValidateRiversSources();
                }
            }
        }

        // Draw a UI line for separation
        //https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/
        void DrawUILine(Color color = default, int thickness = 1, int padding = 15, int margin = 0)
        {
            color = color != default ? color : Color.grey;
            Rect r = EditorGUILayout.GetControlRect(false, GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding * 0.5f;

            switch (margin)
            {
                // expand to maximum width
                case < 0:
                    r.x = 0;
                    r.width = EditorGUIUtility.currentViewWidth;

                    break;
                case > 0:
                    // shrink line width
                    r.x += margin;
                    r.width -= margin * 2;

                    break;
            }

            EditorGUI.DrawRect(r, color);
        }

        void ResetTextField(string textField)
        {
            GUIUtility.keyboardControl = 0;
            GUIUtility.hotControl = 0;
            textField = string.Empty;
            _planetName = string.Empty;
        }

        void SectionTitle(string title)
        {
            EditorGUILayout.LabelField(new GUIContent($" {title}", EditorGUIUtility.IconContent("animationkeyframe").image), EditorStyles.boldLabel);
        }

        void SetParameters()
        {
            if(!_heightMapGenerator || !_selectedTerrainMaterial) return;

            _heightMapGenerator.Seed = _seed;
            _heightMapGenerator.heightRemap = _heightRemap;
            _heightMapGenerator.NoiseScale = _noiseScale;
            _heightMapGenerator.NoiseGain = _noiseGain;
            _heightMapGenerator.NoiseLacunarity = _noiseLacunarity;
            _heightMapGenerator.NormalIntensity = _detailsIntensity;

            _heightMapGenerator.UpdateTerrain();

            _selectedTerrainMaterial.SetFloat("_DisplacementIntensity", _mountainsHeight);
            _selectedTerrainMaterial.SetTexture("_Texture01", _terrainTexture01);
            _selectedTerrainMaterial.SetTexture("_Texture02", _terrainTexture02);
            _selectedTerrainMaterial.SetTexture("_Texture03", _terrainTexture03);
            _selectedTerrainMaterial.SetTexture("_Texture04", _terrainTexture04);
            _selectedTerrainMaterial.SetColor("_Color01", _terrainTextureColor01);
            _selectedTerrainMaterial.SetColor("_Color02", _terrainTextureColor02);
            _selectedTerrainMaterial.SetColor("_Color03", _terrainTextureColor03);
            _selectedTerrainMaterial.SetColor("_Color04", _terrainTextureColor04);
            _selectedTerrainMaterial.SetFloat("_TillingTexture01", _terrainTextureTilling01);
            _selectedTerrainMaterial.SetFloat("_TillingTexture02", _terrainTextureTilling02);
            _selectedTerrainMaterial.SetFloat("_TillingTexture03", _terrainTextureTilling03);
            _selectedTerrainMaterial.SetFloat("_TillingTexture04", _terrainTextureTilling04);
            _selectedTerrainMaterial.SetFloat("_SmoothnessTexture01", _terrainTextureSmoothness01);
            _selectedTerrainMaterial.SetFloat("_SmoothnessTexture02", _terrainTextureSmoothness02);
            _selectedTerrainMaterial.SetFloat("_SmoothnessTexture03", _terrainTextureSmoothness03);
            _selectedTerrainMaterial.SetFloat("_SmoothnessTexture04", _terrainTextureSmoothness04);
            _selectedTerrainMaterial.SetFloat("_TextureSeparationSmoothness", _terrainTexturesSeparationSmoothness);
            _selectedTerrainMaterial.SetFloat("_Texture02Height", _terrainTexture02Height);
            _selectedTerrainMaterial.SetFloat("_Texture03Height", _terrainTexture03Height);
            _selectedTerrainMaterial.SetFloat("_Texture04Height", _terrainTexture04Height);

            _selectedAtmosphere.gameObject.SetActive(_hasAtmosphere);
            _selectedAtmosphereMaterial.SetColor("_BaseColor", _atmosphereMainColor);
            _selectedAtmosphereMaterial.SetColor("_HorizonColor", _atmosphereHorizonColor);
            _selectedAtmosphereMaterial.SetFloat("_Radius", _atmosphereRadius + _displayedSize);
            _selectedAtmosphereMaterial.SetFloat("_Density", _atmosphereDensity);
            _selectedAtmosphereMaterial.SetFloat("_DensityPower", _atmosphereEdgeSmoothness);
            _selectedAtmosphereMaterial.SetFloat("_PlanetVisibility", _atmospherePlanetVisibilityModifier);
            _selectedAtmosphereMaterial.SetFloat("_LightingRadius", _atmosphereLightingDistance);

            _selectedOcean.gameObject.SetActive(_hasOcean);
            float oceanScale = _displayedSize + _oceanHeight;
            _selectedOcean.transform.localScale = new Vector3(oceanScale, oceanScale, oceanScale);
            _selectedOceanMaterial.SetColor("_Color", _oceanColor);
            _selectedOceanMaterial.SetTexture("_BaseColor", _oceanTexture);
            _selectedOceanMaterial.SetTexture("_Normal", _oceanNormalTexture);
            _selectedOceanMaterial.SetFloat("_WaterTextureTilling", _oceanTextureTilling);
            _selectedOceanMaterial.SetFloat("_Smoothness", _oceanSmoothness);
            _selectedOceanMaterial.SetFloat("_Metalness", _oceanMetalness);
            _selectedOceanMaterial.SetFloat("_OceanHeightVariation", _oceanHeightVariationIntensity);
            _selectedOceanMaterial.SetFloat("_HeightNoiseScale", _oceanHeightVariationFrequency);
            _selectedOceanMaterial.SetFloat("_Seed", _oceanHeightVariationSeed);
            _selectedOceanMaterial.SetFloat("_WaterMovementSpeed", _oceanMovementSpeed);

            _selectedRings.gameObject.SetActive(_hasRings);
            float ringsScale = _displayedSize + _ringsSize;
            _selectedRings.transform.localScale = new Vector3(ringsScale, ringsScale, ringsScale);
            _selectedRings.transform.rotation = Quaternion.Euler(_ringsRotation);
            _selectedRingsMaterial.SetFloat("_Width", _ringsWidth);
            _selectedRingsMaterial.SetColor("_Color", _ringsColor);
            _selectedRingsMaterial.SetTexture("_MainTex", _ringsTexture);
        }
    }

    #endregion
}
