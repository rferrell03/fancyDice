using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Face
{
    public int Value { get; set; }
    public int addModifier { get; set; }
    public float multModifier { get; set; }

    // Sprites for the face
    public Sprite TopSprite { get; private set; }
    public Sprite LeftSprite { get; private set; }
    public Sprite RightSprite { get; private set; }
    public Sprite inventorySprite { get; set; }
    public SpecialEffect specialEffect { get; set; }
    private const string spriteSheetPath = "FixedFaces";      // For main dice faces
    private const string inventorySpriteSheetPath = "Dice";   // For inventory display

    public Face(int value = 1, int addMod = 0, float multMod = 1.0f)
    {
        Value = value;
        addModifier = addMod;
        multModifier = multMod;
        AssignSprites();
    }

    /// <summary>
    /// Returns the total money produced by triggering this face.
    /// </summary>
    public float Trigger(Dice dice, FloatingTextManager floatingTextManager, int side, float maxValue, HashSet<Face> triggerChain = null)
    {
        float specialEffectMoney = 0;
        if (specialEffect != null)
        {
            Debug.Log("Special effect activated");
            specialEffectMoney = specialEffect.Activate(this, triggerChain);
            Debug.Log("Special effect activated");
        }
        if (side == 0)
        {
            floatingTextManager.CreateFloatingText($"+{Value}", dice.topSpawn, Value, maxValue);
        }else if (side == 1)
        {
            floatingTextManager.CreateFloatingText($"+{Value}", dice.leftSpawn, Value, maxValue);
        }else if (side == 2)
        {
            floatingTextManager.CreateFloatingText($"+{Value}", dice.rightSpawn, Value, maxValue);
        }
        float moneyGained = ((Value + addModifier) * multModifier) + specialEffectMoney;
        return moneyGained;
    }

    /// <summary>
    /// Upgrades this face by applying the given modifiers.
    /// </summary>
    public void Upgrade(int addMod, float multMod)
    {
        addModifier += addMod;
        multModifier += multMod;

        Debug.Log($"Upgraded face: +{addMod} addMod, +{multMod} multMod");
    }

    /// <summary>
    /// Calculates the cost of this face.
    /// Base cost is $30 for a face with no modifiers.
    /// Each unit of addModifier adds $0.30.
    /// Each 0.5 of multModifier above 1.0 adds $100.
    /// </summary>
    public float GetCost()
    {
        float baseCost = 30f;
        float addCost = addModifier * 0.3f;
        float multCost = 0f;

        if (multModifier > 1f)
        {
            multCost = ((multModifier - 1f) / 0.5f) * 100f;
        }

        return baseCost + addCost + multCost;
    }

    /// <summary>
    /// Assigns sprites to the face.
    /// </summary>
    public void AssignSprites()
    {
        string spriteValue = Value > 6 ? "7" : Value.ToString();
        string inventorySpriteValue = Value > 6 ? "0" : Value.ToString();

        TopSprite = FindSpriteByName($"Top_{spriteValue}", spriteSheetPath);
        LeftSprite = FindSpriteByName($"Left_{spriteValue}", spriteSheetPath);
        RightSprite = FindSpriteByName($"Right_{spriteValue}", spriteSheetPath);
        inventorySprite = FindSpriteByName($"Dice_{inventorySpriteValue}", inventorySpriteSheetPath);
    }

    private Sprite FindSpriteByName(string name, string path)
    {
        Sprite[] allSprites = Resources.LoadAll<Sprite>(path);
        foreach (Sprite sprite in allSprites)
        {
            if (sprite.name == name)
            {
                return sprite;
            }
        }
        Debug.LogWarning($"Sprite '{name}' not found in sprite sheet!");
        return null;
    }

    /// <summary>
    /// Calculates the modified color for display based on the modifiers.
    /// Red intensity is based on multModifier (tinting red), blue intensity is based on addModifier.
    /// If both are present, the color becomes purple.
    /// </summary>
    public Color GetModifiedColor()
    {
        float redIntensity = Mathf.Clamp01((multModifier - 1f) / 10f);
        float blueIntensity = Mathf.Clamp01(addModifier / 10f);
        float yellowIntensity = specialEffect != null ? 0.3f : 0f;

        Color baseColor = Color.white;
        
        if (redIntensity > 0f)
        {
            baseColor = Color.Lerp(baseColor, Color.red, redIntensity);
        }
        
        if (blueIntensity > 0f)
        {
            baseColor = Color.Lerp(baseColor, Color.blue, blueIntensity);
        }
        
        if (yellowIntensity > 0f)
        {
            baseColor = Color.Lerp(baseColor, Color.yellow, yellowIntensity);
        }

        return baseColor;
    }
}
