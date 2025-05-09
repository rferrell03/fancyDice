using UnityEngine;
using TMPro;

public class MoneyDisplayUI : MonoBehaviour
{
    public GameManager gameManager; // Assign this in the Inspector.
    public TextMeshProUGUI moneyText; // Assign your TextMeshProUGUI component in the Inspector.

    void Update()
    {
        // Update the text each frame to reflect the current money amount.
        moneyText.text = "$" + gameManager.money.ToString("0.00");
    }
}
