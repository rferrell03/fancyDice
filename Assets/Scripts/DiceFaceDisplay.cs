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

    private int _selectedDiceIndex = -1;
    private int _selectedFaceIndex = -1;

    public InventoryManager inventoryManager;
    public InventoryUIDisplay inventoryUIDisplay;

    private Image _selectedDiceImage;
    private Image _selectedInventoryImage;

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

        for (int diceIndex = 0; diceIndex < 5; diceIndex++)
        {
            if (diceIndex < gameManager.diceList.Count && gameManager.diceList[diceIndex] != null)
            {
                Dice dice = gameManager.diceList[diceIndex];
                for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                {
                    Face face = dice.faces[faceIndex];
                    GameObject inventorySpriteObj = Instantiate(inventorySpritePrefab, _faceTransforms[diceIndex][faceIndex]);
                    Image inventoryImage = inventorySpriteObj.GetComponent<Image>();

                    if (inventoryImage != null)
                    {
                        inventoryImage.sprite = face.inventorySprite;
                        // Apply the modified color here as well.
                        inventoryImage.color = face.GetModifiedColor();
                    }
                    else
                    {
                        Debug.LogError("Inventory sprite prefab does not have an Image component!");
                        Destroy(inventorySpriteObj);
                    }
                }
            }
            else
            {
                for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                {
                    GameObject inventorySpriteObj = Instantiate(inventorySpritePrefab, _faceTransforms[diceIndex][faceIndex]);
                    Image inventoryImage = inventorySpriteObj.GetComponent<Image>();
                    if (inventoryImage != null)
                    {
                        inventoryImage.sprite = GetSpriteByName("Dice_0");
                        // Optionally, set a default color.
                        inventoryImage.color = Color.white;
                    }
                    else
                    {
                        Debug.LogError("Inventory sprite prefab does not have an Image component!");
                        Destroy(inventorySpriteObj);
                    }
                }
            }
        }
    }


    private Sprite GetSpriteByName(string name)
    {
        if (_spriteSheet == null)
        {
            return null;
        }

        foreach (Sprite sprite in _spriteSheet)
        {
            if (sprite.name == name)
            {
                return sprite;
            }
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
                            EventTrigger trigger = image.gameObject.AddComponent<EventTrigger>();
                            EventTrigger.Entry entry = new EventTrigger.Entry();
                            entry.eventID = EventTriggerType.PointerClick;
                            int localDiceIndex = diceIndex;
                            int localFaceIndex = faceIndex;
                            entry.callback.AddListener((data) => { OnDiceFaceClicked(localDiceIndex, localFaceIndex); });
                            trigger.triggers.Add(entry);
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

    private void OnDiceFaceClicked(int diceIndex, int faceIndex)
    {
        ResetDiceImageColor();
        _selectedDiceIndex = diceIndex;
        _selectedFaceIndex = faceIndex;
        _selectedDiceImage = _faceTransforms[diceIndex][faceIndex].GetComponentInChildren<Image>();
        DarkenImage(_selectedDiceImage);
        Debug.Log($"Dice face clicked: Dice {diceIndex}, Face {faceIndex}");
    }

    public void SwapFaces(Face inventoryFace, Image inventoryImage)
    {
        if (_selectedDiceIndex != -1 && _selectedFaceIndex != -1)
        {
            Dice dice = gameManager.diceList[_selectedDiceIndex];
            Face oldFace = dice.faces[_selectedFaceIndex];

            dice.faces[_selectedFaceIndex] = inventoryFace;

            inventoryManager.AddFace(oldFace);
            inventoryManager.RemoveFace(inventoryFace);

            RefreshDiceDisplay();
            inventoryUIDisplay.RefreshInventoryDisplay();

            ResetDiceImageColor();
            ResetInventoryImageColor();

            _selectedDiceIndex = -1;
            _selectedFaceIndex = -1;
            _selectedDiceImage = null;
            _selectedInventoryImage = null;
        }
        else if (_selectedInventoryImage != null && _selectedDiceIndex == -1)
        {
            Dice dice = gameManager.diceList[_selectedDiceIndex];
            Face oldFace = dice.faces[_selectedFaceIndex];

            dice.faces[_selectedFaceIndex] = inventoryFace;

            inventoryManager.AddFace(oldFace);
            inventoryManager.RemoveFace(inventoryFace);

            RefreshDiceDisplay();
            inventoryUIDisplay.RefreshInventoryDisplay();

            ResetDiceImageColor();
            ResetInventoryImageColor();

            _selectedDiceIndex = -1;
            _selectedFaceIndex = -1;
            _selectedDiceImage = null;
            _selectedInventoryImage = null;
        }
        else if (inventoryImage != null)
        {
            ResetInventoryImageColor();
            _selectedInventoryImage = inventoryImage;
            DarkenImage(_selectedInventoryImage);
        }
        else
        {
            Debug.LogError("No dice face selected to swap.");
        }
    }

    private void DarkenImage(Image image)
    {
        if (image != null)
        {
            image.color = new Color(0.5f, 0.5f, 0.5f);
        }
    }

    private void ResetDiceImageColor()
    {
        if (_selectedDiceImage != null)
        {
            _selectedDiceImage.color = Color.white;
            _selectedDiceImage = null;
        }
    }

    private void ResetInventoryImageColor()
    {
        if (_selectedInventoryImage != null)
        {
            _selectedInventoryImage.color = Color.white;
            _selectedInventoryImage = null;
        }
    }
}