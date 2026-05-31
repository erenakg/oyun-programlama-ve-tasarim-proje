using UnityEngine;

public class DualTargetCamera : MonoBehaviour
{
    public Transform player1;
    public Transform player2;

    [Header("Kamera Ayarları")]
    public float smoothTime = 0.3f; // Kameranın hızı (daha küçük sayı = daha hızlı takip)
    public Vector3 offset = new Vector3(0, 2, -10); // Kameranın karakterlere göre uzaklığı

    private Vector3 velocity = Vector3.zero;

    void LateUpdate() // Kameralar için LateUpdate kullanmak sarsıntıyı önler
    {
        if (player1 == null || player2 == null) return;

        // 1. İki oyuncunun arasındaki orta noktayı hesapla
        Vector3 centerPoint = (player1.position + player2.position) / 2f;

        // 2. Hedef konumu belirle (orta nokta + verdiğimiz uzaklık)
        Vector3 targetPosition = centerPoint + offset;

        // 3. Kamerayı yumuşak bir şekilde hedef konuma taşı
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}