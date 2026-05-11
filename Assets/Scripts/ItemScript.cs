using UnityEngine;
using TMPro;

public class ItemHint : MonoBehaviour
{
    public GameObject hintPrefab;
    private GameObject currentHint;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentHint == null)
        {
            ShowHint();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HideHint();
        }
    }

    void ShowHint()
    {
        currentHint = Instantiate(hintPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        currentHint.transform.SetParent(transform);

        Canvas canvas = currentHint.GetComponentInChildren<Canvas>();
        if (canvas != null && Camera.main != null)
            canvas.worldCamera = Camera.main;

        TextMeshProUGUI text = currentHint.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null) text.text = "Нажмите F";
    }

    void HideHint()
    {
        if (currentHint != null)
        {
            Destroy(currentHint);
            currentHint = null;
        }
    }
}