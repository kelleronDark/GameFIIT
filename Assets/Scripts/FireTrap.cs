using UnityEngine;
using System.Collections;

public class Trap : MonoBehaviour {
    public float activeTime = 2f; // Сколько секунд огонь горит
    public float idleTime = 3f;   // Сколько секунд пауза
    
    private bool isDangerous = true;

    void Start() {
        StartCoroutine(TrapCycle());
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (isDangerous && other.CompareTag("Player")) {
            Debug.Log("АЙ! Огонь жжется!");
        }
    }

    // Корутина для управления циклом огня
    IEnumerator TrapCycle() {
        while (true) {
            // ФАЗА 1: Огонь опасен
            isDangerous = true;
            yield return new WaitForSeconds(activeTime);

            // ФАЗА 2: Огонь НЕ опасен (просто картинка)
            isDangerous = false;
            yield return new WaitForSeconds(idleTime);
        }
    }
}