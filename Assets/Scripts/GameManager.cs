using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("REFERENCIAS A SCRIPTS")]
    public GameObject bola;
    public PinManager pinManager;
    public BallLauncher ballLauncher; // Solo para coordinación de alto nivel
    public BallPhysicsController ballPhysics; // Se mantiene, pero ya no para ToggleBarreras

    [Header("CÁMARAS")]
    public Camera topCamera;
    public Camera preLaunchCamera;
    public Camera ballCamera;
    public float offsetZCámara = -2f;
    public float suavizadoCámara = 5f;

    [Header("UI")]
    public Button botonLanzar;
    public Button botonToggleBarreras;
    public Button botonReiniciar; // Siempre visible
    public TextMeshProUGUI textoInstrucciones;
    public TextMeshProUGUI textoPuntuacion;
    public Canvas panelFinal;

    [Header("LÓGICA DE DETECCIÓN")]
    public BoxCollider colliderDetectorBola;
    public BoxCollider colliderFueraPista;
    public float tiempoDetencionParaPanel = 2f;
    public float umbralVelocidadBola = 0.1f;

    [Header("CONTROL DE BARRERAS")]
    public GameObject barrerasLaterales; // ¡MOVIDO AQUÍ!

    // Propiedad pública para que BallPhysicsController pueda verificar el estado
    private bool usarBarreras = false;
    public bool UsarBarreras => usarBarreras; // Propiedad de solo lectura

    private Rigidbody bolaRb;
    private int bolosDerribados = 0;
    private const int TOTAL_BOLOS = 10;
    private bool bolaEnZonaDetencion = false;
    private bool detencionProcesada = false;
    private float tiempoBolaQuieta = 0f;

    private readonly WaitForSeconds waitHalfSecond = new(0.5f);
    private readonly WaitForSeconds waitOneSecond = new(1f);
    private readonly WaitForSeconds waitTwoSeconds = new(2f);

    public enum EstadoJuego { Posicionamiento, Carga, Lanzada, Finalizado }
    private EstadoJuego estadoActual = EstadoJuego.Posicionamiento;

    void Start()
    {
        if (bola != null)
        {
            bolaRb = bola.GetComponent<Rigidbody>();
            if (bolaRb != null)
            {
                bolaRb.isKinematic = true;
                bolaRb.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        // Inicialización de barreras al inicio
        if (barrerasLaterales != null)
        {
            // El objeto se desactiva si usarBarreras es false (por defecto)
            barrerasLaterales.SetActive(usarBarreras);
        }

        if (botonLanzar != null)
        {
            botonLanzar.onClick.RemoveAllListeners();
            botonLanzar.onClick.AddListener(OnBotonLanzarClick);
        }

        // CORRECCIÓN: Enlace del botón de Barreras a ToggleBarreras() de GameManager
        if (botonToggleBarreras != null)
        {
            botonToggleBarreras.onClick.RemoveAllListeners();
            botonToggleBarreras.onClick.AddListener(ToggleBarreras);
        }

        // Enlace y visibilidad inicial del botón Reiniciar
        if (botonReiniciar != null)
        {
            botonReiniciar.onClick.RemoveAllListeners();
            botonReiniciar.onClick.AddListener(ReiniciarJuego);
            botonReiniciar.gameObject.SetActive(true);
        }

        IniciarJuego();
    }

    void Update()
    {
        if (estadoActual == EstadoJuego.Lanzada)
        {
            pinManager.VerificarBolosPorAngulo(this);

            if (bolaEnZonaDetencion)
            {
                VerificarDetencionBola();
            }
        }
    }

    /// <summary>
    /// Se llama después de que Update se haya ejecutado en todos los scripts.
    /// Garantiza un seguimiento suave de la cámara después de los cálculos de física (FixedUpdate).
    /// </summary>
    void LateUpdate()
    {
        if (estadoActual == EstadoJuego.Lanzada)
        {
            SeguirBolaConCamara();
        }
    }

    // --- LÓGICA DE BARRERAS (MOVIDA AQUÍ) ---
    public void ToggleBarreras()
    {
        usarBarreras = !usarBarreras;
        if (barrerasLaterales != null)
        {
            barrerasLaterales.SetActive(usarBarreras);
            Debug.Log($"Barreras laterales: {(usarBarreras ? "ACTIVADAS" : "DESACTIVADAS")}");
        }
    }
    // ----------------------------------------

    public void IniciarJuego()
    {
        if (pinManager != null) pinManager.ReiniciarBolos();
        if (ballLauncher != null) ballLauncher.ResetearEIniciarPosicionamiento(topCamera, preLaunchCamera);

        bolosDerribados = 0;
        ActualizarPuntuacion();
        detencionProcesada = false;
        tiempoBolaQuieta = 0f;
        estadoActual = EstadoJuego.Posicionamiento;

        if (panelFinal != null) panelFinal.gameObject.SetActive(false);
        if (botonLanzar != null) botonLanzar.gameObject.SetActive(true);
        if (textoInstrucciones != null) textoInstrucciones.text = "Arrastra para mover horizontalmente";
        if (textoInstrucciones != null) textoInstrucciones.gameObject.SetActive(true);
        if (botonReiniciar != null) botonReiniciar.gameObject.SetActive(true);

        topCamera.gameObject.SetActive(true);
        preLaunchCamera.gameObject.SetActive(false);
        ballCamera.gameObject.SetActive(false);
    }

    public void OnBotonLanzarClick()
    {
        estadoActual = EstadoJuego.Carga;
        ballLauncher.IniciarFaseDeCarga();

        topCamera.gameObject.SetActive(false);
        preLaunchCamera.gameObject.SetActive(true);
        if (botonLanzar != null) botonLanzar.gameObject.SetActive(false);
    }

    public void OnFaseCargaIniciada()
    {
        estadoActual = EstadoJuego.Carga;
        if (textoInstrucciones != null)
            textoInstrucciones.text = "Haz clic en la bola y arrastra hacia ATRÁS para cargar.";
    }

    public void OnBolaLanzada()
    {
        estadoActual = EstadoJuego.Lanzada;
        if (pinManager != null) pinManager.ActivarFisicaBolos();

        preLaunchCamera.gameObject.SetActive(false);
        ballCamera.gameObject.SetActive(true);
        if (textoInstrucciones != null) textoInstrucciones.gameObject.SetActive(false);

        StartCoroutine(VerificarFinJuegoLargo());
    }

    public void BoloDerribado()
    {
        if (bolosDerribados < TOTAL_BOLOS)
        {
            bolosDerribados++;
            ActualizarPuntuacion();
        }
    }

    void ActualizarPuntuacion()
    {
        if (textoPuntuacion != null)
        {
            textoPuntuacion.text = $"{bolosDerribados}/{TOTAL_BOLOS}";
        }
    }

    void MostrarPanelFinal()
    {
        if (detencionProcesada) return;

        detencionProcesada = true;
        estadoActual = EstadoJuego.Finalizado;

        if (panelFinal != null)
        {
            panelFinal.gameObject.SetActive(true);
            TextMeshProUGUI textoPanel = panelFinal.GetComponentInChildren<TextMeshProUGUI>();
            if (textoPanel != null)
            {
                textoPanel.text = $"¡Turno terminado!\nPuntuación: {bolosDerribados}/{TOTAL_BOLOS}";
            }
        }

        if (botonReiniciar != null)
        {
            botonReiniciar.gameObject.SetActive(true);
        }
    }

    public void ReiniciarJuego()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

    void VerificarDetencionBola()
    {
        if (detencionProcesada) return;

        bool bolaQuieta = bolaRb.linearVelocity.magnitude < umbralVelocidadBola &&
                            bolaRb.angularVelocity.magnitude < umbralVelocidadBola;

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

    IEnumerator VerificarFinJuegoLargo()
    {
        yield return waitOneSecond;

        while (bola != null && estadoActual == EstadoJuego.Lanzada && bola.transform.position.z < 25f && bola.transform.position.y > -1f)
        {
            yield return waitHalfSecond;
        }

        yield return waitTwoSeconds;

        if (!detencionProcesada)
        {
            MostrarPanelFinal();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == colliderDetectorBola && other.gameObject == bola)
        {
            bolaEnZonaDetencion = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other == colliderDetectorBola && other.gameObject == bola)
        {
            bolaEnZonaDetencion = false;
            tiempoBolaQuieta = 0f;
        }
    }
}