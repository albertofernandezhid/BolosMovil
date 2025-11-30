using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallPhysicsController : MonoBehaviour
{
    private Rigidbody rb;
    private GameManager gameManager;

    [Header("AJUSTES DE FÍSICA Y VELOCIDAD")]
    public float velocidadMaxEstabilidad = 35.0f;
    public float velocidadMaxDrift = 8.0f;
    public float velocidadMinEfecto = 15.0f;
    public float deadZoneX = 0.25f;

    [Header("FUERZAS BASE (Serán escaladas por la Masa)")]
    public float driftFuerzaBase = 35.0f;
    public float fuerzaEfectoCurvaBase = 4.0f;
    public float fuerzaEmpujeBarrerasBase = 10.0f;

    [Header("ESTABILIDAD (Rectitud)")]
    public float velocidadOptimaEstabilidad = 12.0f;
    public float factorEstabilidadMax = 0.9f;
    public float factorEstabilidadMin = 0.1f;

    [Header("CONTROL DE BARRERAS")]
    private const float MASA_BASE_CALIBRACION = 5.0f;
    private const float PISTA_ANCHO_X = 1.5f;
    private const float MAX_X_BOUNDARY = PISTA_ANCHO_X / 2f;
    private const float MIN_DRIFT_FACTOR = 0.2f;

    private int driftDireccionAleatoria = 0;
    private float driftFactorPorMasa = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager == null) Debug.LogError("BallPhysicsController no encontró GameManager.");

        if (Mathf.Abs(transform.position.x) < deadZoneX)
        {
            driftDireccionAleatoria = (Random.Range(0, 2) == 0) ? -1 : 1;
        }

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            driftFactorPorMasa = MASA_BASE_CALIBRACION / rb.mass;
        }
    }

    void FixedUpdate()
    {
        if (rb == null || rb.isKinematic) return;

        float velocidadActual = rb.linearVelocity.magnitude;

        if (gameManager != null && gameManager.UsarBarreras)
        {
            ManejarEstabilidad(velocidadActual);
            ManejarBarreras();
            return;
        }

        ManejarEstabilidad(velocidadActual);
        ManejarDrift(velocidadActual);
    }

    void ManejarBarreras()
    {
        float masaActual = rb.mass;
        float factorEscaladoBarrera = masaActual / MASA_BASE_CALIBRACION;
        float fuerzaEmpujeBarrerasEscalada = fuerzaEmpujeBarrerasBase * factorEscaladoBarrera;

        if (rb.linearVelocity.z < 1.0f)
        {
            rb.AddForce(Vector3.forward * fuerzaEmpujeBarrerasEscalada, ForceMode.Acceleration);
        }
    }

    void ManejarEstabilidad(float velocidad)
    {
        float vNorm = Mathf.Abs(velocidad - velocidadOptimaEstabilidad);
        float t = Mathf.Clamp01(vNorm / velocidadOptimaEstabilidad);

        float angularDamping = Mathf.Lerp(factorEstabilidadMin, factorEstabilidadMax, t);

        rb.angularDamping = angularDamping;

        float factorFuerzaEstabilidad = 1f - Mathf.Clamp01(velocidad / velocidadMaxEstabilidad);
        float fuerzaEstabilidad = 5f * factorFuerzaEstabilidad * rb.mass;

        rb.AddForce(new Vector3(-rb.linearVelocity.x, 0, 0) * fuerzaEstabilidad * Time.fixedDeltaTime, ForceMode.Acceleration);
    }

    void ManejarDrift(float velocidad)
    {
        float xPos = transform.position.x;
        int direccionHorizontal;
        Vector3 direccionCurva;

        if (velocidad >= velocidadMinEfecto)
        {
            if (Mathf.Abs(xPos) < deadZoneX)
            {
                direccionHorizontal = driftDireccionAleatoria;
            }
            else
            {
                direccionHorizontal = (xPos > 0) ? -1 : 1;
            }

            direccionCurva = (direccionHorizontal > 0) ? Vector3.right : Vector3.left;

            float fuerzaCurvaEscalada = fuerzaEfectoCurvaBase * driftFactorPorMasa;
            rb.AddForce(direccionCurva * fuerzaCurvaEscalada, ForceMode.Acceleration);
            return;
        }

        if (velocidad > velocidadMaxDrift && velocidad < velocidadMinEfecto)
        {
            return;
        }

        if (velocidad <= velocidadMaxDrift)
        {
            if (Mathf.Abs(xPos) < deadZoneX)
            {
                direccionHorizontal = driftDireccionAleatoria;
            }
            else
            {
                direccionHorizontal = (xPos > 0) ? 1 : -1;
            }

            direccionCurva = (direccionHorizontal > 0) ? Vector3.right : Vector3.left;

            float factorLentitud = 1f - Mathf.Clamp01(velocidad / velocidadMaxDrift);
            float distanciaNormalizada = Mathf.Clamp01(Mathf.Abs(xPos) / MAX_X_BOUNDARY);
            float factorDistanciaAjustado = MIN_DRIFT_FACTOR + (1.0f - MIN_DRIFT_FACTOR) * distanciaNormalizada;

            float fuerzaLateral = driftFuerzaBase * driftFactorPorMasa * factorLentitud * factorDistanciaAjustado;

            rb.AddForce(direccionCurva * fuerzaLateral, ForceMode.Acceleration);
        }
    }
}