using UnityEngine;

public class AuraPulse : MonoBehaviour
{
    [Header("Pulse")]
    public float minScale = 1.4f;
    public float maxScale = 1.6f;
    public float pulseSpeed = 2.0f; // Цикл

    [Header("Opacity")]
    public float minAlpha = 0.4f;
    public float maxAlpha = 0.6f;

    private SpriteRenderer sr;
    private Vector3 baseScale;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        
        // Масштаб
        float s = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = baseScale * s;

        // Прозрачность
        Color c = sr.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        sr.color = c;
    }
}