using UnityEngine;
[System.Serializable]
public class ShopItem
{
    public enum ItemType { NewFace, UpgradeFace }
    public ItemType itemType;

    // For new faces, these values are generated.
    public Face newFace;

    // For upgrades, this is the reference to the face to upgrade.
    public Face targetFace;

    public float cost;

    // A description that you can show in the UI.
    public int addMod;
    public float multMod;
}
