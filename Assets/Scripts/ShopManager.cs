using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform shopPanel;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Shop Settings")]
    [SerializeField] private int shopItemCount = 3;
    [SerializeField] private float minMultiplierMod = 0.5f;
    [SerializeField] private int minDiceValue = 1;
    [SerializeField] private int maxDiceValue = 7;

    private Queue<GameObject> shopItemPool;
    private const int INITIAL_POOL_SIZE = 5;

    private void Awake()
    {
        ValidateReferences();
        InitializeObjectPool();
    }

    private void Start()
    {
        SetupShop();
    }

    private void ValidateReferences()
    {
        if (shopItemPrefab == null) Debug.LogError("Shop Item Prefab not assigned!");
        if (spawnPoints == null || spawnPoints.Length == 0) Debug.LogError("Spawn Points not assigned!");
        if (shopPanel == null) Debug.LogError("Shop Panel not assigned!");
        if (gameManager == null) Debug.LogError("Game Manager not assigned!");
        if (inventoryManager == null) Debug.LogError("Inventory Manager not assigned!");
    }

    private void InitializeObjectPool()
    {
        shopItemPool = new Queue<GameObject>();
        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            CreatePooledItem();
        }
    }

    private void CreatePooledItem()
    {
        GameObject item = Instantiate(shopItemPrefab, shopPanel);
        item.SetActive(false);
        shopItemPool.Enqueue(item);
    }

    private GameObject GetPooledItem()
    {
        if (shopItemPool.Count == 0)
        {
            CreatePooledItem();
        }
        return shopItemPool.Dequeue();
    }

    private void ReturnToPool(GameObject item)
    {
        item.SetActive(false);
        shopItemPool.Enqueue(item);
    }

    private void SetupShop()
    {
        for (int i = 0; i < shopItemCount; i++)
        {
            if (i >= spawnPoints.Length)
            {
                Debug.LogWarning($"Not enough spawn points for shop item {i}");
                continue;
            }

            GameObject shopItem = GetPooledItem();
            ConfigureShopItem(shopItem, i);
        }
    }

    private void ConfigureShopItem(GameObject shopItem, int index)
    {
        shopItem.SetActive(true);
        shopItem.transform.position = spawnPoints[index].position;

        // Generate face with appropriate modifiers
        Face newFace = GenerateRandomFace();

        // Configure UI elements
        ConfigureUIElements(shopItem, newFace);

        // Setup purchase button
        SetupPurchaseButton(shopItem, newFace);
    }

    private Face GenerateRandomFace()
    {
        float moneyFactor = gameManager.money;
        int addMod = Mathf.Max(0, Random.Range(0, (int)(moneyFactor / 100)));
        float multMod = Mathf.Max(minMultiplierMod, Random.Range(0f, moneyFactor / 10000f));

        Face f = new Face(
             value: Random.Range(minDiceValue, maxDiceValue),
             addMod: addMod,
             multMod: multMod
         );

        // 10% chance to get a special effect
        if (Random.value < 0.1f)
        {
            // Randomly choose a special effect
            int effectType = Random.Range(0, 5);
            switch (effectType)
            {
                case 0:
                    f.specialEffect = new TriggerSameNum();
                    break;
                case 1:
                    f.specialEffect = new CascadingTrigger();
                    break;
                case 2:
                    f.specialEffect = new ComboTrigger();
                    break;
                case 3:
                    f.specialEffect = new EvenTrigger();
                    break;
                case 4:
                    f.specialEffect = new OddTrigger();
                    break;
            }
        }
        else
        {
            f.specialEffect = null;
        }

        return f;
    }

    private void ConfigureUIElements(GameObject shopItem, Face face)
    {
        Image itemImage = shopItem.GetComponentInChildren<Image>();
        if (itemImage != null)
        {
            itemImage.sprite = face.inventorySprite;
            itemImage.color = face.GetModifiedColor();
        }

        TextMeshProUGUI[] textComponents = shopItem.GetComponentsInChildren<TextMeshProUGUI>();
        if (textComponents.Length >= 4)
        {
            textComponents[0].text = $"$ {face.GetCost():F2}";
            textComponents[1].text = $"+ {face.addModifier} per roll";
            textComponents[2].text = $"x {face.multModifier:F2} per roll";
            
            // Add special effect description
            string effectDesc = "No special effect";
            if (face.specialEffect != null)
            {
                if (face.specialEffect is TriggerSameNum)
                {
                    effectDesc = "(Special) Mirror \n Triggers all matching numbers";
                }
                else if (face.specialEffect is CascadingTrigger)
                {
                    effectDesc = "(Special) Cascade \n Triggers next higher numbers";
                }
                else if (face.specialEffect is ComboTrigger)
                {
                    effectDesc = "(Special) Poker \n Triggers all if 3-of-a-kind or straight";
                }
                else if (face.specialEffect is EvenTrigger)
                {
                    effectDesc = "(Special) Even \n Triggers all even numbers";
                }
                else if (face.specialEffect is OddTrigger)
                {
                    effectDesc = "(Special) Odd \n Triggers all odd numbers";
                }
            }
            textComponents[3].text = effectDesc;
        }
    }

    private void SetupPurchaseButton(GameObject shopItem, Face face)
    {
        Button purchaseButton = shopItem.GetComponentInChildren<Button>();
        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(() => OnItemPurchased(face, face.GetCost(), shopItem));
        }
    }

    private void OnItemPurchased(Face purchasedFace, float cost, GameObject shopItem)
    {
        if (gameManager.money < cost)
        {
            Debug.LogWarning("Not enough money to purchase item!");
            return;
        }

        gameManager.SpendMoney(cost);
        inventoryManager.AddFace(purchasedFace);
        
        // Find the spawn point index by comparing positions
        int spawnIndex = -1;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (Vector3.Distance(shopItem.transform.position, spawnPoints[i].position) < 0.1f)
            {
                spawnIndex = i;
                break;
            }
        }

        ReturnToPool(shopItem);
        
        if (spawnIndex != -1)
        {
            GameObject newItem = GetPooledItem();
            ConfigureShopItem(newItem, spawnIndex);
        }
        else
        {
            Debug.LogError("Could not find spawn point for purchased item!");
        }
    }

    private void OnDestroy()
    {
        while (shopItemPool.Count > 0)
        {
            GameObject item = shopItemPool.Dequeue();
            if (item != null)
            {
                Destroy(item);
            }
        }
    }
}
