using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Dice : MonoBehaviour
{
    // Structure to hold the three visible faces from a roll.
    public struct RollResult
    {
        public Face top;
        public Face left;
        public Face right;
    }

    [Header("Audio")]
    public AudioSource rollSound;


    // The final roll result after the animation completes.
    public RollResult finalRollResult;

    // Event fired when the dice roll animation is complete.
    public event System.Action OnRollComplete;

    [Header("Faces & Sprites")]
    public List<Face> faces = new List<Face>();

    [Header("Renderers")]
    public SpriteRenderer topRenderer;
    public SpriteRenderer leftRenderer;
    public SpriteRenderer rightRenderer;

    [Header("Spawn Points")]
    public Transform topSpawn;
    public Transform leftSpawn;
    public Transform rightSpawn;

    // Mapping from top face index to a list of valid side face index pairs.
    private Dictionary<int, List<(int left, int right)>> validSideMappingByIndex = new Dictionary<int, List<(int, int)>>
    {
        { 0, new List<(int, int)> { (1,3), (3,4), (4,2), (2,1) } },
        { 1, new List<(int, int)> { (0,2), (2,5), (5,3), (3,0) } },
        { 2, new List<(int, int)> { (1,0), (0,4), (4,5), (5,1) } },
        { 3, new List<(int, int)> { (0,1), (1,5), (5,4), (4,0) } },
        { 4, new List<(int, int)> { (0,3), (3,5), (5,2), (2,0) } },
        { 5, new List<(int, int)> { (1,2), (2,4), (4,3), (3,1) } }
    };

    /// <summary>
    /// Initializes the dice with default faces (6 faces).
    /// </summary>
    public void Initialize()
    {
        faces.Clear();
        // Create 6 faces corresponding to indices 0..5.
        for (int i = 1; i <= 6; i++)
        {
            faces.Add(new Face(value: i, addMod: 0, multMod: 1.0f));
        }

        // Ensure renderers are assigned properly.
        if (topRenderer == null || leftRenderer == null || rightRenderer == null)
        {
            AssignRenderers();
        }
    }

    /// <summary>
    /// Rolls the dice, returning a RollResult and starting the animation.
    /// </summary>
    public RollResult Roll()
    {
        // Choose a random top index.
        int topIndex = Random.Range(0, faces.Count);
        Face finalTop = faces[topIndex];

        // Look up the valid side pairs for this top index.
        List<(int left, int right)> validPairs = validSideMappingByIndex[topIndex];
        // Choose one valid pair randomly.
        (int leftIndex, int rightIndex) = validPairs[Random.Range(0, validPairs.Count)];
        Face finalLeft = faces[leftIndex];
        Face finalRight = faces[rightIndex];

        if (rollSound != null)
        {
            rollSound.Play();
        }

        // Start the roll animation coroutine.
        StartCoroutine(AnimateRoll(finalTop, finalLeft, finalRight));

        // Return a temporary RollResult (the final one will be set once animation finishes).
        RollResult tempResult = new RollResult
        {
            top = finalTop,
            left = finalLeft,
            right = finalRight
        };
        return tempResult;
    }

    /// <summary>
    /// Coroutine that animates the dice roll by flipping its Y scale and cycling through valid face combinations.
    /// When finished, the final roll result is stored and the OnRollComplete event is fired.
    /// </summary>
    private IEnumerator AnimateRoll(Face finalTop, Face finalLeft, Face finalRight)
    {
        int cycles = 7;         // Number of animation frames.
        float frameDuration = 0.1f;  // Duration per frame in seconds.

        Vector3 originalScale = transform.localScale;

        for (int i = 0; i < cycles; i++)
        {
            // Alternate Y scale between 0.3 and -0.3.
            float yScale = (i % 2 == 0) ? 0.3f : -0.3f;
            transform.localScale = new Vector3(originalScale.x, yScale, originalScale.z);

            // For animation, choose a random top index and its valid side pair.
            int animTopIndex = Random.Range(0, faces.Count);
            Face animTop = faces[animTopIndex];
            List<(int left, int right)> animPairs = validSideMappingByIndex[animTopIndex];
            (int animLeftIndex, int animRightIndex) = animPairs[Random.Range(0, animPairs.Count)];
            Face animLeft = faces[animLeftIndex];
            Face animRight = faces[animRightIndex];

            // Update visuals with these temporary valid faces.
            UpdateVisuals(animTop, animLeft, animRight);

            yield return new WaitForSeconds(frameDuration);
        }

        // Ensure Y scale ends at 0.3.
        transform.localScale = new Vector3(originalScale.x, 0.3f, originalScale.z);

        // Set the final result and update visuals.
        finalRollResult = new RollResult
        {
            top = finalTop,
            left = finalLeft,
            right = finalRight
        };
        UpdateVisuals(finalTop, finalLeft, finalRight);

        // Fire the event to signal that this dice has finished rolling.
        OnRollComplete?.Invoke();
    }

    /// <summary>
    /// Dynamically assigns the SpriteRenderers if not assigned in the Inspector.
    /// </summary>
    private void AssignRenderers()
    {
        topRenderer = transform.Find("TopFace").GetComponent<SpriteRenderer>();
        leftRenderer = transform.Find("LeftFace").GetComponent<SpriteRenderer>();
        rightRenderer = transform.Find("RightFace").GetComponent<SpriteRenderer>();

        if (topRenderer == null || leftRenderer == null || rightRenderer == null)
        {
            Debug.LogError("Missing SpriteRenderer references on dice!");
        }
    }

    /// <summary>
    /// Updates the dice visuals using the sprites from the Face class.
    /// </summary>
    private void UpdateVisuals(Face topFace, Face leftFace, Face rightFace)
    {
        // Get the modified color from one of the faces (or compute each separately if desired)
        Color topColor = topFace.GetModifiedColor();

        // Apply the color to each sprite renderer.
        // You could choose to use different colors per face if needed.
        topRenderer.sprite = topFace.TopSprite;
        topRenderer.color = topColor;

        leftRenderer.sprite = leftFace.LeftSprite;
        leftRenderer.color = leftFace.GetModifiedColor();

        rightRenderer.sprite = rightFace.RightSprite;
        rightRenderer.color = rightFace.GetModifiedColor();
    }
}
