using UnityEngine;

public class Trap : MonoBehaviour {
    private void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            Debug.Log("АЙ! Огонь жжется!");
        }
    }
}