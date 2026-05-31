using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float speed = 5f;
    public float jumpForce = 8f;

    [Header("Tuş Atamaları")]
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode jumpKey;
    // S veya Aşağı yön tuşu şimdilik boş, ileride buraya eklenebilir.

    [Header("Zemin Kontrolü")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    [Header("İp Görseli (Sadece bir karaktere atayın)")]
    public LineRenderer lineRenderer;
    public Transform otherPlayer;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. Zemin Kontrolü (Zıplama bug'larını önlemek için)
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 2. Sağ / Sol Hareketi
        float moveInput = 0f;
        if (Input.GetKey(leftKey)) moveInput = -1f;
        if (Input.GetKey(rightKey)) moveInput = 1f;

        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // 3. Zıplama (Sadece yerdeyken)
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 4. İp Görselini Güncelleme
        if (lineRenderer != null && otherPlayer != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, otherPlayer.position);
        }
    }
}