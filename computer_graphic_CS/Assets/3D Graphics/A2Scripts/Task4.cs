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
            holesRadius: 0.2f,
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

        // Generate the key head 
        GenerateOldeKeyHead(vertices, triangles, divisions, headRadius, holesRadius, holesOffset, thickness);

        // Generate the cylindrical stem
        GenerateCylindricalStem(vertices, triangles, divisions, headRadius, stemLength, stemRadius, thickness);

        // Generate the code region (teeth at the end)
        GenerateCodeRegion(vertices, triangles, divisions, stemLength, stemRadius, keyCode);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    // holes issue (claude helped, but failed)
    private static void GenerateOldeKeyHead(List<Vector3> vertices, List<int> triangles,
        int divisions, float headRadius, float holesRadius, Vector2 holesOffset, float thickness)
    {
        float halfThickness = thickness * 0.5f;
        List<Vector3> frontOuterVerts = new List<Vector3>();
        List<Vector3> frontHole1Verts = new List<Vector3>();
        List<Vector3> frontHole2Verts = new List<Vector3>();

        for (int i = 0; i < divisions; i++)
        {
            float angle = (float)i / divisions * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            // Outer circle
            frontOuterVerts.Add(new Vector3(cos * headRadius, sin * headRadius, halfThickness));

            // First hole (positive offset)
            frontHole1Verts.Add(new Vector3(
                holesOffset.x + cos * holesRadius,
                holesOffset.y + sin * holesRadius,
                halfThickness
            ));

            // Second hole (mirrored - negative y offset)
            frontHole2Verts.Add(new Vector3(
                holesOffset.x + cos * holesRadius,
                -holesOffset.y + sin * holesRadius,
                halfThickness
            ));
        }

        // Back face (z = -halfThickness)
        List<Vector3> backOuterVerts = new List<Vector3>();
        List<Vector3> backHole1Verts = new List<Vector3>();
        List<Vector3> backHole2Verts = new List<Vector3>();

        for (int i = 0; i < divisions; i++)
        {
            float angle = (float)i / divisions * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            backOuterVerts.Add(new Vector3(cos * headRadius, sin * headRadius, -halfThickness));

            backHole1Verts.Add(new Vector3(
                holesOffset.x + cos * holesRadius,
                holesOffset.y + sin * holesRadius,
                -halfThickness
            ));

            backHole2Verts.Add(new Vector3(
                holesOffset.x + cos * holesRadius,
                -holesOffset.y + sin * holesRadius,
                -halfThickness
            ));
        }

        int baseIdx = vertices.Count;

        // Add all face vertices
        vertices.AddRange(frontOuterVerts);
        vertices.AddRange(frontHole1Verts);
        vertices.AddRange(frontHole2Verts);
        vertices.AddRange(backOuterVerts);
        vertices.AddRange(backHole1Verts);
        vertices.AddRange(backHole2Verts);

        // Create front face triangulation (annulus with two holes)
        TriangulateAnnulusWithTwoHoles(triangles, baseIdx, divisions, true);

        // Create back face triangulation
        int backBaseIdx = baseIdx + divisions * 3;
        TriangulateAnnulusWithTwoHoles(triangles, backBaseIdx, divisions, false);

        // Create smooth-shaded outer rim
        int outerRimStartIdx = vertices.Count;
        for (int i = 0; i < divisions; i++)
        {
            float angle = (float)i / divisions * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            Vector3 pos = new Vector3(cos * headRadius, sin * headRadius, 0);

            vertices.Add(pos + Vector3.forward * halfThickness);
            vertices.Add(pos - Vector3.forward * halfThickness);
        }

        for (int i = 0; i < divisions; i++)
        {
            int next = (i + 1) % divisions;

            int currFront = outerRimStartIdx + i * 2;
            int currBack = outerRimStartIdx + i * 2 + 1;
            int nextFront = outerRimStartIdx + next * 2;
            int nextBack = outerRimStartIdx + next * 2 + 1;

            triangles.Add(currFront);
            triangles.Add(currBack);
            triangles.Add(nextFront);

            triangles.Add(nextFront);
            triangles.Add(currBack);
            triangles.Add(nextBack);
        }

        // Create smooth-shaded rims for both holes
        CreateHoleRim(vertices, triangles, divisions, holesOffset, holesRadius, halfThickness);
        CreateHoleRim(vertices, triangles, divisions, new Vector2(holesOffset.x, -holesOffset.y), holesRadius, halfThickness);
    }


    // claude debugging (but failed)
    private static void TriangulateAnnulusWithTwoHoles(List<int> triangles, int baseIdx, int divisions, bool front)
    {
        // Triangulate the annulus with two holes

        for (int i = 0; i < divisions; i++)
        {
            int next = (i + 1) % divisions;

            int outerCurr = baseIdx + i;
            int outerNext = baseIdx + next;
            int hole1Curr = baseIdx + divisions + i;
            int hole1Next = baseIdx + divisions + next;
            int hole2Curr = baseIdx + divisions * 2 + i;
            int hole2Next = baseIdx + divisions * 2 + next;

            // Determine which segments connect to which hole based on angle
            float angle = (float)i / divisions * Mathf.PI * 2f;
            float nextAngle = (float)next / divisions * Mathf.PI * 2f;

            // Upper half connects to hole1, lower half connects to hole2
            bool upperHalf = Mathf.Sin(angle) >= 0;
            bool nextUpperHalf = Mathf.Sin(nextAngle) >= 0;

            if (upperHalf && nextUpperHalf)
            {
                // Connect to first hole
                if (front)
                {
                    triangles.Add(outerCurr);
                    triangles.Add(hole1Curr);
                    triangles.Add(outerNext);

                    triangles.Add(outerNext);
                    triangles.Add(hole1Curr);
                    triangles.Add(hole1Next);
                }
                else
                {
                    triangles.Add(outerCurr);
                    triangles.Add(outerNext);
                    triangles.Add(hole1Curr);

                    triangles.Add(outerNext);
                    triangles.Add(hole1Next);
                    triangles.Add(hole1Curr);
                }
            }
            else if (!upperHalf && !nextUpperHalf)
            {
                // Connect to second hole
                if (front)
                {
                    triangles.Add(outerCurr);
                    triangles.Add(hole2Curr);
                    triangles.Add(outerNext);

                    triangles.Add(outerNext);
                    triangles.Add(hole2Curr);
                    triangles.Add(hole2Next);
                }
                else
                {
                    triangles.Add(outerCurr);
                    triangles.Add(outerNext);
                    triangles.Add(hole2Curr);

                    triangles.Add(outerNext);
                    triangles.Add(hole2Next);
                    triangles.Add(hole2Curr);
                }
            }
            else
            {
                // Transition region - connect outer to outer
                // This creates a bridge between the two hole regions
                if (front)
                {
                    triangles.Add(outerCurr);
                    triangles.Add(outerNext);
                    triangles.Add(hole1Curr);
                }
                else
                {
                    triangles.Add(outerCurr);
                    triangles.Add(hole1Curr);
                    triangles.Add(outerNext);
                }
            }
        }
    }

    private static void CreateHoleRim(List<Vector3> vertices, List<int> triangles,
        int divisions, Vector2 center, float radius, float halfThickness)
    {
        int rimStartIdx = vertices.Count;

        for (int i = 0; i < divisions; i++)
        {
            float angle = (float)i / divisions * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            Vector3 pos = new Vector3(center.x + cos * radius, center.y + sin * radius, 0);

            vertices.Add(pos + Vector3.forward * halfThickness);
            vertices.Add(pos - Vector3.forward * halfThickness);
        }

        for (int i = 0; i < divisions; i++)
        {
            int next = (i + 1) % divisions;

            int currFront = rimStartIdx + i * 2;
            int currBack = rimStartIdx + i * 2 + 1;
            int nextFront = rimStartIdx + next * 2;
            int nextBack = rimStartIdx + next * 2 + 1;

            // Reversed winding for inner surface
            triangles.Add(currFront);
            triangles.Add(nextFront);
            triangles.Add(currBack);

            triangles.Add(nextFront);
            triangles.Add(nextBack);
            triangles.Add(currBack);
        }
    }

    private static void GenerateCylindricalStem(List<Vector3> vertices, List<int> triangles,
        int divisions, float headRadius, float stemLength, float stemRadius, float thickness)
    {
        // The stem starts at the right edge of the head and extends along the x-axis
        float startX = headRadius;
        float endX = startX + stemLength;
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
            triangles.Add(currEnd);
            triangles.Add(nextStart);

            triangles.Add(nextStart);
            triangles.Add(currEnd);
            triangles.Add(nextEnd);
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
        int divisions, float stemLength, float stemRadius, float[] keyCode)
    {
        // The code region is at the end of the stem
        
        float codeStartX = stemLength;
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

        // Create simplified code teeth as rectangular protrusions
        float toothWidth = stemRadius * 0.4f;
        float toothThickness = stemRadius * 1.8f;
        
        for (int seg = 0; seg < numSegments; seg++)
        {
            float toothLength = stemRadius * keyCode[seg] * 2.5f;
            float x = codeStartX + (seg + 0.5f) * segmentLength;
            
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
        triangles.Add(baseIdx + 1);
        triangles.Add(baseIdx + 2);
        triangles.Add(baseIdx);
        triangles.Add(baseIdx + 2);
        triangles.Add(baseIdx + 3);

        // Bottom face
        triangles.Add(baseIdx + 4);
        triangles.Add(baseIdx + 6);
        triangles.Add(baseIdx + 5);
        triangles.Add(baseIdx + 4);
        triangles.Add(baseIdx + 7);
        triangles.Add(baseIdx + 6);

        // Side faces
        // Front face (-z)
        triangles.Add(baseIdx);
        triangles.Add(baseIdx + 4);
        triangles.Add(baseIdx + 1);
        triangles.Add(baseIdx + 1);
        triangles.Add(baseIdx + 4);
        triangles.Add(baseIdx + 5);

        // Back face (+z)
        triangles.Add(baseIdx + 3);
        triangles.Add(baseIdx + 2);
        triangles.Add(baseIdx + 7);
        triangles.Add(baseIdx + 2);
        triangles.Add(baseIdx + 6);
        triangles.Add(baseIdx + 7);

        // Left face (-x)
        triangles.Add(baseIdx);
        triangles.Add(baseIdx + 3);
        triangles.Add(baseIdx + 4);
        triangles.Add(baseIdx + 3);
        triangles.Add(baseIdx + 7);
        triangles.Add(baseIdx + 4);

        // Right face (+x)
        triangles.Add(baseIdx + 1);
        triangles.Add(baseIdx + 5);
        triangles.Add(baseIdx + 2);
        triangles.Add(baseIdx + 2);
        triangles.Add(baseIdx + 5);
        triangles.Add(baseIdx + 6);
    }
}