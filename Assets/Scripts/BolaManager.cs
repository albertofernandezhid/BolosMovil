using UnityEngine;
using System.Collections;

public class BolaManager : MonoBehaviour
{
    [Header("REFERENCIAS")]
    public GameObject bola;
    public GameObject lanzadorAnchor;

    [Header("AJUSTES FÍSICA - BOLA")]
    public float masaBola = 3f;
    public float linearDampingBola = 0.5f;
    public float angularDampingBola = 0.8f;

    [Header("AJUSTES FUERZA LANZAMIENTO")]
    public float fuerzaMinima = 8f;
    public float fuerzaMaxima = 15f;

    [Header("AJUSTES BOLA - COLLIDER")]
    public Vector3 centroColliderBola = new(0f, 9.581745e-06f, -7.256568e-06f);
    public float radioColliderBola = 0.00082f;

    // COMPONENTES CACHEADOS
    private Rigidbody bolaRb;
    private SphereCollider bolaCollider;
    private Vector3 posicionInicialBola;
    private Vector3 posicionInicialAnchor;

    void Start()
    {
        // Cache components
        if (bola == null)
        {
            Debug.LogError("BOLA NO ASIGNADA EN EL INSPECTOR");
            return;
        }

        bolaRb = bola.GetComponent<Rigidbody>();
        bolaCollider = bola.GetComponent<SphereCollider>();

        if (bolaRb == null) Debug.LogError("RIGIDBODY NO ENCONTRADO EN BOLA");
        if (bolaCollider == null) Debug.LogError("SPHERE COLLIDER NO ENCONTRADO EN BOLA");

        ConfigurarFisicaBola();
        posicionInicialBola = bola.transform.position;
        posicionInicialAnchor = lanzadorAnchor.transform.position;
    }

    void ConfigurarFisicaBola()
    {
        // Configure ball physics
        if (bolaRb != null)
        {
            bolaRb.mass = masaBola;
            bolaRb.linearDamping = linearDampingBola;
            bolaRb.angularDamping = angularDampingBola;
            bolaRb.interpolation = RigidbodyInterpolation.Interpolate;
            bolaRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Critical configuration to prevent sinking
            bolaRb.useGravity = true;
            bolaRb.constraints = RigidbodyConstraints.None;
        }

        if (bolaCollider != null)
        {
            bolaCollider.isTrigger = false;
            bolaCollider.center = centroColliderBola;
            bolaCollider.radius = radioColliderBola;
        }
    }

    public void ResetearBola()
    {
        // Reset ball to initial state
        if (bola == null || bolaRb == null)
        {
            Debug.LogError("NO SE PUEDE RESETEAR BOLA - REFERENCIAS NULAS");
            return;
        }

        // Only reset velocities if not kinematic
        if (!bolaRb.isKinematic)
        {
            bolaRb.linearVelocity = Vector3.zero;
            bolaRb.angularVelocity = Vector3.zero;
        }

        // Set kinematic and position
        bolaRb.isKinematic = true;
        bola.transform.SetPositionAndRotation(posicionInicialBola, Quaternion.identity);
        lanzadorAnchor.transform.position = posicionInicialAnchor;

        Debug.Log("Bola reseteada correctamente");
    }

    public void MoverBolaHorizontalmente(float nuevaX)
    {
        // Move ball horizontally
        if (bola == null) return;

        Vector3 anchorPos = lanzadorAnchor.transform.position;
        anchorPos.x = nuevaX;
        lanzadorAnchor.transform.position = anchorPos;

        Vector3 bolaPos = bola.transform.position;
        bolaPos.x = nuevaX;
        bola.transform.position = bolaPos;
    }

    public void PosicionarBolaCarga(Vector3 posicion)
    {
        // Position ball during charge phase
        if (bola == null) return;
        bola.transform.position = posicion;
    }

    public float LanzarBola()
    {
        // Launch the ball
        if (bolaRb == null) return 0f;

        float distanciaCargada = Mathf.Abs(bola.transform.position.z - lanzadorAnchor.transform.position.z);
        float fuerza = Mathf.Lerp(fuerzaMinima, fuerzaMaxima, distanciaCargada);

        // Activate physics
        bolaRb.isKinematic = false;
        Vector3 direccionFuerza = Vector3.forward;
        bolaRb.AddForce(direccionFuerza * fuerza, ForceMode.VelocityChange);

        Debug.Log($"LANZANDO BOLA - Fuerza: {fuerza}, Posicion Y: {bola.transform.position.y}");

        return fuerza;
    }

    public float GetPosicionX()
    {
        return lanzadorAnchor != null ? lanzadorAnchor.transform.position.x : 0f;
    }

    public Vector3 GetPosicionBola()
    {
        return bola != null ? bola.transform.position : Vector3.zero;
    }

    public Vector3 GetAnchorPosition()
    {
        return lanzadorAnchor != null ? lanzadorAnchor.transform.position : Vector3.zero;
    }

    public GameObject GetBolaGameObject()
    {
        return bola;
    }

    public bool BolaExiste()
    {
        return bola != null;
    }
}