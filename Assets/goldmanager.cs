using UnityEngine;

public class GoldManager : MonoBehaviour
{
    [Header("Altın Ayarları")]
    public int currentGold = 0;
    public int targetGold = 3; // Hedef altın sayısı

    [Header("Işınlanma (Teleport)")]
    public Transform teleportPoint; // 3 altın olunca gidilecek yer
    public GameObject character1;
    public GameObject character2;
    public Vector3 characterOffset = new Vector3(0.5f, 0f, 0f);

    [Header("Kamera Kontrolü")]
    public DualTargetCamera dualCameraScript; // Kameradaki kodu buraya bağlayacağız
    public float newCameraZOffset = -5f;      // Işınlanınca ayarlanacak yeni Z değeri

    // Altın objeleri bu fonksiyonu çağıracak
    public void AddGold()
    {
        currentGold++;
        Debug.Log("Altın Toplandı! Mevcut: " + currentGold);

        if (currentGold >= targetGold)
        {
            TeleportCharacters();
        }
    }

    private void TeleportCharacters()
    {
        if (character1 != null && character2 != null && teleportPoint != null)
        {
            // Karakterleri ışınla
            character1.transform.position = teleportPoint.position - characterOffset;
            character2.transform.position = teleportPoint.position + characterOffset;

            // Fiziksel hataları önle
            ResetVelocity(character1);
            ResetVelocity(character2);

            // --- KAMERANIN Z DEĞERİNİ GÜNCELLE ---
            if (dualCameraScript != null)
            {
                // Mevcut X ve Y değerlerini koruyup, sadece Z değerini -5 yapıyoruz
                Vector3 currentOffset = dualCameraScript.offset;
                dualCameraScript.offset = new Vector3(currentOffset.x, currentOffset.y, newCameraZOffset);
            }
        }
    }

    private void ResetVelocity(GameObject character)
    {
        Rigidbody2D rb = character.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}