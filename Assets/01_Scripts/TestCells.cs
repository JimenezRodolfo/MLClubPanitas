using UnityEngine;

public class TestCells : MonoBehaviour
{
    public Cell cellPrefab;
    public int cellCount = 10;
    public float roundDuration = 10f;

    private Cell[] cells;
    private float timer;
    private bool ended = false;

    void Start()
    {
        Cell.ResetCounters();
        timer = roundDuration;
        cells = new Cell[cellCount];

        for (int i = 0; i < cellCount; i++)
        {
            Vector2 pos = new Vector2(Random.Range(-6f, 6f), Random.Range(-3f, 3f));
            Cell cell = Instantiate(cellPrefab, pos, Quaternion.identity);

            Color color = new Color(Random.value, Random.value, Random.value, 1f);
            float size = Random.Range(0.4f, 1.5f);
            cell.Setup(new CellTraits(color, size));

            cells[i] = cell;
        }
    }

    void Update()
    {
        if (ended) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ended = true;
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null) cells[i].Survive();
            }
            Debug.Log("Eliminadas: " + Cell.killedCount + " | Sobrevivieron: " + Cell.survivedCount);
        }
    }
}
