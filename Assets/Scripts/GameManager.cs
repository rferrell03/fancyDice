using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; // Keep System namespace for Action
using System.Linq; // Needed for Max()

public class GameManager : MonoBehaviour
{
    [Header("Prefabs & References")]
    public GameObject dicePrefab;
    public DiceSlotManager diceSlotManager;
    public FloatingTextManager floatingTextManager;

    [Header("Dice Settings")]
    public int startingDiceCount = 1;
    public int maxDiceCount = 5;

    public float money;

    // List that stores the player's dice.
    public List<Dice> diceList = new List<Dice>();

    // Track the current theoretical maximum value a single face can produce
    public float currentMaxFaceValue = 1f; // Initialize to a low default

    private void Start()
    {
        // Ensure FloatingTextManager is assigned
        if (floatingTextManager == null)
        {
            Debug.LogError("FloatingTextManager not assigned in GameManager Inspector!");
        }

        CreateDice(); // Creates initial dice

        // Calculate the initial max face value after creating dice
        UpdateMaxFaceValue();

        if (diceSlotManager != null)
        {
            diceSlotManager.RenderDiceSlots(diceList);
        }
        else
        {
            Debug.LogError("DiceSlotManager not assigned in GameManager!");
        }
    }


    public void CreateDice()
    {
        for (int i = 0; i < startingDiceCount; i++)
        {
            // Check if dicePrefab is assigned
            if (dicePrefab == null)
            {
                Debug.LogError("Dice Prefab not assigned in GameManager Inspector!");
                return; // Stop trying to create dice if prefab is missing
            }

            GameObject diceObj = Instantiate(dicePrefab);
            diceObj.name = "Dice_" + (i + 1);
            Dice diceComp = diceObj.GetComponent<Dice>();
            if (diceComp != null)
            {
                diceComp.Initialize(); // Initializes with default faces
                                       // Check spawn points
                if (diceComp.topSpawn == null || diceComp.leftSpawn == null || diceComp.rightSpawn == null)
                {
                    Debug.LogWarning($"Dice_{i + 1} prefab might be missing spawn point assignments (Top, Left, Right Spawn). Floating text may not appear correctly.");
                }
                diceList.Add(diceComp);
            }
            else
            {
                Debug.LogError("Dice prefab does not have a Dice component attached!");
                Destroy(diceObj); // Clean up the instantiated object if it's not valid
            }
        }
        // Note: Max value is updated in Start after this runs
    }

    /// <summary>
    /// Calculates and updates the maximum possible value any single face
    /// across all dice can currently produce.
    /// Call this after dice/faces are added or upgraded.
    /// </summary>
    public void UpdateMaxFaceValue()
    {
        float maxFound = 1f; // Start with a minimum default

        foreach (Dice dice in diceList)
        {
            if (dice != null && dice.faces != null)
            {
                // Check value from each face on this die
                foreach (Face face in dice.faces)
                {
                    if (face != null)
                    {
                        // Calculate potential value *without* triggering effects, just for max comparison
                        // We use the Face properties directly here
                        float potentialValue = (face.Value + face.addModifier) * face.multModifier;
                        if (potentialValue > maxFound)
                        {
                            maxFound = potentialValue;
                        }
                    }
                }
            }
        }

        currentMaxFaceValue = maxFound;
        // Ensure max value is never zero or negative if modifiers could cause that
        if (currentMaxFaceValue <= 0)
        {
            currentMaxFaceValue = 1f;
        }

        Debug.Log($"Updated Max Face Value for color calculation: {currentMaxFaceValue}");
    }


    /// <summary>
    /// Rolls all dice and handles completion logic.
    /// </summary>
    public void RollAllDice()
    {
        StartCoroutine(RollAllDiceCoroutine());
    }

    public float GetMoney()
    {
        return money;
    }
    public void SpendMoney(float cost)
    {
        money -= cost;
    }

