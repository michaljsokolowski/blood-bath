using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AbilityScript : MonoBehaviour
{
    [Header("Cone Settings")]
    [SerializeField] private float coneRange = 5f;
    [SerializeField] private float coneAngle = 30f;   // half-angle in degrees
    [SerializeField] private int coneSegments = 20;
    [SerializeField] private Color coneColor = new Color(1f, 0.5f, 0f, 0.4f); // orange, semi-transparent

    private bool drawCone = false;
    private MeshFilter coneMeshFilter;
    private MeshRenderer coneMeshRenderer;
    private GameObject coneObject;

    private void Start()
    {
        // Create a child object that will hold the cone mesh
        coneObject = new GameObject("AbilityCone");
        coneObject.transform.SetParent(transform);
        coneObject.transform.localPosition = Vector3.zero;
        coneObject.transform.localRotation = Quaternion.identity;

        coneMeshFilter = coneObject.AddComponent<MeshFilter>();
        coneMeshRenderer = coneObject.AddComponent<MeshRenderer>();

        // Use a transparent unlit material
        coneMeshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        coneMeshRenderer.material.color = coneColor;

        coneMeshFilter.mesh = CreateConeMesh();
        coneObject.SetActive(false);
    }

    private void Update()
    {
        drawCone = Input.GetKey(KeyCode.E);
        coneObject.SetActive(drawCone);
    }

    /// <summary>
    /// Builds a flat cone (fan) mesh that extends forward along the local Z axis.
    /// </summary>
    private Mesh CreateConeMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ConeMesh";

        // vertices: origin + one per arc segment
        int vertCount = coneSegments + 2;
        Vector3[] vertices = new Vector3[vertCount];
        Color[] colors = new Color[vertCount];

        vertices[0] = Vector3.zero; // apex at player position
        colors[0] = coneColor;

        float halfRad = coneAngle * Mathf.Deg2Rad;

        for (int i = 0; i <= coneSegments; i++)
        {
            float t = (float)i / coneSegments;                    // 0 → 1
            float angle = Mathf.Lerp(-halfRad, halfRad, t);       // sweep left to right
            Vector3 dir = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
            vertices[i + 1] = dir * coneRange;
            colors[i + 1] = coneColor;
        }

        // triangles: fan from vertex 0
        int[] triangles = new int[coneSegments * 3];
        for (int i = 0; i < coneSegments; i++)
        {
            triangles[i * 3]     = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        return mesh;
    }
}
