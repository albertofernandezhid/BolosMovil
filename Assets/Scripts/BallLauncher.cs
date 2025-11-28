using UnityEngine;
using System.Collections;
using TMPro; // Asegurate de mantenerlo si se usa para el texto de instrucciones

[RequireComponent(typeof(Rigidbody))]
public class BallLauncher : MonoBehaviour
{
    // Solo necesita una referencia al GameManager para notificar eventos clave.
    public GameManager gameManager;
    public GameObject lanzadorAnchor;

    [Header("AJUSTES FUERZA LANZAMIENTO")]
    public float fuerzaMinima = 8f;
    public float fuerzaMaxima = 15f;

    private Rigidbody rb;
    private float posicionXFijada = 0f;
    private Vector3 posicionInicialBola = new(0f, 0.54f, 0f);

    // Camaras temporales para la deteccion de Raycast
    private Camera camaraPosicionamiento;
    private Camera camaraCarga;

    private enum EstadoLanzador { Idle, Posicionando, Cargando }
    private EstadoLanzador estadoActual = EstadoLanzador.Idle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (gameManager == null)
        {
            // Intentar buscar el GameManager si no esta asignado (Asumiendo que el usuario usa el nuevo GameManager)
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (gameManager == null) Debug.LogError("BallLauncher requiere la referencia a GameManager.");
    }

    public void ResetearEIniciarPosicionamiento(Camera topCam, Camera preLaunchCam)
    {
        // CORRECCION 1: Asegurar que las referencias a camaras se asignan en el momento que se llaman.
        camaraPosicionamiento = topCam;
        camaraCarga = preLaunchCam;

        // 1. Resetear la posicion y cinematica de la bola
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb == null) { Debug.LogError("Rigidbody no encontrado en la bola."); return; }
        if (lanzadorAnchor == null)
        {
            Debug.LogError("ERROR NULO: lanzadorAnchor no esta asignado en el Inspector.");
            return;
        }

        transform.SetPositionAndRotation(posicionInicialBola, Quaternion.identity);
        rb.isKinematic = true;
        lanzadorAnchor.transform.position = new Vector3(0, 0.585f, 0);

        // 2. Iniciar el estado de posicionamiento
        estadoActual = EstadoLanzador.Posicionando;
    }

    public void IniciarFaseDeCarga()
    {
        posicionXFijada = transform.position.x;
        estadoActual = EstadoLanzador.Cargando;
        gameManager.OnFaseCargaIniciada(); // Notifica al GameManager que ha pasado a la fase de Carga
    }

    void Update()
    {
        if (estadoActual == EstadoLanzador.Posicionando)
        {
            ManejarFasePosicionamiento();
        }
        else if (estadoActual == EstadoLanzador.Cargando)
        {
            ManejarFaseCarga();
        }
    }

    void ManejarFasePosicionamiento()
    {
        // CORRECCION 2: Comprobacion de Nulo antes de usar la camara
        if (camaraPosicionamiento == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camaraPosicionamiento.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                StartCoroutine(ArrastreHorizontal());
            }
        }
    }

    void ManejarFaseCarga()
    {
        // CORRECCION 3 (LINEA 91): Comprobacion de Nulo antes de usar la camara
        if (camaraCarga == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camaraCarga.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                StartCoroutine(ArrastreCarga());
            }
        }
    }


    IEnumerator ArrastreHorizontal()
    {
        // CORRECCION 4: Comprobacion de Nulo en el Coroutine
        if (camaraPosicionamiento == null || lanzadorAnchor == null) yield break;

        while (Input.GetMouseButton(0))
        {
            Ray ray = camaraPosicionamiento.ScreenPointToRay(Input.mousePosition);
            Plane plane = new(Vector3.up, Vector3.zero);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);
                float newX = Mathf.Clamp(point.x, -0.6f, 0.6f);

                Vector3 anchorPos = lanzadorAnchor.transform.position;
                anchorPos.x = newX;
                lanzadorAnchor.transform.position = anchorPos;

                Vector3 bolaPos = transform.position;
                bolaPos.x = newX;
                transform.position = bolaPos;
            }
            yield return null;
        }
    }

    IEnumerator ArrastreCarga()
    {
        // CORRECCION 5: Comprobacion de Nulo en el Coroutine
        if (camaraCarga == null || lanzadorAnchor == null || gameManager == null) yield break;

        // Se utiliza gameManager para acceder al texto de instrucciones de la UI
        if (gameManager.textoInstrucciones != null)
            gameManager.textoInstrucciones.text = "Arrastra hacia ATRAS para cargar. Suelta para lanzar";

        float inicioCargaZ = lanzadorAnchor.transform.position.z;

        while (Input.GetMouseButton(0))
        {
            Ray ray = camaraCarga.ScreenPointToRay(Input.mousePosition);
            Plane plane = new(Vector3.up, lanzadorAnchor.transform.position);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);

                // La logica de carga queda totalmente contenida en BallLauncher
                float carga = Mathf.Clamp(inicioCargaZ - point.z, 0f, 1f);

                Vector3 posBola = lanzadorAnchor.transform.position;
                posBola.z = inicioCargaZ - carga;
                posBola.x = posicionXFijada;
                transform.position = posBola;

                if (gameManager.textoInstrucciones != null)
                    gameManager.textoInstrucciones.text = $"Fuerza: {(int)(carga * 100)}% - SUELTA PARA LANZAR";
            }
            yield return null;
        }

        LanzarBola();
    }

    void LanzarBola()
    {
        if (rb == null || gameManager == null) return;

        float distanciaCargada = Mathf.Abs(transform.position.z - lanzadorAnchor.transform.position.z);
        float fuerza = Mathf.Lerp(fuerzaMinima, fuerzaMaxima, distanciaCargada);

        rb.isKinematic = false;
        Vector3 direccionFuerza = Vector3.forward;
        rb.AddForce(direccionFuerza * fuerza, ForceMode.VelocityChange);

        if (gameManager.textoInstrucciones != null) gameManager.textoInstrucciones.gameObject.SetActive(false);
        estadoActual = EstadoLanzador.Idle; // Ya no hay mas interaccion

        // Notifica al GameManager que la bola ha sido lanzada
        gameManager.OnBolaLanzada();
    }
}