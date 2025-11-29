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
    public BallLauncher ballLauncher;
    public BallPhysicsController ballPhysics;

    [Header("GESTION DE LA PARTIDA")]
    public ScoreManager scoreManager;

    [Header("CAMARAS")]
    public Camera topCamera;
    public Camera preLaunchCamera;
    public Camera ballCamera;
    public float offsetZCamara = -2f;
    public float suavizadoCamara = 5f;

    [Header("LOGICA DE FIN DE LANZAMIENTO")]
    public float posicionZFinalCamara = 17f;
    public float tiempoPausaEnFinal = 3f;

    [Header("UI")]
    public Button botonLanzar;
    public Button botonToggleBarreras;
    public Button botonReiniciar;
    public TextMeshProUGUI textoInstrucciones;
    public TextMeshProUGUI textoPuntuacion;
    public Canvas panelFinal;
    public GameObject panelScoreGlobal;

    [Header("LOGICA DE DETECCION")]
    public float tiempoDetencionParaPanel = 2f;
    public float umbralVelocidadBola = 0.1f;
    public float tiempoEsperaConteoBolos = 1.0f;
    public BoxCollider colliderFueraPista;
    public BoxCollider colliderFinalPista;

    [Header("CONTROL DE BARRERAS")]
    public GameObject barrerasLaterales;

    [Header("REFERENCIAS MENU")]
    public MenuManager menuManager;

    private bool usarBarreras = false;
    public bool UsarBarreras => usarBarreras;

    private Rigidbody bolaRb;
    private int bolosDerribados = 0;
    private const int TOTAL_BOLOS = 10;
    private bool detencionProcesada = false;
    private float tiempoBolaQuieta = 0f;
    private bool bolaLlegoAlFinal = false;
    private float tiempoEnFinalCamara = 0f;

    public enum EstadoJuego { Posicionamiento, Carga, Lanzada, Finalizado }
    private EstadoJuego estadoActual = EstadoJuego.Posicionamiento;

    void Start()
    {
        Time.timeScale = 1f;

        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
        }

        if (menuManager == null)
        {
            menuManager = FindFirstObjectByType<MenuManager>();
        }

        if (bola != null)
        {
            bolaRb = bola.GetComponent<Rigidbody>();
            if (bolaRb != null)
            {
                bolaRb.isKinematic = true;
                bolaRb.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        if (barrerasLaterales != null)
        {
            barrerasLaterales.SetActive(usarBarreras);
        }

        if (botonLanzar != null)
        {
            botonLanzar.onClick.RemoveAllListeners();
            botonLanzar.onClick.AddListener(OnBotonLanzarClick);
            botonLanzar.gameObject.SetActive(false);
        }

        if (botonToggleBarreras != null)
        {
            botonToggleBarreras.onClick.RemoveAllListeners();
            botonToggleBarreras.onClick.AddListener(ToggleBarreras);
            botonToggleBarreras.gameObject.SetActive(false);
        }

        if (botonReiniciar != null)
        {
            botonReiniciar.onClick.RemoveAllListeners();
            botonReiniciar.onClick.AddListener(ReiniciarJuego);
            botonReiniciar.gameObject.SetActive(false);
        }

        if (textoInstrucciones != null) textoInstrucciones.gameObject.SetActive(false);
        if (textoPuntuacion != null) textoPuntuacion.gameObject.SetActive(false);
        if (panelFinal != null) panelFinal.gameObject.SetActive(false);
        if (panelScoreGlobal != null) panelScoreGlobal.SetActive(false);

        topCamera.gameObject.SetActive(false);
        preLaunchCamera.gameObject.SetActive(false);
        ballCamera.gameObject.SetActive(false);

        if (menuManager != null)
        {
            menuManager.MostrarMainMenu();
        }
    }

    void Update()
    {
        if (estadoActual == EstadoJuego.Lanzada)
        {
            if (pinManager != null) pinManager.VerificarBolosPorAngulo();

            if (bolaLlegoAlFinal)
            {
                tiempoEnFinalCamara += Time.deltaTime;

                if (tiempoEnFinalCamara >= tiempoPausaEnFinal)
                {
                    IniciarFinalizacionLanzamiento();
                }
            }
            else if (bolaRb != null)
            {
                VerificarDetencionBolaPorVelocidad();
            }
        }
    }

    void LateUpdate()
    {
        if (estadoActual == EstadoJuego.Lanzada)
        {
            SeguirBolaConCamara();
        }
    }

    public void ToggleBarreras()
    {
        usarBarreras = !usarBarreras;
        if (barrerasLaterales != null)
        {
            barrerasLaterales.SetActive(usarBarreras);
        }
    }

    public void IniciarJuego()
    {
        if (pinManager != null) pinManager.ReiniciarBolos();

        if (bolaRb != null) bolaRb.isKinematic = true;
        if (ballLauncher != null) ballLauncher.ResetearEIniciarPosicionamiento(topCamera, preLaunchCamera);

        bolosDerribados = 0;
        ActualizarPuntuacionSimple();
        ResetearVariablesDeDetencion();
        estadoActual = EstadoJuego.Posicionamiento;

        if (panelFinal != null) panelFinal.gameObject.SetActive(false);
        if (panelScoreGlobal != null) panelScoreGlobal.SetActive(false);

        if (textoPuntuacion != null) textoPuntuacion.gameObject.SetActive(true);

        if (botonLanzar != null) botonLanzar.gameObject.SetActive(true);
        if (textoInstrucciones != null) textoInstrucciones.text = "Arrastra para mover horizontalmente";
        if (textoInstrucciones != null) textoInstrucciones.gameObject.SetActive(true);

        if (botonToggleBarreras != null) botonToggleBarreras.gameObject.SetActive(true);

        topCamera.gameObject.SetActive(true);
        preLaunchCamera.gameObject.SetActive(false);
        ballCamera.gameObject.SetActive(false);
    }

    private void ResetearVariablesDeDetencion()
    {
        detencionProcesada = false;
        tiempoBolaQuieta = 0f;
        tiempoEnFinalCamara = 0f;
        bolaLlegoAlFinal = false;
    }

    public void OnBotonLanzarClick()
    {
        estadoActual = EstadoJuego.Carga;
        if (ballLauncher != null) ballLauncher.IniciarFaseDeCarga();

        topCamera.gameObject.SetActive(false);
        preLaunchCamera.gameObject.SetActive(true);
        if (botonLanzar != null) botonLanzar.gameObject.SetActive(false);
    }

    public void OnFaseCargaIniciada()
    {
        estadoActual = EstadoJuego.Carga;
        if (textoInstrucciones != null)
            textoInstrucciones.text = "Haz clic en la bola y arrastra hacia ATRAS para cargar.";
    }

    public void OnBolaLanzada()
    {
        estadoActual = EstadoJuego.Lanzada;
        if (pinManager != null) pinManager.ActivarFisicaBolos();

        preLaunchCamera.gameObject.SetActive(false);
        ballCamera.gameObject.SetActive(true);
        if (textoInstrucciones != null) textoInstrucciones.gameObject.SetActive(false);

        if (botonToggleBarreras != null) botonToggleBarreras.gameObject.SetActive(false);

        if (textoPuntuacion != null) textoPuntuacion.gameObject.SetActive(false);
    }

    public void BoloDerribado()
    {
        if (bolosDerribados < TOTAL_BOLOS)
        {
            bolosDerribados++;
        }
    }

    void VerificarDetencionBolaPorVelocidad()
    {
        if (detencionProcesada) return;

        bool bolaQuieta = bolaRb.linearVelocity.magnitude < umbralVelocidadBola &&
                             bolaRb.angularVelocity.magnitude < umbralVelocidadBola;

        bool bolosActivos = pinManager != null && pinManager.HayBolosMoviendose(umbralVelocidadBola);

        if (bolaQuieta && !bolosActivos)
        {
            tiempoBolaQuieta += Time.deltaTime;

            if (tiempoBolaQuieta >= tiempoDetencionParaPanel)
            {
                IniciarFinalizacionLanzamiento();
            }
        }
        else
        {
            tiempoBolaQuieta = 0f;
        }
    }

    private void IniciarFinalizacionLanzamiento()
    {
        if (detencionProcesada) return;
        detencionProcesada = true;
        StartCoroutine(ProcesarFinalizacionLanzamiento());
    }

    private IEnumerator ProcesarFinalizacionLanzamiento()
    {
        if (bolaRb != null)
        {
            bolaRb.linearVelocity = Vector3.zero;
            bolaRb.angularVelocity = Vector3.zero;
            bolaRb.isKinematic = true;
        }

        yield return new WaitForSecondsRealtime(tiempoEsperaConteoBolos);

        FinalizarLanzamientoYMostrarPanel();
    }

    void FinalizarLanzamientoYMostrarPanel()
    {
        estadoActual = EstadoJuego.Finalizado;

        if (menuManager != null)
        {
            menuManager.MostrarPanelScore();
        }

        if (panelFinal != null && scoreManager != null && scoreManager.jugadorActual != null)
        {
            panelFinal.gameObject.SetActive(true);
            TextMeshProUGUI textoPanel = panelFinal.GetComponentInChildren<TextMeshProUGUI>();
            if (textoPanel != null)
            {
                string nombreJugador = scoreManager.jugadorActual.nombre;
                textoPanel.text = $"Turno terminado!\n{nombreJugador} derribo: {bolosDerribados} bolos.";
            }
        }

        if (scoreManager != null)
        {
            scoreManager.MostrarPanelDeTurno(bolosDerribados);
        }
    }

    public void ReiniciarRondaParaNuevoTurno(string mensajeInstruccion)
    {
        if (pinManager != null) pinManager.ReiniciarBolos();

        if (bolaRb != null) bolaRb.isKinematic = true;
        if (ballLauncher != null) ballLauncher.ResetearEIniciarPosicionamiento(topCamera, preLaunchCamera);

        bolosDerribados = 0;
        ResetearVariablesDeDetencion();
        estadoActual = EstadoJuego.Posicionamiento;

        if (textoInstrucciones != null)
        {
            textoInstrucciones.gameObject.SetActive(true);
            textoInstrucciones.text = mensajeInstruccion;
        }

        if (botonLanzar != null) botonLanzar.gameObject.SetActive(true);
        if (botonToggleBarreras != null) botonToggleBarreras.gameObject.SetActive(true);

        if (scoreManager != null)
        {
            scoreManager.ActualizarUIFrame(scoreManager.frameActual);
        }

        ActualizarPuntuacionSimple();

        if (panelFinal != null) panelFinal.gameObject.SetActive(false);
        if (panelScoreGlobal != null) panelScoreGlobal.SetActive(false);

        if (textoPuntuacion != null) textoPuntuacion.gameObject.SetActive(true);

        topCamera.gameObject.SetActive(true);
        preLaunchCamera.gameObject.SetActive(false);
        ballCamera.gameObject.SetActive(false);
    }

    public void PrepararSegundoLanzamiento(string mensajeInstruccion)
    {
        if (pinManager != null) pinManager.EliminarBolosCaidos();

        if (bolaRb != null) bolaRb.isKinematic = true;
        if (ballLauncher != null) ballLauncher.ResetearEIniciarPosicionamiento(topCamera, preLaunchCamera);

        bolosDerribados = 0;
        ResetearVariablesDeDetencion();
        estadoActual = EstadoJuego.Posicionamiento;

        if (textoInstrucciones != null)
        {
            textoInstrucciones.gameObject.SetActive(true);
            textoInstrucciones.text = mensajeInstruccion;
        }

        if (botonLanzar != null) botonLanzar.gameObject.SetActive(true);
        if (botonToggleBarreras != null) botonToggleBarreras.gameObject.SetActive(true);

        ActualizarPuntuacionSimple();

        if (panelFinal != null) panelFinal.gameObject.SetActive(false);
        if (panelScoreGlobal != null) panelScoreGlobal.SetActive(false);

        if (textoPuntuacion != null) textoPuntuacion.gameObject.SetActive(true);

        topCamera.gameObject.SetActive(true);
        preLaunchCamera.gameObject.SetActive(false);
        ballCamera.gameObject.SetActive(false);
    }

    void SeguirBolaConCamara()
    {
        if (ballCamera != null && ballCamera.gameObject.activeSelf && bola != null)
        {
            Vector3 posicionCamara = ballCamera.transform.position;
            float nuevaZ = bola.transform.position.z + offsetZCamara;

            nuevaZ = Mathf.Min(nuevaZ, posicionZFinalCamara);

            posicionCamara.z = Mathf.Lerp(posicionCamara.z, nuevaZ, suavizadoCamara * Time.deltaTime);
            ballCamera.transform.position = posicionCamara;
        }
    }

    void ActualizarPuntuacionSimple()
    {
        if (textoPuntuacion != null && scoreManager != null && scoreManager.jugadorActual != null)
        {
            textoPuntuacion.text = $"{scoreManager.jugadorActual.nombre} | Puntos: {scoreManager.jugadorActual.puntuacionTotal}";
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FueraPista"))
        {
            if (bola != null && bola.CompareTag("Bola") && other.GetComponent<Collider>().gameObject == colliderFueraPista.gameObject)
            {
                if (estadoActual == EstadoJuego.Lanzada && !detencionProcesada)
                {
                    bolaLlegoAlFinal = true;
                    tiempoEnFinalCamara = 0f;

                    if (bolaRb != null)
                    {
                        bolaRb.linearVelocity = Vector3.zero;
                        bolaRb.angularVelocity = Vector3.zero;
                        bolaRb.isKinematic = true;
                    }
                }
            }
        }

        if (other.CompareTag("FinalPista"))
        {
            if (bola != null && bola.CompareTag("Bola") && other.GetComponent<Collider>().gameObject == colliderFinalPista.gameObject)
            {
                if (estadoActual == EstadoJuego.Lanzada && !detencionProcesada)
                {
                    bolaLlegoAlFinal = true;
                    tiempoEnFinalCamara = 0f;
                }
            }
        }
    }

    public void ReiniciarJuego()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}