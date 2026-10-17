using System;
using System.Collections.Generic;
using UnityEngine;

public class Task4 : MonoBehaviour
{
    void Start()
    {
        transform.localPosition = new Vector3(12f, 4f, 0f);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        Mesh keyMesh = T4MeshUtilities.YeOldeKey(
            divisions: 32,
            headRadius: 1.0f,
            holesRadius: 0.6f,
            holesOffset: new Vector2(0.4f, 0.3f),
            thickness: 0.3f,
            stemLength: 3.0f,
            stemRadius: 0.15f,
            keyCode: new float[] { 1.0f, 0.4f, 1.0f, 1.0f, 0.8f, 1.0f, 1.0f, 0.4f, 1.0f }
        );

        // Add the mesh to this GameObject
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        meshFilter.mesh = keyMesh;
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.color = new Color(0.7f, 0.6f, 0.4f); // Gold-ish color
    }
}

public static class T4MeshUtilities
{
    private const float HoleOverlapCenterOffsetRatio = 0.75f;

    public static Mesh YeOldeKey(
        int divisions,
        float headRadius,
        float holesRadius,
        Vector2 holesOffset,
        float thickness,
        float stemLength,
        float stemRadius,
        float[] keyCode)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float circleCenterOffset = Mathf.Min(
            headRadius * 0.6f,
            holesRadius * HoleOverlapCenterOffsetRatio);
        float stemStartX = Mathf.Sqrt(headRadius * headRadius - circleCenterOffset * circleCenterOffset) - 0.08f;

        // Generate the key head
        GenerateOldeKeyHead(vertices, triangles, divisions, headRadius, holesRadius, holesOffset, thickness);

        // Generate the cylindrical stem
        GenerateCylindricalStem(vertices, triangles, divisions, stemStartX, stemLength, stemRadius, thickness);

        // Generate the code region (teeth at the end)
        GenerateCodeRegion(vertices, triangles, divisions, stemStartX + stemLength, stemRadius, keyCode);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private static void GenerateOldeKeyHead(List<Vector3> vertices, List<int> triangles,
        int divisions, float headRadius, float holesRadius, Vector2 holesOffset, float thickness)
    {
        float centerOffset = Mathf.Min(
            headRadius * 0.6f,
            holesRadius * HoleOverlapCenterOffsetRatio);
        Vector2[] circleCenters =
        {
            new Vector2(0f, -centerOffset),
            new Vector2(0f, centerOffset)
        };

        for (int i = 0; i < circleCenters.Length; i++)
        {
            GenerateHollowCircle(vertices, triangles, divisions, circleCenters[i],
                headRadius, holesRadius, thickness);
        }
    }

    private static void GenerateHollowCircle(List<Vector3> vertices, List<int> triangles,
        int divisions, Vector2 center, float outerRadius, float innerRadius, float thickness)
    {
        float halfThickness = thickness * 0.5f;
        float bevel = Mathf.Min(
            thickness * 0.25f,
            (outerRadius - innerRadius) * 0.25f,
            innerRadius * 0.25f);
        List<Vector2> outerProfile = CreateRoundedRimProfile(outerRadius, halfThickness, bevel, false);
        List<Vector2> innerProfile = CreateRoundedRimProfile(innerRadius, halfThickness, bevel, true);
        List<int> outerRows = AddCircularProfile(vertices, divisions, center, outerProfile);
        List<int> innerRows = AddCircularProfile(vertices, divisions, center, innerProfile);

        TriangulateAnnulus(triangles, outerRows[0], innerRows[0], divisions, true);
        TriangulateAnnulus(
            triangles,
            outerRows[outerRows.Count - 1],
            innerRows[innerRows.Count - 1],
            divisions,
            false);

        ConnectCircularProfile(triangles, outerRows, divisions, false);
        ConnectCircularProfile(triangles, innerRows, divisions, true);
    }

