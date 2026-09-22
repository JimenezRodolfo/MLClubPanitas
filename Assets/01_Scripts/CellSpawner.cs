using UnityEngine;

public class CellSpawner : MonoBehaviour
{
    public GameObject cellPrefab;

    public int cellsPerRound = 10;

    public float minX = -7f;
    public float maxX = 7f;

    public float minY = -4f;
    public float maxY = 4f;

    public void SpawnCells()
    {
        for (int i = 0; i < cellsPerRound; i++)
        {
            Vector2 randomPosition = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );

            GameObject obj = Instantiate(
                cellPrefab,
                randomPosition,
                Quaternion.identity
            );

            Cell cell = obj.GetComponent<Cell>();
            if (cell != null && LearningManager.Instance != null)
            {
                cell.Setup(LearningManager.Instance.GetNextTraits());
            }
        }
    }

    public void ClearCells()
    {
        Cell[] cells = FindObjectsByType<Cell>(FindObjectsSortMode.None);

        foreach (Cell cell in cells)
        {
            cell.Survive();
        }
    }
}