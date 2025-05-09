using UnityEngine;
using TMPro;

public class FloatingTextManager : MonoBehaviour
{
    public GameObject floatingTextPrefab;
    public Canvas canvas;

    // Define the gradient colors
    private Color colorLow = Color.blue;
    private Color colorMidLow = Color.green;
    private Color colorMidHigh = Color.yellow;
    private Color colorHigh = Color.red;
    // Max value is now passed in, no longer stored here

    /// <summary>
    /// Spawns floating text at the specified spawn point position with color based on value relative to a maximum.
    /// </summary>
    /// <param name="text">The string to display (e.g., "+3").</param>
    /// <param name="spawnPoint">The world position transform where the text should start.</param>
    /// <param name="value">The numerical value this text represents (for color calculation).</param>
    /// <param name="maxValue">The current maximum potential value used for normalization.</param>
    public void CreateFloatingText(string text, Transform spawnPoint, float value, float maxValue) // Added maxValue parameter
    {
        // --- Initial null checks remain the same ---
        if (floatingTextPrefab == null || canvas == null || spawnPoint == null)
        {
            Debug.LogError("FloatingTextManager is missing references (Prefab, Canvas, or SpawnPoint was null).");
            return;
        }
        if (Camera.main == null)
        {
            Debug.LogError("Main Camera not found. Ensure your camera is tagged 'MainCamera'.");
            return;
        }

        // Convert the world position to canvas space.
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(spawnPoint.position);

        // Add random offset to screen position
        screenPosition.x += Random.Range(-10f, 10f); // Increased range to +/- 2.5
        screenPosition.y += Random.Range(-15f, 10f); // Increased range to +/- 2.5

        // Instantiate the floating text on the canvas.
        GameObject floatingTextObj = Instantiate(floatingTextPrefab, canvas.transform);

        // Set its position relative to the canvas.
        RectTransform rectTransform = floatingTextObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("FloatingText Prefab is missing RectTransform component.");
            Destroy(floatingTextObj); return;
        }
        rectTransform.position = screenPosition;

        // Get the TextMeshPro component.
        TextMeshProUGUI textComponent = floatingTextObj.GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            Debug.LogError("FloatingText Prefab is missing TextMeshProUGUI component.");
            Destroy(floatingTextObj); return;
        }

        // --- Color Calculation ---
        // Ensure maxValue is at least 1 to avoid division by zero/weirdness
        float safeMaxValue = Mathf.Max(1f, maxValue);
        // Normalize the value between 0 and 1 based on the PASSED-IN max value
        float normalizedValue = Mathf.Clamp01(value / safeMaxValue);
        Color valueColor;

        // Determine color based on normalized value using Lerp (same logic as before)
        if (normalizedValue <= 0.33f)
        {
            valueColor = Color.Lerp(colorLow, colorMidLow, normalizedValue / 0.33f);
        }
        else if (normalizedValue <= 0.66f)
        {
            valueColor = Color.Lerp(colorMidLow, colorMidHigh, (normalizedValue - 0.33f) / 0.33f);
        }
        else
        {
            valueColor = Color.Lerp(colorMidHigh, colorHigh, (normalizedValue - 0.66f) / 0.34f);
        }

        textComponent.color = valueColor;
        // --- End Color Calculation ---


        // Set the text content.
        textComponent.text = text;

        // Start the floating animation.
        FloatingText floatingText = floatingTextObj.GetComponent<FloatingText>();
        if (floatingText == null)
        {
            Debug.LogError("FloatingText Prefab is missing FloatingText script component.");
            Destroy(floatingTextObj); return;
        }
        floatingText.StartFloating();
    }
}
