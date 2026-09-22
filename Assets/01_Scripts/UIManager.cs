using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameManager gameManager;

    public TMP_Text timeText;
    public TMP_Text roundText;
    public TMP_Text scoreText;
    public TMP_Text resultText;

    void Update()
    {
        timeText.text = "Tiempo: " + Mathf.CeilToInt(gameManager.timeRemaining);
        roundText.text = "Ronda: " + gameManager.roundNumber;
        scoreText.text = "Puntos: " + Cell.killedCount;
    }

    public void ShowRoundResults(int killed, int survived)
    {
        resultText.text =
            "Eliminadas: " + killed +
            "\nSobrevivientes: " + survived;
    }

    public void ClearResults()
    {
        resultText.text = "";
    }
}