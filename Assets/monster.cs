using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public enum PatrolMode
    {
        Loop,       // Bitişten tekrar baştaki noktaya bağlanır ve sürekli döner
        PingPong    // Bitiş noktasına gelince geriye doğru sarmaya başlar
    }

    [Header("Hareket Ayarları")]
    public float speed = 3f;
    public float waitTime = 1f;
    public PatrolMode patrolMode = PatrolMode.Loop;

    [Header("Devriye Noktaları")]
    public List<Transform> patrolPoints;
    
    [Header("Spline (Kıvrım) Ayarları")]
    [Tooltip("İki nokta arasına eklenecek görünmez yumuşatma noktası sayısı. Sayı arttıkça kavis pürüzsüzleşir.")]
    public int splineResolution = 10; 

    // Arka planda hesaplanan kusursuz kavisli yol
    private List<Vector2> pathPoints = new List<Vector2>();
    // Hangi noktaların durup beklenecek "Ana Durak" olduğunu tutar
    private HashSet<int> keyPointIndices = new HashSet<int>(); 

    private int currentTargetIndex = 0;
    private bool isWaiting = false;
    private bool isMovingForward = true;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        CalculatePath();

        if (pathPoints.Count > 0)
        {
            transform.position = pathPoints[0];
        }
    }

    void Update()
    {
        if (pathPoints.Count == 0 || isWaiting) return;

        // --- YÖN ÇEVİRME (FLIP) ---
        float directionX = pathPoints[currentTargetIndex].x - transform.position.x;
        if (directionX > 0.05f)
        {
            spriteRenderer.flipX = false; // Sağa bak
        }
        else if (directionX < -0.05f)
        {
            spriteRenderer.flipX = true;  // Sola bak
        }
    }

    void FixedUpdate()
    {
        if (pathPoints.Count == 0 || isWaiting)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 currentPos = rb.position;
        Vector2 targetPos = pathPoints[currentTargetIndex];

        // Hedefe yeterince yaklaştıysak (Overshoot'u engellemek için)
        if (Vector2.Distance(currentPos, targetPos) < 0.2f)
        {
            // Eğer geldiğimiz yer senin eklediğin o ana noktalardan biriyse bekleme yap
            if (keyPointIndices.Contains(currentTargetIndex))
            {
                StartCoroutine(WaitRoutine());
            }
            else
            {
                // Ana durak değilse (ara kavis noktasıysa) beklemeden hızla bir sonrakine geç
                AdvanceIndex();
            }
        }
        else
        {
            // Doğrudan hedef noktaya fiziksel kuvvetle ilerle
            Vector2 moveDirection = (targetPos - currentPos).normalized;
            rb.linearVelocity = moveDirection * speed;
        }
    }

    private IEnumerator WaitRoutine()
    {
        isWaiting = true;
        rb.linearVelocity = Vector2.zero;
        
        yield return new WaitForSeconds(waitTime);
        
        AdvanceIndex();
        isWaiting = false;
    }

    // Hedef index'i seçili moda göre ileri veya geri sarar
    private void AdvanceIndex()
    {
        if (patrolMode == PatrolMode.Loop)
        {
            currentTargetIndex = (currentTargetIndex + 1) % pathPoints.Count;
        }
        else // PingPong Modu
        {
            if (isMovingForward)
            {
                currentTargetIndex++;
                if (currentTargetIndex >= pathPoints.Count)
                {
                    currentTargetIndex = pathPoints.Count - 2; // Sondan bir öncekine dön
                    isMovingForward = false;                   // Yönü tersine çevir
                }
            }
            else
            {
                currentTargetIndex--;
                if (currentTargetIndex < 0)
                {
                    currentTargetIndex = 1; // Baştan bir sonrakine dön
                    isMovingForward = true; // Yönü tekrar ileri yap
                }
            }
        }
    }

    // --- MATEMATİKSEL SPLINE (CATMULL-ROM) OLUŞTURMA ---
    private void CalculatePath()
    {
        pathPoints.Clear();
        keyPointIndices.Clear();

        if (patrolPoints == null || patrolPoints.Count == 0) return;
        
        // Boş bırakılan Transform varsa sistemi çökertmemesi için koruma
        foreach (Transform t in patrolPoints) if (t == null) return; 

        int numPoints = patrolPoints.Count;

        // Kavis oluşturmak için en az 3 nokta gerekir, azsa düz çizgi çeker
        if (numPoints < 3)
        {
            for (int i = 0; i < numPoints; i++)
            {
                pathPoints.Add(patrolPoints[i].position);
                keyPointIndices.Add(i);
            }
            return;
        }

        for (int i = 0; i < numPoints; i++)
        {
            // Ana durak noktasının indeksini kaydet (Burada durup bekleyecek)
            keyPointIndices.Add(pathPoints.Count);

            // PingPong modundaysak ve son noktadaysak, spline'ı bağlama, direkt bitir
            if (patrolMode == PatrolMode.PingPong && i == numPoints - 1)
            {
                pathPoints.Add(patrolPoints[i].position);
                break;
            }

            Vector2 p0 = patrolPoints[GetWrappedIndex(i - 1, numPoints)].position;
            Vector2 p1 = patrolPoints[i].position;
            Vector2 p2 = patrolPoints[GetWrappedIndex(i + 1, numPoints)].position;
            Vector2 p3 = patrolPoints[GetWrappedIndex(i + 2, numPoints)].position;

            // PingPong modu için köşeleri sertleştir (yumuşak dönüş yerine tam noktada dursun)
            if (patrolMode == PatrolMode.PingPong)
            {
                if (i == 0) p0 = p1;
                if (i == numPoints - 2) p3 = p2;
            }

            // Çözünürlüğe göre ara noktaları hesapla
            for (int j = 0; j < splineResolution; j++)
            {
                float t = j / (float)splineResolution;
                pathPoints.Add(GetCatmullRomPosition(t, p0, p1, p2, p3));
            }
        }
    }

    private int GetWrappedIndex(int i, int max)
    {
        if (i < 0) return max - 1;
        if (i >= max) return 0;
        return i;
    }

    private Vector2 GetCatmullRomPosition(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Vector2 a = 2f * p1;
        Vector2 b = p2 - p0;
        Vector2 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector2 d = -p0 + 3f * p1 - 3f * p2 + p3;

        return 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
    }

    // --- EDİTÖR İÇİ ÇİZİM (Oyunu başlatmadan rotayı görebilmen için) ---
    private void OnDrawGizmos()
    {
        if (patrolPoints == null || patrolPoints.Count < 2) return;
        foreach (Transform t in patrolPoints) if (t == null) return;

        Gizmos.color = Color.cyan;
        int numPoints = patrolPoints.Count;

        if (numPoints < 3)
        {
            for (int i = 0; i < numPoints - 1; i++) Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
            if (patrolMode == PatrolMode.Loop && numPoints == 2) Gizmos.DrawLine(patrolPoints[1].position, patrolPoints[0].position);
        }
        else
        {
            for (int i = 0; i < numPoints; i++)
            {
                if (patrolMode == PatrolMode.PingPong && i == numPoints - 1) break;

                Vector2 p0 = patrolPoints[GetWrappedIndex(i - 1, numPoints)].position;
                Vector2 p1 = patrolPoints[i].position;
                Vector2 p2 = patrolPoints[GetWrappedIndex(i + 1, numPoints)].position;
                Vector2 p3 = patrolPoints[GetWrappedIndex(i + 2, numPoints)].position;

                if (patrolMode == PatrolMode.PingPong)
                {
                    if (i == 0) p0 = p1;
                    if (i == numPoints - 2) p3 = p2;
                }

                Vector2 previousPoint = p1;
                for (int j = 1; j <= splineResolution; j++)
                {
                    float t = j / (float)splineResolution;
                    Vector2 currentPoint = GetCatmullRomPosition(t, p0, p1, p2, p3);
                    Gizmos.DrawLine(previousPoint, currentPoint);
                    previousPoint = currentPoint;
                }
            }
        }

        // Ana noktaları Kırmızı top olarak göster
        Gizmos.color = Color.red;
        foreach (Transform t in patrolPoints)
        {
            Gizmos.DrawSphere(t.position, 0.15f);
        }
    }
}