using System;
using System.Collections.Generic;
using UnityEngine;

public class Task3 : MonoBehaviour
{
    void Start()
    {
        transform.localPosition = new Vector3(8f, 4f, 0f);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        
        meshFilter.mesh = T3MeshUtilities.Key(
            divisions: 32,
            headRadius: 1.0f,
            holeRadius: 0.2f,
            holeOffset: new Vector2(-0.4f, 0),
            thickness: 0.2f,
            segmentLength: 0.2f,
            segmentWidth: 0.4f,
            keyCode: new float[] { 1.0f, 1.0f, 1.0f, 0.7f, 1.0f, 0.7f, 0.9f, 1.0f, 0.4f }
        );

    }
}

public static class T3MeshUtilities
{
    public static Mesh Key(
        int divisions,
        float headRadius,
        float holeRadius,
        Vector2 holeOffset,
        float thickness,
        float segmentLength,
        float segmentWidth,
        float[] keyCode)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Generate the key head (disc with hole) centered at origin
        GenerateKeyHead(vertices, triangles, divisions, headRadius, holeRadius, holeOffset, thickness);

        // Generate the key stem (teeth) starting from the right edge of the head
        GenerateKeyStem(vertices, triangles, headRadius, segmentLength, segmentWidth, thickness, keyCode);

