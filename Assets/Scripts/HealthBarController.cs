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
        if (fillRect == null || fillImage == null) return;

        float healthPercent = Mathf.Clamp01((float)currentHealth / maxHealth);
        
        // 1. Меняем ширину
        fillRect.sizeDelta = new Vector2(maxBarWidth * healthPercent, fillRect.sizeDelta.y);
        
        // 2. Обновляем цвет в зависимости от процента
        UpdateColor(healthPercent); 
    }

    private void UpdateColor(float percent)
    {
        Color targetColor;

        if (percent > 0.6f)
            targetColor = greenColor;
        else if (percent > 0.3f)
            targetColor = yellowColor;
        else
            targetColor = redColor;

        fillImage.color = targetColor;
    }
}