    private static List<Vector2> CreateRoundedRimProfile(
        float radius, float halfThickness, float bevel, bool inner)
    {
        const int bevelSegments = 4;
        const int sideSegments = 2;
        List<Vector2> profile = new List<Vector2>();
        float bevelRadiusSign = inner ? 1f : -1f;

        for (int i = 0; i <= bevelSegments; i++)
        {
            float angle = (float)i / bevelSegments * Mathf.PI * 0.5f;
            float profileRadius = radius + bevelRadiusSign * bevel * Mathf.Cos(angle);
            float z = halfThickness - bevel + bevel * Mathf.Cos(angle);
            profile.Add(new Vector2(profileRadius, z));
        }

        float sideZ = halfThickness - bevel;
        for (int i = 1; i <= sideSegments; i++)
        {
            float t = (float)i / (sideSegments + 1);
            profile.Add(new Vector2(radius, Mathf.Lerp(sideZ, -sideZ, t)));
        }
        profile.Add(new Vector2(radius, -sideZ));

        for (int i = bevelSegments - 1; i >= 0; i--)
        {
            float angle = (float)i / bevelSegments * Mathf.PI * 0.5f;
            float profileRadius = radius + bevelRadiusSign * bevel * Mathf.Cos(angle);
            float z = -halfThickness + bevel - bevel * Mathf.Cos(angle);
            profile.Add(new Vector2(profileRadius, z));
        }

        return profile;
    }

    private static List<int> AddCircularProfile(
        List<Vector3> vertices, int divisions, Vector2 center, List<Vector2> profile)
    {
        List<int> rowStarts = new List<int>(profile.Count);
        foreach (Vector2 crossSection in profile)
        {
            rowStarts.Add(vertices.Count);
            for (int i = 0; i < divisions; i++)
            {
                float angle = (float)i / divisions * Mathf.PI * 2f;
                vertices.Add(new Vector3(
                    center.x + Mathf.Cos(angle) * crossSection.x,
                    center.y + Mathf.Sin(angle) * crossSection.x,
                    crossSection.y));
            }
        }

        return rowStarts;
    }

    private static void TriangulateAnnulus(
        List<int> triangles, int outerBaseIdx, int innerBaseIdx, int divisions, bool front)
    {
        for (int i = 0; i < divisions; i++)
        {
            int next = (i + 1) % divisions;
            int outerCurr = outerBaseIdx + i;
            int outerNext = outerBaseIdx + next;
            int innerCurr = innerBaseIdx + i;
            int innerNext = innerBaseIdx + next;

            if (front)
            {
                triangles.Add(outerCurr);
                triangles.Add(outerNext);
                triangles.Add(innerCurr);
                triangles.Add(outerNext);
                triangles.Add(innerNext);
                triangles.Add(innerCurr);
            }
            else
            {
                triangles.Add(outerCurr);
                triangles.Add(innerCurr);
                triangles.Add(outerNext);
                triangles.Add(outerNext);
                triangles.Add(innerCurr);
                triangles.Add(innerNext);
            }
        }
    }

    private static void ConnectCircularProfile(
        List<int> triangles, List<int> rowStarts, int divisions, bool inner)
    {
        for (int row = 0; row < rowStarts.Count - 1; row++)
        {
            int currentRow = rowStarts[row];
            int nextRow = rowStarts[row + 1];
            for (int i = 0; i < divisions; i++)
            {
                int next = (i + 1) % divisions;
                int current = currentRow + i;
                int currentNext = currentRow + next;
                int nextProfile = nextRow + i;
                int nextProfileNext = nextRow + next;

                if (inner)
                {
                    triangles.Add(current);
                    triangles.Add(currentNext);
                    triangles.Add(nextProfile);
                    triangles.Add(currentNext);
                    triangles.Add(nextProfileNext);
                    triangles.Add(nextProfile);
                }
                else
                {
                    triangles.Add(current);
                    triangles.Add(nextProfile);
                    triangles.Add(currentNext);
                    triangles.Add(currentNext);
                    triangles.Add(nextProfile);
                    triangles.Add(nextProfileNext);
                }
            }
        }
    }

