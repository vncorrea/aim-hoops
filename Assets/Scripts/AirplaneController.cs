using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    [Header("Sistema de Queda")]
    [SerializeField] private bool canBeHit = true;
    [SerializeField] private float fallSpeed = 10f;
    [SerializeField] private float fallRotationSpeed = 5f;
    [SerializeField] private float destroyAfterFall = 5f;
    
    [Header("Sistema de Vitória")]
    [SerializeField] private bool showVictoryMessage = true;
    [SerializeField] private float victoryDisplayTime = 3f; // tempo para mostrar "VOCÊ GANHOU!"
    [SerializeField] private string menuSceneName = "Menu"; // nome da cena do menu
    [SerializeField] private int menuSceneIndex = 0; // índice da cena do menu
    [SerializeField] private TMPro.TMP_Text gameOverText; // mesmo texto usado no TimerController
    [SerializeField] private GameObject gameOverPanel; // mesmo painel usado no TimerController
    
    private bool isMoving = false;
    private bool isFalling = false;
    private bool victoryShown = false;
    private Vector3 initialPosition;
    private Rigidbody rb;
    
    void Start()
    {
        // Salva a posição inicial
        initialPosition = transform.position;
        startPosition = initialPosition;
        
        // Pega o Rigidbody (adiciona se não existir)
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; // Começa kinematic para não cair
        }
        
        // Inicia o movimento se configurado
        if (moveOnStart)
        {
            StartMovement();
        }
    }
    
    void Update()
    {
        if (isFalling)
        {
            HandleFalling();
        }
        else if (isMoving)
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
    
    void HandleFalling()
    {
        // Adiciona rotação durante a queda
        transform.Rotate(Vector3.forward, fallRotationSpeed * Time.deltaTime);
        transform.Rotate(Vector3.right, fallRotationSpeed * 0.5f * Time.deltaTime);
    }
    
    void OnTriggerEnter(Collider other)
    {
        
        // Verifica se foi atingido pela bola de basquete
        if (canBeHit && other.CompareTag("Ball") && !isFalling)
        {
            HitByBall();
        }
        else
        {
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        
        // Verifica se foi atingido pela bola de basquete (colisão física)
        if (canBeHit && collision.gameObject.CompareTag("Ball") && !isFalling)
        {
            HitByBall();
        }
    }
    
    void HitByBall()
    {
        if (victoryShown) return; // Evita múltiplas execuções
        
        victoryShown = true;
        
        // Para o movimento normal
        isMoving = false;
        isFalling = true;
        
        // Ativa a física para o avião cair
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            
            // Adiciona um pouco de força para simular o impacto
            Vector3 impactForce = Vector3.down * fallSpeed + Random.insideUnitSphere * 2f;
            rb.AddForce(impactForce, ForceMode.Impulse);
        }
        
        // Mostra mensagem de vitória
        if (showVictoryMessage)
        {
            ShowVictoryMessage();
        }
        
        // Volta para o menu após um tempo
        Invoke(nameof(ReturnToMenu), victoryDisplayTime);
    }
    
    void ShowVictoryMessage()
    {
        
        // Usa o mesmo sistema do TimerController
        if (gameOverText != null)
        {
            gameOverText.text = "VOCÊ GANHOU!";
            gameOverText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameOverText não configurado no AirplaneController!");
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    void ReturnToMenu()
    {
        
        // Tenta carregar por nome primeiro
        if (!string.IsNullOrEmpty(menuSceneName))
        {
            try
            {
                SceneManager.LoadScene(menuSceneName);
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Não foi possível carregar a cena '{menuSceneName}': {e.Message}");
            }
        }
        
        // Se falhar, tenta carregar por índice
        try
        {
            SceneManager.LoadScene(menuSceneIndex);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Não foi possível carregar a cena com índice {menuSceneIndex}: {e.Message}");
            Debug.LogError("Verifique se as cenas estão adicionadas ao Build Settings!");
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
    
    // Métodos para controle da queda
    public void ForceFall()
    {
        if (!isFalling)
        {
            HitByBall();
        }
    }
    
    public void SetCanBeHit(bool canHit)
    {
        canBeHit = canHit;
    }
    
    public bool IsFalling()
    {
        return isFalling;
    }
    
    public void RespawnAirplane()
    {
        // Para a queda e reseta o avião
        isFalling = false;
        isMoving = false;
        victoryShown = false;
        
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Reseta posição e rotação
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
        
        // Reinicia o movimento
        if (moveOnStart)
        {
            StartMovement();
        }
    }
    
    // Método para testar manualmente (botão no Inspector)
    [ContextMenu("Test Hit By Ball")]
    public void TestHitByBall()
    {
        HitByBall();
    }
}
