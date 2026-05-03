using UnityEngine;

public class KeySlotPulse : MonoBehaviour
{
    public float pulseScale = 1.15f; // Насколько сильно увеличивается (1.15 = +15%)
    public float pulseDuration = 0.3f; // Как долго длится анимация
    
    private Vector3 originalScale;
    private bool isPulsing = false;
    private float pulseTimer = 0f;

    void Start()
    {
        // Запоминаем исходный размер слота
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (isPulsing)
        {
            pulseTimer += Time.deltaTime / pulseDuration;
            
            // Плавное увеличение и уменьшение (синусоида)
            float scaleMultiplier = Mathf.Lerp(1f, pulseScale, Mathf.Sin(pulseTimer * Mathf.PI));
            transform.localScale = originalScale * scaleMultiplier;
            
            // Когда время вышло, возвращаем исходный размер
            if (pulseTimer >= 1f)
            {
                isPulsing = false;
                transform.localScale = originalScale;
            }
        }
    }

    // Этот метод мы будем вызывать из KeyInventory, когда добавляем ключ
    public void Pulse()
    {
        isPulsing = true;
        pulseTimer = 0f;
    }
}