using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Necesario para Linq

public class PinManager : MonoBehaviour
{
    [Header("REFERENCIAS")]
    public GameManager gameManager;
    public GameObject boloPrefab;

    [Header("AJUSTES FÍSICA - BOLOS")]
    public float masaBolo = 2f;
    public float linearDampingBolo = 0.3f;
    public float angularDampingBolo = 0.1f;
    [Tooltip("Coordenadas locales. Valores más positivos en Y = más inestables")]
    public Vector3 centroMasaBolo = new(0f, 0.4f, 0.1f);

    [Header("CONFIGURACIÓN BOLOS - POSICIÓN")]
    public Vector3[] posicionesBolos = new Vector3[10]
    {
        new(0f, 0.61f, 19f),
        new(-0.2f, 0.61f, 19.3f),
        new(0.2f, 0.61f, 19.3f),
        new(-0.4f, 0.61f, 19.6f),
        new(0f, 0.61f, 19.6f),
        new(0.4f, 0.61f, 19.6f),
        new(-0.6f, 0.61f, 19.9f),
        new(-0.2f, 0.61f, 19.9f),
        new(0.2f, 0.61f, 19.9f),
        new(0.6f, 0.61f, 19.9f)
    };
    public Vector3 rotacionInicialBolos = new Vector3(0f, 0f, 0f);

    // ESTADO INTERNO
    private readonly List<GameObject> bolos = new();
    private readonly List<Rigidbody> bolosRb = new();
    private readonly List<BoloController> boloControllers = new();

    // --- REINICIO Y CREACIÓN ---
    public void ReiniciarBolos()
    {
        foreach (var bolo in bolos)
        {
            if (bolo != null) Destroy(bolo);
        }
        bolos.Clear();
        bolosRb.Clear();
        boloControllers.Clear();

        CrearBolos();
    }

    void CrearBolos()
    {
        for (int i = 0; i < posicionesBolos.Length; i++)
        {
            GameObject boloObj = Instantiate(boloPrefab, posicionesBolos[i], Quaternion.Euler(rotacionInicialBolos));
            boloObj.transform.localScale = Vector3.one;

            ConfigurarFisicaBolo(boloObj);
            bolos.Add(boloObj);

            BoloController controller = boloObj.GetComponent<BoloController>();
            if (controller != null)
            {
                boloControllers.Add(controller);
                controller.Inicializar(gameManager); // Asigna el GameManager
            }
        }
        // Inicialmente, están Kinematic. Se activan al lanzar la bola.
    }

    void ConfigurarFisicaBolo(GameObject bolo)
    {
        Rigidbody rb = bolo.GetComponent<Rigidbody>();
        Collider collider = bolo.GetComponent<Collider>();

        if (rb != null)
        {
            rb.isKinematic = true; // Inicia desactivado
            rb.mass = masaBolo;
            rb.linearDamping = linearDampingBolo;
            rb.angularDamping = angularDampingBolo;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Centro de masa
            rb.centerOfMass = centroMasaBolo;
            rb.maxAngularVelocity = 100f;
            rb.sleepThreshold = 0.005f;

            if (collider != null && collider is MeshCollider meshCollider)
            {
                meshCollider.convex = true;
            }

            bolosRb.Add(rb);
        }
    }

    // --- ACTIVACIÓN ---
    public void ActivarFisicaBolos()
    {
        // Activamos la física con un pequeño retraso para evitar problemas de Start()
        StartCoroutine(ActivarBolosConRetraso());
    }

    IEnumerator ActivarBolosConRetraso()
    {
        yield return new WaitForSeconds(0.1f);

        int boloIndex = 0;
        foreach (Rigidbody rb in bolosRb)
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                // Pequeño delay escalado por bolo (opcional, para estabilidad)
                float delay = 0.02f * boloIndex;
                StartCoroutine(ActivarBoloIndividual(rb, delay));
            }
            boloIndex++;
        }
    }

    IEnumerator ActivarBoloIndividual(Rigidbody rb, float delay = 0f)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (rb == null) yield break;

        rb.WakeUp();

        BoloController controller = rb.GetComponent<BoloController>();
        if (controller != null)
        {
            controller.OnBoloActivado();
        }
    }

    // --- DETECCIÓN DE CAÍDA ---
    public void VerificarBolosPorAngulo(GameManager gm)
    {
        // Verifica todos los bolos para contar los derribados si aún no lo están
        foreach (var controller in boloControllers)
        {
            if (controller != null && !controller.FueDerribado)
            {
                // Lógica de inclinación (debería estar en BoloController, pero el PinManager lo invoca)
                controller.VerificarInclinacion();
            }
        }
    }
}