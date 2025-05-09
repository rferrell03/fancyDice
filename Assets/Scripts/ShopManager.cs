using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public GameObject shopItemPrefab;  // The prefab for a shop item
    public Transform[] spawnPoints;    // Points where items should spawn (relative to the Canvas)
    public Transform shopPanel;        // Reference to the "Shop" panel (where all items will be parented to)
    public GameManager gameManager;    // Reference to GameManager for player money
    public InventoryManager inventoryManager; // Reference to InventoryManager

    private void Start()
    {
        SetupShop();
    }

    private void SetupShop()
    {
        // Generate 3 random Face objects for the shop items
        for (int i = 0; i < 3; i++) // Always spawn 3 items
        {
            // Generate a random addMod based on player money
            int addMod = Mathf.Max(0, Random.Range(0, (int)gameManager.money / 100));

            // Generate a random multMod based on player money
            float multMod = Mathf.Max(0.5f, Random.Range(0f, gameManager.money / 10000f));

            // Create a new Face with the random addMod and multMod
            Face newFace = new Face(value: Random.Range(1, 7), addMod: addMod, multMod: multMod);

            // Instantiate the prefab and parent it to the Shop panel
            GameObject shopItem = Instantiate(shopItemPrefab, shopPanel);

            // Set the position of the item based on the spawn point, maintaining the Canvas/UI scaling
            shopItem.transform.position = spawnPoints[i].position;

            // Get the Image component and change the sprite
            Image itemImage = shopItem.GetComponentInChildren<Image>();  // Assuming Image is a child of the prefab
            if (itemImage != null)
            {
                itemImage.sprite = newFace.inventorySprite;  // Set the sprite from the Face object
            }

            // Get the TextMeshPro components (assuming there are 2 for name and price)
            TextMeshProUGUI[] textComponents = shopItem.GetComponentsInChildren<TextMeshProUGUI>();
            if (textComponents.Length >= 3)
            {
                // Set the text for name and price (assuming the first text is the name, and the second is the price)
                textComponents[0].text = "$ " + newFace.GetCost().ToString("F2");  // Display the cost of the item
                textComponents[1].text = "+ " + newFace.addModifier + " per roll"; // Display the add modifier
                textComponents[2].text = "x " + newFace.multModifier + " per roll"; // Display the multiplier modifier
            }

            // Get the purchase button and add a listener to it
            Button purchaseButton = shopItem.GetComponentInChildren<Button>();
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(() => OnItemPurchased(newFace, newFace.GetCost(), shopItem));
            }
        }
    }

    private void OnItemPurchased(Face purchasedFace, float cost, GameObject shopItem)
    {
        // 1. Reduce the player's money by calling SpendMoney
        gameManager.SpendMoney(cost);

        // 2. Add the purchased face to the inventory
        inventoryManager.AddFace(purchasedFace);

        // 3. Reset the shop (clear existing items and generate new ones)
        ResetShop();
    }

    private void ResetShop()
    {
        // Clear all current items in the shop
        foreach (Transform child in shopPanel)
        {
            Destroy(child.gameObject);
        }

        // Generate new items for the shop
        SetupShop();
    }
}