        // Connect the head to the blade
        ConnectHeadToBlade(vertices, triangles, headRadius, segmentWidth, thickness);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private static void GenerateKeyHead(List<Vector3> vertices, List<int> triangles, 
        int divisions, float headRadius, float holeRadius, Vector2 holeOffset, float thickness)
    {
        float halfThickness = thickness * 0.5f;

        // Create vertices for outer and inner circles on both front and back faces
        // For smooth shading on curved surfaces, we need separate vertices per face
        
        // Front face vertices (z = halfThickness)
        List<Vector3> frontOuterVerts = new List<Vector3>();
        List<Vector3> frontInnerVerts = new List<Vector3>();
        
        for (int i = 0; i < divisions; i++)
        {
            float angle = (float)i / divisions * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            
            // Outer circle
            frontOuterVerts.Add(new Vector3(cos * headRadius, sin * headRadius, halfThickness));
            
            // Inner circle (hole)
            frontInnerVerts.Add(new Vector3(
                holeOffset.x + cos * holeRadius,
                holeOffset.y + sin * holeRadius,
                halfThickness
            ));
        }

        // Back face vertices (z = -halfThickness)
        List<Vector3> backOuterVerts = new List<Vector3>();
        List<Vector3> backInnerVerts = new List<Vector3>();
        
        for (int i = 0; i < divisions; i++)
        {
            float angle = (float)i / divisions * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            
            backOuterVerts.Add(new Vector3(cos * headRadius, sin * headRadius, -halfThickness));
            backInnerVerts.Add(new Vector3(
                holeOffset.x + cos * holeRadius,
                holeOffset.y + sin * holeRadius,
                -halfThickness
            ));
        }

        int baseIdx = vertices.Count;
        
        // Add front face vertices
        vertices.AddRange(frontOuterVerts);
        vertices.AddRange(frontInnerVerts);
        
        // Add back face vertices
        vertices.AddRange(backOuterVerts);
        vertices.AddRange(backInnerVerts);

        // Create front face triangles (flat shaded)
        for (int i = 0; i < divisions; i++)
        {
            int next = (i + 1) % divisions;
            
            int outerCurr = baseIdx + i;
            int outerNext = baseIdx + next;
            int innerCurr = baseIdx + divisions + i;
            int innerNext = baseIdx + divisions + next;
            
            // Front face (CCW from front view)
            triangles.Add(outerCurr);
            triangles.Add(innerCurr);
            triangles.Add(outerNext);
            
            triangles.Add(outerNext);
            triangles.Add(innerCurr);
            triangles.Add(innerNext);
        }

        // Create back face triangles (flat shaded)
        for (int i = 0; i < divisions; i++)
        {
            int next = (i + 1) % divisions;
            
            int outerCurr = baseIdx + divisions * 2 + i;
            int outerNext = baseIdx + divisions * 2 + next;
            int innerCurr = baseIdx + divisions * 3 + i;
            int innerNext = baseIdx + divisions * 3 + next;
            
            // Back face (CW from front view = CCW from back view)
            triangles.Add(outerCurr);
            triangles.Add(outerNext);
            triangles.Add(innerCurr);
            
            triangles.Add(outerNext);
            triangles.Add(innerNext);
            triangles.Add(innerCurr);
        }

        // Create smooth-shaded outer rim (connecting front and back outer edges)
        int outerRimStartIdx = vertices.Count;
        for (int i = 0; i < divisions; i++)
        {
            float angle = (float)i / divisions * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            Vector3 pos = new Vector3(cos * headRadius, sin * headRadius, 0);
            
            vertices.Add(pos + Vector3.forward * halfThickness);  // Front
            vertices.Add(pos - Vector3.forward * halfThickness);  // Back
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

        // Create smooth-shaded inner rim (hole edge)
        int innerRimStartIdx = vertices.Count;
        for (int i = 0; i < divisions; i++)
        {
            float angle = (float)i / divisions * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            Vector3 pos = new Vector3(
                holeOffset.x + cos * holeRadius,
                holeOffset.y + sin * holeRadius,
                0
            );
            
            vertices.Add(pos + Vector3.forward * halfThickness);  // Front
            vertices.Add(pos - Vector3.forward * halfThickness);  // Back
        }

        for (int i = 0; i < divisions; i++)
        {
            int next = (i + 1) % divisions;
            
            int currFront = innerRimStartIdx + i * 2;
            int currBack = innerRimStartIdx + i * 2 + 1;
            int nextFront = innerRimStartIdx + next * 2;
            int nextBack = innerRimStartIdx + next * 2 + 1;
            
            // Reversed winding for inner surface
            triangles.Add(currFront);
            triangles.Add(nextFront);
            triangles.Add(currBack);
            
            triangles.Add(nextFront);
            triangles.Add(nextBack);
            triangles.Add(currBack);
        }
    }

    private static void GenerateKeyStem(List<Vector3> vertices, List<int> triangles,
        float headRadius, float segmentLength, float segmentWidth, float thickness, float[] keyCode)
    {
        float halfThickness = thickness * 0.5f;
        int numSegments = keyCode.Length;

        // Starting position (at the right edge of the head, so blade doesn't overlap)
        float startX = headRadius;

        // Build the stem profile vertices
        // Top side: flat (constant width)
        // Bottom side: undulating (based on keyCode)
        List<Vector2> topProfile = new List<Vector2>();
        List<Vector2> bottomProfile = new List<Vector2>();

        // Start point - both sides start at full width
        float topY = segmentWidth * 0.5f;
        topProfile.Add(new Vector2(startX, topY));
        bottomProfile.Add(new Vector2(startX, -segmentWidth * 0.5f));

        for (int i = 0; i < numSegments; i++)
        {
            float x = startX + (i + 1) * segmentLength;
            
            // Top side stays flat at maximum width
            topProfile.Add(new Vector2(x, topY));
            
            // Bottom side undulates based on keyCode
            float bottomWidth = segmentWidth * keyCode[i];
            bottomProfile.Add(new Vector2(x, topY - bottomWidth));
        }

        // Create vertices for front face (flat shaded - need separate vertices)
        int frontFaceStart = vertices.Count;
        for (int i = 0; i < topProfile.Count; i++)
        {
            vertices.Add(new Vector3(topProfile[i].x, topProfile[i].y, halfThickness));
        }
        for (int i = 0; i < bottomProfile.Count; i++)
        {
            vertices.Add(new Vector3(bottomProfile[i].x, bottomProfile[i].y, halfThickness));
        }

        // Front face triangles - create a strip
        int topCount = topProfile.Count;
        for (int i = 0; i < topCount - 1; i++)
        {
            int topCurr = frontFaceStart + i;
            int topNext = frontFaceStart + i + 1;
            int botCurr = frontFaceStart + topCount + i;
            int botNext = frontFaceStart + topCount + i + 1;

            triangles.Add(topCurr);
            triangles.Add(botCurr);
            triangles.Add(topNext);

            triangles.Add(topNext);
            triangles.Add(botCurr);
            triangles.Add(botNext);
        }

        // Create vertices for back face (flat shaded)
        int backFaceStart = vertices.Count;
        for (int i = 0; i < topProfile.Count; i++)
        {
            vertices.Add(new Vector3(topProfile[i].x, topProfile[i].y, -halfThickness));
        }
        for (int i = 0; i < bottomProfile.Count; i++)
        {
            vertices.Add(new Vector3(bottomProfile[i].x, bottomProfile[i].y, -halfThickness));
        }

        // Back face triangles (reversed winding)
        for (int i = 0; i < topCount - 1; i++)
        {
            int topCurr = backFaceStart + i;
            int topNext = backFaceStart + i + 1;
            int botCurr = backFaceStart + topCount + i;
            int botNext = backFaceStart + topCount + i + 1;

            triangles.Add(topCurr);
            triangles.Add(topNext);
            triangles.Add(botCurr);

            triangles.Add(topNext);
            triangles.Add(botNext);
            triangles.Add(botCurr);
        }

        // Create smooth-shaded top edge
        int topEdgeStart = vertices.Count;
        for (int i = 0; i < topProfile.Count; i++)
        {
            Vector3 pos = new Vector3(topProfile[i].x, topProfile[i].y, 0);
            vertices.Add(pos + Vector3.forward * halfThickness);
            vertices.Add(pos - Vector3.forward * halfThickness);
        }

        for (int i = 0; i < topProfile.Count - 1; i++)
        {
            int currFront = topEdgeStart + i * 2;
            int currBack = topEdgeStart + i * 2 + 1;
            int nextFront = topEdgeStart + (i + 1) * 2;
            int nextBack = topEdgeStart + (i + 1) * 2 + 1;

            triangles.Add(currFront);
            triangles.Add(nextFront);
            triangles.Add(currBack);

            triangles.Add(nextFront);
            triangles.Add(nextBack);
            triangles.Add(currBack);
        }

        // Create smooth-shaded bottom edge
        int bottomEdgeStart = vertices.Count;
        for (int i = 0; i < bottomProfile.Count; i++)
        {
            Vector3 pos = new Vector3(bottomProfile[i].x, bottomProfile[i].y, 0);
            vertices.Add(pos + Vector3.forward * halfThickness);
            vertices.Add(pos - Vector3.forward * halfThickness);
        }

        for (int i = 0; i < bottomProfile.Count - 1; i++)
        {
            int currFront = bottomEdgeStart + i * 2;
            int currBack = bottomEdgeStart + i * 2 + 1;
            int nextFront = bottomEdgeStart + (i + 1) * 2;
            int nextBack = bottomEdgeStart + (i + 1) * 2 + 1;

            triangles.Add(currFront);
            triangles.Add(currBack);
            triangles.Add(nextFront);

            triangles.Add(nextFront);
            triangles.Add(currBack);
            triangles.Add(nextBack);
        }

        // Create flat-shaded left cap (connecting to head)
        int leftCapStart = vertices.Count;
        float leftTopY = topProfile[0].y;
        float leftBottomY = bottomProfile[0].y;
        
        vertices.Add(new Vector3(startX, leftTopY, halfThickness));
        vertices.Add(new Vector3(startX, leftBottomY, halfThickness));
        vertices.Add(new Vector3(startX, leftTopY, -halfThickness));
        vertices.Add(new Vector3(startX, leftBottomY, -halfThickness));

        triangles.Add(leftCapStart);
        triangles.Add(leftCapStart + 1);
        triangles.Add(leftCapStart + 2);

        triangles.Add(leftCapStart + 2);
        triangles.Add(leftCapStart + 1);
        triangles.Add(leftCapStart + 3);

        // Create flat-shaded right cap (end of key)
        int rightCapStart = vertices.Count;
        float endX = startX + numSegments * segmentLength;
        float endTopY = topProfile[topProfile.Count - 1].y;
        float endBottomY = bottomProfile[bottomProfile.Count - 1].y;
        
        vertices.Add(new Vector3(endX, endTopY, halfThickness));
        vertices.Add(new Vector3(endX, endBottomY, halfThickness));
        vertices.Add(new Vector3(endX, endTopY, -halfThickness));
        vertices.Add(new Vector3(endX, endBottomY, -halfThickness));

        triangles.Add(rightCapStart);
        triangles.Add(rightCapStart + 2);
        triangles.Add(rightCapStart + 1);

        triangles.Add(rightCapStart + 2);
        triangles.Add(rightCapStart + 3);
        triangles.Add(rightCapStart + 1);
    }

    // Claude AI helped to construct
    private static void ConnectHeadToBlade(List<Vector3> vertices, List<int> triangles,
        float headRadius, float segmentWidth, float thickness)
    {
        float halfThickness = thickness * 0.5f;
        float halfSegmentWidth = segmentWidth * 0.5f;

        //triangular faces to connect the circular head edge to the rectangular blade start
        // The blade starts at x = headRadius with width from -halfSegmentWidth to +halfSegmentWidth
        // create a smooth transition by adding triangles that fan out from the blade edges to the head circumference

        // Create vertices for the connection on both front and back faces
        // These will be flat-shaded, so we need separate vertices for each face

        // Front face connection (z = halfThickness)
        int frontConnectionStart = vertices.Count;

        // Blade top-right corner
        vertices.Add(new Vector3(headRadius, halfSegmentWidth, halfThickness));
        // Blade bottom-right corner
        vertices.Add(new Vector3(headRadius, -halfSegmentWidth, halfThickness));

        // Find points on the head circle that align with the blade edges
        // Top connection point on circle
        float topAngle = Mathf.Asin(halfSegmentWidth / headRadius);
        vertices.Add(new Vector3(headRadius * Mathf.Cos(topAngle), headRadius * Mathf.Sin(topAngle), halfThickness));

        // Bottom connection point on circle
        float bottomAngle = -topAngle;
        vertices.Add(new Vector3(headRadius * Mathf.Cos(bottomAngle), headRadius * Mathf.Sin(bottomAngle), halfThickness));

        // Front face triangles
        // Top triangle
        triangles.Add(frontConnectionStart);     // blade top corner
        triangles.Add(frontConnectionStart + 2); // circle top point
        triangles.Add(frontConnectionStart + 1); // blade bottom corner (forms triangle)

        // Bottom triangle  
        triangles.Add(frontConnectionStart + 1); // blade bottom corner
        triangles.Add(frontConnectionStart + 2); // circle top point
        triangles.Add(frontConnectionStart + 3); // circle bottom point

        // Back face connection (z = -halfThickness)
        int backConnectionStart = vertices.Count;

        // Blade corners
        vertices.Add(new Vector3(headRadius, halfSegmentWidth, -halfThickness));
        vertices.Add(new Vector3(headRadius, -halfSegmentWidth, -halfThickness));

        // Circle points
        vertices.Add(new Vector3(headRadius * Mathf.Cos(topAngle), headRadius * Mathf.Sin(topAngle), -halfThickness));
        vertices.Add(new Vector3(headRadius * Mathf.Cos(bottomAngle), headRadius * Mathf.Sin(bottomAngle), -halfThickness));

        // Back face triangles (reversed winding)
        triangles.Add(backConnectionStart);
        triangles.Add(backConnectionStart + 1);
        triangles.Add(backConnectionStart + 2);

        triangles.Add(backConnectionStart + 1);
        triangles.Add(backConnectionStart + 3);
        triangles.Add(backConnectionStart + 2);

        // Create smooth-shaded side faces for the transition
        // Top transition edge
        int topTransitionStart = vertices.Count;
        vertices.Add(new Vector3(headRadius, halfSegmentWidth, halfThickness));
        vertices.Add(new Vector3(headRadius, halfSegmentWidth, -halfThickness));
        vertices.Add(new Vector3(headRadius * Mathf.Cos(topAngle), headRadius * Mathf.Sin(topAngle), halfThickness));
        vertices.Add(new Vector3(headRadius * Mathf.Cos(topAngle), headRadius * Mathf.Sin(topAngle), -halfThickness));

        triangles.Add(topTransitionStart);
        triangles.Add(topTransitionStart + 2);
        triangles.Add(topTransitionStart + 1);

        triangles.Add(topTransitionStart + 1);
        triangles.Add(topTransitionStart + 2);
        triangles.Add(topTransitionStart + 3);

        // Bottom transition edge
        int bottomTransitionStart = vertices.Count;
        vertices.Add(new Vector3(headRadius, -halfSegmentWidth, halfThickness));
        vertices.Add(new Vector3(headRadius, -halfSegmentWidth, -halfThickness));
        vertices.Add(new Vector3(headRadius * Mathf.Cos(bottomAngle), headRadius * Mathf.Sin(bottomAngle), halfThickness));
        vertices.Add(new Vector3(headRadius * Mathf.Cos(bottomAngle), headRadius * Mathf.Sin(bottomAngle), -halfThickness));

        triangles.Add(bottomTransitionStart);
        triangles.Add(bottomTransitionStart + 1);
        triangles.Add(bottomTransitionStart + 2);

        triangles.Add(bottomTransitionStart + 1);
        triangles.Add(bottomTransitionStart + 3);
        triangles.Add(bottomTransitionStart + 2);
    }
}