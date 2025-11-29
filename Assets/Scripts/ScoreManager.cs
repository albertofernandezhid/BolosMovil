using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Jugador
{
    public string nombre = "Jugador";
    public int puntuacionTotal = 0;
    public List<int> lanzamientosFrame = new();
    public List<int> framesCompletados = new(new int[10]);

    public Jugador(int index)
    {
        nombre = $"Jugador {index}";
    }
}

[System.Serializable]
public class JugadorUI
{
    public Jugador datosJugador;
    public GameObject panelRaiz;
    public TextMeshProUGUI nombreTMP;
    public TextMeshProUGUI totalGlobalTMP;
    public List<FrameScore> framesUI = new();
}

public class ScoreManager : MonoBehaviour
{
    [Header("CONFIGURACION DE LA PARTIDA")]
    public int maxJugadores = 6;
    public int maxFrames = 10;

    [Header("ESTADO DE LA PARTIDA")]
    public int numJugadoresActuales = 1;
    public int frameActual = 1;
    public Jugador jugadorActual;
    public int indiceJugadorActual = 0;

    private readonly List<Jugador> listaJugadores = new();
    private GameManager gameManager;
    private MenuManager menuManager;

    [Header("REFERENCIAS UI DE TURNO")]
    public Canvas panelDeTurno;
    public Button botonSiguienteLanzamiento;
    public Button botonSiguienteJugador;
    public Button botonSalirAlMenu;
    public TextMeshProUGUI textoPanelTurno;

    [Header("GESTION DE UI DE PUNTUACION (MULTIJUGADOR)")]
    public GameObject panelScoreJugadorPrefab;
    public Transform contenedorJugadoresRaiz;
    public GameObject frameScorePrefab;

    private readonly List<JugadorUI> listaUIJugadores = new();

    void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (menuManager == null)
        {
            menuManager = FindFirstObjectByType<MenuManager>();
        }

        if (botonSiguienteLanzamiento != null)
        {
            botonSiguienteLanzamiento.onClick.RemoveAllListeners();
            botonSiguienteLanzamiento.onClick.AddListener(ContinuarLanzamiento);
        }
        if (botonSiguienteJugador != null)
        {
            botonSiguienteJugador.onClick.RemoveAllListeners();
            botonSiguienteJugador.onClick.AddListener(FinalizarTurnoYPasar);
        }
        if (botonSalirAlMenu != null)
        {
            botonSalirAlMenu.onClick.RemoveAllListeners();
            botonSalirAlMenu.onClick.AddListener(SalirAlMenu);
        }

