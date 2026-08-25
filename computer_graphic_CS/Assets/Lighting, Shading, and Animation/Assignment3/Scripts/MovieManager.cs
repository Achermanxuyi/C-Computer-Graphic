using System.Collections;
using UnityEngine;

public class MovieManager : MonoBehaviour
{
    [Header("Prefabs")]
    public SmallSpiderMaker smallSpiderPrefab;
    public BigSpiderMaker bigSpiderPrefab;
    public MakeFly flyPrefab;
    public SpiderWebMaker spiderWebPrefab;

    [Header("Camera")]
    public Camera mainCamera;
    public float cameraSmoothTime = 0.25f;
    public Vector3 cameraOffset = new Vector3(0f, 3f, -6f);

    // Instances & controllers
    private GameObject smallSpiderGO;
    private SmallSpiderMaker smallSpiderInstance;
    private SmallSpiderAniController smallSpiderAni;
    private Transform smallSpiderT;

    private GameObject bigSpiderGO;
    private BigSpiderMaker bigSpiderInstance;
    private BigSpiderAniController bigSpiderAni;
    private Transform bigSpiderT;

    private GameObject flyGO;
    private MakeFly flyInstance;
    private FlyAniController flyAni;
    private Transform flyT;
    private AudioSource flyAudioSource; 
    private GameObject webGO;
    private SpiderWebMaker webInstance;
    private WebAniController webAni;
    private Transform webT;

    private float movieTime = 0f;

    // State tracking
    private bool webTriggered = false;
    private bool bigSpiderNotified = false;

    [Header("Cinematic")]
    public float eyeFocusDuration = 5f;
    private bool eyeFocusStarted = false;
    private float eyeFocusTimer = 0f;
    [Header("Phase D Rotation")]
    public float preClimbRotationSec = 1.0f;   // rotate to align BEFORE climbing
    public float postClimbRotationSec = 1.0f;  // rotate to face big spider AFTER arrival
    private bool preClimbRotationDone = false;
    private bool postClimbRotationStarted = false;
    private float preClimbRotTimer = 0f;
    private float postClimbRotTimer = 0f;
    [Header("Phase E (Big Spider)")]
    public float phaseESec = 4f; // duration of big spider climbing
    private bool bigSpiderClimbStarted = false;
    private float bigSpiderClimbTimer = 0f;

    [Header("Phase F (Fly Exit)")]
    public float phaseFSec = 5f;
    public float phaseFStartTime; // Calculated start time for phase F
    private bool phaseFStarted = false;

    [Header("Audio")]
    public AudioSource soundtrackSource;
    public AudioClip flyBuzzSound; // Assign a buzzing sound in the Inspector

    void Start()
    {
        // instantiate everything
        InitialiseAudio();
        InitialiseSmallSpider();
        InitialiseWeb();
        InitialiseBigSpider();
        InitialiseFly();

        // ensure camera exists
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogWarning("MovieManager: No camera assigned and no Camera.main found.");

        phaseFStartTime = phaseASec + phaseBSec + phaseCSec + phaseDRiseSec + phaseESec + postClimbRotationSec;
    }

    void Update()
    {
        movieTime += Time.deltaTime;
        UpdateSmallSpiderPath(Time.deltaTime);
        UpdateFlyFollow(Time.deltaTime);
        UpdateCamera(Time.deltaTime);

        // Check if it's time to start Phase F (fly exit)
        if (!phaseFStarted && movieTime >= phaseFStartTime)
        {
            phaseFStarted = true;
            StartCoroutine(PhaseFFlyExit());
        }
    }

