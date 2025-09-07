// Assets/Scripts/ScoreZone.cs
using UnityEngine;

public class ScoreZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball")) return;

        var rb = other.attachedRigidbody;
        if (rb != null && rb.linearVelocity.y < 0f)
        {
            GameManager.Instance?.AddPoint(1);
        }
    }
}