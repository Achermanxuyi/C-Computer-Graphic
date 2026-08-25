using UnityEngine;

public class LampMaker : MonoBehaviour
{

    // Lamp components game objects
    public GameObject lampBase;
    public GameObject arm1;
    public GameObject arm2;
    public GameObject lampShade;
    public GameObject baseJoint;
    public GameObject elbowJoint;
    public GameObject shadeJoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

        meshFilter.mesh = MeshUtilities.Cylinder(16, 0.1f, 0.01f);

        // Add lamp base
        lampBase = new GameObject();
        lampBase.name = "Lamp Base";

        meshRenderer = lampBase.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        meshFilter = lampBase.AddComponent<MeshFilter>();
        meshFilter.mesh = MeshUtilities.Cylinder(16, 0.02f, 0.01f);

        lampBase.transform.parent = transform;
        lampBase.transform.localPosition = new Vector3(0, 0.02f, 0);

        // make the sweep profile the arm
        Vector3[] armProfile = new Vector3[] {
            new Vector3(0.0f, -0.12f, 0.0f),
            new Vector3(0.014f, -0.114f, 0.0f),
            new Vector3(0.02f, -0.1f, 0.0f),
            new Vector3(0.02f, 0.1f, 0.0f),
            new Vector3(0.014f, 0.114f, 0.0f),
            new Vector3(0.0f, 0.12f, 0.0f),
            new Vector3(-0.014f, 0.114f, 0.0f),
            new Vector3(-0.02f, 0.1f, 0.0f),
            new Vector3(-0.02f, -0.1f, 0.0f),
            new Vector3(-0.014f, -0.114f, 0.0f)
        };

        Matrix4x4[] armPath = new Matrix4x4[10];
        armPath[0] = Matrix4x4.Scale(new Vector3(0, 0, 1)) *
                     Matrix4x4.Translate(new Vector3(0, 0, -0.01f));

        armPath[1] = Matrix4x4.Scale(new Vector3(0.9f, 0.98f, 1)) *
             Matrix4x4.Translate(new Vector3(0, 0, -0.01f));

        armPath[2] = Matrix4x4.Translate(new Vector3(0, 0, -0.0075f));
        armPath[3] = Matrix4x4.Translate(new Vector3(0, 0, 0.0075f));

        armPath[4] = Matrix4x4.Scale(new Vector3(0.9f, 0.98f, 1)) *
             Matrix4x4.Translate(new Vector3(0, 0, 0.01f));
        armPath[5] = Matrix4x4.Scale(new Vector3(0, 0, 1)) *
                     Matrix4x4.Translate(new Vector3(0, 0, 0.01f));

        armPath[6] = armPath[5];

        armPath[7] = Matrix4x4.Scale(new Vector3(0.9f, 0.98f, 1)) *
             Matrix4x4.Translate(new Vector3(0, 0, 0.01f));

        armPath[8] = armPath[7];
        armPath[9] = Matrix4x4.Scale(new Vector3(0, 0, 1)) *
                     Matrix4x4.Translate(new Vector3(0, 0, 0.01f));

        arm1 = new GameObject();
        arm1.name = "Lamp Arm1";

        meshRenderer = arm1.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        meshFilter = arm1.AddComponent<MeshFilter>();
        meshFilter.mesh = MeshUtilities.Sweep(armProfile, armPath, false);

        arm1.transform.parent = lampBase.transform;
        arm1.transform.localPosition = new Vector3(0, 0.3f, 0); // note: not correct yet

        // place the joints
        baseJoint = new GameObject();
        baseJoint.name = "Base Joint";
        baseJoint.transform.parent = lampBase.transform;
        baseJoint.transform.localPosition = new Vector3(0, 0, 0);

        elbowJoint = new GameObject();
        elbowJoint.name = "Elbow Joint";
        elbowJoint.transform.parent = baseJoint.transform;
        elbowJoint.transform.localPosition = new Vector3(0, 0.2f, 0);
        elbowJoint.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 45));

        shadeJoint = new GameObject();
        shadeJoint.name = "Shade Joint";
        shadeJoint.transform.parent = elbowJoint.transform;
        shadeJoint.transform.localPosition = new Vector3(0, 0.2f, 0);
        shadeJoint.transform.localRotation = Quaternion.identity;

        // attach arm1 to baseJoint
        arm1.transform.parent = baseJoint.transform;
        arm1.transform.localPosition = new Vector3(0, 0.1f, 0);
        arm1.transform.localRotation = Quaternion.identity;

        // create arm2 and attach to elbowJoint
        arm2 = Instantiate(arm1);
        arm2.name = "Lamp Arm2";
        arm2.transform.parent = elbowJoint.transform;
        arm2.transform.localPosition = new Vector3(0, 0.1f, 0);
        arm2.transform.localRotation = Quaternion.identity;

        // profile for the lamp shade
        Vector3[] shadeProfile = new Vector3[] {
            new Vector3(0.0f, -0.12f, 0.0f),   // bottom center (inner)
            new Vector3(0.0f, 0.114f, 0.0f),   // top center (inner)
            new Vector3(0.08f, -0.06f, 0.0f),  // bottom-right (transition)
            new Vector3(0.12f, 0.1f, 0.0f),    // mid-right
            new Vector3(0.11f, 0.114f, 0.0f),  // top-right (inner rim)
            new Vector3(0.14f, 0.14f, 0.0f),   // top-right (outer)
            new Vector3(0.134f, 0.144f, 0.0f), // top-right (rim edge)
            new Vector3(0.16f, 0.16f, 0.0f),   // furthest top-right
            new Vector3(0.154f, 0.164f, 0.0f), // final outer point
        };

        // Create a circular sweep path for the lamp shade
        int shadeSegments = 16;
        Matrix4x4[] shadePath = new Matrix4x4[shadeSegments + 1];
        
        for (int i = 0; i <= shadeSegments; i++)
        {
            float angle = (float)i / shadeSegments * Mathf.PI * 2;
            shadePath[i] = Matrix4x4.Rotate(Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0));
        }

        // create the lamp shade object
        lampShade = new GameObject();
        lampShade.name = "Lamp Shade";

        meshRenderer = lampShade.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        meshFilter = lampShade.AddComponent<MeshFilter>();
        meshFilter.mesh = MeshUtilities.Sweep(shadeProfile, shadePath, true);

        // scale the lamp shade
        lampShade.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // attach to the shade joint
        lampShade.transform.parent = shadeJoint.transform;
        lampShade.transform.localPosition = new Vector3(0, 0, 0);
        lampShade.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
