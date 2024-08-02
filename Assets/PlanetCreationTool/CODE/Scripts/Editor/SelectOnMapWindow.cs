using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class SelectOnMapWindow : EditorWindow
{
    // Public
    public EditorWindow LinkedWindow;
    public Texture HeightMap;

    // Private
    private Vector2 _scrollPosition;

    // Consts
    private const int TOP_MARGIN = 20;
    private const int BOTTOM_MARGIN = 15;
    private const int LEFT_AND_RIGHT_MARGINS = 10;

    private void OnGUI()
    {
        ScrollAndMarginsStart();

        SectionTitle("Select a river source location");

        EditorGUI.DrawPreviewTexture(new Rect(0, 60, 512, 512), HeightMap);
        //Debug.Log(heightMap);

        //GUILayout.Label(HeightMap);

        if (GUILayout.Button("Close"))
        {
            LinkedWindow.Focus();
            this.Close();
        }

        ScrollAndMarginsEnd();
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
}
