// Assets/Scripts/CamRigTarget.cs
using UnityEngine;

public class CamRigTarget : MonoBehaviour
{
    [SerializeField] Transform ball;
    [SerializeField] Transform hoopTarget;
    [SerializeField] float back = 3.5f;
    [SerializeField] float height = 1.6f;

    void LateUpdate()
    {
        if (!ball || !hoopTarget) return;

        // Direção só no plano XZ (sem componente Y)
        Vector3 toHoop = hoopTarget.position - ball.position;
        Vector3 flatDir = Vector3.ProjectOnPlane(toHoop, Vector3.up).normalized;
        if (flatDir.sqrMagnitude < 1e-4f) flatDir = Vector3.forward; // fallback

        // "Atrás" da bola em relação ao aro, + altura
        Vector3 pos = ball.position - flatDir * back + Vector3.up * height;
        transform.position = pos;

        // Olha pro aro (opcional, a vcam usa LookAt de qualquer forma)
        transform.rotation = Quaternion.LookRotation(hoopTarget.position - pos, Vector3.up);
    }
}