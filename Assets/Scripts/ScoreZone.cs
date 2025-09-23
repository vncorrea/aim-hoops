// Assets/Scripts/ScoreZone.cs
using UnityEngine;
using System.Collections;

public class ScoreZone : MonoBehaviour
{
    [SerializeField] private float bringBackDelay = 0.8f; // espera o blend da câmera
    [SerializeField] private float minHeightAboveHoop = 0.05f; // altura mínima acima da cesta para contar ponto

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball")) return;
        var rb = other.attachedRigidbody;
        if (rb && IsValidScore(rb))
        {
            GameManager.Instance?.AddPoint(1);
            var ctrl = FindObjectOfType<ThrowController>();
            if (ctrl) StartCoroutine(ReturnBall(ctrl, 0.8f));
        }
    }
    
    private bool IsValidScore(Rigidbody ballRb)
    {
        // Única verificação: A bola deve estar descendo (velocidade Y negativa)
        if (ballRb.linearVelocity.y >= 0f) 
        {
            Debug.Log($"ScoreZone: Bola subindo (velY: {ballRb.linearVelocity.y:F2}) - não conta");
            return false;
        }
        
        // Se chegou até aqui, a bola está descendo = ponto válido!
        Debug.Log($"ScoreZone: PONTO VÁLIDO! Bola descendo (velY: {ballRb.linearVelocity.y:F2})");
        return true;
    }


    private IEnumerator ReturnBall(ThrowController ctrl, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        ctrl.RespawnBall();
    }
}
