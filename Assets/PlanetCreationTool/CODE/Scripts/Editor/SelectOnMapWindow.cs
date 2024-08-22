using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;
using System.Linq;
using UnityEngine.UIElements;

public class SelectOnMapWindow : EditorWindow
{
    // Public
    public EditorWindow LinkedWindow;
    public PlanetGenerationTool LinkedWindowScript;
    public Texture HeightMap;
    public Texture RiversMap;

    // Private
    private Vector2 _scrollPosition;
    private VisualElement _gridElement;
    private Dictionary<Vector2, bool> _temporarySourcesChanges = new Dictionary<Vector2, bool>();
    private Shader _combineTexturesShader;
    private Material _combineTexturesMaterial;
    private RenderTexture _combinedPreviewTexture;

    // Consts
    private const int TOP_MARGIN = 20;
    private const int SELECTION_AREA_TOP_MARGIN = 30;
    private const int BOTTOM_MARGIN = 15;
    private const int LEFT_AND_RIGHT_MARGINS = 10;
    private const int CELL_SIZE = 8;
    private const int TEXTURE_SIZE = 512;
    private const string COMBINE_TEXTURES_SHADER = "CombineTextures";

    #region Custom GUI

    private void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // Grid
        _gridElement = new VisualElement();
        root.Add(_gridElement);
    }

    private void OnGUI()
    {
        ScrollAndMarginsStart();

        SectionTitle("Select the rivers source location (click to place or remove a source)");

        DrawPreviewTexture();

        GUILayout.Space(TEXTURE_SIZE + TOP_MARGIN);

        if (_temporarySourcesChanges.Count > 0)
        {
            if (GUILayout.Button("Validate"))
            {
                ValidateChanges();
                SetupGrid();
            }

            if (GUILayout.Button("Cancel"))
            {
                _temporarySourcesChanges.Clear();
                LinkedWindow.Focus();
                this.Close();
            }
        }
        else
        {
            if (GUILayout.Button("Back"))
            {
                _temporarySourcesChanges.Clear();
                LinkedWindow.Focus();
                this.Close();
            }
        }

        ScrollAndMarginsEnd();
    }

    #endregion

    private void OnEnable()
    {
        InitCombineTexture();
    }

    private void Update()
    {
        UpdateVisualization();
        Repaint();
    }

    #region Custom methods

    public void SetupGrid()
    {
        _gridElement.Clear();
        _temporarySourcesChanges.Clear();

        HashSet<Vector2> temporarySourcesList = new HashSet<Vector2>();

        if (LinkedWindowScript.RiversSources != null)
        {
            foreach (var element in LinkedWindowScript.RiversSources)
            {
                temporarySourcesList.Add(element);
            }
        }

        for (int x = 0; x < TEXTURE_SIZE / CELL_SIZE; x++)
        {
            for (int y = 0; y < TEXTURE_SIZE / CELL_SIZE; y++)
            {
                bool hasASource = temporarySourcesList.Contains(new Vector2(x, y));

                Cell cell = new Cell(x, y, hasASource, OnChangeCell);
                _gridElement.Add(cell);
            }
        }

        temporarySourcesList.Clear();
    }

    private void OnChangeCell(int x, int y, bool isPressed, bool isActive)
    {
        Vector2 cellPosition = new Vector2(x, y);

        if (_temporarySourcesChanges.ContainsKey(cellPosition)) { _temporarySourcesChanges.Remove(cellPosition); }

        if (!isPressed) { return; }

        bool isAnAddition = !isActive;
        _temporarySourcesChanges.Add(cellPosition, isAnAddition);
    }

    private void ValidateChanges()
    {
        List<Vector2> sourcesList = LinkedWindowScript.RiversSources;

        foreach (var cell in _temporarySourcesChanges)
        {
            if (cell.Value) //Value contains the operation needed (addition or removal of a source)
            {
                //Add source
                sourcesList.Add(cell.Key);
            }
            else
            {
                //Remove source
                sourcesList.Remove(cell.Key);
            }
        }

        LinkedWindowScript.RiversSources = sourcesList;
        LinkedWindowScript.ValidateRiversSources(this);
    }

    private void UpdateVisualization()
    {
        LinkedWindowScript.UpdateRiversVisualization(this);
    }

    void SectionTitle(string title)
    {
        EditorGUILayout.LabelField(new GUIContent($" {title}", EditorGUIUtility.IconContent("animationkeyframe").image), EditorStyles.boldLabel);
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

    void InitCombineTexture()
    {
        _combineTexturesShader = Resources.Load<Shader>(COMBINE_TEXTURES_SHADER);
        _combineTexturesMaterial = new Material(_combineTexturesShader);
        _combinedPreviewTexture = new RenderTexture(2048, 2048, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
    }

    void DrawPreviewTexture()
    {
        if(_combineTexturesMaterial == null || _combinedPreviewTexture == null) { InitCombineTexture(); }

        _combineTexturesMaterial.SetTexture("_HeightMap", HeightMap);
        _combineTexturesMaterial.SetTexture("_RiversMap", RiversMap);
        _combineTexturesMaterial.SetColor("_RiversColor", Color.blue);

        RenderTexture windowRenderTexture = RenderTexture.active;

        Graphics.Blit(null, _combinedPreviewTexture, _combineTexturesMaterial, 0);
        RenderTexture.active = windowRenderTexture;

        EditorGUI.DrawPreviewTexture(new Rect(0, SELECTION_AREA_TOP_MARGIN, 512, 512), _combinedPreviewTexture);

        //EditorGUI.DrawPreviewTexture(new Rect(0, SELECTION_AREA_TOP_MARGIN, 512, 512), HeightMap);
        //EditorGUI.DrawPreviewTexture(new Rect(0, SELECTION_AREA_TOP_MARGIN, 512, 512), RiversMap);
    }

    #endregion

    // -------------- Cell Class -----------------

    public class Cell : Button
    {
        public int x;
        public int y;
        public bool isPressed;
        public bool isActive;
        public Callback callback;

        public delegate void Callback(int x, int y, bool isPressed, bool isActive);

        public Cell(int x, int y, bool isActive, Callback callback)
        {
            this.x = x;
            this.y = y;
            this.isPressed = false;
            this.isActive = isActive;
            this.callback = callback;

            UpdateGridStyle();

            clicked += OnClick;
        }

        private void OnClick()
        {
            isPressed = !isPressed;
            callback.Invoke(x, y, isPressed, isActive);
            UpdateGridStyle();
        }

        private void UpdateGridStyle()
        {
            style.borderBottomLeftRadius = 10;
            style.borderBottomRightRadius = 10;
            style.borderTopLeftRadius = 10;
            style.borderTopRightRadius = 10;

            style.borderBottomWidth = 1;
            style.borderLeftWidth = 1;
            style.borderTopWidth = 1;
            style.borderRightWidth = 1;

            style.borderBottomColor = Color.black;
            style.borderLeftColor = Color.black;
            style.borderRightColor = Color.black;
            style.borderTopColor = Color.black;

            if (isPressed && !isActive) { style.backgroundColor = Color.green; }
            else if(isPressed && isActive) { style.backgroundColor = Color.red; }
            else if(!isPressed && isActive)
            {
                style.backgroundColor = (Color)new Vector4(0,0,1,1.0f);
            }
            else 
            {
                style.backgroundColor = Color.clear;

                style.borderBottomLeftRadius = 0;
                style.borderBottomRightRadius = 0;
                style.borderTopLeftRadius = 0;
                style.borderTopRightRadius = 0;

                style.borderBottomWidth = 0;
                style.borderLeftWidth = 0;
                style.borderTopWidth = 0;
                style.borderRightWidth = 0;
            }

            style.position = Position.Absolute;
            style.left = x * CELL_SIZE + LEFT_AND_RIGHT_MARGINS;
            style.top = y * CELL_SIZE + SELECTION_AREA_TOP_MARGIN + TOP_MARGIN;

            style.width = CELL_SIZE;
            style.height = CELL_SIZE;

            style.marginBottom = 0;
            style.marginLeft = 0;
            style.marginRight = 0;
            style.marginTop = 0;

            style.paddingBottom = 0;
            style.paddingLeft = 0;
            style.paddingRight = 0;
            style.paddingTop = 0;

            style.opacity = 1f;
        }
    }

}
