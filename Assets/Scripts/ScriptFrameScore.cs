using UnityEngine;
using TMPro;

public class FrameScore : MonoBehaviour
{
    [Header("Tiros Individuales")]
    public TextMeshProUGUI tiro1_TMP;
    public TextMeshProUGUI tiro2_TMP;

    [Header("Puntuacion Acumulada")]
    public TextMeshProUGUI totalFrame_TMP;

    private int frameNumber;
    private string playerName;

    void Awake()
    {
        Transform framePanelRoot = transform.parent;

        if (framePanelRoot == null)
        {
            return;
        }

        TextMeshProUGUI[] todosLosTmps = framePanelRoot.GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (var tmp in todosLosTmps)
        {
            if (tmp.gameObject.name == "TotalFrame_TMP")
            {
                totalFrame_TMP = tmp;
            }
            else if (tmp.gameObject.name == "Tiro1_TMP")
            {
                tiro1_TMP = tmp;
            }
            else if (tmp.gameObject.name == "Tiro2_TMP")
            {
                tiro2_TMP = tmp;
            }
        }
    }

    public void InicializarDebug(int frame, string jugadorNombre)
    {
        frameNumber = frame;
        playerName = jugadorNombre;
    }

    public void ActualizarFrameUI(string tiro1, string tiro2, string totalAcumulado)
    {
        if (tiro1_TMP != null)
        {
            tiro1_TMP.text = tiro1;
        }

        if (tiro2_TMP != null)
        {
            tiro2_TMP.text = tiro2;
        }

        if (totalFrame_TMP != null)
        {
            totalFrame_TMP.text = totalAcumulado;
        }
    }
}