using TMPro;
using UnityEngine;

public class GameVersion : MonoBehaviour
{
    private void Start()
    {
        this.GetComponent<TMP_Text>().text = $"v{Application.version}";
    }
}