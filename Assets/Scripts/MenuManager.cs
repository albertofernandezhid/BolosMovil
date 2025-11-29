using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Paneles de Menú")]
    public GameObject mainMenuPanel;
    public GameObject optionsMenuPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameUI; // Referencia a la UI de la partida (textoPuntuacion, etc.)

    [Header("Referencias de Scripts")]
    public GameManager gameManager;

    private void Start()
    {
        // Asegurar que solo el menú principal esté activo al inicio.
        MostrarMainMenu();

        // El MenuManager debe persistir si reiniciamos la escena (opcional, pero útil para ajustes)
        DontDestroyOnLoad(gameObject);
    }

    // --- Control de Estados de Menú ---

    public void MostrarMainMenu()
    {
        Time.timeScale = 0f; // Pausar el tiempo mientras está en el menú principal/opciones

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (optionsMenuPanel != null) optionsMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameUI != null) gameUI.SetActive(false); // Ocultar la UI del juego
    }

    public void MostrarOptionsMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsMenuPanel != null) optionsMenuPanel.SetActive(true);
    }

    public void OcultarOptionsMenu()
    {
        MostrarMainMenu(); // Volver al menú principal
    }

    // --- Control de Pausa en Partida ---

    public void PausarJuego()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        Time.timeScale = 0f;
    }

    public void ReanudarJuego()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
        Time.timeScale = 1f;
    }

    // --- Acciones de Botones ---

    public void IniciarPartida()
    {
        // Ocultar todos los menús
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsMenuPanel != null) optionsMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true); // Mostrar la UI del juego

        Time.timeScale = 1f; // Reanudar el tiempo

        if (gameManager != null)
        {
            // Nota: Se asume que IniciarJuego() ya reinicia la bola/bolos.
            gameManager.IniciarJuego();
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