using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class MeshUtilities
{
    // NOTE: size = extents * 2
	public static Mesh Cube(float extent)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4 * 6]
        {
            //front
		    new Vector3(-extent,-extent,-extent),
            new Vector3(extent, -extent, -extent),
            new Vector3(extent, extent, -extent),
            new Vector3(-extent, extent, -extent),
            
            // back
            new Vector3(-extent, -extent, extent),
            new Vector3(extent, -extent, extent),
            new Vector3(extent, extent, extent),
            new Vector3(-extent, extent, extent),
            
            // left
            new Vector3(-extent, -extent, -extent),
            new Vector3(-extent, extent, -extent),
            new Vector3(-extent, extent, extent),
            new Vector3(-extent, -extent, extent),
            
            // right
            new Vector3(extent, -extent, -extent),
            new Vector3(extent, extent, -extent),
            new Vector3(extent, extent, extent),
            new Vector3(extent, -extent, extent),
            
            // bottom
            new Vector3(-extent, -extent, -extent),
            new Vector3(-extent, -extent, extent),
            new Vector3(extent, -extent, extent),
            new Vector3(extent, -extent, -extent),
            
            // top
            new Vector3(-extent, extent, -extent),
            new Vector3(-extent, extent, extent),
            new Vector3(extent, extent, extent),
            new Vector3(extent, extent, -extent)
        };
        mesh.vertices = vertices;

        int[] tris = new int[6 * 2 * 3]
        {
            //front
            3, 2, 1,
            3, 1, 0,

            // back
            4,5,6,
            4,6,7,

            // left
            11,10,9,
            11,9,8,

            // right
            12,13,14,
            12,14,15,

            // bottom
            19,18,17,
            19,17,16,

            // top
            20,21,22,
            20,22,23
        };
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    public static Mesh Pyramid(float extent)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[16]
        {
            // Front face
            new Vector3(-extent, -extent, -extent),   // 0 bottom-left
            new Vector3(extent, -extent, -extent),    // 1 bottom-right
            new Vector3(0, extent, 0),                // 2 apex

            // Right face
            new Vector3(extent, -extent, -extent),    // 3 bottom-front
            new Vector3(extent, -extent, extent),     // 4 bottom-back
            new Vector3(0, extent, 0),                // 5 apex

            // Back face
            new Vector3(extent, -extent, extent),     // 6 bottom-right
            new Vector3(-extent, -extent, extent),    // 7 bottom-left
            new Vector3(0, extent, 0),                // 8 apex

            // Left face
            new Vector3(-extent, -extent, extent),    // 9 bottom-back
            new Vector3(-extent, -extent, -extent),   // 10 bottom-front
            new Vector3(0, extent, 0),                // 11 apex

            // Bottom face (square)
            new Vector3(-extent, -extent, -extent),   // 12 front-left
            new Vector3(extent, -extent, -extent),    // 13 front-right
            new Vector3(extent, -extent, extent),     // 14 back-right
            new Vector3(-extent, -extent, extent)     // 15 back-left
        };
        mesh.vertices = vertices;

        int[] tris = new int[6 * 3]
        {
            // Front face
            0, 1, 2,

            // Right face
            3, 4, 5,

            // Back face
            6, 7, 8,

            // Left face
            9, 10, 11,

            // Bottom face (two triangles)
            12, 14, 13,
            12, 15, 14
        };
        mesh.triangles = tris;

        mesh.RecalculateNormals();
        return mesh;
    }


    // divisions = number of divisions in the circle (there are no divisions in the height)
    public static Mesh Cylinder(int divisions, float radius, float height)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4 * divisions + 2]; // need top and bottom vertices
        Vector2[] uvs = new Vector2[vertices.Length];

        float dTheta = Mathf.PI * 2.0f / divisions;

        for (int i = 0; i < divisions; i++)
        {
            float theta = i * dTheta;
            float x = radius * Mathf.Cos(theta);
            float z = radius * Mathf.Sin(theta);
            // top vertex
            vertices[i] = new Vector4(x, height, z);
            // bottom vertex
            vertices[i + divisions] = new Vector4(x, -height, z);

            // cap vertices
            vertices[2 * divisions + i] = new Vector3(x, height, z); // top cap
            vertices[3 * divisions + i] = new Vector3(x, -height, z); // bottom cap

            // UVs for side
            float u = (float)i / divisions;
            uvs[i] = new Vector2(u, 1);               // top edge of texture
            uvs[i + divisions] = new Vector2(u, 0);   // bottom edge of texture

            // UVs for caps (polar coordinates mapping)
            float uCap = (x / radius + 1f) * 0.5f;
            float vCap = (z / radius + 1f) * 0.5f;
            uvs[2 * divisions + i] = new Vector2(uCap, vCap);
            uvs[3 * divisions + i] = new Vector2(uCap, vCap);
        }

        //center vertices
        vertices[divisions * 4] = new Vector3(0, height, 0); // top center
        vertices[divisions * 4 + 1] = new Vector3(0, -height, 0); // bottom center
        uvs[4 * divisions] = new Vector2(0.5f, 0.5f);
        uvs[4 * divisions + 1] = new Vector2(0.5f, 0.5f);


        int[] sideTris = new int[divisions * 6];                        // two tris for each side
        for (int i = 0; i < divisions; i++)
        {
            sideTris[i * 6] = i;                                        // current top vertex
            sideTris[i * 6 + 1] = (i + 1) % divisions;                  // next top vertex (wrapping)
            sideTris[i * 6 + 2] = divisions + (i + 1) % divisions;      // next bottom vertex (wrapping)

            sideTris[i * 6 + 3] = i;                                    // current top vertex
            sideTris[i * 6 + 4] = divisions + (i + 1) % divisions;      // next bottom vertex (wrapping)
            sideTris[i * 6 + 5] = divisions + i;                        // current bottom vertex
        }

        // top cap triangles
        int[] topCapTris = new int[divisions * 3];
        int topCenterIndex = divisions * 4; 
        for (int i = 0; i < divisions; i++)
        {
            topCapTris[i * 3] = topCenterIndex;                     
            topCapTris[i * 3 + 1] = divisions * 2 + (i + 1) % divisions;            
            topCapTris[i * 3 + 2] = divisions * 2 + i;                               
        }

        // bottom cap triangles
        int[] bottomCapTris = new int[divisions * 3];
        int bottomCenterIndex = divisions * 4 + 1; 
        for (int i = 0; i < divisions; i++)
        {
            bottomCapTris[i * 3] = bottomCenterIndex;                     
            bottomCapTris[i * 3 + 1] = divisions * 3 + i;            
            bottomCapTris[i * 3 + 2] = divisions * 3 + (i + 1) % divisions;                               
        }

        // Combine all triangles
        int[] tris = new int[sideTris.Length + topCapTris.Length + bottomCapTris.Length];
        sideTris.CopyTo(tris, 0);
        topCapTris.CopyTo(tris, sideTris.Length);
        bottomCapTris.CopyTo(tris, sideTris.Length + topCapTris.Length);

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
    
    public static Matrix4x4[] MakePathFromSpline(SplineContainer spline, int divisions)
    {
        var currentSpline = spline.Splines[0];
        Matrix4x4[] path = new Matrix4x4[divisions + 1];

        for (int i = 0; i <= divisions; i++)
        {
            var percent = i / (float) divisions;
            currentSpline.Evaluate(percent, out var pos, out var tangent, out var up);
            path[i] = Matrix4x4.TRS(pos, Quaternion.LookRotation(tangent, up), Vector3.one);
        }

        return path;
    }

    public static Vector3[] MakeProfileFromSpline(SplineContainer spline, int divisions)
    {
        var currentSpline = spline.Splines[0];
        Vector3[] profile = new Vector3[divisions];

        for (int i = 0; i < divisions; i++)
        {
            var percent = i / (float)divisions;
            currentSpline.Evaluate(percent, out var pos, out var tangent, out var up);
            profile[i] = new Vector3(pos.x, pos.z, 0f);
        }

        return profile;

    }


    public static Mesh Sweep(Vector3[] profile, Matrix4x4[] path, bool closed)
    {
        Mesh mesh = new Mesh();

        int numVerts = path.Length * profile.Length;
        int numTris;

        if (closed)
            numTris = 2 * path.Length * profile.Length;
        else
            numTris = 2 * (path.Length - 1) * profile.Length;


        Vector3[] vertices = new Vector3[numVerts];
        int[] tris = new int[numTris * 3];

        for (int i = 0; i < path.Length; i++)
        {
            for (int j = 0; j < profile.Length; j++)
            {
                Vector3 v = path[i].MultiplyPoint(profile[j]);
                vertices[i * profile.Length + j] = v;

                if (closed || i < path.Length - 1)
                {

                    tris[6 * (i * profile.Length + j)] = (j + i * profile.Length);
                    tris[6 * (i * profile.Length + j) + 1] = ((j + 1) % profile.Length + i * profile.Length);
                    tris[6 * (i * profile.Length + j) + 2] = ((j + 1) % profile.Length + ((i + 1) % path.Length) * profile.Length);
                    tris[6 * (i * profile.Length + j) + 3] = (j + i * profile.Length);
                    tris[6 * (i * profile.Length + j) + 4] = ((j + 1) % profile.Length + ((i + 1) % path.Length) * profile.Length);
                    tris[6 * (i * profile.Length + j) + 5] = (j + ((i + 1) % path.Length) * profile.Length);
                }
            }
        }

        mesh.vertices = vertices;

        mesh.triangles = tris;

        mesh.RecalculateNormals();

        return mesh;
    }

	public static Matrix4x4[] MakeCirclePath(float radius, int divisions)
	{
		Matrix4x4[] path = new Matrix4x4[divisions];
		for (int i = 0; i < divisions; i++)
		{
			float angle = (360.0f * i) / divisions;
			path[i] = Matrix4x4.Rotate(Quaternion.Euler(0, -angle, 0))* Matrix4x4.Translate(new Vector3(radius,0,0));
		}
		return path;
	}

	public static Vector3[] MakeCircleProfile(float radius, int divisions)
	{
		Vector3[] profile = new Vector3[divisions];
		for (int i = 0; i < divisions; i++)
		{
			float angle = (2.0f * Mathf.PI * i) / divisions;
			profile[i] = new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle),0);
		}
		return profile;
	}

}