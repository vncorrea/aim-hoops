// Assets/Scripts/ScoreZone.cs
using UnityEngine;
using System.Collections;

public class ScoreZone : MonoBehaviour
{
    [SerializeField] private float bringBackDelay = 0.8f; // espera o blend da câmera

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball")) return;
        var rb = other.attachedRigidbody;
        if (rb && rb.linearVelocity.y < 0f)
        {
            GameManager.Instance?.AddPoint(1);
            var ctrl = FindObjectOfType<ThrowController>();
            if (ctrl) StartCoroutine(ReturnBall(ctrl, 0.8f));
        }
    }


    private IEnumerator ReturnBall(ThrowController ctrl, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        ctrl.RespawnBall();
    }
}
