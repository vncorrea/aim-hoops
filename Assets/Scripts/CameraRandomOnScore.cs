// Assets/Scripts/CameraRandomOnScore.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRandomOnScore : MonoBehaviour
{
    [Header("O que eu vou mover")]
    [Tooltip("Pode ser a Main Camera ou a Cinemachine VCam (transform).")]
    [SerializeField] private Transform cameraToMove;

    [Header("Para onde olhar")]
    [SerializeField] private Transform hoopTarget;

    [Header("Spots")]
    [Tooltip("Pai com os pontos (children) permitidos para a câmera.")]
    [SerializeField] private Transform spotsParent;

    [Header("Transição")]
    [SerializeField] private float moveSeconds = 0.75f;
    [SerializeField] private bool avoidRepeat = true;

    private readonly List<Transform> spots = new();
    private int lastIndex = -1;
    private Coroutine movingCo;

    void Awake()
    {
        if (!cameraToMove && Camera.main) cameraToMove = Camera.main.transform;

        spots.Clear();
        if (spotsParent)
        {
            foreach (Transform t in spotsParent)
                if (t != spotsParent) spots.Add(t);
        }
    }

    void OnEnable()  => GameManager.Scored += SwitchNow;
    void OnDisable() => GameManager.Scored -= SwitchNow;

    void SwitchNow()
    {
        if (!cameraToMove || !hoopTarget || spots.Count == 0) return;

        int idx = Random.Range(0, spots.Count);
        if (avoidRepeat && spots.Count > 1)
            while (idx == lastIndex) idx = Random.Range(0, spots.Count);

        lastIndex = idx;
        var spot = spots[idx];

        if (movingCo != null) StopCoroutine(movingCo);
        movingCo = StartCoroutine(MoveToSpot(spot.position));
    }

    IEnumerator MoveToSpot(Vector3 targetPos)
    {
        Vector3 startPos = cameraToMove.position;
        Quaternion startRot = cameraToMove.rotation;

        Quaternion targetRot = Quaternion.LookRotation(
            hoopTarget.position - targetPos, Vector3.up);

        if (moveSeconds <= 0f)
        {
            cameraToMove.SetPositionAndRotation(targetPos, targetRot);
            yield break;
        }

        float t = 0f;
        while (t < moveSeconds)
        {
            float u = t / moveSeconds;         // smoothstep
            u = u * u * (3f - 2f * u);

            cameraToMove.SetPositionAndRotation(
                Vector3.Lerp(startPos, targetPos, u),
                Quaternion.Slerp(startRot, targetRot, u));

            t += Time.deltaTime;
            yield return null;
        }

        cameraToMove.SetPositionAndRotation(targetPos, targetRot);
    }
}
