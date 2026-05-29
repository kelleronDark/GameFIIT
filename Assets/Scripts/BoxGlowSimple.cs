using UnityEngine;

public class BoxGlowSimple : MonoBehaviour
{
    [Header("Settings")]
    public Color glowColor = new Color(1f, 1f, 1f, 1f);
    public float pulseSpeed = 3f;
    public float pulseIntensity = 0.15f;
    
    private SpriteRenderer sr;
    private Color originalColor;
    private Collider2D col;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        
        if (sr != null)
            originalColor = sr.color;
    }

    void Update()
    {
        if (sr != null && col != null && col.enabled && IsOnGround())
        {
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            sr.color = Color.Lerp(originalColor, glowColor, pulse * pulseIntensity);
        }
        else
        {
            sr.color = originalColor;
        }
    }

    private bool IsOnGround()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, 0.2f);
    }
}