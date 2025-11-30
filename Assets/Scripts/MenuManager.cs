using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Paneles de Menú")]
    public GameObject mainMenuPanel;
    public GameObject mainMenuContent;
    public GameObject optionsMenuPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameUI;
    public GameObject scorePanel;
    public GameObject opcionesPanel;

    [Header("UI Max Score")]
    public TextMeshProUGUI maxScoreTMP;
    public float blinkSpeed = 0.5f;

    [Header("Referencias de Scripts")]
    public GameManager gameManager;
    public ScoreManager scoreManager;

    private Coroutine blinkCoroutine;

    private void Start()
    {
        MostrarMainMenu();
    }

    public void ActualizarMaxScoreUI()
    {
        if (maxScoreTMP == null || scoreManager == null) return;

        int maxScore = scoreManager.ObtenerMaxScoreGuardado();

        if (maxScore > 0)
        {
            maxScoreTMP.text = $"Max score: {maxScore}";
            maxScoreTMP.fontSize = 36;
            maxScoreTMP.alignment = TextAlignmentOptions.Center;
            maxScoreTMP.gameObject.SetActive(true);

            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
            }
            blinkCoroutine = StartCoroutine(BlinkEffect());
        }
        else
        {
            maxScoreTMP.gameObject.SetActive(false);
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
        }
    }

    private IEnumerator BlinkEffect()
    {
        while (true)
        {
            if (maxScoreTMP != null)
            {
                maxScoreTMP.alpha = (maxScoreTMP.alpha > 0.1f) ? 0f : 1f;
            }
            yield return new WaitForSecondsRealtime(blinkSpeed);
        }
    }

    public void MostrarMainMenu()
    {
        Time.timeScale = 0f;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (mainMenuContent != null) mainMenuContent.SetActive(true);
        if (optionsMenuPanel != null) optionsMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameUI != null) gameUI.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(false);
        if (opcionesPanel != null) opcionesPanel.SetActive(false);
        if (gameManager != null && gameManager.panelSeleccionBola != null) gameManager.panelSeleccionBola.SetActive(false);
        if (scoreManager != null && scoreManager.panelFinalDePartida != null) scoreManager.panelFinalDePartida.SetActive(false);

        ActualizarMaxScoreUI();
    }

    public void MostrarOptionsMenu()
    {
        Time.timeScale = 0f;
        if (mainMenuContent != null) mainMenuContent.SetActive(false);
        if (optionsMenuPanel != null) optionsMenuPanel.SetActive(true);
    }

    public void OcultarOptionsMenu()
    {
        MostrarMainMenu();
    }

    public void ToggleOpciones()
    {
        if (opcionesPanel != null)
        {
            opcionesPanel.SetActive(!opcionesPanel.activeSelf);

            if (gameUI != null && !opcionesPanel.activeSelf)
            {
                gameUI.SetActive(true);
            }

            if (scorePanel != null) scorePanel.SetActive(false);
        }
    }

    public void PausarJuego()
    {
        Time.timeScale = 0f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(false);
        if (opcionesPanel != null) opcionesPanel.SetActive(false);
    }

    public void ReanudarJuego()
    {
        Time.timeScale = 1f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
        if (opcionesPanel != null) opcionesPanel.SetActive(false);
    }

    public void IniciarPartida()
    {
        Time.timeScale = 1f;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsMenuPanel != null) optionsMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(false);
        if (opcionesPanel != null) opcionesPanel.SetActive(false);
        if (gameManager != null && gameManager.panelSeleccionBola != null) gameManager.panelSeleccionBola.SetActive(false);

        if (gameUI != null) gameUI.SetActive(true);

        if (gameManager != null)
        {
            gameManager.IniciarJuego();
        }
    }

    public void MostrarPanelScore()
    {
        Time.timeScale = 0f;
        if (gameUI != null) gameUI.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(true);
        if (opcionesPanel != null) opcionesPanel.SetActive(false);

        if (scoreManager != null)
        {
            scoreManager.ConfigurarPanelScoreParaReanudar(false);
        }
    }

    public void MostrarPanelScoreDesdeOpciones()
    {
        Time.timeScale = 0f;
        if (gameUI != null) gameUI.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(true);
        if (opcionesPanel != null) opcionesPanel.SetActive(false);

        if (scoreManager != null)
        {
            scoreManager.ConfigurarPanelScoreParaReanudar(true);
        }
    }

    public void OcultarPanelScore()
    {
        Time.timeScale = 1f;
        if (scorePanel != null) scorePanel.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
    }

    public void IniciarPartidaConJugadores(int numJugadores)
    {
        if (scoreManager != null)
        {
            scoreManager.IniciarPartidaMultijugador(numJugadores);
        }
        IniciarPartida();
    }

    public void ReiniciarPartida()
    {
        if (scoreManager != null)
        {
            int numJugadores = scoreManager.numJugadoresActuales;
            IniciarPartidaConJugadores(numJugadores);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        }
        else
        {
            ReanudarJuego();
        }
    }

    public void ReiniciarEscena()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SalirJuego()
    {
        Application.Quit();
    }

    public void OnBotonAbrirSeleccionBola()
    {
        if (opcionesPanel != null) opcionesPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        if (gameManager != null)
        {
            gameManager.ActivarPanelSeleccionBola(true);
        }
    }
}