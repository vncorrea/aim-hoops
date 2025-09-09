// Assets/Scripts/ThrowController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class ThrowController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Slider yawSlider;    // -45..45 (graus)
    [SerializeField] private Slider pitchSlider;  // 15..60 (graus)
    [SerializeField] private ChargeButton chargeButton;
    [SerializeField] private Rigidbody ballRb;
    [SerializeField] private Transform ballSpawn;
    [SerializeField] private Transform hoopTarget;  // opcional p/ mirar no aro

    [Header("Força")]
    [SerializeField] private float minForce = 1f;
    [SerializeField] private float maxForce = 100f;
    [SerializeField] private float chargeTime = 1.4f;
    [SerializeField] private TMP_Text forceText;

    [Header("Reset")]
    [SerializeField] private float autoResetDelay = 4.0f;
    [SerializeField] private float fallY = -5f;

    [Header("Mira & Debug")]
    [SerializeField] private bool useHoopAim = true;   // mira em direção ao aro no plano XZ
    [SerializeField] private bool invertYaw = false;   // inverte sentido do yaw
    [SerializeField] private bool showDebug = true;    // mostra linhas
    [SerializeField] private float dirLineLength = 2.5f;

    [Header("Preview Trajetória")]
    [SerializeField] private bool showTrajectory = true;
    [SerializeField] private int trajSteps = 40;
    [SerializeField] private float trajTimeStep = 0.05f;

    // Linhas
    [SerializeField] private LineRenderer dirLine;     // seta de direção
    [SerializeField] private LineRenderer trajLine;    // trajetória

    [SerializeField] float yawMin = -60f, yawMax = 60f;
    [SerializeField] float pitchMin = 0f, pitchMax = 85f;

    private float charge01; // 0..1
    private bool charging;
    private bool shotInProgress;

    void Awake()
    {
        // Setup automático dos LineRenderers (se não arrastar no Inspector)
        if (dirLine == null) dirLine = CreateLine("_DirLine", 0.035f);
        if (trajLine == null) trajLine = CreateLine("_TrajLine", 0.022f);
        // cores básicas diferentes pra distinguir
        dirLine.material.color = new Color(1f, 0.2f, 0.2f, 1f);   // vermelho
        trajLine.material.color = new Color(0.2f, 0.6f, 1f, 1f);  // azul
        ClearLines();
    }

    void Start()
    {
        if (chargeButton != null)
        {
            chargeButton.OnPressed += StartCharge;
            chargeButton.OnReleased += ReleaseShot;
        }
        RespawnBall();
        UpdateForceText(0f);
    }

    void Update()
    {
        if (ballRb && ballRb.transform.position.y < fallY && !charging)
        {
            RespawnBall();
        }

        // Enquanto carrega, atualiza a seta/preview com a força atual
        if (charging && showDebug)
        {
            var forceNow = Mathf.Lerp(minForce, maxForce, charge01);
            Vector3 dir = ComputeShotDirection(out Quaternion shotRot);
            DrawDirection(dir);
            if (showTrajectory) DrawTrajectory(dir, forceNow);
        }
    }

    void StartCharge()
    {
        if (shotInProgress) return;
        charging = true;
        StartCoroutine(ChargeRoutine());
    }

    IEnumerator ChargeRoutine()
    {
        charge01 = 0f;
        while (charging && charge01 < 1f)
        {
            charge01 += Time.deltaTime / chargeTime;
            charge01 = Mathf.Clamp01(charge01);
            UpdateForceText(Mathf.Lerp(minForce, maxForce, charge01));
            yield return null;
        }
    }

    void ReleaseShot()
    {
        if (!charging || shotInProgress) return;

        charging = false;
        shotInProgress = true;

        float force = Mathf.Lerp(minForce, maxForce, charge01);

        // Preparar corpo para receber impulso
        ballRb.isKinematic = false;
        ballRb.useGravity = true;
        ballRb.constraints = RigidbodyConstraints.None;

        // Reposiciona no spawn para consistência
        if (ballSpawn) ballRb.transform.SetPositionAndRotation(ballSpawn.position, ballSpawn.rotation);

        // Direção do tiro
        Vector3 dir = ComputeShotDirection(out Quaternion shotRot);

        // Zera velocidades (agora pode, pois não é kinematic)
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        // Impulso
        ballRb.AddForce(dir.normalized * force, ForceMode.Impulse);

        // Debug visível
        if (showDebug)
        {
            DrawDirection(dir);
            if (showTrajectory) DrawTrajectory(dir, force);
            // Também funciona na Scene (ligar Gizmos na Game se quiser ver aqui)
            Debug.DrawRay(ballRb.position, dir.normalized * dirLineLength, Color.red, 2f);
        }

        // Limpa UI/estado
        UpdateForceText(0f);
        charge01 = 0f;

        // Agenda reset
        StartCoroutine(ResetAfterDelay());
    }

    IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(autoResetDelay);

        if (!ballRb) yield break;

        if (ballRb.linearVelocity.magnitude < 0.3f || ballRb.transform.position.y < fallY)
        {
            RespawnBall();
        }
        shotInProgress = false;
        ClearLines();
    }

    void RespawnBall()
    {
        if (!ballRb || !ballSpawn) return;

        // Desliga kinematic para zerar velocidades com segurança
        ballRb.isKinematic = false;
        ballRb.useGravity = false;
        ballRb.constraints = RigidbodyConstraints.FreezeRotation;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.transform.SetPositionAndRotation(ballSpawn.position, ballSpawn.rotation);

        // Agora congela de novo
        ballRb.isKinematic = true;
        ballRb.Sleep();

        ClearLines();
    }

    Vector3 ComputeShotDirection(out Quaternion shotRot)
    {
        // sliders 0..1 -> graus
        float yaw01 = yawSlider ? yawSlider.value : 0f;
        float pitch01 = pitchSlider ? pitchSlider.value : 0.5f;

        float yawDeg = Mathf.Lerp(yawMin, yawMax, yaw01);
        float pitchDeg = Mathf.Lerp(pitchMin, pitchMax, pitch01);
        if (invertYaw) yawDeg = -yawDeg;

        // base: direção horizontal para o aro (ou Z do mundo)
        Vector3 baseForward;
        if (useHoopAim && hoopTarget)
        {
            Vector3 toHoop = hoopTarget.position - (ballSpawn ? ballSpawn.position : transform.position);
            baseForward = Vector3.ProjectOnPlane(toHoop, Vector3.up).normalized;
        }
        else baseForward = Vector3.forward;
        if (baseForward.sqrMagnitude < 1e-6f) baseForward = Vector3.forward;

        // 1) aplica yaw ao redor de Y
        Vector3 yawed = Quaternion.AngleAxis(yawDeg, Vector3.up) * baseForward;

        // 2) aplica pitch ao redor do eixo lateral (perp a Up e à direção yaw-ada)
        Vector3 sideAxis = Vector3.Cross(Vector3.up, yawed).normalized;
        Vector3 dir = Quaternion.AngleAxis(-pitchDeg, sideAxis) * yawed;

        shotRot = Quaternion.LookRotation(dir, Vector3.up);
        return dir.normalized;
    }

    // ---------- Debug / Preview ----------

    LineRenderer CreateLine(string name, float width)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.widthMultiplier = width;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 4;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.alignment = LineAlignment.View;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.useWorldSpace = true;
        return lr;
    }

    void DrawDirection(Vector3 dir)
    {
        if (!showDebug || dirLine == null) return;
        Vector3 start = ballSpawn ? ballSpawn.position : transform.position;
        Vector3 end = start + dir.normalized * dirLineLength;
        dirLine.positionCount = 2;
        dirLine.SetPosition(0, start);
        dirLine.SetPosition(1, end);
    }

    void DrawTrajectory(Vector3 dir, float force)
    {
        if (!showTrajectory || trajLine == null) return;
        Vector3 start = ballSpawn ? ballSpawn.position : transform.position;

        // Impulso → Δv = F / m
        float mass = Mathf.Max(0.0001f, ballRb ? ballRb.mass : 1f);
        Vector3 v0 = dir.normalized * (force / mass);
        Vector3 g = Physics.gravity;

        if (trajSteps < 2) trajSteps = 2;
        if (trajTimeStep <= 0f) trajTimeStep = 0.02f;

        trajLine.positionCount = trajSteps;
        for (int i = 0; i < trajSteps; i++)
        {
            float t = i * trajTimeStep;
            // s = s0 + v0 t + 0.5 g t^2
            Vector3 p = start + v0 * t + 0.5f * g * (t * t);
            trajLine.SetPosition(i, p);
        }
    }

    void ClearLines()
    {
        if (dirLine) dirLine.positionCount = 0;
        if (trajLine) trajLine.positionCount = 0;
    }

    void UpdateForceText(float f)
    {
        if (forceText) forceText.text = $"Força: {f:0.0}";
    }
}