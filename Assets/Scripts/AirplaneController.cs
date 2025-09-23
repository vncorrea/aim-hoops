using UnityEngine;

public class AirplaneController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float speed = 60f;
    [SerializeField] private Vector3 direction = Vector3.forward;
    [SerializeField] private bool moveOnStart = true;
    
    [Header("Loop")]
    [SerializeField] private bool loopMovement = true;
    [SerializeField] private float resetDistance = 100000000f;
    [SerializeField] private Vector3 startPosition;
    
    [Header("Rotação")]
    [SerializeField] private bool rotateWithMovement = true;
    [SerializeField] private float rotationSpeed = 2f;
    
    private bool isMoving = false;
    private Vector3 initialPosition;
    
    void Start()
    {
        // Salva a posição inicial
        initialPosition = transform.position;
        startPosition = initialPosition;
        
        // Inicia o movimento se configurado
        if (moveOnStart)
        {
            StartMovement();
        }
    }
    
    void Update()
    {
        if (isMoving)
        {
            MoveForward();
            
            if (loopMovement)
            {
                CheckForReset();
            }
        }
    }
    
    void MoveForward()
    {
        // Move o avião na direção especificada
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
        
        // Rotaciona o avião para seguir a direção do movimento
        if (rotateWithMovement)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    void CheckForReset()
    {
        // Verifica se o avião se afastou muito da posição inicial
        float distanceFromStart = Vector3.Distance(transform.position, startPosition);
        
        if (distanceFromStart > resetDistance)
        {
            // Reseta para a posição inicial
            transform.position = startPosition;
        }
    }
    
    // Métodos públicos para controle
    public void StartMovement()
    {
        isMoving = true;
    }
    
    public void StopMovement()
    {
        isMoving = false;
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection.normalized;
    }
    
    public void ResetToStart()
    {
        transform.position = startPosition;
    }
    
    // Método para configurar o avião para voar em uma direção específica
    public void SetFlightPath(Vector3 startPos, Vector3 endPos, float flightSpeed)
    {
        startPosition = startPos;
        direction = (endPos - startPos).normalized;
        speed = flightSpeed;
        transform.position = startPos;
    }
}
