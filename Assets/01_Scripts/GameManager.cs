using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CellSpawner cellSpawner;

    public float roundDuration = 10f;
    public float timeRemaining;

    public int roundNumber = 0;

    public int roundKills = 0;
    public int roundSurvivors = 0;

    public UIManager uiManager;
    public bool roundActive;

    private int killsBeforeRound;
    private int survivorsBeforeRound;

    void Start()
    {
        StartRound();
    }

    void Update()
    {
        if (!roundActive)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            EndRound();
        }
    }

    void StartRound()
    {
        roundNumber++;

        timeRemaining = roundDuration;
        roundActive = true;

        killsBeforeRound = Cell.killedCount;
        survivorsBeforeRound = Cell.survivedCount;
        uiManager.ClearResults();

        cellSpawner.SpawnCells();
    }

    void EndRound()
    {
        roundActive = false;

        cellSpawner.ClearCells();

        roundKills = Cell.killedCount - killsBeforeRound;
        roundSurvivors = Cell.survivedCount - survivorsBeforeRound;

        uiManager.ShowRoundResults(roundKills,roundSurvivors);

        StartCoroutine(NextRound());
    }

    IEnumerator NextRound()
    {
        yield return new WaitForSeconds(1f);

        StartRound();
    }
}