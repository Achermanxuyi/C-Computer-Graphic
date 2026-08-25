using System.Collections.Generic;
using UnityEngine;

public class Task1 : MonoBehaviour
{
    void Start()
    {
        transform.localPosition = new Vector3(0, 0.1f, 0);

        // Create sub-objects
        CreateHatCrown();
        CreateHatBand();
        CreateHatBrim();
    }

    void CreateHatCrown()
    {
        List<Vector3> profile = new List<Vector3>();
        int profileSegments = 32;

        profile.Add(new Vector3(0f, 0.94f, 0f));

        for (int i = 0; i < profileSegments; i++)
        {
            float t = i / (float)(profileSegments - 1);
            float x = Mathf.Lerp(0.85f, 0.95f, t) + 0.01f * Mathf.Cos(t * Mathf.PI * 3f);
            float y = Mathf.Sin((1 - t) * Mathf.PI * 0.5f) * 1.15f;
            profile.Add(new Vector3(x, y, 0f));
        }

        int pathSegments = 64;
        Matrix4x4[] path = new Matrix4x4[pathSegments];
        for (int i = 0; i < pathSegments; i++)
        {
            float angle = 2f * Mathf.PI * i / pathSegments;
            Quaternion rot = Quaternion.Euler(0f, Mathf.Rad2Deg * angle, 0f);

            float transY = 0.015f * Mathf.Sin(angle * 4f);
            float scale = 1f + 0.03f * Mathf.Sin(angle * 5f);

            path[i] = Matrix4x4.TRS(new Vector3(0, transY, 0), rot, Vector3.one * scale);
        }

        Mesh crownMesh = MeshUtilities.Sweep(profile.ToArray(), path, true);

        GameObject crown = new GameObject("Hat_Crown");
        crown.transform.SetParent(this.transform, false);
        crown.transform.localPosition = new Vector3(0, 0.02f, 0);
        crown.AddComponent<MeshFilter>().mesh = crownMesh;
        crown.AddComponent<MeshRenderer>().material = CreateMaterial(new Color(0.06f, 0.06f, 0.06f));
    }

    void CreateHatBand()
    {
        List<Vector3> profile = new List<Vector3>();

        float outerR = 0.98f;
        float innerR = 0.95f;
        float bottom = 0.06f;
        float top = 0.25f;

        profile.Add(new Vector3(outerR, top, 0));
        profile.Add(new Vector3(outerR, bottom, 0));
        profile.Add(new Vector3(innerR, bottom, 0));
        profile.Add(new Vector3(innerR, top, 0));


        int pathSegments = 64;
        Matrix4x4[] path = new Matrix4x4[pathSegments];
        for (int i = 0; i < pathSegments; i++)
        {
            float angle = 2f * Mathf.PI * i / pathSegments;
            Quaternion rot = Quaternion.Euler(0f, Mathf.Rad2Deg * angle, 0f);

            float transY = 0.015f * Mathf.Sin(angle * 4f);
            float scale = 1f + 0.03f * Mathf.Sin(angle * 5f);

            path[i] = Matrix4x4.TRS(new Vector3(0, transY, 0), rot, Vector3.one * scale);
        }

        Mesh bandMesh = MeshUtilities.Sweep(profile.ToArray(), path, true);

        GameObject band = new GameObject("Hat_Band");
        band.transform.SetParent(this.transform, false);
        band.transform.localPosition = new Vector3(0, 0.01f, 0);
        band.AddComponent<MeshFilter>().mesh = bandMesh;
        band.AddComponent<MeshRenderer>().material = CreateMaterial(Color.white);
    }

    void CreateHatBrim()
    {
        List<Vector3> profile = new List<Vector3>
        {
            new Vector3(1.6f, -0.02f, 0f),
            new Vector3(1.6f,  0.00f, 0f),
            new Vector3(1.55f, 0.05f, 0f),
            new Vector3(0.95f, 0.04f, 0f),
            new Vector3(0.95f, 0.00f, 0f),
            new Vector3(0.95f,-0.02f, 0f)

        };

        int pathSegments = 64;

        Matrix4x4[] path = new Matrix4x4[pathSegments];
        for (int i = 0; i < pathSegments; i++)
        {
            float angle = 2f * Mathf.PI * i / pathSegments;
            Quaternion rot = Quaternion.Euler(0f, Mathf.Rad2Deg * angle, 0f);

            float transY = 0.015f * Mathf.Sin(angle * 4f);
            float scale = 1f + 0.03f * Mathf.Sin(angle * 5f);

            path[i] = Matrix4x4.TRS(new Vector3(0, transY, 0), rot, Vector3.one * scale);
        }

        Mesh brimMesh = MeshUtilities.Sweep(profile.ToArray(), path, true);

        GameObject brim = new GameObject("Hat_Brim");
        brim.transform.SetParent(this.transform, false);
        brim.transform.localPosition = Vector3.zero;
        brim.AddComponent<MeshFilter>().mesh = brimMesh;
        brim.AddComponent<MeshRenderer>().material = CreateMaterial(new Color(0.04f, 0.04f, 0.04f));
    }

    Material CreateMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.SetFloat("_Metallic", 0.05f);
        mat.SetFloat("_Glossiness", 0.6f);
        return mat;
    }
}
