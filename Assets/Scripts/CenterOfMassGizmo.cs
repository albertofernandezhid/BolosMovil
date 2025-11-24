using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CenterOfMassGizmo : MonoBehaviour
{
    [Header("Configuración Visual")]
    public Color color = Color.yellow;
    public float size = 0.1f;
    public bool mostrarEnEditor = true;
    public bool mostrarEnRuntime = true;
    public bool mostrarLinea = true;

    private Rigidbody rb;
    private GameObject debugSphere;
    private LineRenderer lineRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (mostrarEnRuntime)
        {
            CrearVisualizacionRuntime();
        }
    }

    void Update()
    {
        // Actualizar visualización en runtime
        if (mostrarEnRuntime && debugSphere != null && rb != null)
        {
            Vector3 worldCoM = transform.TransformPoint(rb.centerOfMass);
            debugSphere.transform.position = worldCoM;

            // Actualizar línea si está activada
            if (mostrarLinea && lineRenderer != null)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, worldCoM);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (mostrarEnEditor && mostrarEnEditor)
        {
            DibujarCentroMasaEditor();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (mostrarEnEditor && !mostrarEnEditor)
        {
            DibujarCentroMasaEditor();
        }
    }

    private void CrearVisualizacionRuntime()
    {
        // Crear esfera para visualización en runtime
        debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere.name = $"{gameObject.name}_CenterOfMass";
        debugSphere.transform.SetParent(this.transform);

        // Configurar esfera
        Renderer sphereRenderer = debugSphere.GetComponent<Renderer>();
        sphereRenderer.material = new Material(Shader.Find("Standard"))
        {
            color = color,
            enableInstancing = true
        };

        // Quitar collider
        Destroy(debugSphere.GetComponent<Collider>());

        // Escalar
        debugSphere.transform.localScale = Vector3.one * size;

        // Crear línea si está activada
        if (mostrarLinea)
        {
            lineRenderer = debugSphere.AddComponent<LineRenderer>();
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.positionCount = 2;
        }
    }

    private void DibujarCentroMasaEditor()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb == null) return;

        Gizmos.color = color;
        Vector3 worldCoM = transform.TransformPoint(rb.centerOfMass);

        Gizmos.DrawSphere(worldCoM, size);

        if (mostrarLinea)
        {
            Gizmos.DrawLine(transform.position, worldCoM);
        }
    }

    void OnDestroy()
    {
        if (debugSphere != null)
            Destroy(debugSphere);
    }

    // Método público para toggle de visibilidad
    public void ToggleVisualizacion(bool estado)
    {
        mostrarEnRuntime = estado;
        if (debugSphere != null)
            debugSphere.SetActive(estado);
        if (lineRenderer != null)
            lineRenderer.enabled = estado;
    }
}