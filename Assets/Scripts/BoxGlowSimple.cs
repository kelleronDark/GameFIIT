using UnityEngine;

public class BoxGlowSimple : MonoBehaviour
{
    [Header("Settings")]
    public Color glowColor = new Color(1f, 1f, 1f, 1f); // Белый (или золотой: 1, 0.9, 0.6)
    public float pulseSpeed = 3f;       // Скорость пульсации
    public float pulseIntensity = 0.15f; // Сила изменения цвета (0.1 = едва заметно, 0.3 = ярко)
    
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
        // Свечение работает только если:
        // 1. Есть SpriteRenderer
        // 2. Коллайдер включен (коробка не в руках)
        // 3. Коробка на земле (есть опора снизу)
        if (sr != null && col != null && col.enabled && IsOnGround())
        {
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            // Плавно меняем цвет между оригиналом и glowColor
            sr.color = Color.Lerp(originalColor, glowColor, pulse * pulseIntensity);
        }
        else
        {
            // Возвращаем оригинальный цвет, если условия не выполнены
            sr.color = originalColor;
        }
    }

    // Простая проверка: есть ли что-то под коробкой
    private bool IsOnGround()
    {
        // Raycast вниз на 0.2 единицы
        return Physics2D.Raycast(transform.position, Vector2.down, 0.2f);
    }
}