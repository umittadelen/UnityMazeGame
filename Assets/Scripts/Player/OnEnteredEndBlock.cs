using UnityEngine;

public class OnEnteredEndBlock : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Debug 1: Did physics detect ANYTHING?
        Debug.Log($"Something entered the trigger! It was: {other.name} (Tag: {other.tag})");

        if (other.CompareTag("Player"))
        {
            // Debug 2: Tag matched. Trying to find Controller...
            Debug.Log("Tag matches 'Player'. Looking for GameUIController...");

            if (GameUIController.Instance != null)
            {
                Debug.Log("Controller found. Sending Win Signal...");
                GameUIController.Instance.GameWon();
            }
            else
            {
                Debug.LogError("CRITICAL: GameUIController.Instance is NULL! Is the script in the scene?");
            }
        }
        else
        {
            Debug.LogWarning($"Object is not tagged 'Player'. It is tagged '{other.tag}'");
        }
    }
}