    private static void GenerateCylindricalStem(List<Vector3> vertices, List<int> triangles,
        int divisions, float startX, float stemLength, float stemRadius, float thickness)
    {
        float endX = startX + stemLength + 0.1f;
        float halfThickness = thickness * 0.5f;

        // Create connection from head to stem (transition from head thickness to stem)
        // At the head edge, connect to the cylindrical stem
        int connectionStartIdx = vertices.Count;

        // Create a ring at the head edge that matches the stem radius
        for (int i = 0; i < divisions; i++)
        {
            float angle = (float)i / divisions * Mathf.PI * 2f;
            float y = Mathf.Cos(angle) * stemRadius;
            float z = Mathf.Sin(angle) * stemRadius;

            vertices.Add(new Vector3(startX, y, z));
        }

        // Create the main cylindrical stem
        int stemStartIdx = vertices.Count;
        for (int i = 0; i < divisions; i++)
        {
            float angle = (float)i / divisions * Mathf.PI * 2f;
            float y = Mathf.Cos(angle) * stemRadius;
            float z = Mathf.Sin(angle) * stemRadius;

            vertices.Add(new Vector3(endX, y, z));
        }

        // Create triangles for the cylinder surface (smooth shaded)
        for (int i = 0; i < divisions; i++)
        {
            int next = (i + 1) % divisions;

            int currStart = connectionStartIdx + i;
            int nextStart = connectionStartIdx + next;
            int currEnd = stemStartIdx + i;
            int nextEnd = stemStartIdx + next;

            triangles.Add(currStart);
            triangles.Add(nextStart);
            triangles.Add(currEnd);

            triangles.Add(nextStart);
            triangles.Add(nextEnd);
            triangles.Add(currEnd);
        }

        // Create connection cap between head and stem (flat shaded)
        CreateHeadToStemConnection(vertices, triangles, divisions, startX, stemRadius, halfThickness);
    }

    private static void CreateHeadToStemConnection(List<Vector3> vertices, List<int> triangles,
        int divisions, float x, float stemRadius, float halfThickness)
    {
        // Create a flat cap that connects the head's rectangular edge to the stem's circular edge
        int capCenterIdx = vertices.Count;
        
        // Center points on the head edge (top and bottom of rectangular edge)
        vertices.Add(new Vector3(x, 0, halfThickness));
        vertices.Add(new Vector3(x, 0, -halfThickness));

        // Create triangles fanning out from center to circle
        int circleStartIdx = vertices.Count;
        for (int i = 0; i < divisions; i++)
        {
            float angle = (float)i / divisions * Mathf.PI * 2f;
            float y = Mathf.Cos(angle) * stemRadius;
            float z = Mathf.Sin(angle) * stemRadius;

            vertices.Add(new Vector3(x, y, z));
        }

        // Fan triangles
        for (int i = 0; i < divisions; i++)
        {
            int next = (i + 1) % divisions;
            int curr = circleStartIdx + i;
            int nextVert = circleStartIdx + next;

            // Determine which center point to use based on z position
            int center = (vertices[curr].z >= 0) ? capCenterIdx : capCenterIdx + 1;

            triangles.Add(center);
            triangles.Add(curr);
            triangles.Add(nextVert);
        }
    }

