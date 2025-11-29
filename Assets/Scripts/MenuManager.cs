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

    [Header("Referencias de Scripts")]
    public GameManager gameManager;
    public ScoreManager scoreManager; // Esta referencia es clave

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

    public void PausarJuego()
    {
        Time.timeScale = 0f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(false);
    }

    public void ReanudarJuego()
    {
        Time.timeScale = 1f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
    }

    public void IniciarPartida()
    {
        Time.timeScale = 1f;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsMenuPanel != null) optionsMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(false);

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

    // Nueva función para reiniciar el juego sin recargar la escena
    public void ReiniciarPartida()
    {
        if (scoreManager != null)
        {
            // 1. Obtener el número de jugadores que se estaba usando.
            int numJugadores = scoreManager.numJugadoresActuales;

            // 2. Ejecutar la lógica de inicio de partida con ese número.
            IniciarPartidaConJugadores(numJugadores);

            // 3. Ocultar el panel de pausa
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        }
        else
        {
            // Fallback en caso de que ScoreManager no esté disponible
            Debug.LogError("ScoreManager no está asignado o disponible para reiniciar la partida.");
            // Si falla, al menos salimos de la pausa
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