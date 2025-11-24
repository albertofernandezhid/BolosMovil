using System.Xml.Linq;
using UnityEngine;

public class BoloController : MonoBehaviour
{
    private GameManager gameManager;
    private bool yaContado = false;
    private Rigidbody rb;
    private bool deteccionHabilitada = false;
    private float tiempoActivacion;
    private float ultimaColisionTiempo = 0f;
    private const float tiempoMinimoEntreColisiones = 0.5f; // Medio segundo entre detecciones

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindAnyObjectByType<GameManager>();
        deteccionHabilitada = false;
    }

    void Update()
    {
        if (!deteccionHabilitada && rb != null && !rb.isKinematic)
        {
            if (Time.time - tiempoActivacion > 2.0f)
            {
                deteccionHabilitada = true;
                Debug.Log($"?? Bolo {name} - Detección habilitada");
            }
            return;
        }

        if (!yaContado && deteccionHabilitada && rb != null && !rb.isKinematic)
        {
            // Sistema de detección por ángulo/posición (opcional)
            float productoPunto = Vector3.Dot(transform.up, Vector3.up);
            bool estaCaido = productoPunto < 0.3f;
            bool estaEnElSuelo = transform.position.y < 0.15f;
            bool seEstaMoviendo = rb.linearVelocity.magnitude > 0.5f;

            if (estaEnElSuelo && seEstaMoviendo && Time.time - ultimaColisionTiempo > tiempoMinimoEntreColisiones)
            {
                Debug.Log($"?? Bolo {name} detectado por posición");
                MarcarComoDerribado();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!yaContado && deteccionHabilitada && rb != null && !rb.isKinematic)
        {
            Debug.Log($"?? COLISIÓN - Bolo: {name}, Con: {collision.gameObject.name}, Fuerza: {collision.relativeVelocity.magnitude:F2}");

            if (Time.time - ultimaColisionTiempo < tiempoMinimoEntreColisiones)
            {
                Debug.Log($"? COLISIÓN IGNORADA - Demasiado pronto");
                return;
            }

            if (collision.relativeVelocity.magnitude > 3.0f)
            {
                ultimaColisionTiempo = Time.time;
                Debug.Log($"?? COLISIÓN ACEPTADA");
                MarcarComoDerribado();
            }
        }
    }

    public void OnBoloActivado()
    {
        tiempoActivacion = Time.time;
    }

    private void MarcarComoDerribado()
    {
        if (!yaContado)
        {
            yaContado = true;
            Debug.Log($"? BLO {gameObject.name} MARCADO CORRECTAMENTE");

            // ? DESACTIVAR MÁS COMPONENTES PARA PREVENIR DETECCIÓN FUTURA
            if (TryGetComponent<Collider>(out var collider))
            {
                collider.enabled = false;
            }

            // ? Cambiar layer para evitar más colisiones
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            if (gameManager != null)
            {
                gameManager.BoloDerribado();
            }
        }
    }
}