    private static void GenerateCodeRegion(List<Vector3> vertices, List<int> triangles,
        int divisions, float codeStartX, float stemRadius, float[] keyCode)
    {
        // The code region is at the end of the stem
        
        float segmentLength = stemRadius * 0.5f; // Length of each code segment
        int numSegments = keyCode.Length;

        // Create the base of the code region (end cap of stem)
        int endCapStartIdx = vertices.Count;
        Vector3 endCapCenter = new Vector3(codeStartX, 0, 0);
        vertices.Add(endCapCenter);

        for (int i = 0; i < divisions; i++)
        {
            float angle = (float)i / divisions * Mathf.PI * 2f;
            float y = Mathf.Cos(angle) * stemRadius;
            float z = Mathf.Sin(angle) * stemRadius;

            vertices.Add(new Vector3(codeStartX, y, z));
        }

        // Create end cap triangles
        for (int i = 0; i < divisions; i++)
        {
            int next = (i + 1) % divisions;
            triangles.Add(endCapStartIdx);
            triangles.Add(endCapStartIdx + 1 + next);
            triangles.Add(endCapStartIdx + 1 + i);
        }

        // Place the teeth on the final code-length portion of the stem.
        float toothWidth = stemRadius * 0.4f;
        float toothThickness = stemRadius * 1.8f;
        
        for (int seg = 0; seg < numSegments; seg++)
        {
            float toothLength = stemRadius * keyCode[seg] * 2.5f;
            float x = codeStartX - (numSegments - seg - 0.5f) * segmentLength;
            
            // Create a rectangular tooth extending downward (-y direction)
            CreateTooth(vertices, triangles, x, toothLength, toothWidth, toothThickness);
        }
    }

    private static void CreateTooth(List<Vector3> vertices, List<int> triangles,
        float x, float length, float width, float thickness)
    {
        // Create a rectangular box tooth
        float halfWidth = width * 0.5f;
        float halfThickness = thickness * 0.5f;
        float y = 0; // Start at stem center
        float yEnd = y - length;

        int baseIdx = vertices.Count;

        // Top face vertices (at y = 0)
        vertices.Add(new Vector3(x - halfWidth, y, -halfThickness));
        vertices.Add(new Vector3(x + halfWidth, y, -halfThickness));
        vertices.Add(new Vector3(x + halfWidth, y, halfThickness));
        vertices.Add(new Vector3(x - halfWidth, y, halfThickness));

        // Bottom face vertices (at y = yEnd)
        vertices.Add(new Vector3(x - halfWidth, yEnd, -halfThickness));
        vertices.Add(new Vector3(x + halfWidth, yEnd, -halfThickness));
        vertices.Add(new Vector3(x + halfWidth, yEnd, halfThickness));
        vertices.Add(new Vector3(x - halfWidth, yEnd, halfThickness));

        // Top face
        triangles.Add(baseIdx);
        triangles.Add(baseIdx + 2);
        triangles.Add(baseIdx + 1);
        triangles.Add(baseIdx);
        triangles.Add(baseIdx + 3);
        triangles.Add(baseIdx + 2);

        // Bottom face
        triangles.Add(baseIdx + 4);
        triangles.Add(baseIdx + 5);
        triangles.Add(baseIdx + 6);
        triangles.Add(baseIdx + 4);
        triangles.Add(baseIdx + 6);
        triangles.Add(baseIdx + 7);

        // Side faces
        // Front face (-z)
        triangles.Add(baseIdx);
        triangles.Add(baseIdx + 1);
        triangles.Add(baseIdx + 4);
        triangles.Add(baseIdx + 1);
        triangles.Add(baseIdx + 5);
        triangles.Add(baseIdx + 4);

        // Back face (+z)
        triangles.Add(baseIdx + 3);
        triangles.Add(baseIdx + 7);
        triangles.Add(baseIdx + 2);
        triangles.Add(baseIdx + 2);
        triangles.Add(baseIdx + 7);
        triangles.Add(baseIdx + 6);

        // Left face (-x)
        triangles.Add(baseIdx);
        triangles.Add(baseIdx + 4);
        triangles.Add(baseIdx + 3);
        triangles.Add(baseIdx + 3);
        triangles.Add(baseIdx + 4);
        triangles.Add(baseIdx + 7);

        // Right face (+x)
        triangles.Add(baseIdx + 1);
        triangles.Add(baseIdx + 2);
        triangles.Add(baseIdx + 5);
        triangles.Add(baseIdx + 2);
        triangles.Add(baseIdx + 6);
        triangles.Add(baseIdx + 5);
    }
}