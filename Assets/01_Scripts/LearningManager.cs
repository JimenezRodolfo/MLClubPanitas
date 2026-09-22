using UnityEngine;

public class LearningManager : MonoBehaviour
{
    public static LearningManager Instance;

    // ----- Límites -----
    public float minSize = 0.4f;
    public float maxSize = 1.5f;
    public float minColorValue = 0f;
    public float maxColorValue = 1f;

    // ----- Parámetros de aprendizaje -----
    public float explorationRate = 0.2f;      // probabilidad de probar algo al azar
    public float minExplorationRate = 0.05f;  // la exploración nunca baja de esto
    public float explorationDecay = 0.95f;    // se multiplica cada ronda
    public float mutationStrength = 0.1f;     // cuánto cambia un hijo respecto a su padre
    public int candidatesPerCell = 10;        // candidatos que se evalúan por célula
    public float kernelWidth = 0.15f;         // "radio de influencia" de cada experiencia
    public float sizeInfluence = 0.5f;        // cuánto pesa el tamaño frente al color (0 a 1)
    public float memoryDecay = 0.99f;         // las experiencias viejas pesan menos
    public int maxMemory = 300;               // límite de memoria (rendimiento)

    // ----- Estadísticas (para UI o para la demo) -----
    public int roundsPlayed = 0;
    public float lastSurvivalRate = 0f;

    // Memoria: dos arreglos paralelos (rasgos y si sobrevivió)
    private CellTraits[] memTraits;
    private bool[] memSurvived;
    private int memCount = 0;

    // Mejor célula conocida (se usa como "padre" de las nuevas)
    private CellTraits bestParent;
    private bool hasBestParent = false;

    private int survivedThisRound = 0;
    private int killedThisRound = 0;

    void Awake()
    {
        Instance = this;
        memTraits = new CellTraits[maxMemory];
        memSurvived = new bool[maxMemory];
    }

    // La llama Cell cuando termina su vida (eliminada o sobreviviente)
    public void RegisterResult(CellTraits traits, bool survived)
    {
        if (survived) survivedThisRound++;
        else killedThisRound++;

        Remember(traits, survived);
    }

    // El spawner pide rasgos para una célula nueva
    public CellTraits GetNextTraits()
    {
        // Sin experiencia o toca explorar: rasgos al azar
        if (memCount == 0 || Random.value < explorationRate)
        {
            return RandomTraits();
        }

        CellTraits best = RandomTraits();
        float bestScore = -1f;

        // Se generan varios candidatos y se elige el de mayor probabilidad de sobrevivir
        for (int i = 0; i < candidatesPerCell; i++)
        {
            CellTraits candidate;
            if (hasBestParent) candidate = Mutate(bestParent);
            else candidate = RandomTraits();

            float score = Score(candidate);
            if (score > bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }
        return best;
    }

    // El GameManager la llama al terminar cada ronda (después de Survive())
    public void EndRound()
    {
        int total = survivedThisRound + killedThisRound;
        if (total > 0) lastSurvivalRate = (float)survivedThisRound / total;
        else lastSurvivalRate = 0f;

        roundsPlayed++;
        Debug.Log("[ML] Ronda " + roundsPlayed + ": sobrevivieron " + survivedThisRound
                  + "/" + total + " | exploracion " + explorationRate.ToString("0.00"));

        survivedThisRound = 0;
        killedThisRound = 0;
        explorationRate = Mathf.Max(minExplorationRate, explorationRate * explorationDecay);

        FindBestParent();
    }

    // Borra lo aprendido (por si hay botón de reiniciar)
    public void ResetLearning()
    {
        memCount = 0;
        hasBestParent = false;
        survivedThisRound = 0;
        killedThisRound = 0;
        roundsPlayed = 0;
    }

    // ================== Lógica interna ==================

    private void Remember(CellTraits traits, bool survived)
    {
        // Si la memoria está llena, se olvida lo más antiguo
        if (memCount >= maxMemory)
        {
            for (int i = 1; i < memCount; i++)
            {
                memTraits[i - 1] = memTraits[i];
                memSurvived[i - 1] = memSurvived[i];
            }
            memCount--;
        }

        memTraits[memCount] = traits;
        memSurvived[memCount] = survived;
        memCount++;
    }

    private CellTraits RandomTraits()
    {
        Color c = new Color(
            Random.Range(minColorValue, maxColorValue),
            Random.Range(minColorValue, maxColorValue),
            Random.Range(minColorValue, maxColorValue),
            1f);
        float size = Random.Range(minSize, maxSize);
        return new CellTraits(c, size);
    }

    // Crea un "hijo": el padre con un pequeño cambio al azar
    private CellTraits Mutate(CellTraits parent)
    {
        float m = mutationStrength;

        float r = Mathf.Clamp(parent.color.r + Random.Range(-m, m), minColorValue, maxColorValue);
        float g = Mathf.Clamp(parent.color.g + Random.Range(-m, m), minColorValue, maxColorValue);
        float b = Mathf.Clamp(parent.color.b + Random.Range(-m, m), minColorValue, maxColorValue);
        float size = Mathf.Clamp(parent.size + Random.Range(-m, m) * (maxSize - minSize), minSize, maxSize);

        return new CellTraits(new Color(r, g, b, 1f), size);
    }

    // Busca entre las células que sobrevivieron la que tiene mejor
    // probabilidad de supervivencia estimada. Esa será el padre.
    private void FindBestParent()
    {
        hasBestParent = false;
        float bestScore = -1f;

        for (int i = 0; i < memCount; i++)
        {
            if (!memSurvived[i]) continue;

            float score = Score(memTraits[i]);
            if (score > bestScore)
            {
                bestScore = score;
                bestParent = memTraits[i];
                hasBestParent = true;
            }
        }
    }

    // Estima la probabilidad de sobrevivir de unos rasgos (entre 0 y 1):
    // se miran las experiencias cercanas y se calcula qué fracción sobrevivió.
    // Las experiencias recientes y las más cercanas pesan más.
    private float Score(CellTraits candidate)
    {
        float survivedWeight = 0f;
        float totalWeight = 0f;
        float twoSigma2 = 2f * kernelWidth * kernelWidth;

        for (int i = 0; i < memCount; i++)
        {
            float d2 = DistanceSqr(candidate, memTraits[i]);
            float w = Weight(i) * Mathf.Exp(-d2 / twoSigma2);

            totalWeight += w;
            if (memSurvived[i]) survivedWeight += w;
        }

        // Sin datos cercanos, se asume 50% de probabilidad
        float prior = 0.5f;
        return (survivedWeight + prior * 0.5f) / (totalWeight + prior);
    }

    // Peso por antigüedad: la experiencia más reciente pesa 1
    private float Weight(int index)
    {
        return Mathf.Pow(memoryDecay, memCount - 1 - index);
    }

    // Distancia al cuadrado entre dos conjuntos de rasgos (r, g, b, tamaño normalizado)
    private float DistanceSqr(CellTraits a, CellTraits b)
    {
        float dr = a.color.r - b.color.r;
        float dg = a.color.g - b.color.g;
        float db = a.color.b - b.color.b;
        float ds = (a.size - b.size) / (maxSize - minSize) * sizeInfluence;
        return dr * dr + dg * dg + db * db + ds * ds;
    }
}
