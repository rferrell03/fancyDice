using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class InventoryUIDisplay : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public GameObject inventorySlotPrefab;
    public Transform contentPanel;
    public DiceFaceDisplay diceFaceDisplay;

    private List<GameObject> _inventorySlots = new List<GameObject>();

    void Start()
    {
        if (inventoryManager == null || inventorySlotPrefab == null || contentPanel == null)
        {
            Debug.LogError("InventoryUIDisplay: One or more references not assigned in the Inspector.");
            return;
        }

        DisplayInventory();
    }

    public void RefreshInventoryDisplay()
    {
        ClearInventoryDisplay();
        DisplayInventory();
    }

    private void ClearInventoryDisplay()
    {
        foreach (GameObject slot in _inventorySlots)
        {
            Destroy(slot);
        }
        _inventorySlots.Clear();
    }

    private void DisplayInventory()
    {
        List<Face> faces = inventoryManager.GetInventoryFaces();

        foreach (Face face in faces)
        {
            GameObject inventorySlot = Instantiate(inventorySlotPrefab, contentPanel);
            _inventorySlots.Add(inventorySlot);

            Image slotImage = inventorySlot.GetComponent<Image>();
            if (slotImage != null)
            {
                slotImage.sprite = face.inventorySprite;
                // Apply the modified color based on face modifiers.
                slotImage.color = face.GetModifiedColor();

                EventTrigger trigger = slotImage.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;
                Face localFace = face;
                Image localImage = slotImage;
                entry.callback.AddListener((data) => { OnInventoryFaceClicked(localFace, localImage); });
                trigger.triggers.Add(entry);
            }
            else
            {
                Debug.LogError("Inventory slot prefab does not have an Image component.");
            }
        }
    }


    private void OnInventoryFaceClicked(Face face, Image image)
    {
        if (diceFaceDisplay != null)
        {
            diceFaceDisplay.SwapFaces(face, image);
        }
        else
        {
            Debug.LogError("DiceFaceDisplay not assigned in InventoryUIDisplay.");
        }
    }
}