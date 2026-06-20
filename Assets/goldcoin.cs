using UnityEngine;

public class GoldCoin : MonoBehaviour
{
    private GoldManager goldManager;

    void Start()
    {
        // Oyun başladığında sahnede GoldManager'ı bul ve bağlan
        goldManager = FindFirstObjectByType<GoldManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Karakterlerimizden biri ("karakter" etiketli) altına çarparsa
        if (other.CompareTag("karakter"))
        {
            if (goldManager != null)
            {
                goldManager.AddGold(); // Sayıyı 1 artır
            }
            
            // Altın toplandığı için onu sahneden yok et
            Destroy(gameObject);
        }
    }
}