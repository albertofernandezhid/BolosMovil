using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class BallLauncher : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject lanzadorAnchor;
    public Camera topCamera;
    public Camera preLaunchCamera;

    [Header("AJUSTES FUERZA LANZAMIENTO")]
    public float fuerzaMinima = 8f;
    public float fuerzaMaxima = 15f;

    private Rigidbody rb;
    private float posicionXFijada = 0f;
    private Vector3 posicionInicialBola = new(0f, 0.54f, 0f);

    private enum EstadoLanzador { Idle, Posicionando, Cargando }
    private EstadoLanzador estadoActual = EstadoLanzador.Idle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (gameManager == null) Debug.LogError("BallLauncher requiere la referencia a GameManager.");
    }

    void Update()
    {
        if (estadoActual == EstadoLanzador.Posicionando)
        {
            ManejarFasePosicionamiento();
        }
        else if (estadoActual == EstadoLanzador.Cargando)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = preLaunchCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
                {
                    StartCoroutine(ArrastreCarga());
                }
            }
        }
    }

    public void ResetearBola()
    {
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
        estadoActual = EstadoLanzador.Posicionando;
    }

    // ESTA FUNCIÓN ESTABA CAUSANDO EL ERROR SI EL ARCHIVO NO ESTABA GUARDADO
    public void FijarPosicionEIniciarCarga()
    {
        posicionXFijada = transform.position.x;
        estadoActual = EstadoLanzador.Cargando;
        if (gameManager.textoInstrucciones != null)
            gameManager.textoInstrucciones.text = "Haz clic en la bola y arrastra hacia ATRÁS para cargar.";
    }


    void ManejarFasePosicionamiento()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = topCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                StartCoroutine(ArrastreHorizontal());
            }
        }
    }

    IEnumerator ArrastreHorizontal()
    {
        while (Input.GetMouseButton(0))
        {
            Ray ray = topCamera.ScreenPointToRay(Input.mousePosition);
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
        if (gameManager.textoInstrucciones != null) gameManager.textoInstrucciones.text = "Arrastra hacia ATRÁS para cargar. Suelta para lanzar";

        float inicioCargaZ = lanzadorAnchor.transform.position.z;

        while (Input.GetMouseButton(0))
        {
            Ray ray = preLaunchCamera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new(Vector3.up, lanzadorAnchor.transform.position);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);

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

        gameManager.OnBolaLanzada();
    }
}