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

    [Header("AJUSTES FÍSICA - BOLA")]
    public float masaBola = 3f;
    public float linearDampingBola = 0.5f;
    public float angularDampingBola = 0.8f;

    [Header("AJUSTES FÍSICA - BOLOS")]
    public float masaBolo = 2f;
    public float linearDampingBolo = 0.3f;
    public float angularDampingBolo = 0.1f;

    [Header("CENTRO DE MASA BOLOS")]
    [Tooltip("Coordenadas locales. Valores más positivos en Z = más inestables")]
    public Vector3 centroMasaBolo = new(0f, 0f, 0.2f);

    [Header("AJUSTES FUERZA LANZAMIENTO")]
    public float fuerzaMinima = 8f;
    public float fuerzaMaxima = 15f;

    [Header("AJUSTES BOLOS - COLLIDER Y ESCALA")]
    public Vector3 escalaBolo = new(10f, 10f, 10f);
    public Vector3 centroColliderBolo = new(-2.328306e-10f, 4.656612e-10f, 0.02020354f);
    public Vector3 tamañoColliderBolo = new(0.01182694f, 0.01233853f, 0.04040708f);

    [Header("AJUSTES BOLA - COLLIDER")]
    public Vector3 centroColliderBola = new(0f, 9.581745e-06f, -7.256568e-06f);
    public float radioColliderBola = 0.00082f;

    // COMPONENTES CACHEADOS
    private Rigidbody bolaRb;
    private SphereCollider bolaCollider;
    private readonly List<GameObject> bolos = new();
    private readonly List<Rigidbody> bolosRb = new();
    private bool enFaseCarga = false;
    private float posicionXFijada = 0f;
    private Vector3 posicionInicialBola;
    private int bolosDerribados = 0;
    private readonly int totalBolos = 10;

    private enum EstadoJuego { Posicionamiento, Carga, Lanzada, Finalizado }
    private EstadoJuego estadoActual = EstadoJuego.Posicionamiento;

    void Start()
    {
        // CACHEAR COMPONENTES UNA VEZ
        bolaRb = bola.GetComponent<Rigidbody>();
        bolaCollider = bola.GetComponent<SphereCollider>();

        ConfigurarFisicaBola();
        posicionInicialBola = bola.transform.position;

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

            // USAR COMPONENTE CACHEADO
            if (bolaCollider != null)
            {
                bolaCollider.center = centroColliderBola;
                bolaCollider.radius = radioColliderBola;
            }
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

        // OPTIMIZADO: Usar SetPositionAndRotation en una llamada
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

        bolosDerribados = 0;
        ActualizarPuntuacion();

        enFaseCarga = false;
        estadoActual = EstadoJuego.Posicionamiento;
        CrearBolos();
    }

    void CrearBolos()
    {
        Vector3[] posiciones = new Vector3[10]
        {
            new(0f, 0.4f, 19f),
            new(-0.2f, 0.4f, 19.3f),
            new(0.2f, 0.4f, 19.3f),
            new(-0.4f, 0.4f, 19.6f),
            new(0f, 0.4f, 19.6f),
            new(0.4f, 0.4f, 19.6f),
            new(-0.6f, 0.4f, 19.9f),
            new(-0.2f, 0.4f, 19.9f),
            new(0.2f, 0.4f, 19.9f),
            new(0.6f, 0.4f, 19.9f)
        };

        for (int i = 0; i < posiciones.Length; i++)
        {
            GameObject boloObj = Instantiate(boloPrefab, posiciones[i], Quaternion.Euler(-90f, 0f, 0f));
            boloObj.transform.localScale = escalaBolo;

            ConfigurarFisicaBolo(boloObj);
            bolos.Add(boloObj);

            Debug.Log($"Bolo {i + 1} creado. Rotación: {boloObj.transform.rotation.eulerAngles}");
        }
    }

    void ConfigurarFisicaBolo(GameObject bolo)
    {
        Rigidbody rb = bolo.GetComponent<Rigidbody>();
        BoxCollider collider = bolo.GetComponent<BoxCollider>();

        if (rb != null)
        {
            // SOLUCIÓN PARA CENTRO DE MASA: Temporalmente hacer no-kinematic
            bool eraKinematic = rb.isKinematic;
            rb.isKinematic = false; // Temporalmente no-kinematic para configurar centro de masa

            rb.mass = masaBolo;
            rb.linearDamping = linearDampingBolo;
            rb.angularDamping = angularDampingBolo;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // ? AHORA SÍ PODEMOS CONFIGURAR EL CENTRO DE MASA
            rb.centerOfMass = centroMasaBolo;

            // Volver al estado kinematic original
            rb.isKinematic = eraKinematic;

            Debug.Log($"? Bolo {bolo.name} - CentroMasa APLICADO: {rb.centerOfMass}");

            // VERIFICAR COLLIDER CRÍTICAMENTE
            if (collider != null)
            {
                Debug.Log($"?? Collider {bolo.name} - enabled:{collider.enabled}, trigger:{collider.isTrigger}");

                // FORZAR CONFIGURACIÓN CORRECTA
                collider.enabled = true;
                collider.isTrigger = false;
                collider.center = centroColliderBolo;
                collider.size = tamañoColliderBolo;
            }
            else
            {
                Debug.LogError($"? Bolo {bolo.name} - NO TIENE BOX COLLIDER!");
                // Crear collider de emergencia
                BoxCollider newCollider = bolo.AddComponent<BoxCollider>();
                newCollider.center = centroColliderBolo;
                newCollider.size = tamañoColliderBolo;
                newCollider.isTrigger = false;
            }

            bolosRb.Add(rb);
        }
        else
        {
            Debug.LogError($"? Bolo {bolo.name} - NO TIENE RIGIDBODY!");
        }
    }

    void ActivarFisicaBolos()
    {
        Debug.Log("?? ACTIVANDO FÍSICA DE BOLOS...");
        foreach (Rigidbody rb in bolosRb)
        {
            if (rb != null)
            {
                // FORZAR ACTIVACIÓN COMPLETA
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // Asegurar que el centro de masa se mantiene
                if (rb.centerOfMass != centroMasaBolo)
                {
                    rb.centerOfMass = centroMasaBolo;
                }

                Debug.Log($"? Bolo {rb.name} activado - Kinematic: {rb.isKinematic}, CentroMasa: {rb.centerOfMass}");
            }
        }
    }

    public void BoloDerribado()
    {
        // ? PREVENIR CONTEO DUPLICADO
        if (bolosDerribados < totalBolos)
        {
            bolosDerribados++;
            ActualizarPuntuacion();
            Debug.Log($"?? BoloDerribado llamado. Total: {bolosDerribados}/{totalBolos}");
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

        foreach (var bolo in bolos)
        {
            if (bolo != null && bolo.activeInHierarchy)
            {
                Rigidbody rb = bolo.GetComponent<Rigidbody>();
                BoloController controller = bolo.GetComponent<BoloController>();

                if (rb != null && !rb.isKinematic && controller != null)
                {
                    // Sistema de detección mejorado
                    float productoPunto = Vector3.Dot(bolo.transform.up, Vector3.up);
                    bool estaCaido = productoPunto < 0.7f; // Más sensible
                    bool estaEnElSuelo = bolo.transform.position.y < 0.15f;
                    bool seMueve = rb.linearVelocity.magnitude > 0.1f;

                    if ((estaCaido || estaEnElSuelo) && Time.time > 1.0f)
                    {
                        nuevosDerribados++;
                    }
                }
            }
        }

        // ? SOLO ACTUALIZAR SI ES VÁLIDO
        if (nuevosDerribados > bolosDerribados && nuevosDerribados <= totalBolos)
        {
            bolosDerribados = nuevosDerribados;
            ActualizarPuntuacion();
            Debug.Log($"?? Bolos derribados CORRECTOS: {bolosDerribados}/{totalBolos}");
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

        Debug.Log("?? Botón Lanzar clickeado - Cambiando a fase de carga");
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

        Debug.Log($"?? LANZANDO BOLA - Fuerza: {fuerza}, Carga: {distanciaCargada}");

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

        if (textoInstrucciones != null)
        {
            textoInstrucciones.gameObject.SetActive(true);
            textoInstrucciones.text = $"¡Juego terminado! Puntuación: {bolosDerribados}/{totalBolos}";
        }

        yield return new WaitForSeconds(3f);
        ReiniciarJuego();
    }

    void ReiniciarJuego()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}