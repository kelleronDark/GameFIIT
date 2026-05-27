using UnityEngine;
using System.Collections;

public class Trap : MonoBehaviour {
    public float activeTime = 2f;
    public float idleTime = 3f;
    
    private bool isDangerous = true;

    void Start() {
        StartCoroutine(TrapCycle());
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (isDangerous && other.CompareTag("Player")) {
            Debug.Log("АЙ! Огонь жжется!");
        }
    }

    IEnumerator TrapCycle() {
        while (true) {
            isDangerous = true;
            yield return new WaitForSeconds(activeTime);

            isDangerous = false;
            yield return new WaitForSeconds(idleTime);
        }
    }
}