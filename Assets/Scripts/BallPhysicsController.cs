using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallPhysicsController : MonoBehaviour
{
    private Rigidbody rb;
    private GameManager gameManager;

    [Header("AJUSTES DE FÍSICA Y VELOCIDAD")]
    public float velocidadMaxEstabilidad = 30.0f;
    // Umbral de velocidad para iniciar el drift hacia los laterales
    public float velocidadMaxDrift = 3.0f;
    // Fuerza máxima de empuje lateral cuando la bola es lenta (ajustar a 8.0f o más)
    public float driftFuerzaBase = 8.0f;
    public float deadZoneX = 0.25f;

    [Header("ESTABILIDAD (Rectitud)")]
    public float maxAngularDamping = 0.8f;
    public float minAngularDamping = 0.1f;

    [Header("DRIFT DE EFECTO (Curva)")]
    // Umbral a partir del cual se aplica el drift de curva (lanzamiento muy fuerte)
    public float velocidadMinEfecto = 15.0f;
    // Fuerza lateral constante para el efecto de curva
    public float fuerzaEfectoCurva = 0.5f;

    [Header("CONTROL DE BARRERAS")]
    // Fuerza mínima para empujar la bola hacia adelante si las barreras están activas y la bola es lenta
    public float fuerzaEmpujeBarreras = 5.0f; // Aumentado para asegurar inercia

    // CONSTANTES PRIVADAS
    // Borde máximo de la pista para normalizar la distancia (desde BallLauncher)
    private const float MAX_X_BOUNDARY = 0.6f;
    // Factor mínimo de drift aplicado (incluso en el centro de la pista)
    private const float MIN_DRIFT_FACTOR = 0.2f;

    private int driftDireccionAleatoria = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager == null) Debug.LogError("BallPhysicsController no encontró GameManager.");

        // Inicializa una dirección lateral aleatoria si la bola empieza en el centro
        if (Mathf.Abs(transform.position.x) < deadZoneX)
        {
            driftDireccionAleatoria = (Random.Range(0, 2) == 0) ? -1 : 1;
        }

        // Corrección del problema de "tirones"
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void FixedUpdate()
    {
        if (rb == null || rb.isKinematic) return;

        float velocidadActual = rb.linearVelocity.magnitude;

        // --- LÓGICA DE BARRERAS (PRIORIDAD ALTA) ---
        if (gameManager != null && gameManager.UsarBarreras)
        {
            // Aplicamos estabilidad
            ManejarEstabilidad(velocidadActual);

            // Empuje de inercia: Si la velocidad Z es muy baja, la empujamos hacia adelante
            // **Este empuje asegura que la bola siga rodando por inercia junto a la barrera.**
            if (rb.linearVelocity.z < 1.0f)
            {
                rb.AddForce(Vector3.forward * fuerzaEmpujeBarreras, ForceMode.Acceleration);
            }
            return; // No aplicamos ningún drift lateral (ni bajo, ni efecto curva).
        }
        // ------------------------------------------

        // Si las barreras están DESACTIVADAS, aplicamos toda la física normal:
        ManejarEstabilidad(velocidadActual);
        ManejarDrift(velocidadActual);
    }

    void ManejarEstabilidad(float velocidad)
    {
        float t = Mathf.Clamp01(velocidad / velocidadMaxEstabilidad);
        rb.angularDamping = Mathf.Lerp(maxAngularDamping, minAngularDamping, t);
    }

    void ManejarDrift(float velocidad)
    {
        // 1. --- ZONA DE VELOCIDAD ALTA (DRIFT DE EFECTO) ---
        if (velocidad >= velocidadMinEfecto)
        {
            // Aplicar el efecto curva constante (ej: fijo a la derecha para un 'hook')
            Vector3 direccionEfecto = Vector3.right;
            rb.AddForce(direccionEfecto * fuerzaEfectoCurva, ForceMode.Acceleration);
            return;
        }

        // 2. --- ZONA DE VELOCIDAD MEDIA (RECTO) ---
        if (velocidad > velocidadMaxDrift && velocidad < velocidadMinEfecto)
        {
            return;
        }

        // 3. --- ZONA DE VELOCIDAD BAJA (DRIFT A CANAL LATERAL) ---
        if (velocidad <= velocidadMaxDrift)
        {
            float xPos = transform.position.x;
            Vector3 direccionDrift;

            // Determinar la dirección de drift (aleatorio en el centro, forzado al borde fuera)
            if (Mathf.Abs(xPos) < deadZoneX)
            {
                direccionDrift = (driftDireccionAleatoria > 0) ? Vector3.right : Vector3.left;
            }
            else
            {
                direccionDrift = (xPos > 0) ? Vector3.right : Vector3.left;
            }

            // --- CÁLCULO DE FUERZA PROPORCIONAL A LA DISTANCIA (NUEVO) ---

            // Factor basado en la lentitud de la bola.
            float factorLentitud = 1f - Mathf.Clamp01(velocidad / velocidadMaxDrift);

            // Factor basado en la distancia al centro (0.0 en el centro, 1.0 en el borde)
            float distanciaNormalizada = Mathf.Clamp01(Mathf.Abs(xPos) / MAX_X_BOUNDARY);

            // Aplicamos un factor mínimo (MIN_DRIFT_FACTOR = 0.2f) para que siempre haya drift en el centro,
            // y luego escalamos el resto de la fuerza (0.8f) por la distancia.
            float factorDistanciaAjustado = MIN_DRIFT_FACTOR + (1.0f - MIN_DRIFT_FACTOR) * distanciaNormalizada;

            // Fuerza final: fuerza base * factor de lentitud * factor de distancia ajustado
            float fuerzaLateral = driftFuerzaBase * factorLentitud * factorDistanciaAjustado;

            rb.AddForce(direccionDrift * fuerzaLateral, ForceMode.Acceleration);
        }
    }
}