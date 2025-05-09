using UnityEngine;

public class InventoryButton : MonoBehaviour
{
    public GameObject menuPanel; // Reference to your inventory menu panel
    public DiceFaceDisplay diceFaceDisplay; // Reference to your DiceFaceDisplay script
    public GameObject objectToToggle; // Reference to the object you want to disable/enable

    private void OnMouseDown()
    {
        if (menuPanel != null)
        {
            // Toggle the menu visibility
            bool isActive = menuPanel.activeSelf;
            menuPanel.SetActive(!isActive); // Toggle visibility

            // Disable or enable the object to toggle
            if (objectToToggle != null)
            {
                objectToToggle.SetActive(isActive); // If the menu was open, enable the object; otherwise, disable it
            }

            // Refresh displays when opening the inventory
            if (!isActive) // If we're opening the inventory
            {
                if (diceFaceDisplay != null)
                {
                    diceFaceDisplay.RefreshDiceDisplay();
                }
                else
                {
                    Debug.LogWarning("DiceFaceDisplay not assigned in the Inspector.");
                }

                // Refresh inventory display
                if (diceFaceDisplay.inventoryUIDisplay != null)
                {
                    diceFaceDisplay.inventoryUIDisplay.RefreshInventoryDisplay();
                }
                else
                {
                    Debug.LogWarning("InventoryUIDisplay not assigned in the Inspector.");
                }
            }
        }
        else
        {
            Debug.LogWarning("Menu panel not assigned in the Inspector.");
        }
    }

    void Start()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false); // Ensure the menu is closed on start
        }
    }
}