    #region Initialization
    void InitialiseSmallSpider()
    {
        if (smallSpiderPrefab == null) { Debug.LogError("Assign smallSpiderPrefab"); return; }
        smallSpiderGO = Instantiate(smallSpiderPrefab.gameObject);
        smallSpiderInstance = smallSpiderGO.GetComponent<SmallSpiderMaker>();
        smallSpiderAni = smallSpiderGO.GetComponent<SmallSpiderAniController>();
        smallSpiderT = smallSpiderGO.transform;

        // initial design says spider initial position is set inside its initializer
        // but to be deterministic here we put it to -20,0.2,0 as you described:
        smallSpiderT.position = new Vector3(-20f, 0.2f, 0f);
        smallSpiderT.rotation = Quaternion.Euler(0f, -90f, 0f);

        // enable leg & eyes animation initially
        if (smallSpiderAni != null)
        {
            smallSpiderAni.isPlaying = true;
            smallSpiderAni.animateLegs = true;
            smallSpiderAni.animateEyes = true;
            smallSpiderAni.legWaveSpeed = 5f;
        }
    }

    void InitialiseWeb()
    {
        if (spiderWebPrefab == null) { Debug.LogError("Assign spiderWebPrefab"); return; }
        webGO = Instantiate(spiderWebPrefab.gameObject);
        webInstance = webGO.GetComponent<SpiderWebMaker>();
        webAni = webGO.GetComponent<WebAniController>();
        webT = webGO.transform;

        // place web at default you used earlier
        webT.localPosition = new Vector3(-0.25f, 2.5f, 5f);
        webT.localRotation = Quaternion.Euler(-75f, 0f, 0f);

        if (webAni != null)
            webAni.isPlaying = false;
    }

    void InitialiseBigSpider()
    {
        if (bigSpiderPrefab == null) { Debug.LogError("Assign bigSpiderPrefab"); return; }
        bigSpiderGO = Instantiate(bigSpiderPrefab.gameObject);
        bigSpiderInstance = bigSpiderGO.GetComponent<BigSpiderMaker>();
        bigSpiderAni = bigSpiderGO.GetComponent<BigSpiderAniController>();
        bigSpiderT = bigSpiderGO.transform;

        // attach to web so it appears on web
        bigSpiderGO.transform.SetParent(webT, false);
        bigSpiderT.localPosition = new Vector3(1.8f, 0.2f, 0f);
        bigSpiderT.localRotation = Quaternion.Euler(0f, 90f, 0f);
        bigSpiderT.localScale = Vector3.one * 0.3f;
    }

    void InitialiseFly()
    {
        if (flyPrefab == null) { Debug.LogError("Assign flyPrefab"); return; }
        flyGO = Instantiate(flyPrefab.gameObject);
        flyInstance = flyGO.GetComponent<MakeFly>();
        flyAni = flyGO.GetComponent<FlyAniController>();
        flyT = flyGO.transform;

        // Get or add AudioSource component to the fly
        flyAudioSource = flyGO.GetComponent<AudioSource>();
        if (flyAudioSource == null)
        {
            Debug.LogWarning("Fly Prefab is missing an AudioSource component! Adding one now.");
            flyAudioSource = flyGO.AddComponent<AudioSource>();
        }

        // Configure the fly's AudioSource for 3D spatial audio
        flyAudioSource.spatialBlend = 1.0f; // Full 3D sound
        flyAudioSource.loop = true;
        flyAudioSource.playOnAwake = false;
        flyAudioSource.minDistance = 1f;
        flyAudioSource.maxDistance = 20f;
        flyAudioSource.rolloffMode = AudioRolloffMode.Linear;
        flyAudioSource.volume = 0.5f;

        // Assign the buzz sound if available
        if (flyBuzzSound != null)
        {
            flyAudioSource.clip = flyBuzzSound;
        }
        else
        {
            Debug.LogWarning("No fly buzz sound assigned! Assign flyBuzzSound in the Inspector for audio.");
        }

        // Start playing the fly sound
        if (flyAudioSource.clip != null)
        {
            flyAudioSource.Play();
        }

        // initial fly position relative to spider path
        flyT.position = smallSpiderT.position + new Vector3(-2f, 1.8f, 0f);
        flyT.rotation = Quaternion.Euler(0f, -90f, 0f);

        if (flyAni != null)
        {
            flyAni.isPlaying = true;
            flyAni.animateWings = true;
            flyAni.animateLeg = false;
        }
    }

