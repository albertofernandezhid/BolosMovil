using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BolosManager : MonoBehaviour
{
    [Header("REFERENCIAS")]
    public GameObject boloPrefab;
    public GameManager gameManager;

    [Header("AJUSTES FÍSICA - BOLOS")]
    public float masaBolo = 2f;
    public float linearDampingBolo = 0.3f;
    public float angularDampingBolo = 0.1f;

    [Header("CENTRO DE MASA BOLOS")]
    [Tooltip("Coordenadas locales. Valores más positivos en Z = más inestables")]
    public Vector3 centroMasaBolo = new(0f, 0f, 0.2f);

    [Header("AJUSTES BOLOS - COLLIDER Y ESCALA")]
    public Vector3 escalaBolo = new(10f, 10f, 10f);
    public Vector3 centroColliderBolo = new(-2.328306e-10f, 4.656612e-10f, 0.02020354f);
    public Vector3 tamañoColliderBolo = new(0.01182694f, 0.01233853f, 0.04040708f);

    // LISTAS DE BOLOS
    private readonly List<GameObject> bolos = new();
    private readonly List<Rigidbody> bolosRb = new();

    public void CrearBolos()
    {
        // Clear existing pins
        foreach (var bolo in bolos)
        {
            if (bolo != null) Destroy(bolo);
        }
        bolos.Clear();
        bolosRb.Clear();

        // Pin positions for Y = 0.61
        Vector3[] posiciones = new Vector3[10]
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

        for (int i = 0; i < posiciones.Length; i++)
        {
            GameObject boloObj = Instantiate(boloPrefab, posiciones[i], Quaternion.identity);
            boloObj.transform.localScale = escalaBolo;

            ConfigurarFisicaBolo(boloObj);
            bolos.Add(boloObj);

            Debug.Log($"Bolo {i + 1} creado. Posicion: {posiciones[i]}");
        }
    }

    void ConfigurarFisicaBolo(GameObject bolo)
    {
        // Configure pin physics
        Rigidbody rb = bolo.GetComponent<Rigidbody>();
        BoxCollider collider = bolo.GetComponent<BoxCollider>();

        if (rb != null)
        {
            rb.mass = masaBolo;
            rb.linearDamping = linearDampingBolo;
            rb.angularDamping = angularDampingBolo;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Force correct center of mass
            rb.centerOfMass = centroMasaBolo;

            // Ensure kinematic initially
            rb.isKinematic = true;

            bolosRb.Add(rb);

            Debug.Log($"Bolo {bolo.name} - CentroMasa: {rb.centerOfMass}, Kinematic: {rb.isKinematic}");
        }
    }

    public void ActivarFisicaBolos()
    {
        // Activate pin physics
        Debug.Log("ACTIVANDO FISICA DE BOLOS...");

        foreach (Rigidbody rb in bolosRb)
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                StartCoroutine(ActivarBoloIndividual(rb));
            }
        }
    }

    IEnumerator ActivarBoloIndividual(Rigidbody rb, float delay = 0f)
    {
        // Activate individual pin with delay
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (rb == null) yield break;

        rb.WakeUp();

        BoloController controller = rb.GetComponent<BoloController>();
        if (controller != null)
        {
            controller.OnBoloActivado();
        }
    }

    public int ContarBolosDerribados()
    {
        // Count knocked down pins
        int nuevosDerribados = 0;
        foreach (var bolo in bolos)
        {
            if (bolo != null && bolo.activeInHierarchy)
            {
                Rigidbody rb = bolo.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
                    float productoPunto = Vector3.Dot(bolo.transform.up, Vector3.up);
                    bool estaEnElSuelo = bolo.transform.position.y < 0.3f;
                    bool seMueve = rb.linearVelocity.magnitude > 0.1f;

                    if (estaEnElSuelo && seMueve)
                    {
                        nuevosDerribados++;
                    }
                }
            }
        }
        return nuevosDerribados;
    }
}