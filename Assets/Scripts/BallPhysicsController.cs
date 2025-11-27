using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallPhysicsController : MonoBehaviour
{
    private Rigidbody rb;

    [Header("AJUSTES DE FÍSICA Y VELOCIDAD")]
    public float velocidadMaxEstabilidad = 30.0f;
    public float velocidadMaxDrift = 4.0f;
    public float driftFuerzaBase = 0.8f;
    public float deadZoneX = 0.25f;

    [Header("ESTABILIDAD (Rectitud)")]
    public float maxAngularDamping = 0.8f;
    public float minAngularDamping = 0.1f;

    [Header("CONTROL DE BARRERAS")]
    public GameObject barrerasLaterales;

    private bool usarBarreras = false;
    private int driftDireccionAleatoria = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (Mathf.Abs(transform.position.x) < deadZoneX)
        {
            driftDireccionAleatoria = (Random.Range(0, 2) == 0) ? -1 : 1;
        }

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

    void ManejarEstabilidad(float velocidad)
    {
        float t = Mathf.Clamp01(velocidad / velocidadMaxEstabilidad);
        rb.angularDamping = Mathf.Lerp(maxAngularDamping, minAngularDamping, t);
    }

    void ManejarDrift(float velocidad)
    {
        if (velocidad > velocidadMaxDrift) return;
        if (usarBarreras) return;

        float xPos = transform.position.x;
        Vector3 direccionDrift;

        if (Mathf.Abs(xPos) < deadZoneX)
        {
            direccionDrift = (driftDireccionAleatoria > 0) ? Vector3.right : Vector3.left;
        }
        else
        {
            direccionDrift = (xPos > 0) ? Vector3.right : Vector3.left;
        }

        float factorLentitud = 1f - Mathf.Clamp01(velocidad / velocidadMaxDrift);
        float fuerzaLateral = driftFuerzaBase * factorLentitud;

        rb.AddForce(direccionDrift * fuerzaLateral, ForceMode.Acceleration);
    }

    [ContextMenu("Toggle Barreras")]
    public void ToggleBarreras()
    {
        usarBarreras = !usarBarreras;
        if (barrerasLaterales != null)
        {
            barrerasLaterales.SetActive(usarBarreras);
        }
    }
}