    void InitialiseAudio()
    {
        // Get the AudioSource component, which MUST be on this same GameObject
        soundtrackSource = GetComponent<AudioSource>();

        if (soundtrackSource == null)
        {
            Debug.LogError("MovieManager requires an AudioSource component on the same GameObject to play the soundtrack.");
        }

        soundtrackSource.loop = true;
    }
    #endregion

    #region Small spider path & states

    // Timings & phases:
    // Phase A: move straight from (-20,0.2,0) -> (10,0.2,0)
    // Phase B: semicircle arc from (10,0.2,0) -> (10,0.2,-4) turning right (yaw -90 -> 90)
    // Phase C: move (10,0.2,-4) -> (0,0.2,4.5) (arrive then rotate to 0,180,0)
    // Phase D: move to web final (-0.25,5.2,5.5)

    // configurable durations (seconds)
    public float phaseASec = 8f;
    public float phaseBSec = 4f;
    public float phaseCSec = 6f;
    public float phaseDRiseSec = 4f;

    // internal timers for phases
    private float phaseTimer = 0f;

    void UpdateSmallSpiderPath(float dt)
    {
        if (smallSpiderT == null) return;

        // compute overall phase from movieTime
        float t = movieTime;

        Vector3 startA = new Vector3(-20f, 0.2f, 0f);
        Vector3 endA = new Vector3(10f, 0.2f, 0f);
        Vector3 endB = new Vector3(10f, 0.2f, -4f);

        if (t <= phaseASec)
        {
            // Phase A: straight
            float n = Mathf.Clamp01(t / phaseASec);
            smallSpiderT.position = Vector3.Lerp(startA, endA, n);
            // face forward along path (smooth yaw)
            Vector3 dir = (endA - startA).normalized;
            if (dir.sqrMagnitude > 0.001f)
            {
                // keep its initial rotation (0, -90, 0)
                smallSpiderT.rotation = Quaternion.Euler(0f, -90f, 0f);
            }
        }
        else if (t <= phaseASec + phaseBSec)
        {
            // Phase B: semicircle between endA and endB turning right.
            float localT = (t - phaseASec) / phaseBSec; // 0..1
            smallSpiderT.position = MoveAlongSemiCircle(endA, endB, localT, true);

            // yaw from -90 -> +90 during the turn (smooth)
            Quaternion from = Quaternion.Euler(0f, -90f, 0f);
            Quaternion to = Quaternion.Euler(0f, 90f, 0f);
            smallSpiderT.rotation = Quaternion.Slerp(from, to, localT);

            // Trigger web animation when passing -3.x on X (only once)
            if (!webTriggered && smallSpiderT.position.x >= -3f)
            {
                webTriggered = true;
                if (webAni != null) webAni.isPlaying = true;
                // notify big spider to start paying attention
                if (bigSpiderAni != null)
                {
                    bigSpiderAni.isPlaying = true;
                    bigSpiderNotified = true;
                }
            }
        }
        else if (t <= phaseASec + phaseBSec + phaseCSec)
        {
            // Phase C: move from endB -> (0,0.2,4.5)
            float localT = (t - (phaseASec + phaseBSec)) / phaseCSec;
            Vector3 phaseCStart = endB;
            Vector3 phaseCEnd = new Vector3(0f, 0.2f, 4.5f);

            smallSpiderT.position = Vector3.Lerp(phaseCStart, phaseCEnd, localT);

            // At the *moment of arrival* we smoothly rotate to (0,180,0).
            if (localT >= 0.999f)
            {
                StartCoroutine(RotateOverSeconds(smallSpiderT, Quaternion.Euler(0f, 180f, 0f), 1.0f));
            }
        }
        else
        {
            // PHASE D+E handled here:
            // We will: 1) pre-rotate to face climb direction / be parallel to web (while staying at last pos)
            // 2) climb from last -> final over remaining time
            // 3) after arrival, do post-rotation to face big spider
            // 4) then trigger big spider climb (Phase E)

            // compute times relative to phaseDRiseSec
            float phaseDstart = phaseASec + phaseBSec + phaseCSec;
            float tIntoD = Mathf.Clamp01((t - phaseDstart) / phaseDRiseSec); // 0..1 across phaseDRiseSec

            Vector3 last = new Vector3(0f, 0.2f, 4.5f);
            Vector3 final = new Vector3(-0.25f, 5.2f, 5.5f);

            // 1) PRE-CLIMB ROTATION stage: during the first preClimbRotationSec seconds, rotate in place to face climb target
            if (!preClimbRotationDone)
            {
                // Start rotating in place before climb. Use a LookRotation toward the climb target so spider faces the web.
                if (preClimbRotTimer == 0f)
                {
                    // nothing special to init
                }

                preClimbRotTimer += Time.deltaTime;
                float rotN = Mathf.Clamp01(preClimbRotTimer / preClimbRotationSec);

                // Desired look direction is toward the climb final location (so spider will "face the web")
                Vector3 desiredDir = (final - smallSpiderT.position);
                if (desiredDir.sqrMagnitude < 0.0001f)
                    desiredDir = smallSpiderT.forward; // fallback

                Quaternion desiredRot = Quaternion.LookRotation(desiredDir.normalized);

                // Slerp from current to desired; use rotN to smoothly ease in
                smallSpiderT.rotation = Quaternion.Slerp(smallSpiderT.rotation, desiredRot, rotN);

                // stay at the last ground position until rotation completes
                smallSpiderT.position = last;

                if (rotN >= 0.999f)
                {
                    preClimbRotationDone = true;
                    // ensure a clean final rotation
                    smallSpiderT.rotation = desiredRot;
                }

                // not yet climbing; exit early
                return;
            }

            // 2) CLIMB stage: move from last -> final over 'climbT' which maps tIntoD across the remaining fraction
            // We reserved the first preClimbRotationSec seconds for rotation, so compute the remaining local progress.
            float climbStartDelay = preClimbRotationSec;
            float climbTotal = Mathf.Max(0.0001f, phaseDRiseSec - climbStartDelay);
            float tSincePhaseDStart = (movieTime - phaseDstart) - climbStartDelay;
            float climbT = Mathf.Clamp01(tSincePhaseDStart / climbTotal);

            // Interpolate position from last -> final using climbT
            smallSpiderT.position = Vector3.Lerp(last, final, climbT);

            // Maintain the rotation that was set at the end of pre-rotation while climbing
            // (small tilt may naturally occur because LookRotation accounts for up vector; keep as-is)
            // Optionally we could nudge pitch upward slightly to align with web plane if needed.

            // 3) After arrival, do post rotation to face big spider
            if (climbT >= 0.999f)
            {
                // ensure exact final pos
                smallSpiderT.position = final;

                if (!postClimbRotationStarted)
                {
                    postClimbRotationStarted = true;
                    postClimbRotTimer = 0f;
                }

                if (postClimbRotationStarted && postClimbRotTimer < postClimbRotationSec)
                {
                    postClimbRotTimer += Time.deltaTime;
                    float postN = Mathf.Clamp01(postClimbRotTimer / postClimbRotationSec);

                    // Look at big spider (if big spider exists), otherwise face world-forward
                    Vector3 faceDir;
                    if (bigSpiderT != null)
                        faceDir = (bigSpiderT.position - smallSpiderT.position);
                    else
                        faceDir = Vector3.forward;

                    if (faceDir.sqrMagnitude < 0.0001f)
                        faceDir = smallSpiderT.forward;

                    Quaternion faceRot = Quaternion.LookRotation(faceDir.normalized);

                    // Slerp from current rotation to faceRot
                    smallSpiderT.rotation = Quaternion.Slerp(smallSpiderT.rotation, faceRot, postN);
                }
                else
                {
                    // Post rotation done; ensure it's exact
                    if (bigSpiderT != null)
                        smallSpiderT.rotation = Quaternion.LookRotation((bigSpiderT.position - smallSpiderT.position).normalized);

                    // Trigger big spider climb only once
                    if (!bigSpiderClimbStarted)
                    {
                        bigSpiderClimbStarted = true;
                        bigSpiderClimbTimer = 0f;
                    }
                }
            }

            // 4) Big spider climbing (Phase E) - unchanged, but ensure it moves toward the small spider
            if (bigSpiderT != null && bigSpiderClimbStarted)
            {
                bigSpiderClimbTimer += dt;
                float climbPhaseT = Mathf.Clamp01(bigSpiderClimbTimer / phaseESec);

                // choose start on the web and target slightly near the small spider
                Vector3 start = webT.TransformPoint(new Vector3(1.8f, -0.5f, 0f));
                Vector3 target = smallSpiderT.position + (Vector3.back * 0.25f); // stop a little behind
                bigSpiderT.position = Vector3.Lerp(start, target, climbPhaseT);

                // animate big spider to look at small spider while climbing
                bigSpiderAni.LookAtTarget(smallSpiderT.position);
                bigSpiderAni.isPlaying = true;
            }
        }



        // Keep animator flags on
        if (smallSpiderAni != null)
        {
            smallSpiderAni.isPlaying = true;
            smallSpiderAni.legWaveSpeed = 5f;
        }

        // Make the small spider eyes look at the fly (if available)
        if (flyT != null && smallSpiderAni != null)
        {
            smallSpiderAni.LookAtTarget(flyT.position);
        }

        // Make big spider look at small spider after web triggered
        if (webTriggered && bigSpiderAni != null)
        {
            bigSpiderAni.LookAtTarget(smallSpiderT.position);
        }
    }

