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

    [Header("CÁMARAS")]
    public Camera topCamera;
    public Camera preLaunchCamera;
    public Camera ballCamera;
    public float offsetZCámara = -2f;
    public float suavizadoCámara = 5f;

    [Header("UI")]
    public Button botonLanzar;
    public Button botonToggleBarreras;
    public Button botonReiniciar;
    public TextMeshProUGUI textoInstrucciones;
    public TextMeshProUGUI textoPuntuacion;
    public Canvas panelFinal;

    [Header("LÓGICA DE DETECCIÓN")]
    public BoxCollider colliderDetectorBola;
    public BoxCollider colliderFueraPista;
    public float tiempoDetencionParaPanel = 2f;
    public float umbralVelocidadBola = 0.1f;

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
            }
        }

        if (botonLanzar != null)
        {
            botonLanzar.onClick.RemoveAllListeners();
            botonLanzar.onClick.AddListener(OnBotonLanzarClick);
        }
        if (botonToggleBarreras != null && ballPhysics != null)
        {
            botonToggleBarreras.onClick.RemoveAllListeners();
            botonToggleBarreras.onClick.AddListener(ballPhysics.ToggleBarreras);
        }

        if (botonReiniciar != null)
        {
            botonReiniciar.onClick.RemoveAllListeners();
            botonReiniciar.onClick.AddListener(ReiniciarJuego);
            botonReiniciar.gameObject.SetActive(false);
        }

        IniciarJuego();
    }

    void Update()
    {
        if (estadoActual == EstadoJuego.Lanzada)
        {
            pinManager.VerificarBolosPorAngulo(this);
            SeguirBolaConCamara();

            if (bolaEnZonaDetencion)
            {
                VerificarDetencionBola();
            }
        }
    }

    public void IniciarJuego()
    {
        if (pinManager != null) pinManager.ReiniciarBolos();
        if (ballLauncher != null) ballLauncher.ResetearBola();

        bolosDerribados = 0;
        ActualizarPuntuacion();
        detencionProcesada = false;
        tiempoBolaQuieta = 0f;
        estadoActual = EstadoJuego.Posicionamiento;

        if (panelFinal != null) panelFinal.gameObject.SetActive(false);
        if (botonLanzar != null) botonLanzar.gameObject.SetActive(true);
        if (textoInstrucciones != null) textoInstrucciones.text = "Arrastra para mover horizontalmente";
        if (textoInstrucciones != null) textoInstrucciones.gameObject.SetActive(true);
        if (botonReiniciar != null) botonReiniciar.gameObject.SetActive(false);

        topCamera.gameObject.SetActive(true);
        preLaunchCamera.gameObject.SetActive(false);
        ballCamera.gameObject.SetActive(false);
    }

    public void OnBotonLanzarClick()
    {
        if (ballLauncher != null) ballLauncher.FijarPosicionEIniciarCarga();

        estadoActual = EstadoJuego.Carga;

        topCamera.gameObject.SetActive(false);
        preLaunchCamera.gameObject.SetActive(true);
        if (botonLanzar != null) botonLanzar.gameObject.SetActive(false);
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