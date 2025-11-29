using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Referencias de Scripts")]
    public GameManager gameManager;
    public ScoreManager scoreManager;

    private void Start()
    {
        MostrarMainMenu();
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

        if (gameUI != null) gameUI.SetActive(true);

        if (gameManager != null)
        {
            gameManager.IniciarJuego();
        }
    }

    // Usada por GameManager después de un lanzamiento
    public void MostrarPanelScore()
    {
        Time.timeScale = 0f;
        if (gameUI != null) gameUI.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(true);

        if (opcionesPanel != null) opcionesPanel.SetActive(false);

        // El GameManager (o ScoreManager.MostrarPanelDeTurno) es quien configura los botones de SIGUIENTE.
        // Aquí solo nos aseguramos de que no se muestre el botón Reanudar si no es el caso.
        if (scoreManager != null)
        {
            scoreManager.ConfigurarPanelScoreParaReanudar(false);
        }
    }

    // Usada por el botón Score/Opciones en PanelJuego
    public void MostrarPanelScoreDesdeOpciones()
    {
        Time.timeScale = 0f;
        if (gameUI != null) gameUI.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(true);
        if (opcionesPanel != null) opcionesPanel.SetActive(false);

        // CLAVE: SOLO llama a ConfigurarPanelScoreParaReanudar(true) para mostrar el botón REANUDAR.
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
            Debug.LogError("ScoreManager no está asignado o disponible para reiniciar la partida.");
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
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}