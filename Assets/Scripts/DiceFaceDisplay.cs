// DiceFaceDisplay.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class DiceFaceDisplay : MonoBehaviour
{
    public GameManager gameManager;
    [SerializeField]
    public Transform[] diceFaceTransforms;
    private List<List<Transform>> _faceTransforms = new List<List<Transform>>();
    public GameObject inventorySpritePrefab;
    public string spriteSheetName = "Dice";

    private Sprite[] _spriteSheet;

    // Selection state
    private class SelectionState
    {
        public bool IsDiceFace => DiceIndex != -1;
        public bool IsInventoryFace => InventoryFace != null;
        public bool HasSelection => IsDiceFace || IsInventoryFace;

        public int DiceIndex = -1;
        public int FaceIndex = -1;
        public Face InventoryFace = null;
        public Image SelectedImage = null;
    }
    private SelectionState _selection = new SelectionState();

    public InventoryManager inventoryManager;
    public InventoryUIDisplay inventoryUIDisplay;

    void Awake()
    {
        for (int i = 0; i < 5; i++)
        {
            _faceTransforms.Add(new List<Transform>());
            for (int j = 0; j < 6; j++)
            {
                _faceTransforms[i].Add(diceFaceTransforms[i * 6 + j]);
            }
        }
    }

    void Start()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager not assigned to DiceFaceDisplay!");
            return;
        }

        if (diceFaceTransforms.Length != 30)
        {
            Debug.LogError("Face transforms not assigned correctly to DiceFaceDisplay! You need 30 transforms (5 dice * 6 faces).");
            return;
        }

        _spriteSheet = Resources.LoadAll<Sprite>(spriteSheetName);
        if (_spriteSheet == null || _spriteSheet.Length == 0)
        {
            Debug.LogError("Failed to load sprite sheet: " + spriteSheetName);
            return;
        }

        DisplayDiceFaces();
        SetupFaceClickListeners();
    }

    public void RefreshDiceDisplay()
    {
        DisplayDiceFaces();
        SetupFaceClickListeners();
    }

    public void DisplayDiceFaces()
    {
        // Clear existing faces
        foreach (List<Transform> diceTransforms in _faceTransforms)
        {
            foreach (Transform faceTransform in diceTransforms)
            {
                if (faceTransform != null)
                {
                    foreach (Transform child in faceTransform)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        // Display current faces
        for (int diceIndex = 0; diceIndex < 5; diceIndex++)
        {
            if (diceIndex < gameManager.diceList.Count && gameManager.diceList[diceIndex] != null)
            {
                Dice dice = gameManager.diceList[diceIndex];
                for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                {
                    Face face = dice.faces[faceIndex];
                    CreateFaceDisplay(diceIndex, faceIndex, face);
                }
            }
            else
            {
                for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                {
                    CreateFaceDisplay(diceIndex, faceIndex, null);
                }
            }
        }
    }

    private void CreateFaceDisplay(int diceIndex, int faceIndex, Face face)
    {
        GameObject inventorySpriteObj = Instantiate(inventorySpritePrefab, _faceTransforms[diceIndex][faceIndex]);
        Image inventoryImage = inventorySpriteObj.GetComponent<Image>();

        if (inventoryImage != null)
        {
            if (face != null)
            {
                inventoryImage.sprite = face.inventorySprite;
                inventoryImage.color = face.GetModifiedColor();
            }
            else
            {
                inventoryImage.sprite = GetSpriteByName("Dice_0");
                inventoryImage.color = Color.white;
            }
        }
        else
        {
            Debug.LogError("Inventory sprite prefab does not have an Image component!");
            Destroy(inventorySpriteObj);
        }
    }

    private Sprite GetSpriteByName(string name)
    {
        if (_spriteSheet == null) return null;

        foreach (Sprite sprite in _spriteSheet)
        {
            if (sprite.name == name) return sprite;
        }
        Debug.LogWarning($"Sprite '{name}' not found in sprite sheet '{spriteSheetName}'");
        return null;
    }

    private void SetupFaceClickListeners()
    {
        for (int diceIndex = 0; diceIndex < 5; diceIndex++)
        {
            if (diceIndex < gameManager.diceList.Count && gameManager.diceList[diceIndex] != null)
            {
                for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                {
                    Transform faceTransform = _faceTransforms[diceIndex][faceIndex];
                    if (faceTransform != null)
                    {
                        Image image = faceTransform.GetComponentInChildren<Image>();
                        if (image != null)
                        {
                            SetupClickHandler(image, diceIndex, faceIndex);
                        }
                        else
                        {
                            Debug.LogError("Dice face transform does not have an Image component!");
                        }
                    }
                }
            }
        }
    }

    private void SetupClickHandler(Image image, int diceIndex, int faceIndex)
    {
        EventTrigger trigger = image.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        int localDiceIndex = diceIndex;
        int localFaceIndex = faceIndex;
        entry.callback.AddListener((data) => { OnDiceFaceClicked(localDiceIndex, localFaceIndex); });
        trigger.triggers.Add(entry);
    }

    private void OnDiceFaceClicked(int diceIndex, int faceIndex)
    {
        // If clicking the same face, deselect it
        if (_selection.IsDiceFace && _selection.DiceIndex == diceIndex && _selection.FaceIndex == faceIndex)
        {
            ClearSelection();
            return;
        }

        // If clicking a different face on the same dice, just move selection
        if (_selection.IsDiceFace && _selection.DiceIndex == diceIndex)
        {
            UpdateDiceFaceSelection(diceIndex, faceIndex);
            return;
        }

        // If we have a selected inventory face, swap with the clicked dice face
        if (_selection.IsInventoryFace)
        {
            SwapDiceAndInventoryFaces(diceIndex, faceIndex, _selection.InventoryFace);
            return;
        }

        // If we have a selected dice face, swap with the clicked dice face
        if (_selection.IsDiceFace)
        {
            SwapDiceFaces(_selection.DiceIndex, _selection.FaceIndex, diceIndex, faceIndex);
            return;
        }

        // Otherwise, just select the clicked face
        UpdateDiceFaceSelection(diceIndex, faceIndex);
    }

    private void UpdateDiceFaceSelection(int diceIndex, int faceIndex)
    {
        ClearSelection();
        _selection.DiceIndex = diceIndex;
        _selection.FaceIndex = faceIndex;
        _selection.SelectedImage = _faceTransforms[diceIndex][faceIndex].GetComponentInChildren<Image>();
        if (_selection.SelectedImage != null)
        {
            DarkenImage(_selection.SelectedImage);
        }
        else
        {
            Debug.LogError($"Failed to get Image component for dice {diceIndex}, face {faceIndex}");
        }
    }

    private void SwapDiceFaces(int dice1Index, int face1Index, int dice2Index, int face2Index)
    {
        Dice dice1 = gameManager.diceList[dice1Index];
        Dice dice2 = gameManager.diceList[dice2Index];

        Face temp = dice1.faces[face1Index];
        dice1.faces[face1Index] = dice2.faces[face2Index];
        dice2.faces[face2Index] = temp;

        // Clear selection before refreshing display
        ClearSelection();
        RefreshDiceDisplay();
    }

    private void SwapDiceAndInventoryFaces(int diceIndex, int faceIndex, Face inventoryFace)
    {
        Dice dice = gameManager.diceList[diceIndex];
        Face oldFace = dice.faces[faceIndex];

        // Remove the inventory face first
        inventoryManager.RemoveFace(inventoryFace);
        
        // Then add the old face to inventory
        inventoryManager.AddFace(oldFace);
        
        // Finally update the dice face
        dice.faces[faceIndex] = inventoryFace;

        // Clear selection before refreshing displays
        ClearSelection();
        RefreshDiceDisplay();
        inventoryUIDisplay.RefreshInventoryDisplay();
    }

    public void SwapFaces(Face inventoryFace, Image inventoryImage)
    {
        // If clicking the same inventory face, deselect it
        if (_selection.IsInventoryFace && _selection.SelectedImage == inventoryImage)
        {
            ClearSelection();
            return;
        }

        // If we have a selected dice face, swap with the inventory face
        if (_selection.IsDiceFace)
        {
            SwapDiceAndInventoryFaces(_selection.DiceIndex, _selection.FaceIndex, inventoryFace);
            return;
        }

        // Otherwise, just select the inventory face
        ClearSelection();
        _selection.InventoryFace = inventoryFace;
        _selection.SelectedImage = inventoryImage;
        if (_selection.SelectedImage != null)
        {
            DarkenImage(_selection.SelectedImage);
        }
        else
        {
            Debug.LogError("Failed to get Image component for inventory face");
        }
    }

    private void ClearSelection()
    {
        // Reset all face colors to white
        for (int diceIndex = 0; diceIndex < 5; diceIndex++)
        {
            if (diceIndex < gameManager.diceList.Count && gameManager.diceList[diceIndex] != null)
            {
                for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                {
                    Transform faceTransform = _faceTransforms[diceIndex][faceIndex];
                    if (faceTransform != null)
                    {
                        Image image = faceTransform.GetComponentInChildren<Image>();
                        if (image != null)
                        {
                            image.color = Color.white;
                        }
                    }
                }
            }
        }

        // Reset selection state
        _selection = new SelectionState();
    }

    private void DarkenImage(Image image)
    {
        if (image != null)
        {
            // Store the original color's alpha
            float alpha = image.color.a;
            // Create a darker version while preserving alpha
            Color darkerColor = new Color(0.5f, 0.5f, 0.5f, alpha);
            image.color = darkerColor;
        }
    }
}