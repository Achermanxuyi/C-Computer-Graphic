using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyAniController : MonoBehaviour
{
    [Header("References")]
    private MakeFly flyMaker;

    [Header("Wing Animation Settings")]
    public float wingFlapSpeed = 100f;
    public float leftWingMinAngle = -30f;
    public float leftWingMaxAngle = -60f;
    public float rightWingMinAngle = -30f;
    public float rightWingMaxAngle = -60f;

    [Header("Leg Animation Settings")]
    public int animatedLegIndex = 0;
    public float legWaveSpeed = 0.5f;
    public float legWaveAmount = 15f;
    public float kneeWaveAmount = 25f;
    public float footWaveAmount = 15f;

    private float wingAnimTime = 0f;
    private float legAnimTime = 0f;

    private Quaternion baseInitialRot;
    private Quaternion kneeInitialRot;
    private Quaternion footInitialRot;

    private Transform baseJoint;
    private Transform kneeJoint;
    private Transform footJoint;

    private bool legInitialized = false;

    public bool isPlaying = false; // controlled by MovieManager
    public bool animateWings = true;
    public bool animateLeg = true;

    
    void Awake()
    {
        // find the MakeFly component on the same GameObject.
        flyMaker = GetComponent<MakeFly>();

        if (flyMaker == null)
        {
            Debug.LogError("MakeFly script not found on the same GameObject as FlyAniController!");
        }
    }
    

    void Start()
    {

    }

    void Update()
    {
        if (!isPlaying) return;

        if (flyMaker == null)
        {
            Debug.LogWarning("MakeFly reference is not set!");
            return;
        }

        if (animateWings)
        {
            AnimateWings();
        }

        // AnimateWings();

        if (animateLeg)
        {
            // Wait until legs are created, then initialize once
            if (!legInitialized && flyMaker.legJoints != null && flyMaker.legJoints.Length > 0)
            {
                InitializeLeg(animatedLegIndex);
                legInitialized = true;
            }

            if (legInitialized)
                AnimateSingleLegWave();
        }

    }

    void AnimateWings()
    {
        if (flyMaker.leftWingJoint == null || flyMaker.rightWingJoint == null)
            return;

        wingAnimTime += Time.deltaTime * wingFlapSpeed;

        float flapAmount = (Mathf.Sin(wingAnimTime) + 1f) / 2f;

        float leftWingZ = Mathf.Lerp(leftWingMinAngle, leftWingMaxAngle, flapAmount);
        flyMaker.leftWingJoint.localRotation = Quaternion.Euler(180, 210, leftWingZ);

        float rightWingZ = -Mathf.Lerp(rightWingMinAngle, rightWingMaxAngle, flapAmount);
        flyMaker.rightWingJoint.localRotation = Quaternion.Euler(0, -30, rightWingZ);
    }

    void InitializeLeg(int index)
    {
        if (flyMaker.legJoints == null || flyMaker.kneeJoints == null)
        {
            Debug.LogWarning("Leg joints not initialized in MakeFly!");
            return;
        }

        if (index < 0 || index >= flyMaker.legJoints.Length)
        {
            Debug.LogWarning($"Invalid leg index {index}! Valid range: 0-{flyMaker.legJoints.Length - 1}");
            return;
        }

        baseJoint = flyMaker.legJoints[index];
        kneeJoint = flyMaker.kneeJoints[index];

        if (kneeJoint != null && kneeJoint.childCount > 0)
        {
            foreach (Transform child in kneeJoint)
            {
                if (child.name.Contains("FootJoint"))
                {
                    footJoint = child;
                    break;
                }
            }
        }

        if (baseJoint != null) baseInitialRot = baseJoint.localRotation;
        if (kneeJoint != null) kneeInitialRot = kneeJoint.localRotation;
        if (footJoint != null) footInitialRot = footJoint.localRotation;
        
        Debug.Log($"Successfully initialized leg {index}");
    }

    void AnimateSingleLegWave()
    {
        if (baseJoint == null || kneeJoint == null)
            return;

        legAnimTime += Time.deltaTime * legWaveSpeed;

        float lift = Mathf.Sin(legAnimTime) * legWaveAmount;
        float forward = Mathf.Cos(legAnimTime) * (legWaveAmount * 0.5f);

        baseJoint.localRotation = baseInitialRot * Quaternion.Euler(-lift, forward, 0);

        float kneeLift = Mathf.Sin(legAnimTime + Mathf.PI / 2f) * kneeWaveAmount;
        kneeJoint.localRotation = kneeInitialRot * Quaternion.Euler(-kneeLift, 0, 0);

        if (footJoint != null)
        {
            float footMotion = Mathf.Sin(legAnimTime + Mathf.PI) * footWaveAmount;
            footJoint.localRotation = footInitialRot * Quaternion.Euler(footMotion * 0.5f, 0, 0);
        }
    }

    public void SetLegIndex(int index)
    {
        animatedLegIndex = index;
        legInitialized = false; // Force re-initialization
    }

    public void SetWaveSpeed(float speed)
    {
        legWaveSpeed = speed;
    }
}