    private IEnumerator RollAllDiceCoroutine()
    {
        int totalDice = diceList.Count;
        int completedDice = 0;
        float totalRollMoney = 0f;

        // Check if FloatingTextManager reference is valid before starting
        if (floatingTextManager == null)
        {
            Debug.LogError("Cannot roll dice - FloatingTextManager is not assigned in GameManager.");
            yield break; // Exit the coroutine
        }


        // Subscribe to each dice's completion event and roll them.
        foreach (Dice dice in diceList)
        {
            Dice currentDice = dice;  // Local copy for closure
            bool alreadyCompleted = false; // Flag to prevent double execution

            // Define the completion action separately
            Action onComplete = () =>
            {
                // Ensure this lambda only runs once per dice roll completion
                if (alreadyCompleted) return;
                alreadyCompleted = true;


                // Check if finalRollResult is valid
                if (currentDice.finalRollResult.top == null || currentDice.finalRollResult.left == null || currentDice.finalRollResult.right == null)
                {
                    Debug.LogError($"Dice {currentDice.name} finished rolling but finalRollResult faces are null!");
                    completedDice++; // Still count as completed to avoid infinite loop
                    return; // Skip processing for this dice
                }

                // Trigger faces and get actual money gained
                float topMoney = currentDice.finalRollResult.top.Trigger(currentDice, floatingTextManager, 0, currentMaxFaceValue);
                float leftMoney = currentDice.finalRollResult.left.Trigger(currentDice, floatingTextManager, 1, currentMaxFaceValue);
                float rightMoney = currentDice.finalRollResult.right.Trigger(currentDice, floatingTextManager, 2, currentMaxFaceValue);

                // Accumulate the total money gained
                totalRollMoney += topMoney + leftMoney + rightMoney;

                completedDice++;

                // Unsubscribe after completion
                //currentDice.OnRollComplete -= onComplete;
            };

            // Subscribe the action
            currentDice.OnRollComplete += onComplete;


            // Start the roll animation for this dice
            currentDice.Roll();
        }

        // Wait until all dice have finished rolling
        while (completedDice < totalDice)
        {
            yield return null;
        }

        // Add the accumulated money to the total after all dice finish rolling
        money += totalRollMoney;
        Debug.Log($"Money added after roll: {totalRollMoney:0.00} | New Total: {money:0.00}"); // Format log output
    }


    /// <summary>
    /// Adds a new dice and updates the max face value if successful.
    /// </summary>
    public void AddNewDice()
    {
        if (diceList.Count < maxDiceCount)
        {
            // Check references
            if (dicePrefab == null)
            {
                Debug.LogError("Dice Prefab not assigned in GameManager Inspector! Cannot add new dice.");
                return;
            }
            if (diceSlotManager == null)
            {
                Debug.LogError("DiceSlotManager not assigned in GameManager Inspector! Cannot render new dice slot.");
            }


            GameObject diceObj = Instantiate(dicePrefab);
            diceObj.name = "Dice_" + (diceList.Count + 1);
            Dice diceComp = diceObj.GetComponent<Dice>();
            if (diceComp != null)
            {
                diceComp.Initialize();
                // Check spawn points
                if (diceComp.topSpawn == null || diceComp.leftSpawn == null || diceComp.rightSpawn == null)
                {
                    Debug.LogWarning($"Newly added Dice_{diceList.Count + 1} prefab might be missing spawn point assignments.");
                }
                diceList.Add(diceComp);

                // Update max value since new faces were added
                UpdateMaxFaceValue();

                // Render slots only if manager exists
                if (diceSlotManager != null)
                {
                    diceSlotManager.RenderDiceSlots(diceList);
                }
            }
            else
            {
                Debug.LogError("Dice prefab does not have a Dice component attached!");
                Destroy(diceObj); // Clean up
            }

        }
        else
        {
            Debug.Log("Maximum dice reached.");
        }
    }

    // --- You would call UpdateMaxFaceValue() here after any face upgrade logic ---
    // public void ExampleUpgradeFaceFunction()
    // {
    //     // ... perform upgrade on a face ...
    //     UpdateMaxFaceValue(); // Recalculate max potential value
    // }
}