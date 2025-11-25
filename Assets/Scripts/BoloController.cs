using UnityEngine;

public class BoloController : MonoBehaviour
{
    private GameManager gameManager;
    private Rigidbody rb;

    // ESTADO
    private bool yaContado = false; // "Cerrojo" interno

    // --- ESTA ES LA LÍNEA QUE FALTABA PARA QUE GAMEMANAGER NO DE ERROR ---
    public bool FueDerribado => yaContado;
    // --------------------------------------------------------------------

    private bool deteccionHabilitada = false;
    private float tiempoActivacion;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindFirstObjectByType<GameManager>();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    public void OnBoloActivado()
    {
        tiempoActivacion = Time.time;
        Invoke(nameof(HabilitarDeteccion), 0.5f);
    }

    void HabilitarDeteccion()
    {
        deteccionHabilitada = true;
    }

    void Update()
    {
        if (yaContado) return;

        if (!deteccionHabilitada || rb == null || rb.isKinematic) return;

        VerificarInclinacion();
    }

    void VerificarInclinacion()
    {
        // 0.707f equivale a 45 grados
        float inclinacion = Vector3.Dot(Vector3.up, transform.up);

        if (inclinacion < 0.707f)
        {
            RegistrarCaida("Por Inclinación (> 45º)");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (yaContado) return;

        // Verifica si choca con el suelo "Fuera de Pista" definido en GameManager
        if (gameManager != null && other == gameManager.colliderFueraPista)
        {
            RegistrarCaida("Por Salida de Pista");
        }
    }

    private void RegistrarCaida(string motivo)
    {
        if (yaContado) return;

        yaContado = true; // Bloqueamos para que no cuente dos veces

        Debug.Log($"Bolo derribado! Motivo: {motivo}");

        if (gameManager != null)
        {
            gameManager.BoloDerribado();
        }

        // No desactivamos el collider para mantener la física realista
    }
}