using UnityEngine;

public class BoloController : MonoBehaviour
{
    private GameManager gameManager;
    private bool yaContado = false;
    private Rigidbody rb;
    private float tiempoUltimaColision = 0f;
    private const float tiempoEsperaColision = 0.1f; // Evitar múltiples colisiones rápidas

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    void Update()
    {
        if (!yaContado && rb != null && !rb.isKinematic)
        {
            // Sistema mejorado de detección
            float productoPunto = Vector3.Dot(transform.up, Vector3.up);
            bool estaCaido = productoPunto < 0.6f; // Más de 45 grados
            bool estaEnElSuelo = transform.position.y < 0.2f;
            bool seEstaMoviendo = rb.linearVelocity.magnitude > 0.5f;

            if ((estaCaido || estaEnElSuelo) && Time.time > 1.0f) // Esperar 1 segundo después del lanzamiento
            {
                MarcarComoDerribado();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!yaContado && rb != null && !rb.isKinematic)
        {
            // PREVENIR MÚLTIPLES DETECCIONES RÁPIDAS
            if (Time.time - tiempoUltimaColision < tiempoEsperaColision)
                return;

            // Solo colisiones fuertes con bola u otros bolos
            bool esColisionFuerte = collision.relativeVelocity.magnitude > 1.0f;
            bool esConBolaOBolo = collision.gameObject.CompareTag("Bola") ||
                                 collision.gameObject.CompareTag("Bolo");

            if (esColisionFuerte && esConBolaOBolo)
            {
                tiempoUltimaColision = Time.time;
                MarcarComoDerribado();
            }
        }
    }

    private void MarcarComoDerribado()
    {
        if (!yaContado)
        {
            yaContado = true;
            Debug.Log($"=== BLO {gameObject.name} MARCADO CORRECTAMENTE ===");

            // Desactivar detecciones futuras
            if (TryGetComponent<Collider>(out var collider))
            {
                collider.enabled = false;
            }

            if (gameManager != null)
            {
                gameManager.BoloDerribado();
            }
        }
    }
}