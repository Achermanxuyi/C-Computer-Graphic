using UnityEngine;

public class PyramidMaker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

        meshFilter.mesh = MeshUtilities.Pyramid(1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
