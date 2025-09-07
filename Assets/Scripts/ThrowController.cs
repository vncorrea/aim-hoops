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
    [SerializeField] private bool holdBallAtSpawn = true;

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

        ballRb.isKinematic = false;
        ballRb.useGravity = true;
        ballRb.constraints = RigidbodyConstraints.None; // libera pra física agir

        float force = Mathf.Lerp(minForce, maxForce, charge01);
        Shoot(force);
        UpdateForceText(0f);
    }

    void Shoot(float force)
    {
        shotInProgress = true;

        // Direção a partir dos sliders (yaw = esquerda/direita, pitch = inclinação)
        float yaw = yawSlider ? yawSlider.value : 0f;       // -45..45
        float pitch = pitchSlider ? pitchSlider.value : 35; // 15..60 padrão 35

        // Constrói direção rotacionando o vetor forward pela inclinação e yaw
        Quaternion rot = Quaternion.Euler(-pitch, yaw, 0f);
        Vector3 dir = rot * Vector3.forward;

        // Zera estado da bola e aplica impulso
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.AddForce(dir.normalized * force, ForceMode.Impulse);

        // Agenda um reset brando
        StartCoroutine(ResetAfterDelay());
    }

    IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(autoResetDelay);
        // Só reseta se a bola estiver quase parada ou muito longe
        if (ballRb.linearVelocity.magnitude < 0.3f || ballRb.transform.position.y < fallY)
        {
            RespawnBall();
        }
        shotInProgress = false;
    }

    void RespawnBall()
    {
        if (!ballRb || !ballSpawn) return;
        ballRb.isKinematic = true;
        ballRb.useGravity = false;
        ballRb.constraints = RigidbodyConstraints.FreezeRotation; // opcional: também FreezePosition para segurar 100%
        ballRb.transform.SetPositionAndRotation(ballSpawn.position, ballSpawn.rotation);
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
    }

    void UpdateForceText(float f)
    {
        if (forceText) forceText.text = $"Força: {f:0.0}";
    }
}