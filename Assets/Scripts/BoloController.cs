using UnityEngine;

[RequireComponent(typeof(Rigidbody))] // <--- AÑADIDO: Para garantizar que el componente existe
public class BoloController : MonoBehaviour
{
    private GameManager gameManager;
    private Rigidbody rb;
    private Vector3 posicionInicial;
    private bool yaContado = false;
    private bool deteccionHabilitada = false;

    public float umbralInclinacion = 0.707f;
    public float umbralDistanciaCaida = 0.1f;

    public bool FueDerribado => yaContado;

    // CAMBIO: Inicializar rb en Awake, ya que es requerido.
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // CAMBIO: Se elimina la obtención de Rigidbody de aquí.
    public void Inicializar(GameManager gm, Vector3 posInicial)
    {
        gameManager = gm;
        posicionInicial = posInicial;
    }

    public void OnBoloActivado()
    {
        Invoke(nameof(HabilitarDeteccion), 0.5f);
    }

    void HabilitarDeteccion()
    {
        deteccionHabilitada = true;
    }

    public void VerificarCaida()
    {
        if (yaContado) return;
        if (!deteccionHabilitada || rb == null || rb.isKinematic) return;

        float inclinacion = Vector3.Dot(Vector3.up, transform.up);
        bool estaInclinado = inclinacion < umbralInclinacion;

        float distanciaMovida = Vector3.Distance(posicionInicial, transform.position);
        bool movidoSuficiente = distanciaMovida > umbralDistanciaCaida;

        if (estaInclinado || movidoSuficiente)
        {
            RegistrarCaida(); // <--- CAMBIO: Parámetro "motivo" eliminado
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (yaContado) return;

        if (gameManager != null && other.gameObject == gameManager.colliderFueraPista.gameObject)
        {
            RegistrarCaida(); // <--- CAMBIO: Parámetro "motivo" eliminado
        }
    }

    private void RegistrarCaida() // <--- CAMBIO: Parámetro "motivo" eliminado
    {
        if (yaContado) return;

        yaContado = true;

        if (gameManager != null)
        {
            gameManager.BoloDerribado();
        }
    }
}