    // semicircle helper: assumes endpoints are diameter ends -> center = midpoint
    Vector3 MoveAlongSemiCircle(Vector3 a, Vector3 b, float t01, bool turnRight)
    {
        t01 = Mathf.Clamp01(t01);
        Vector3 center = (a + b) * 0.5f;
        Vector3 aRel = a - center;
        float radius = aRel.magnitude; // half-distance

        float startAngle = Mathf.Atan2(aRel.z, aRel.x); // radians
        float sweep = Mathf.PI; // semicircle
        float dir = (turnRight) ? -1f : 1f; // right turn negative sweep on XZ plane (choose sign by testing)
        float angle = startAngle + dir * sweep * t01;

        float x = center.x + Mathf.Cos(angle) * radius;
        float z = center.z + Mathf.Sin(angle) * radius;
        return new Vector3(x, a.y, z);
    }

    IEnumerator RotateOverSeconds(Transform tr, Quaternion targetRot, float seconds)
    {
        if (tr == null) yield break;
        Quaternion start = tr.rotation;
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            tr.rotation = Quaternion.Slerp(start, targetRot, elapsed / seconds);
            yield return null;
        }
        tr.rotation = targetRot;
    }

    #endregion

    #region Fly follow logic

    // control weights
    public float flyFollowSmoothing = 4f;
    public float flyAheadChance = 0.35f; // chance to be ahead when toggled
    public float flyAheadToggleInterval = 1.2f; // seconds

    private float flyAheadTimer = 0f;
    private bool flyAhead = false;

    void UpdateFlyFollow(float dt)
    {
        if (flyT == null || smallSpiderT == null || flyAni == null || phaseFStarted) return;

        flyAheadTimer += dt;
        if (flyAheadTimer >= flyAheadToggleInterval)
        {
            flyAheadTimer = 0f;
            flyAhead = Random.value < flyAheadChance;
        }

        // target offset relative to spider
        Vector3 baseOffset = new Vector3(flyAhead ? 2f : -1.2f, Random.Range(1f, 2f), Random.Range(-1.2f, 1.2f));
        Vector3 desiredPos = smallSpiderT.position + baseOffset;

        // Smoothly follow
        flyT.position = Vector3.Lerp(flyT.position, desiredPos, Mathf.Clamp01(Time.deltaTime * flyFollowSmoothing));

        // Orientation: always look toward the small spider
        Vector3 dirToSpider = smallSpiderT.position - flyT.position;
        if (dirToSpider.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(-dirToSpider.normalized);
            flyT.rotation = Quaternion.Slerp(flyT.rotation, look, Mathf.Clamp01(Time.deltaTime * 6f));
        }

        // X rotation oscillation between -45 and +45 (pitch)
        float pitch = Mathf.Sin(Time.time * 2f + (flyAhead ? 0.5f : 0f)) * 45f;
        Vector3 euler = flyT.localEulerAngles;
        // convert to -180..180 for clamp
        float currentX = flyT.eulerAngles.x;
        if (currentX > 180f) currentX -= 360f;
        float newX = Mathf.Clamp(pitch, -45f, 45f);
        Vector3 newEuler = new Vector3(newX, flyT.eulerAngles.y, flyT.eulerAngles.z);
        flyT.eulerAngles = newEuler;

        // Keep fly wings animating
        flyAni.animateWings = true;
        flyAni.isPlaying = true;
    }

    #endregion

    IEnumerator PhaseFFlyExit()
    {
        if (flyT == null) yield break;

        Debug.Log("Starting Phase F: Fly Exit.");
        
        Vector3 startPos = flyT.position;
        // Target: move to the coordinate (5, 5, 10)
        Vector3 endPos = new Vector3(15f, 5f, 2f); 
        float duration = phaseFSec;

        float startTime = Time.time;
        
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            
            // Move the fly smoothly
            flyT.position = Vector3.Lerp(startPos, endPos, t);
            
            // Make the fly orient itself towards its flight path
            // LookRotation will automatically handle the rotation towards the direction of travel
            Vector3 direction = (endPos - startPos).normalized;
            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                flyT.rotation = Quaternion.Slerp(flyT.rotation, targetRot, Time.deltaTime * 5f);
            }

            yield return null; // Wait until next frame
        }

        // Ensure final position is exact
        flyT.position = endPos;
        
        Debug.Log("Phase F: Fly Exit Complete. Fly sound will be quieter due to distance (3D audio).");
    }

    #region Camera

    private Vector3 cameraVelocity = Vector3.zero;

    void UpdateCamera(float dt)
    {
        if (mainCamera == null || smallSpiderT == null) return;

        // default desired camera position (following spider)
        Vector3 desired = smallSpiderT.position + cameraOffset;

        // bias slightly toward the fly if it's far away
        if (flyT != null)
        {
            Vector3 between = (flyT.position - smallSpiderT.position);
            desired += between * 0.02f;
        }

        float endOfPhaseD = phaseASec + phaseBSec + phaseCSec + phaseDRiseSec;
        if (movieTime >= endOfPhaseD && !eyeFocusStarted)
        {
            eyeFocusStarted = true;
            eyeFocusTimer = 0f;
        }

        // --- Handle cinematic focus phase ---
        if (eyeFocusStarted)
        {
            eyeFocusTimer += dt;
            float t01 = Mathf.Clamp01(eyeFocusTimer / eyeFocusDuration);

            // Target cinematic camera position
            Vector3 targetPos = new Vector3(-1.505f, 5.086f, 4.781f);
            Quaternion targetRot = new Quaternion(-0.02588f, -0.51945f, -0.01263f, -0.85402f);

            // Prevent negative Y or Z values
            targetPos.y = Mathf.Max(targetPos.y, 0f);
            targetPos.z = Mathf.Max(targetPos.z, 0f);

            // Smoothly move toward the target
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, t01 * 0.6f);

            // Look toward small spider's head
            Vector3 lookTarget = smallSpiderT.position + Vector3.up * 0.4f;
            mainCamera.transform.rotation = Quaternion.Slerp(
                mainCamera.transform.rotation,
                Quaternion.LookRotation(lookTarget - mainCamera.transform.position),
                dt * 2f
            );

            // Hold after reaching
            if (eyeFocusTimer >= eyeFocusDuration)
                return;
        }

        else
        {
            // --- Default follow mode ---
            mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, desired, ref cameraVelocity, cameraSmoothTime);

            // look at small spider (slightly above)
            Vector3 lookTarget = smallSpiderT.position + Vector3.up * 0.4f;
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, Quaternion.LookRotation(lookTarget - mainCamera.transform.position), dt * 4f);
        }
    }


    #endregion
}