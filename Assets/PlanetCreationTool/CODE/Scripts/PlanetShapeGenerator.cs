using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[SelectionBase]
public class PlanetShapeGenerator : MonoBehaviour
{
    //[Range(1, 250)][SerializeField] private int _resolution = 1;
    //[Range(0.1f, 10f)][SerializeField] private float _size = 1;

    private Vector3[] _directions =
    {
        Vector3.up,
        Vector3.down,
        Vector3.right,
        Vector3.left,
        Vector3.forward,
        Vector3.back
    };

    private MeshFilter[] _filter;
    private MeshRenderer[] _meshRenderer;

    public MeshRenderer[] GenerateInitialMesh(GameObject parent, float size, int resolution)
    {
        _filter = new MeshFilter[6];
        _meshRenderer = new MeshRenderer[6];

        for (int i = 0; i < 6; i++)
        {
            var children = new GameObject("Planet Face");
            children.transform.parent = parent.transform;
            _filter[i] = children.AddComponent<MeshFilter>();
            _meshRenderer[i] = children.AddComponent<MeshRenderer>();

            GenerateSphereSectionMesh(_meshRenderer[i], _filter[i], resolution, size, _directions[i]);
        }

        return _meshRenderer;
    }

    public void UpdatePlanetShape(GameObject selectedPlanetTerrain, float size, int resolution)
    {
        for (int i = 0; i < 6; i++)
        {
            Transform terrainSection = selectedPlanetTerrain.transform.GetChild(i);
            MeshFilter sectionMeshFilter;
            if (!terrainSection.TryGetComponent<MeshFilter>(out sectionMeshFilter))
            {
                sectionMeshFilter = terrainSection.AddComponent<MeshFilter>();
            }
            Vector3 direction = _directions[i];

            UpdateSphereSectionMesh(sectionMeshFilter, resolution, size, direction);
        }
    }

    private Mesh GenerateSphereSectionMesh(MeshRenderer renderer, MeshFilter filter, int resolution, float size, Vector3 direction)
    {
        renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        Mesh planeMesh = UpdateSphereSectionMesh(filter, resolution, size, direction);

        return planeMesh;
    }

    private Mesh UpdateSphereSectionMesh(MeshFilter filter, int resolution, float size, Vector3 direction)
    {
        Mesh planeMesh = new Mesh();

        int vertexPerRow = resolution + 1;
        int numberOfVertices = vertexPerRow * vertexPerRow;
        (Vector3[], Vector3[]) verticesAndNormales = GetSphereVertices(vertexPerRow, size, direction);
        Vector3[] vertices = verticesAndNormales.Item1;
        Vector3[] normals = verticesAndNormales.Item2;
        int[] triangles = GetTriangles(vertexPerRow);

        planeMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        planeMesh.vertices = vertices;
        planeMesh.triangles = triangles;
        planeMesh.normals = normals;
        //planeMesh.RecalculateNormals();

        filter.mesh = planeMesh;

        return planeMesh;
    }

    private(Vector3[], Vector3[]) GetSphereVertices(int vertexPerRow, float size, Vector3 direction)
    {
        Vector3[] vertices = new Vector3[vertexPerRow * vertexPerRow];
        Vector3[] normals = new Vector3[vertices.Length];

        Vector3 axisA = new Vector3(direction.y, direction.z, direction.x);
        Vector3 axisB = Vector3.Cross(direction, axisA);

        for (int y = 0; y < vertexPerRow; y++)
        {
            for (int x = 0; x < vertexPerRow; x++)
            {
                float percentX = (float)x / (vertexPerRow - 1);
                float percentY = (float)y / (vertexPerRow - 1);
                Vector3 pointOnUnitCube = direction + (percentX - 0.5f) * 2 * axisA + (percentY - 0.5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                vertices[x + y * vertexPerRow] = pointOnUnitSphere * size;
                normals[x + y * vertexPerRow] = pointOnUnitSphere;
            }
        }

        return (vertices, normals);
    }

    private int[] GetTriangles(int vertexPerRow)
    {
        int[] triangles = new int[(vertexPerRow - 1) * (vertexPerRow - 1) * 6];
        int triIndex = 0;

        for (int y = 0; y < vertexPerRow - 1; y++)
        {
            for (int x = 0; x < vertexPerRow - 1; x++)
            {
                int i = x + y * vertexPerRow;

                triangles[triIndex++] = i;
                triangles[triIndex++] = i + vertexPerRow + 1;
                triangles[triIndex++] = i + vertexPerRow;

                triangles[triIndex++] = i;
                triangles[triIndex++] = i + 1;
                triangles[triIndex++] = i + vertexPerRow + 1;
            }
        }

        return triangles;
    }
}