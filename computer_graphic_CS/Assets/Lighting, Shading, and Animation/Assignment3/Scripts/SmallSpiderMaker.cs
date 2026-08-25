using System;
using UnityEngine;

public class SmallSpiderMaker : MonoBehaviour
{
    [Header("Appearance")]
    public int legCount = 8;
    public float headRadius = 0.8f;
    public float bodyRadius = 0.35f;
    public float upperLegLength = 0.75f;
    public float lowerLegLength = 1.5f;
    public float footLength = 0.1f;
    public float legThickness = 0.04f;
    public Material smallSpiderMaterial;
    public Material pupilMaterial;

    [Header("Leg Spread Settings")]
    public float legSpreadAngle = -45f; // How much legs spread outward (0-90 degrees)

    [Header("Animation References")]
    public Transform leftPupilJoint;
    public Transform rightPupilJoint;
    public Transform headTransform;
    public Transform bodyTransform;
    public Transform[] legJoints; // Array to store all leg base joints
    public Transform[] kneeJoints; // Array to store all knee joints

    // Internal references
    GameObject headObj;
    GameObject bodyObj;

    void Start()
    {
        // transform.localPosition = new Vector3(-20, 0.2f, 0);
        // transform.localRotation = Quaternion.Euler(0, -90, 0);
        // transform.localScale = Vector3.one * 0.2f;

        // create small spider head
        headObj = CreateHead(transform);
        headObj.transform.SetParent(transform, false);
        headObj.transform.localPosition = Vector3.zero;
        headTransform = headObj.transform;

        // attach eyes to the head
        GameObject eyes = CreateEyes(headObj.transform);
        eyes.transform.SetParent(headObj.transform, false);
        eyes.transform.localPosition = new Vector3(0, 0.12f, -0.47f);

        // create small spider body
        bodyObj = CreateBody(transform);
        bodyObj.transform.SetParent(transform, false);
        bodyObj.transform.localPosition = new Vector3(0, 0, 0.375f);
        bodyTransform = bodyObj.transform;

        // attach legs to spider head
        GameObject legs = CreateLegs(headObj.transform);
        legs.transform.SetParent(headObj.transform, false);
        legs.transform.localPosition = Vector3.zero;
    }

    GameObject CreateHead(Transform root)
    {
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(transform, false);
        head.transform.localPosition = Vector3.zero;
        head.transform.localRotation = Quaternion.identity;
        head.transform.localScale = new Vector3(1.25f, 1, 1) * headRadius;

        var mr = head.GetComponent<MeshRenderer>();
        mr.sharedMaterial = smallSpiderMaterial;

        return head;
    }

    GameObject CreateEyes(Transform root)
    {
        GameObject eyes = new GameObject("Eyes");

        GameObject leftEye = CreateSingleEye("LeftEye", out Transform leftPupil);
        leftEye.transform.SetParent(eyes.transform, false);
        leftEye.transform.localPosition = new Vector3(-0.18f, 0, 0);
        leftPupilJoint = leftPupil;

        GameObject rightEye = CreateSingleEye("RightEye", out Transform rightPupil);
        rightEye.transform.SetParent(eyes.transform, false);
        rightEye.transform.localPosition = new Vector3(0.18f, 0, 0);
        rightPupilJoint = rightPupil;

        return eyes;
    }

    GameObject CreateSingleEye(String name, out Transform pupilJoint)
    {
        GameObject singleEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        singleEye.name = name;
        singleEye.transform.localScale = new Vector3(0.36f, 0.36f, 0.12f);

        GameObject pupilJointObj = new GameObject("PupilJoint");
        pupilJointObj.transform.SetParent(singleEye.transform, false);
        pupilJointObj.transform.localPosition = new Vector3(0, 0, 0.25f);
        pupilJointObj.transform.localRotation = Quaternion.identity;
        pupilJoint = pupilJointObj.transform;

        GameObject pupil = new GameObject("Pupil");
        pupil.transform.SetParent(pupilJointObj.transform, false);
        pupil.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        MeshFilter mf = pupil.AddComponent<MeshFilter>();
        MeshRenderer mr = pupil.AddComponent<MeshRenderer>();
        mr.material = pupilMaterial;
        mf.mesh = MeshUtilities.Cylinder(12, 0.15f, 0.75f);

        return singleEye;
    }

    GameObject CreateBody(Transform root)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = Vector3.zero;
        body.transform.localRotation = Quaternion.identity;
        body.transform.localScale = new Vector3(1.25f, 1, 1) * bodyRadius;

        var mr = body.GetComponent<MeshRenderer>();
        mr.sharedMaterial = smallSpiderMaterial;
        
