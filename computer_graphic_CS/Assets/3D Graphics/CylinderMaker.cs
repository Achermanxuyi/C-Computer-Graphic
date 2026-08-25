using UnityEngine;

public class CylinderMaker : MonoBehaviour
{

    [Tooltip("Number of radial divisions (use 16 for density 16)")]
    public int divisions = 16;
    public float radius = 2.0f;
    public float height = 2.0f;
    public Material material;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

        if (material == null)
        {
            meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        }
        else
        {
            meshRenderer.sharedMaterial = material;
        }

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

        meshFilter.mesh = MeshUtilities.Cylinder(divisions, radius, height); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
