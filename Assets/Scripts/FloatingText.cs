using UnityEngine;
using TMPro;
using System.Collections;
public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 30f;
    public float fadeDuration = 1.5f;

    private TextMeshProUGUI textComponent;
    private RectTransform rectTransform;
    private float lifetime;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        lifetime = fadeDuration;
    }

    /// <summary>
    /// Starts the floating animation.
    /// </summary>
    public void StartFloating()
    {
        StartCoroutine(FloatAndFade());
    }

    /// <summary>
    /// Coroutine to move the text upward and fade it out.
    /// </summary>
    private IEnumerator FloatAndFade()
    {
        Color originalColor = textComponent.color;
        Vector2 moveDirection = Vector2.up;

        while (lifetime > 0)
        {
            float deltaTime = Time.deltaTime;

            // Move the text upward.
            rectTransform.anchoredPosition += moveDirection * moveSpeed * deltaTime;

            // Gradually fade the text.
            float alpha = Mathf.Lerp(0, 1, lifetime / fadeDuration);
            textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            lifetime -= deltaTime;
            yield return null;
        }

        // Destroy the object after the animation finishes.
        Destroy(gameObject);
    }
}
