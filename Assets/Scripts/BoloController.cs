using UnityEngine;

public class BoloController : MonoBehaviour
{
    private GameManager gameManager;
    private bool yaContado = false;
    private Rigidbody rb;
    private bool deteccionHabilitada = false;
    private float tiempoActivacion;
    private float ultimaColisionTiempo = 0f;
    private const float tiempoMinimoEntreColisiones = 0.5f;

    public bool FueDerribado => yaContado;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindFirstObjectByType<GameManager>();
        deteccionHabilitada = false;

        // Aseguramos detección continua para evitar que atraviesen el suelo a alta velocidad
        if (rb != null) rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Update()
    {
        // Si ya fue contado, NO hacemos return, solo dejamos de verificar si se cayó.
        // Pero permitimos que la física siga su curso.
        if (yaContado) return;

        if (!deteccionHabilitada && rb != null && !rb.isKinematic)
        {
            if (Time.time - tiempoActivacion > 2.0f)
            {
                deteccionHabilitada = true;
            }
            return;
        }

        if (deteccionHabilitada && rb != null && !rb.isKinematic)
        {
            VerificarSiEstaDerribado();
        }
    }

    void VerificarSiEstaDerribado()
    {
        float productoPunto = Vector3.Dot(transform.up, Vector3.up);
        bool estaInclinado = productoPunto < 0.5f;

        bool estaEnElSuelo = transform.position.y < 0.2f;
        bool seEstaMoviendo = rb.linearVelocity.magnitude > 0.1f;

        if (estaInclinado && Time.time - tiempoActivacion > 1.0f)
        {
            MarcarComoDerribado();
            return;
        }

        if (estaEnElSuelo && seEstaMoviendo && Time.time - ultimaColisionTiempo > tiempoMinimoEntreColisiones)
        {
            MarcarComoDerribado();
            return;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Aunque ya esté contado, permitimos colisiones físicas, 
        // pero la lógica de puntuación se detiene gracias al if(yaContado) en MarcarComoDerribado.

        if (!deteccionHabilitada && rb != null && !rb.isKinematic)
        {
            if (collision.relativeVelocity.magnitude > 3.0f)
            {
                deteccionHabilitada = true;
            }
        }

        if (deteccionHabilitada && rb != null && !rb.isKinematic)
        {
            if (Time.time - ultimaColisionTiempo < tiempoMinimoEntreColisiones) return;

            if (collision.relativeVelocity.magnitude > 3.0f)
            {
                ultimaColisionTiempo = Time.time;
                MarcarComoDerribado();
            }
            else
            {
                ultimaColisionTiempo = Time.time;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (yaContado) return;

        if ((other.gameObject.CompareTag("FueraPista") || other == gameManager?.colliderFueraPista))
        {
            MarcarComoDerribado();
        }
    }

    public void OnBoloActivado()
    {
        tiempoActivacion = Time.time;
    }

    private void MarcarComoDerribado()
    {
        // Esta comprobación evita que se sumen puntos infinitos
        if (!yaContado && gameManager != null)
        {
            yaContado = true;

            // --- CAMBIO IMPORTANTE ---
            // ELIMINADA la línea: collider.enabled = false;
            // Esto asegura que el bolo siga chocando con el suelo y otros bolos.

            // Opcional: Cambiamos la layer para que Raycasts (como el del ratón) lo ignoren, 
            // pero la física sigue actuando.
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            gameManager.BoloDerribado();
            Debug.Log($"Bolo {name} marcado como derribado.");
        }
    }

    // FixedUpdate eliminado ya que la amortiguación manual podía causar 
    // comportamientos extraños con la física de Unity.
}