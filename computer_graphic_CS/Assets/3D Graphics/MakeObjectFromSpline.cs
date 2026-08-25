using UnityEngine;
using UnityEngine.Splines;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Profile Spline")]
    public SplineContainer splineProfile;

    [Header("Path Spline")]
    public int pathDivisions = 100;
    public int profileDivisions = 16;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

        // Vector3[] circleProfile = MeshUtilities.MakeCircleProfile(0.1f, 16);
        Vector3[] profile = MeshUtilities.MakeProfileFromSpline(splineProfile, profileDivisions);
        for (int i = 0; i < profile.Length; i++)
        {
            profile[i] *= 0.2f;
        }

        var spline = gameObject.GetComponent<SplineContainer>();

        var path = MeshUtilities.MakePathFromSpline(spline, pathDivisions);

        meshFilter.mesh = MeshUtilities.Sweep(profile, path, false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
