using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    public Image fillImage; 
    public RectTransform fillRect; 
    
    [Header("Color Settings")]
    public Color greenColor = new Color(0.18f, 0.8f, 0.44f);
    public Color yellowColor = new Color(0.95f, 0.77f, 0.06f);
    public Color redColor = new Color(0.9f, 0.3f, 0.23f);

    private float maxBarWidth;

    void Awake()
    {
        if (fillRect != null)
        {
            maxBarWidth = fillRect.sizeDelta.x;
        }
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        // Рассчитываем процент
        float healthPercent = Mathf.Clamp01((float)currentHealth / maxHealth);

        // Меняем ширину
        if (fillRect != null)
        {
            fillRect.sizeDelta = new Vector2(maxBarWidth * healthPercent, fillRect.sizeDelta.y);
        }

        // ОБЯЗАТЕЛЬНО обновляем цвет здесь!
        UpdateColor(healthPercent);
    }

    private void UpdateColor(float percent)
    {
        if (fillImage == null) return;

        if (percent > 0.6f)
            fillImage.color = greenColor;
        else if (percent > 0.3f)
            fillImage.color = yellowColor;
        else
            fillImage.color = redColor;
    }
}