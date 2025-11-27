using UnityEngine;
using System.Collections;
using TMPro; // Asegúrate de mantenerlo si se usa para el texto de instrucciones

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

    // Cámaras temporales para la detección de Raycast
    private Camera camaraPosicionamiento;
    private Camera camaraCarga;

    private enum EstadoLanzador { Idle, Posicionando, Cargando }
    private EstadoLanzador estadoActual = EstadoLanzador.Idle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (gameManager == null) Debug.LogError("BallLauncher requiere la referencia a GameManager.");
    }

    public void ResetearEIniciarPosicionamiento(Camera topCam, Camera preLaunchCam)
    {
        // 1. Resetear la posición y cinemática de la bola
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb == null) { Debug.LogError("Rigidbody no encontrado en la bola."); return; }
        if (lanzadorAnchor == null)
        {
            Debug.LogError("ERROR NULO: lanzadorAnchor no está asignado en el Inspector.");
            return;
        }

        transform.SetPositionAndRotation(posicionInicialBola, Quaternion.identity);
        rb.isKinematic = true;
        lanzadorAnchor.transform.position = new Vector3(0, 0.585f, 0);

        // 2. Establecer cámaras para la entrada de usuario
        camaraPosicionamiento = topCam;
        camaraCarga = preLaunchCam;

        // 3. Iniciar el estado de posicionamiento
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
        // Se utiliza gameManager para acceder al texto de instrucciones de la UI
        if (gameManager.textoInstrucciones != null)
            gameManager.textoInstrucciones.text = "Arrastra hacia ATRÁS para cargar. Suelta para lanzar";

        float inicioCargaZ = lanzadorAnchor.transform.position.z;

        while (Input.GetMouseButton(0))
        {
            Ray ray = camaraCarga.ScreenPointToRay(Input.mousePosition);
            Plane plane = new(Vector3.up, lanzadorAnchor.transform.position);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);

                // La lógica de carga queda totalmente contenida en BallLauncher
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
        float distanciaCargada = Mathf.Abs(transform.position.z - lanzadorAnchor.transform.position.z);
        float fuerza = Mathf.Lerp(fuerzaMinima, fuerzaMaxima, distanciaCargada);

        rb.isKinematic = false;
        Vector3 direccionFuerza = Vector3.forward;
        rb.AddForce(direccionFuerza * fuerza, ForceMode.VelocityChange);

        if (gameManager.textoInstrucciones != null) gameManager.textoInstrucciones.gameObject.SetActive(false);
        estadoActual = EstadoLanzador.Idle; // Ya no hay más interacción

        // Notifica al GameManager que la bola ha sido lanzada
        gameManager.OnBolaLanzada();
    }
}