        if (panelDeTurno != null) panelDeTurno.gameObject.SetActive(false);
    }

    public void IniciarPartidaMultijugador(int num)
    {
        numJugadoresActuales = Mathf.Clamp(num, 1, maxJugadores);
        listaJugadores.Clear();
        listaUIJugadores.Clear();

        for (int i = 1; i <= numJugadoresActuales; i++)
        {
            listaJugadores.Add(new Jugador(i));
        }

        indiceJugadorActual = 0;
        frameActual = 1;
        jugadorActual = listaJugadores[indiceJugadorActual];

        ConfigurarUITodosLosJugadores();

        if (gameManager != null)
        {
            gameManager.ReiniciarRondaParaNuevoTurno(
                $"{jugadorActual.nombre}, Frame {frameActual} (Tiro 1). Arrastra para mover."
            );
        }
    }

    private void ConfigurarUITodosLosJugadores()
    {
        if (contenedorJugadoresRaiz != null)
        {
            List<GameObject> childrenToDestroy = new();
            foreach (Transform child in contenedorJugadoresRaiz)
            {
                childrenToDestroy.Add(child.gameObject);
            }
            foreach (GameObject child in childrenToDestroy)
            {
                Destroy(child);
            }
        }

        if (panelScoreJugadorPrefab == null || contenedorJugadoresRaiz == null)
        {
            return;
        }

        for (int i = 0; i < listaJugadores.Count; i++)
        {
            Jugador jugador = listaJugadores[i];

            GameObject jugadorPanelObj = Instantiate(panelScoreJugadorPrefab, contenedorJugadoresRaiz);

            JugadorUI ui = new()
            {
                datosJugador = jugador,
                panelRaiz = jugadorPanelObj
            };

            TextMeshProUGUI[] todosLosTmps = jugadorPanelObj.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (var tmp in todosLosTmps)
            {
                if (tmp.gameObject.name == "NombreJugador_TMP")
                {
                    ui.nombreTMP = tmp;
                }
                else if (tmp.gameObject.name == "PuntuacionTotalGlobal_TMP")
                {
                    ui.totalGlobalTMP = tmp;
                }
            }

            if (ui.nombreTMP != null)
            {
                ui.nombreTMP.text = jugador.nombre;
            }

            Transform contenedorFrames = jugadorPanelObj.transform.Find("PanelScoreIndividual/ContenedorFrames");

            if (contenedorFrames == null)
            {
                HorizontalLayoutGroup hlg = jugadorPanelObj.transform.GetComponentsInChildren<HorizontalLayoutGroup>(true)
                       .FirstOrDefault(hlg => hlg != null && hlg.gameObject.name == "ContenedorFrames");

                if (hlg != null)
                {
                    contenedorFrames = hlg.transform;
                }
            }

            if (contenedorFrames != null)
            {
                for (int j = 0; j < maxFrames; j++)
                {
                    if (frameScorePrefab != null)
                    {
                        GameObject frameObj = Instantiate(frameScorePrefab, contenedorFrames);

                        FrameScore frameScript = frameObj.GetComponentInChildren<FrameScore>(true);

                        if (frameScript != null)
                        {
                            ui.framesUI.Add(frameScript);
                        }
                    }
                }
            }

            listaUIJugadores.Add(ui);
        }
    }

    public void ActualizarUIFrame(int frame)
    {
        JugadorUI uiActual = listaUIJugadores.FirstOrDefault(ui => ui.datosJugador == jugadorActual);
        if (uiActual == null) return;

        int index = frame - 1;
        int puntuacionTotalRecalculada = 0;

        if (index >= 0 && index < uiActual.framesUI.Count)
        {
            FrameScore frameUI = uiActual.framesUI[index];

            string tiro1 = "";
            string tiro2 = "";

            if (jugadorActual.lanzamientosFrame.Count > 0)
            {
                tiro1 = jugadorActual.lanzamientosFrame[0].ToString();

                if (jugadorActual.lanzamientosFrame.Count > 1)
                {
                    tiro2 = jugadorActual.lanzamientosFrame[1].ToString();
                }
            }

            string totalAcumulado = jugadorActual.framesCompletados[index] > 0 ? jugadorActual.framesCompletados[index].ToString() : "";

            frameUI.ActualizarFrameUI(tiro1, tiro2, totalAcumulado);
        }

        for (int i = 0; i < maxFrames; i++)
        {
            puntuacionTotalRecalculada += jugadorActual.framesCompletados[i];
        }

        if (jugadorActual.lanzamientosFrame.Count > 0 && jugadorActual.framesCompletados[index] == 0)
        {
            puntuacionTotalRecalculada += jugadorActual.lanzamientosFrame.Sum();
        }

        jugadorActual.puntuacionTotal = puntuacionTotalRecalculada;

        if (uiActual.totalGlobalTMP != null)
        {
            uiActual.totalGlobalTMP.text = $"Puntos totales: {jugadorActual.puntuacionTotal}";
        }
    }

    public void MostrarPanelDeTurno(int bolosDerribadosEnLanzamiento)
    {
        jugadorActual.lanzamientosFrame.Add(bolosDerribadosEnLanzamiento);
        int lanzamientosRealizados = jugadorActual.lanzamientosFrame.Count;
        int bolosTotalesDerribadosFrame = jugadorActual.lanzamientosFrame.Sum();

        if (botonSiguienteLanzamiento != null) botonSiguienteLanzamiento.gameObject.SetActive(false);
        if (botonSiguienteJugador != null) botonSiguienteJugador.gameObject.SetActive(false);
        if (botonSalirAlMenu != null) botonSalirAlMenu.gameObject.SetActive(true);

        bool frameTerminado = false;

        if (lanzamientosRealizados >= 2 || (lanzamientosRealizados == 1 && bolosDerribadosEnLanzamiento == 10))
        {
            if (botonSiguienteJugador != null) botonSiguienteJugador.gameObject.SetActive(true);
            frameTerminado = true;
        }
        else
        {
            if (botonSiguienteLanzamiento != null) botonSiguienteLanzamiento.gameObject.SetActive(true);
        }

        if (frameTerminado)
        {
            jugadorActual.framesCompletados[frameActual - 1] = bolosTotalesDerribadosFrame;
            ActualizarUIFrame(frameActual);
        }
        else
        {
            ActualizarUIFrame(frameActual);
        }

        if (textoPanelTurno != null)
        {
            textoPanelTurno.text = $"{jugadorActual.nombre} - Frame {frameActual}\nBolos caidos: {bolosDerribadosEnLanzamiento}";
        }

        if (panelDeTurno != null)
        {
            panelDeTurno.gameObject.SetActive(true);
        }

        // Time.timeScale se gestiona en MenuManager.MostrarPanelScore()
    }

    private void DesactivarBotonesDeTurno()
    {
        if (botonSiguienteLanzamiento != null) botonSiguienteLanzamiento.gameObject.SetActive(false);
        if (botonSiguienteJugador != null) botonSiguienteJugador.gameObject.SetActive(false);
        if (botonSalirAlMenu != null) botonSalirAlMenu.gameObject.SetActive(false);
    }

    public void ContinuarLanzamiento()
    {
        DesactivarBotonesDeTurno();

        if (menuManager != null)
        {
            menuManager.OcultarPanelScore(); // Time.timeScale = 1f; gameUI.SetActive(true);
        }

        if (gameManager != null)
        {
            gameManager.PrepararSegundoLanzamiento(
                $"{jugadorActual.nombre}, Frame {frameActual} (Tiro 2). Arrastra para mover."
            );
        }
    }

    public void FinalizarTurnoYPasar()
    {
        DesactivarBotonesDeTurno();

        if (menuManager != null)
        {
            menuManager.OcultarPanelScore(); // Time.timeScale = 1f; gameUI.SetActive(true);
        }

        PasarTurno();
    }

    public void SalirAlMenu()
    {
        if (menuManager != null)
        {
            menuManager.MostrarMainMenu(); // Incluye Time.timeScale = 0f;
        }
    }

    private void PasarTurno()
    {
        jugadorActual.lanzamientosFrame.Clear();

        bool esUltimoJugador = (indiceJugadorActual == listaJugadores.Count - 1);

        if (frameActual >= maxFrames && esUltimoJugador)
        {
            return;
        }

        if (esUltimoJugador)
        {
            frameActual++;
            indiceJugadorActual = 0;
        }
        else
        {
            indiceJugadorActual++;
        }

        jugadorActual = listaJugadores[indiceJugadorActual];

        if (gameManager != null)
        {
            gameManager.ReiniciarRondaParaNuevoTurno(
                $"{jugadorActual.nombre}, Frame {frameActual} (Tiro 1). Arrastra para mover."
            );
        }
    }
}