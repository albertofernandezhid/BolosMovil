using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("REFERENCIAS")]
    public GameObject lanzadorAnchor;
    public GameObject bola;
    public GameObject boloPrefab;

    [Header("CAMARAS")]
    public Camera topCamera;
    public Camera preLaunchCamera;
    public Camera ballCamera;

    [Header("CONFIGURACIÓN CÁMARA SEGUIDORA")]
    public float offsetZCámara = -2f;
    public float suavizadoCámara = 5f;

    [Header("UI")]
    public Button botonLanzar;
    public TextMeshProUGUI textoInstrucciones;
    public TextMeshProUGUI textoPuntuacion;
    public Canvas panelFinal;

    [Header("DETECCIÓN BOLA")]
    public BoxCollider colliderDetectorBola;
    public BoxCollider colliderFueraPista;
    [Tooltip("Tiempo en segundos que debe estar la bola quieta para mostrar el panel")]
    public float tiempoDetencionParaPanel = 2f;

    [Header("AJUSTES FÍSICA - BOLA")]
    public float masaBola = 3f;
    public float linearDampingBola = 0.5f;
    public float angularDampingBola = 0.8f;

    [Header("AJUSTES FÍSICA - BOLOS")]
    public float masaBolo = 2f;
    public float linearDampingBolo = 0.3f;
    public float angularDampingBolo = 0.1f;

    [Header("CENTRO DE MASA BOLOS")]
    [Tooltip("Coordenadas locales. Valores más positivos en Y = más inestables")]
    public Vector3 centroMasaBolo = new(0f, 0.4f, 0.1f); // Más alto y hacia adelante

    [Header("AJUSTES FUERZA LANZAMIENTO")]
    public float fuerzaMinima = 8f;
    public float fuerzaMaxima = 15f;

    [Header("CONFIGURACIÓN BOLOS - POSICIÓN CORREGIDA")]
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

    [Header("ROTACIÓN INICIAL BOLOS")]
    public Vector3 rotacionInicialBolos = new Vector3(0f, 0f, 0f);

    [Header("UMBRALES DETECCIÓN CAÍDA")]
    [Tooltip("Producto punto mínimo para considerar bolo de pie (0-1). 0.5 = ~60°")]
    public float umbralProductoPunto = 0.5f; // Más permisivo - 60 grados
    [Tooltip("Velocidad mínima para considerar movimiento")]
    public float umbralVelocidad = 0.1f; // Más bajo

    // COMPONENTES CACHEADOS
    private Rigidbody bolaRb;
    private readonly List<GameObject> bolos = new();
    private readonly List<Rigidbody> bolosRb = new();
    private readonly List<BoloController> boloControllers = new();
    private bool enFaseCarga = false;
    private float posicionXFijada = 0f;
    private Vector3 posicionInicialBola = new(0f, 0.54f, 0f);
    private int bolosDerribados = 0;
    private readonly int totalBolos = 10;
    private bool bolaEnZonaDetencion = false;
    private bool detencionProcesada = false;
    private float tiempoBolaQuieta = 0f;

    private enum EstadoJuego { Posicionamiento, Carga, Lanzada, Finalizado }
    private EstadoJuego estadoActual = EstadoJuego.Posicionamiento;

    void Start()
    {
        bolaRb = bola.GetComponent<Rigidbody>();
        ConfigurarFisicaBola();

        if (panelFinal != null)
        {
            panelFinal.gameObject.SetActive(false);
        }

        if (botonLanzar != null)
        {
            botonLanzar.onClick.RemoveAllListeners();
            botonLanzar.onClick.AddListener(OnBotonLanzarClick);
        }

        IniciarJuego();
    }

    void ConfigurarFisicaBola()
    {
        if (bolaRb != null)
        {
            bolaRb.mass = masaBola;
            bolaRb.linearDamping = linearDampingBola;
            bolaRb.angularDamping = angularDampingBola;
            bolaRb.interpolation = RigidbodyInterpolation.Interpolate;
            bolaRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    void IniciarJuego()
    {
        foreach (var bolo in bolos)
        {
            if (bolo != null) Destroy(bolo);
        }
        bolos.Clear();
        bolosRb.Clear();
        boloControllers.Clear();

        bola.transform.SetPositionAndRotation(posicionInicialBola, Quaternion.identity);
        bolaRb.isKinematic = true;

        lanzadorAnchor.transform.position = new Vector3(0, 0.585f, 0);

        topCamera.gameObject.SetActive(true);
        preLaunchCamera.gameObject.SetActive(false);
        ballCamera.gameObject.SetActive(false);

        if (botonLanzar != null)
        {
            botonLanzar.gameObject.SetActive(true);
            botonLanzar.interactable = true;
        }

        if (textoInstrucciones != null)
        {
            textoInstrucciones.text = "Arrastra para mover horizontalmente";
            textoInstrucciones.gameObject.SetActive(true);
        }

        if (panelFinal != null)
        {
            panelFinal.gameObject.SetActive(false);
        }

        bolosDerribados = 0;
        ActualizarPuntuacion();

        enFaseCarga = false;
        estadoActual = EstadoJuego.Posicionamiento;
        bolaEnZonaDetencion = false;
        detencionProcesada = false;
        tiempoBolaQuieta = 0f;

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
            }
        }
    }

    void ConfigurarFisicaBolo(GameObject bolo)
    {
        Rigidbody rb = bolo.GetComponent<Rigidbody>();
        Collider collider = bolo.GetComponent<Collider>();

        if (rb != null)
        {
            bool estadoOriginal = rb.isKinematic;

            rb.mass = masaBolo;
            rb.linearDamping = linearDampingBolo;
            rb.angularDamping = angularDampingBolo;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Configurar centro de masa para mayor inestabilidad
            rb.isKinematic = false;
            rb.centerOfMass = centroMasaBolo;
            rb.isKinematic = estadoOriginal;

            // MEJORAR FÍSICA PARA COMPORTAMIENTO REALISTA
            rb.maxAngularVelocity = 100f; // Permitir más rotación
            rb.sleepThreshold = 0.005f; // Más sensible

            if (collider != null)
            {
                collider.enabled = true;
                collider.isTrigger = false;

                // Asegurar collider convexo
                if (collider is MeshCollider meshCollider)
                {
                    meshCollider.convex = true;
                }
            }

            bolosRb.Add(rb);
        }
    }

    void ActivarFisicaBolos()
    {
        Debug.Log("ACTIVANDO FÍSICA DE BOLOS...");

        int boloIndex = 0;
        foreach (Rigidbody rb in bolosRb)
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                float delay = 0.02f * boloIndex;
                StartCoroutine(ActivarBoloIndividual(rb, delay));
                boloIndex++;
            }
        }
    }

    IEnumerator ActivarBoloIndividual(Rigidbody rb, float delay = 0f)
    {
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

    public void BoloDerribado()
    {
        if (bolosDerribados < totalBolos)
        {
            bolosDerribados++;
            ActualizarPuntuacion();
            Debug.Log($"BoloDerribado - Total: {bolosDerribados}/{totalBolos}");
        }
    }

    void ActualizarPuntuacion()
    {
        if (textoPuntuacion != null)
        {
            textoPuntuacion.text = $"{bolosDerribados}/{totalBolos}";
        }
    }

    void VerificarBolosPorAngulo()
    {
        int nuevosDerribados = 0;

        foreach (var controller in boloControllers)
        {
            if (controller != null && controller.FueDerribado)
            {
                nuevosDerribados++;
            }
        }

        if (nuevosDerribados > bolosDerribados)
        {
            bolosDerribados = nuevosDerribados;
            ActualizarPuntuacion();
        }
    }

    void VerificarDetencionBola()
    {
        if (estadoActual != EstadoJuego.Lanzada || detencionProcesada) return;

        bool bolaQuieta = bolaRb.linearVelocity.magnitude < 0.1f &&
                         bolaRb.angularVelocity.magnitude < 0.1f;

        if (bolaQuieta)
        {
            tiempoBolaQuieta += Time.deltaTime;

            if (tiempoBolaQuieta >= tiempoDetencionParaPanel)
            {
                MostrarPanelFinal();
            }
        }
        else
        {
            tiempoBolaQuieta = 0f;
        }
    }

    void MostrarPanelFinal()
    {
        if (detencionProcesada) return;

        detencionProcesada = true;
        Debug.Log("Mostrando panel final - Bola detenida");

        if (panelFinal != null)
        {
            panelFinal.gameObject.SetActive(true);

            TextMeshProUGUI textoPanel = panelFinal.GetComponentInChildren<TextMeshProUGUI>();
            if (textoPanel != null)
            {
                textoPanel.text = $"¡Turno terminado!\nPuntuación: {bolosDerribados}/{totalBolos}";
            }
        }

        estadoActual = EstadoJuego.Finalizado;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == colliderDetectorBola && other.gameObject == bola)
        {
            bolaEnZonaDetencion = true;
            Debug.Log("Bola entró en zona de detección");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other == colliderDetectorBola && other.gameObject == bola)
        {
            bolaEnZonaDetencion = false;
            tiempoBolaQuieta = 0f;
            Debug.Log("Bola salió de zona de detección");
        }
    }

    public void OnBotonLanzarClick()
    {
        if (enFaseCarga) return;

        posicionXFijada = lanzadorAnchor.transform.position.x;
        enFaseCarga = true;
        estadoActual = EstadoJuego.Carga;

        topCamera.gameObject.SetActive(false);
        preLaunchCamera.gameObject.SetActive(true);
        ballCamera.gameObject.SetActive(false);

        if (botonLanzar != null) botonLanzar.gameObject.SetActive(false);
        if (textoInstrucciones != null) textoInstrucciones.text = "Arrastra hacia ATRÁS para cargar. Suelta para lanzar";

        Debug.Log("Botón Lanzar clickeado - Cambiando a fase de carga");
    }

    void Update()
    {
        if (bola == null) return;

        if (!enFaseCarga)
        {
            ManejarFasePosicionamiento();
        }
        else if (estadoActual == EstadoJuego.Carga)
        {
            ManejarFaseCarga();
        }
        else if (estadoActual == EstadoJuego.Lanzada)
        {
            VerificarBolosPorAngulo();
            SeguirBolaConCamara();

            if (bolaEnZonaDetencion)
            {
                VerificarDetencionBola();
            }
        }
    }

    void SeguirBolaConCamara()
    {
        if (ballCamera != null && ballCamera.gameObject.activeSelf && bola != null)
        {
            Vector3 posicionCamara = ballCamera.transform.position;
            float nuevaZ = bola.transform.position.z + offsetZCámara;
            nuevaZ = Mathf.Min(nuevaZ, 17f);
            posicionCamara.z = Mathf.Lerp(posicionCamara.z, nuevaZ, suavizadoCámara * Time.deltaTime);
            ballCamera.transform.position = posicionCamara;
        }
    }

    void ManejarFasePosicionamiento()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = topCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == bola)
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

                Vector3 bolaPos = bola.transform.position;
                bolaPos.x = newX;
                bola.transform.position = bolaPos;
            }
            yield return null;
        }
    }

    void ManejarFaseCarga()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(ArrastreCarga());
        }
    }

    IEnumerator ArrastreCarga()
    {
        while (Input.GetMouseButton(0))
        {
            Ray ray = preLaunchCamera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new(Vector3.up, lanzadorAnchor.transform.position);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);
                float carga = Mathf.Clamp((lanzadorAnchor.transform.position.z - point.z), 0f, 1f);

                Vector3 posBola = lanzadorAnchor.transform.position;
                posBola.z -= carga;
                posBola.x = posicionXFijada;
                bola.transform.position = posBola;

                if (textoInstrucciones != null)
                    textoInstrucciones.text = $"Fuerza: {(int)(carga * 100)}% - SUELTA PARA LANZAR";
            }
            yield return null;
        }

        LanzarBola();
    }

    void LanzarBola()
    {
        float distanciaCargada = Mathf.Abs(bola.transform.position.z - lanzadorAnchor.transform.position.z);
        float fuerza = Mathf.Lerp(fuerzaMinima, fuerzaMaxima, distanciaCargada);

        Debug.Log($"LANZANDO BOLA - Fuerza: {fuerza}, Carga: {distanciaCargada}");

        bolaRb.isKinematic = false;
        Vector3 direccionFuerza = Vector3.forward;
        bolaRb.AddForce(direccionFuerza * fuerza, ForceMode.VelocityChange);

        StartCoroutine(ActivarBolosConRetraso());

        preLaunchCamera.gameObject.SetActive(false);
        ballCamera.gameObject.SetActive(true);

        if (textoInstrucciones != null) textoInstrucciones.gameObject.SetActive(false);

        estadoActual = EstadoJuego.Lanzada;
        StartCoroutine(VerificarFinJuego());
    }

    IEnumerator ActivarBolosConRetraso()
    {
        yield return new WaitForSeconds(0.1f);
        ActivarFisicaBolos();
    }

    IEnumerator VerificarFinJuego()
    {
        yield return new WaitForSeconds(1f);

        while (bola != null && bola.transform.position.z < 25f && bola.transform.position.y > -1f)
        {
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(2f);

        if (!detencionProcesada)
        {
            MostrarPanelFinal();
        }

        yield return new WaitForSeconds(3f);
        ReiniciarJuego();
    }

    void ReiniciarJuego()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}