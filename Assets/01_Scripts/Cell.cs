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
        if (resolved) return;
        resolved = true;

        killedCount++;
        if (LearningManager.Instance != null)
        {
            LearningManager.Instance.RegisterResult(traits, false);
        }
        Destroy(gameObject);
    }

    public void Survive()
    {
        if (resolved) return;
        resolved = true;

        survivedCount++;
        if (LearningManager.Instance != null)
        {
            LearningManager.Instance.RegisterResult(traits, true);
        }
        Destroy(gameObject);
    }

    public static void ResetCounters()
    {
        killedCount = 0;
        survivedCount = 0;
    }
}
