using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public List<Face> inventoryFaces = new List<Face>();

    void Start()
    {
        // Add 3 default faces for debugging
        inventoryFaces.Add(new Face(1, 3, 2.5f));
        inventoryFaces.Add(new Face(3, 3));
        inventoryFaces.Add(new Face(5, 0, 10f));
    }

    public void AddFace(Face face)
    {
        if (face != null)
        {
            inventoryFaces.Add(face);
        }
        else
        {
            Debug.LogError("Attempted to add a null face to inventory.");
        }
    }

    public void RemoveFace(Face face)
    {
        if (face != null && inventoryFaces.Contains(face))
        {
            inventoryFaces.Remove(face);
        }
        else if (face != null)
        {
        }
        else
        {
            Debug.LogError("Attempted to remove a null face from inventory.");
        }
    }

    // Optional: Method to get all faces in the inventory
    public List<Face> GetInventoryFaces()
    {
        return inventoryFaces;
    }

    // Optional: Method to check if a face is in the inventory
    public bool ContainsFace(Face face)
    {
        return inventoryFaces.Contains(face);
    }
}