using UnityEngine;

public class BolaFisicaAvanzada : MonoBehaviour
{
    private Rigidbody rb;

    [Header("AJUSTES DE FÍSICA Y VELOCIDAD")]
    public float velocidadMaxEstabilidad = 30.0f; // Máx estabilidad (Rectitud)
    public float velocidadMaxDrift = 4.0f;      // Umbral para empezar el Drift
    public float driftFuerzaBase = 0.8f;       // Fuerza lateral del Drift
    public float deadZoneX = 0.25f;         // Zona central donde se aplica el Drift Aleatorio

    [Header("CONTROL DE BARRERAS")]
    [Tooltip("Arrastra aquí el objeto padre de las barreras laterales.")]
    public GameObject barrerasLaterales;

    private bool usarBarreras = false;
    // Estado interno para la dirección aleatoria (-1 Izq, 1 Der, 0 Sin fijar)
    private int driftDireccionAleatoria = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("El script 'BolaFisicaAvanzada' requiere un Rigidbody en el mismo objeto.");
            enabled = false;
        }

        // --- LÓGICA DE RANDOMIZACIÓN ---
        // Si la bola inicia en la deadZoneX, fijamos una dirección aleatoria para el drift.
        if (Mathf.Abs(transform.position.x) < deadZoneX)
        {
            // Random.Range(0, 2) devuelve 0 o 1
            driftDireccionAleatoria = (Random.Range(0, 2) == 0) ? -1 : 1;
            Debug.Log($"Drift aleatorio fijado a: {(driftDireccionAleatoria == 1 ? "Derecha" : "Izquierda")}");
        }

        // Aseguramos que el estado inicial de las barreras coincida con la variable.
        if (barrerasLaterales != null)
        {
            barrerasLaterales.SetActive(usarBarreras);
        }
    }

    void FixedUpdate()
    {
        if (rb == null || rb.isKinematic) return;

        float velocidadActual = rb.linearVelocity.magnitude;

        ManejarEstabilidad(velocidadActual);
        ManejarDrift(velocidadActual);
    }

    // --- LÓGICA DE ESTABILIDAD (DRAG ANGULAR) ---
    void ManejarEstabilidad(float velocidad)
    {
        float t = Mathf.Clamp01(velocidad / velocidadMaxEstabilidad);
        rb.angularDamping = Mathf.Lerp(maxAngularDrag, minAngularDrag, t);
    }

    // --- LÓGICA DE DRIFT Y BARRERAS ---
    void ManejarDrift(float velocidad)
    {
        if (velocidad > velocidadMaxDrift) return;

        // 1. Si las barreras están activas, NO aplicamos el drift forzado. 
        // La colisión física se encarga de que siga la pista.
        if (usarBarreras)
        {
            return;
        }

        // 2. Aplicamos Drift si no hay barreras y va lento.
        float xPos = transform.position.x;
        Vector3 direccionDrift;

        if (Mathf.Abs(xPos) < deadZoneX)
        {
            // CENTRO: Usamos la dirección aleatoria fijada al inicio.
            direccionDrift = (driftDireccionAleatoria > 0) ? Vector3.right : Vector3.left;
        }
        else
        {
            // LATERAL: Usamos el lateral más cercano (comportamiento de gutter).
            direccionDrift = (xPos > 0) ? Vector3.right : Vector3.left;
        }

        // Aplicar fuerza (proporcional a qué tan lento va)
        float factorLentitud = 1f - Mathf.Clamp01(velocidad / velocidadMaxDrift);
        float fuerzaLateral = driftFuerzaBase * factorLentitud;

        rb.AddForce(direccionDrift * fuerzaLateral, ForceMode.Acceleration);
    }

    // --- MÉTODO PÚBLICO PARA EL BOTÓN UI ---
    [ContextMenu("Toggle Barreras")] // Permite probarlo directamente desde el Inspector
    public void ToggleBarreras()
    {
        usarBarreras = !usarBarreras;
        if (barrerasLaterales != null)
        {
            barrerasLaterales.SetActive(usarBarreras);
            Debug.Log($"Barreras laterales {(usarBarreras ? "ACTIVADAS" : "DESACTIVADAS")}");
        }
        else
        {
            Debug.LogWarning("El objeto 'barrerasLaterales' no está asignado. No se puede activar/desactivar.");
        }
    }
}