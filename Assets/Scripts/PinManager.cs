using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PinManager : MonoBehaviour
{
    [Header("REFERENCIAS")]
    public GameManager gameManager;
    public GameObject boloPrefab;

    [Header("AJUSTES FISICA - BOLOS")]
    public float masaBolo = 2f;
    public float linearDampingBolo = 0.3f;
    public float angularDampingBolo = 0.1f;
    public Vector3 centroMasaBolo = new(0f, 0.4f, 0.1f);

    [Header("CONFIGURACION BOLOS - POSICION")]
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
    public Vector3 rotacionInicialBolos = new(0f, 0f, 0f);

    private readonly List<GameObject> bolos = new();
    private readonly List<Rigidbody> bolosRb = new();
    private readonly List<BoloController> boloControllers = new();

    private readonly WaitForSeconds delayBoloActivation = new(0.1f);

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
            GameObject boloObj = Instantiate(boloPrefab, posicionesBolos[i], Quaternion.Euler(rotacionInicialBolos), transform);

            Rigidbody rb = boloObj.GetComponent<Rigidbody>();
            BoloController controller = boloObj.GetComponent<BoloController>();

            if (rb != null)
            {
                ConfigurarFisicaBolo(boloObj, rb);
                bolosRb.Add(rb);
            }

            if (controller != null)
            {
                controller.Inicializar(gameManager, posicionesBolos[i]);
                boloControllers.Add(controller);
            }

            bolos.Add(boloObj);
        }
    }

    void ConfigurarFisicaBolo(GameObject bolo, Rigidbody rb)
    {
        Collider collider = bolo.GetComponent<Collider>();

        rb.isKinematic = true;
        rb.mass = masaBolo;
        rb.linearDamping = linearDampingBolo;
        rb.angularDamping = angularDampingBolo;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.centerOfMass = centroMasaBolo;
        rb.maxAngularVelocity = 100f;
        rb.sleepThreshold = 0.005f;

        if (collider != null && collider is MeshCollider meshCollider)
        {
            meshCollider.convex = true;
        }
    }

    public void EliminarBolosCaidos()
    {
        List<GameObject> bolosAEliminar = new();
        List<BoloController> controllersAEliminar = new();
        List<Rigidbody> rbsAEliminar = new();

        foreach (var controller in boloControllers)
        {
            if (controller != null && controller.FueDerribado)
            {
                bolosAEliminar.Add(controller.gameObject);
                controllersAEliminar.Add(controller);

                // Obtener el Rigidbody del controlador. Se acepta GetComponent aquí 
                // porque este método se llama una vez al final de la tirada.
                Rigidbody rb = controller.GetComponent<Rigidbody>();
                if (rb != null) rbsAEliminar.Add(rb);
            }
        }

        foreach (var bolo in bolosAEliminar)
        {
            if (bolo != null) Destroy(bolo);
        }

        bolos.RemoveAll(bolosAEliminar.Contains);
        boloControllers.RemoveAll(controllersAEliminar.Contains);
        bolosRb.RemoveAll(rbsAEliminar.Contains);
    }

    public void ActivarFisicaBolos()
    {
        StartCoroutine(ActivarBolosConRetraso());
    }

    IEnumerator ActivarBolosConRetraso()
    {
        yield return delayBoloActivation;

        // CAMBIO: Iterar sobre la lista de controladores para acceder al objeto completo
        int boloIndex = 0;
        foreach (BoloController controller in boloControllers)
        {
            Rigidbody rb = controller.GetComponent<Rigidbody>(); // Asignación aceptable
            if (rb != null)
            {
                rb.isKinematic = false;
                float delay = 0.02f * boloIndex;
                // CAMBIO: Pasar el Rigidbody y el Controller para evitar GetComponent en el Coroutine
                StartCoroutine(ActivarBoloIndividual(rb, controller, delay));
            }
            boloIndex++;
        }
    }

    IEnumerator ActivarBoloIndividual(Rigidbody rb, BoloController controller, float delay = 0f) // <--- CAMBIO: Recibe el Controller
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (rb == null) yield break;

        rb.WakeUp();

        // CAMBIO: Ya tenemos la referencia al controller, no necesitamos GetComponent
        if (controller != null)
        {
            controller.OnBoloActivado();
        }
    }

    public void VerificarBolosPorAngulo()
    {
        foreach (var controller in boloControllers)
        {
            if (controller != null && !controller.FueDerribado)
            {
                controller.VerificarCaida();
            }
        }
    }

    public bool HayBolosMoviendose(float umbralVelocidad)
    {
        foreach (Rigidbody rb in bolosRb)
        {
            if (rb != null && rb.gameObject.activeSelf && !rb.isKinematic)
            {
                if (rb.linearVelocity.magnitude > umbralVelocidad || rb.angularVelocity.magnitude > umbralVelocidad)
                {
                    return true;
                }
            }
        }
        return false;
    }
}