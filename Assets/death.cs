using System.Collections;
using UnityEngine;
using TMPro;

public class death : MonoBehaviour
{
    [Header("Death Zone")]
    [SerializeField] private BoxCollider2D _boxCollider;

    [Header("Teleport")]
    [SerializeField] private Transform _respawnPoint;
    [SerializeField] private GameObject _character1;
    [SerializeField] private GameObject _character2;
    [SerializeField] private Vector3 _characterOffset = Vector3.zero;

    [Header("UI")]
    [SerializeField] private TMP_Text _deathText;
    [SerializeField] private GameObject _deathBackground; // Arka plan için eklendi
    [SerializeField] private float _messageDuration = 2f;

    [Header("Tag")]
    [SerializeField] private string _characterTag = "karakter";

    private bool _isRespawning;

    private void Awake()
    {
        if (_boxCollider == null)
            _boxCollider = GetComponent<BoxCollider2D>();

        if (_boxCollider != null)
            _boxCollider.isTrigger = true;

        // Oyun başında yazıyı kapat
        if (_deathText != null)
            _deathText.gameObject.SetActive(false);
            
        // Oyun başında arka planı da kapat
        if (_deathBackground != null)
            _deathBackground.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTriggerDeath(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryTriggerDeath(collision.collider);
    }

    private void TryTriggerDeath(Collider2D other)
    {
        if (_isRespawning) return;
        if (other == null) return;
        if (!other.CompareTag(_characterTag)) return;

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        _isRespawning = true;

        // Yazıyı ve arka planı aktif et
        if (_deathText != null)
            _deathText.gameObject.SetActive(true);
            
        if (_deathBackground != null)
            _deathBackground.SetActive(true);

        RespawnCharacter(_character1);
        RespawnCharacter(_character2);

        // Belirlenen süre kadar bekle
        yield return new WaitForSeconds(_messageDuration);

        // Süre dolunca yazıyı ve arka planı tekrar kapat
        if (_deathText != null)
            _deathText.gameObject.SetActive(false);
            
        if (_deathBackground != null)
            _deathBackground.SetActive(false);

        _isRespawning = false;
    }

    private void RespawnCharacter(GameObject character)
    {
        if (character == null || _respawnPoint == null) return;

        character.transform.position = _respawnPoint.position + _characterOffset;

        Rigidbody2D rb = character.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}