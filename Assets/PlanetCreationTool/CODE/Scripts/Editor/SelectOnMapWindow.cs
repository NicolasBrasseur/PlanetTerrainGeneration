using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine.Rendering;

public class SelectOnMapWindow : EditorWindow
{
    // Public
    public EditorWindow LinkedWindow;
    public Texture HeightMap;

    // Private
    private Vector2 _scrollPosition;
    private VisualElement _gridElement;

    // Consts
    private const int TOP_MARGIN = 20;
    private const int BOTTOM_MARGIN = 15;
    private const int LEFT_AND_RIGHT_MARGINS = 10;

    private void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // Grid
        _gridElement = new VisualElement();
        root.Add(_gridElement);

        Debug.Log("test");

        SetupGrid();
    }

    private void OnGUI()
    {
        ScrollAndMarginsStart();

        SectionTitle("Select a river source location");

        EditorGUI.DrawPreviewTexture(new Rect(0, 60, 512, 512), HeightMap); //GUILayout.Label(HeightMap);

        if (GUILayout.Button("Close"))
        {
            LinkedWindow.Focus();
            this.Close();
        }

        ScrollAndMarginsEnd();
    }

    public void SetupGrid()
    {
        _gridElement.Clear();

        for (int x = 0; x < 128; x++)
        {
            for (int y = 0; y < 128; y++)
            {
                Cell cell = new Cell(x, y, false, OnChangeCell);
                _gridElement.Add(cell);
            }
        }
    }

    private void OnChangeCell(int x, int y, bool isPressed, bool isActive)
    {

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

            SetupStyle();

            clicked += OnClick;
        }

        private void OnClick()
        {
            isPressed = !isPressed;
            callback.Invoke(x, y, isPressed, isActive);
            SetupStyle();

            Debug.Log(x + " " + y);
        }

        private void SetupStyle()
        {
            style.backgroundColor = isPressed ? Color.blue : Color.clear;

            style.position = Position.Absolute;
            style.left = x * 4 + LEFT_AND_RIGHT_MARGINS;
            style.top = y * 4 + 60 + TOP_MARGIN;

            style.width = 4;
            style.height = 4;

            style.marginBottom = 0;
            style.marginLeft = 0;
            style.marginRight = 0;
            style.marginTop = 0;

            style.paddingBottom = 0;
            style.paddingLeft = 0;
            style.paddingRight = 0;
            style.paddingTop = 0;

            style.borderBottomLeftRadius = 0;
            style.borderBottomRightRadius = 0;
            style.borderTopLeftRadius = 0;
            style.borderTopRightRadius = 0;

            style.borderBottomWidth = 0;
            style.borderLeftWidth = 0;
            style.borderTopWidth = 0;
            style.borderRightWidth = 0;

            style.opacity = 1f;
        }
    }

}
