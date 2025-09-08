// Assets/Scripts/ThrowController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ThrowController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Slider yawSlider;    // -45..45 (graus)
    [SerializeField] private Slider pitchSlider;  // 15..60 (graus)
    [SerializeField] private ChargeButton chargeButton;
    [SerializeField] private Rigidbody ballRb;
    [SerializeField] private Transform ballSpawn;

    [Header("Força")]
    [SerializeField] private float minForce = 6f;
    [SerializeField] private float maxForce = 22f;
    [SerializeField] private float chargeTime = 1.4f; // seg para ir 0→100%
    [SerializeField] private TMP_Text forceText;      // opcional

    [Header("Reset")]
    [SerializeField] private float autoResetDelay = 4.0f;
    [SerializeField] private float fallY = -5f;

    [SerializeField] private Transform hoopTarget;  // arraste o HoopTarget no Inspector
    [SerializeField] private bool invertYaw = false;

    private float charge01; // 0..1
    private bool charging;
    private bool shotInProgress;

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
        // Se a bola cair muito, reseta.
        if (ballRb && ballRb.transform.position.y < fallY && !charging)
        {
            RespawnBall();
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

        // Usa o charge atual (NÃO zera antes!)
        float force = Mathf.Lerp(minForce, maxForce, charge01);

        // Prepara o corpo para receber o impulso
        ballRb.isKinematic = false;
        ballRb.useGravity = true;
        ballRb.constraints = RigidbodyConstraints.None;

        // Calcula direção: base olhando pro aro no plano XZ, com ajustes de yaw/pitch
        float yaw = yawSlider ? yawSlider.value : 0f;
        float pitch = pitchSlider ? pitchSlider.value : 35f;

        Vector3 toHoopFlat = hoopTarget
            ? Vector3.ProjectOnPlane(hoopTarget.position - ballRb.position, Vector3.up).normalized
            : Vector3.forward;

        if (toHoopFlat.sqrMagnitude < 1e-6f) toHoopFlat = Vector3.forward;

        float yawUsed = invertYaw ? -yaw : yaw;
        Quaternion rot = Quaternion.LookRotation(toHoopFlat, Vector3.up) * Quaternion.Euler(-pitch, yawUsed, 0f);
        Vector3 dir = rot * Vector3.forward;

        // (opcional) reposiciona no spawn para tiros consistentes
        ballRb.transform.SetPositionAndRotation(ballSpawn.position, ballSpawn.rotation);

        // Zera velocidades (agora pode, pois não é kinematic)
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        // Impulso
        Debug.DrawRay(ballRb.position, dir.normalized * 3f, Color.red, 2f);
        ballRb.AddForce(dir.normalized * force, ForceMode.Impulse);

        // Limpa UI/estado
        UpdateForceText(0f);
        charge01 = 0f;

        // Agenda reset
        StartCoroutine(ResetAfterDelay());
    }

    IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(autoResetDelay);

        // Só reseta se a bola estiver quase parada ou tiver caído
        if (!ballRb) yield break;

        if (ballRb.linearVelocity.magnitude < 0.3f || ballRb.transform.position.y < fallY)
        {
            RespawnBall();
        }
        shotInProgress = false;
    }

    void RespawnBall()
    {
        if (!ballRb || !ballSpawn) return;

        // Garanta que NÃO está kinematic antes de mexer em velocidades
        ballRb.isKinematic = false;
        ballRb.useGravity = false;
        ballRb.constraints = RigidbodyConstraints.FreezeRotation;

        // Zera velocidades
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        // Move para o ponto de spawn
        ballRb.transform.SetPositionAndRotation(ballSpawn.position, ballSpawn.rotation);

        // Agora sim, deixa kinematic para "ficar paradinha"
        ballRb.isKinematic = true;
        ballRb.Sleep(); // opcional, garante repouso
    }

    void UpdateForceText(float f)
    {
        if (forceText) forceText.text = $"Força: {f:0.0}";
    }
}