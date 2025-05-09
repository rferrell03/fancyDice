using UnityEngine;
using System.Collections.Generic;

public class DiceSlotManager : MonoBehaviour
{
    [Header("Prefabs & Settings")]
    // Prefab for the dice slot (e.g., a dark round square sprite)
    public GameObject diceSlotPrefab;
    // How far apart the slots are (adjust to match your scene scale)
    public float slotSpacing = 2f;

    // List of currently rendered dice visuals (slots with dice as children)
    private List<GameObject> diceVisuals = new List<GameObject>();

    /// <summary>
    /// Renders dice slots based on the GameManager's dice list and places each dice into its slot.
    /// </summary>
    /// <param name="diceList">The list of dice created by GameManager.</param>
    public void RenderDiceSlots(List<Dice> diceList)
    {
        // Clear any existing slot visuals.
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        diceVisuals.Clear();

        // Calculate the starting X position to center the slots.
        float totalWidth = (diceList.Count - 1) * slotSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < diceList.Count; i++)
        {
            // Determine the slot's position in world space.
            Vector3 slotPos = new Vector3(startX + i * slotSpacing, 0f, 0f);

            // Instantiate the dice slot.
            GameObject slot = Instantiate(diceSlotPrefab, slotPos, Quaternion.identity, transform);
            slot.name = "DiceSlot_" + (i + 1);

            // Ensure the slot renders behind the dice by adjusting its z-position.
            slot.transform.position += new Vector3(0f, 0f, 0.1f);

            // Now, take the already-created dice from the GameManager's list...
            Dice dice = diceList[i];
            // ...and reparent it to this slot.
            dice.transform.SetParent(slot.transform, false);
            // Center the dice within the slot.
            dice.transform.localPosition = Vector3.zero;

            // Optionally, add the slot to our local list.
            diceVisuals.Add(slot);
        }
    }
}