        return body;
    }

    GameObject CreateLegs(Transform root)
    {
        GameObject legs = new GameObject("Legs");
        
        // Initialize the leg joints arrays
        legJoints = new Transform[legCount];
        kneeJoints = new Transform[legCount];
        
        // Calculate angle between legs
        float angleStep = 360f / legCount;
        
        for (int i = 0; i < legCount; i++)
        {
            // Calculate angle for this leg (starting from front-right, going clockwise)
            float angle = i * angleStep + 22.5f;
            
            // Create the leg and store its joint references
            GameObject leg = CreateSingleLeg(legs.transform, "Leg_" + i, angle, i, out Transform baseJoint, out Transform kneeJoint);
            leg.transform.SetParent(legs.transform, false);
            
            // Store the joint references for animation
            legJoints[i] = baseJoint;
            kneeJoints[i] = kneeJoint;
        }
        
        return legs;
    }
    
    GameObject CreateSingleLeg(Transform root, String legRef, float angle, int legIndex, out Transform baseJoint, out Transform kneeJoint)
    {
        GameObject singleLeg = new GameObject(legRef);
        
        // Position inside the body
        float attachRadius = headRadius * 0.5f;
        float angleRad = angle * Mathf.Deg2Rad;
        Vector3 attachPoint = new Vector3(
            Mathf.Cos(angleRad) * attachRadius,
            -0.25f,
            Mathf.Sin(angleRad) * attachRadius
        );
        
        singleLeg.transform.localPosition = attachPoint;
        
        GameObject baseJointObj = new GameObject("BaseJoint");
        baseJointObj.transform.SetParent(singleLeg.transform, false);
        baseJointObj.transform.localPosition = Vector3.zero;

        Quaternion outwardRotation = Quaternion.LookRotation(new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad)), Vector3.up);
        Quaternion tilt = Quaternion.Euler(legSpreadAngle, 0, 0);
        baseJointObj.transform.localRotation = outwardRotation * tilt * Quaternion.Euler(0, 180f, 0);

        baseJoint = baseJointObj.transform;
        
        GameObject upperLeg = CreateLegSegment("UpperLeg", upperLegLength, legThickness);
        upperLeg.transform.SetParent(baseJointObj.transform, false);
        upperLeg.transform.localPosition = new Vector3(0, 0, -upperLegLength / 2f);
        upperLeg.transform.localRotation = Quaternion.Euler(90f, 0, 0);
        
        // debugged by ChatGPT
        // --- KneeJoint pivot (no scale applied here) ---
        GameObject kneeJointObj = new GameObject("KneeJoint");
        kneeJointObj.transform.SetParent(baseJointObj.transform, false);
        kneeJointObj.transform.localPosition = new Vector3(0, 0, -upperLegLength);
        kneeJointObj.transform.localRotation = Quaternion.Euler(-100f, 0, 0);
        kneeJoint = kneeJointObj.transform;

        // --- Visual sphere for knee joint (isolated scale) ---
        GameObject kneeVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        kneeVisual.name = "KneeVisual";
        kneeVisual.transform.SetParent(kneeJointObj.transform, false);
        kneeVisual.transform.localScale = Vector3.one * legThickness * 0.75f; // scale sphere only
        kneeVisual.transform.localPosition = Vector3.zero; // center sphere at pivot
        kneeVisual.GetComponent<MeshRenderer>().sharedMaterial = smallSpiderMaterial;

        kneeJoint = kneeJointObj.transform;
        
        GameObject lowerLeg = CreateLegSegment("LowerLeg", lowerLegLength, legThickness * 0.8f);
        lowerLeg.transform.SetParent(kneeJointObj.transform, false);
        lowerLeg.transform.localPosition = new Vector3(0, 0, -lowerLegLength / 2f);
        lowerLeg.transform.localRotation = Quaternion.Euler(-90f, 0, 0);
        
        // --- FootJoint pivot (no scaling here) ---
        GameObject footJoint = new GameObject("FootJoint");
        footJoint.transform.SetParent(kneeJointObj.transform, false);
        footJoint.transform.localPosition = new Vector3(0, 0, -lowerLegLength);
        footJoint.transform.localRotation = Quaternion.Euler(-120f, 0, 0);

        // --- Visual sphere for FootJoint ---
        GameObject footVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        footVisual.name = "FootVisual";
        footVisual.transform.SetParent(footJoint.transform, false);
        footVisual.transform.localScale = Vector3.one * legThickness * 0.75f;   // scale only this sphere
        footVisual.transform.localPosition = Vector3.zero;               // align with pivot
        footVisual.GetComponent<MeshRenderer>().sharedMaterial = smallSpiderMaterial;
        Destroy(footVisual.GetComponent<Collider>());                    // optional

        // --- Foot mesh (child of pivot, unaffected by scale) ---
        GameObject foot = CreateLegSegment("Foot", footLength, legThickness * 0.6f);
        foot.transform.SetParent(footJoint.transform, false);
        foot.transform.localPosition = new Vector3(0, 0, footLength / 2f);
        foot.transform.localRotation = Quaternion.Euler(90f, 0, 0);

        
        return singleLeg;
    }
    
    GameObject CreateLegSegment(String name, float length, float thickness)
    {
        GameObject segment = new GameObject(name);
        
        MeshFilter mf = segment.AddComponent<MeshFilter>();
        MeshRenderer mr = segment.AddComponent<MeshRenderer>();
        mr.material = smallSpiderMaterial;
        
        // Cylinder extends from -height to +height, so we pass length/2 as height
        // This way the total length is correct
        mf.mesh = MeshUtilities.Cylinder(8, thickness / 2f, length / 2f);
        
        return segment;
    }
}