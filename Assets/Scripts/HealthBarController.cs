using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    public Image fillImage; // Сюда перетащи объект Health_Fill
    public RectTransform fillRect; // Сюда его же (RectTransform)
    
    [Header("Color Settings")]
    public Color greenColor = new Color(0.18f, 0.8f, 0.44f);  // Сочный изумрудный
    public Color yellowColor = new Color(0.95f, 0.77f, 0.06f); // Теплый золотой
    public Color redColor = new Color(0.9f, 0.3f, 0.23f);     // Мягкий гранатовый

    private float maxBarWidth;

    void Awake()
    {
        // Запоминаем максимальную ширину полоски при 100% ХП
        if (fillRect != null)
        {
            maxBarWidth = fillRect.sizeDelta.x;
            fillImage.color = redColor;
        }
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        // Рассчитываем процент (от 0 до 1)
        float healthPercent = Mathf.Clamp01((float)currentHealth / maxHealth);

        // Меняем только ширину полоски
        fillRect.sizeDelta = new Vector2(maxBarWidth * healthPercent, fillRect.sizeDelta.y);
    }

    private void UpdateColor(float percent)
    {
        if (percent > 0.6f)
            fillImage.color = greenColor;
        else if (percent > 0.3f)
            fillImage.color = yellowColor;
        else
            fillImage.color = redColor;
    }
}