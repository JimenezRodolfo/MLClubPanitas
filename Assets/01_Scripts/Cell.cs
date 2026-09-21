using UnityEngine;

public class Cell : MonoBehaviour
{
    public static int killedCount = 0;
    public static int survivedCount = 0;
    public CellTraits traits { get; private set; }
    private SpriteRenderer spriteRenderer;
    private bool resolved = false;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Setup(CellTraits newTraits)
    {
        traits = newTraits;
        resolved = false;
        spriteRenderer.color = traits.color;
        transform.localScale = Vector3.one * traits.size;
    }
    void OnMouseDown()
    {
        Kill();
    }
    public void Kill()
    {
        if (!resolved)
        {
            resolved = true;
            killedCount++;
            Destroy(gameObject);
        }
    }
    public void Survive()
    {
        if (!resolved)
        {
            resolved = true;
            survivedCount++;
            Destroy(gameObject);
        }
    }
    public static void ResetCounters()
    {
        killedCount = 0;
        survivedCount = 0;
    }
}
