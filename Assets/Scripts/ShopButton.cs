using UnityEngine;

public class ShopButton : MonoBehaviour
{
    public GameObject shopPanel;          // The shop panel UI
    public GameObject objectToToggle;     // Main gameplay UI to hide when shop opens

    private bool isShopOpen = false;

    private void OnMouseDown()
    {
        ToggleShop();
    }

    /// <summary>
    /// Toggles the shop open/close state.
    /// </summary>
    private void ToggleShop()
    {
        isShopOpen = !isShopOpen;

        shopPanel.SetActive(isShopOpen);
        if (objectToToggle != null)
        {
            objectToToggle.SetActive(!isShopOpen);   // Toggle gameplay UI
        }
    }

    private void Start()
    {
        shopPanel.SetActive(false);  // Ensure the shop is closed on start
    }
}
