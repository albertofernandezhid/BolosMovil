using UnityEngine;

public class BoloController : MonoBehaviour
{
    private GameManager gameManager;
    private Rigidbody rb;
    private bool yaContado = false;
    private bool deteccionHabilitada = false;

    // Constante para la detección de inclinación
    public float umbralInclinacion = 0.707f; // Equivalente a 45 grados

    public bool FueDerribado => yaContado;

    // Función de inicialización llamada desde PinManager
    public void Inicializar(GameManager gm)
    {
        gameManager = gm;
        rb = GetComponent<Rigidbody>();
    }

    public void OnBoloActivado()
    {
        // Habilita la detección después de un breve periodo para evitar caídas iniciales
        Invoke(nameof(HabilitarDeteccion), 0.5f);
    }

    void HabilitarDeteccion()
    {
        deteccionHabilitada = true;
    }

    // Llamado por PinManager
    public void VerificarInclinacion()
    {
        if (yaContado) return;

        if (!deteccionHabilitada || rb == null || rb.isKinematic) return;

        float inclinacion = Vector3.Dot(Vector3.up, transform.up);

        if (inclinacion < umbralInclinacion)
        {
            RegistrarCaida("Por Inclinación (> " + (90 - Mathf.Acos(umbralInclinacion) * Mathf.Rad2Deg).ToString("F0") + "º)");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (yaContado) return;

        // Verifica si choca con el suelo "Fuera de Pista"
        if (gameManager != null && other == gameManager.colliderFueraPista)
        {
            RegistrarCaida("Por Salida de Pista");
        }
    }

    private void RegistrarCaida(string motivo)
    {
        if (yaContado) return;

        yaContado = true;

        if (gameManager != null)
        {
            gameManager.BoloDerribado();
        }
    }
}