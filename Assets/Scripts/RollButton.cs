using UnityEngine;

public class RollButton : MonoBehaviour
{
    public GameManager gameManager;

    private bool isRolling = false;  // Prevents multiple clicks

    private void OnMouseDown()
    {
        if (isRolling) return;  // Ignore clicks during animation

        if (gameManager != null)
        {
            isRolling = true;
            StartCoroutine(RollAndWait());
        }
        else
        {
            Debug.LogWarning("GameManager not assigned in the Inspector.");
        }
    }

    /// <summary>
    /// Rolls all dice and waits until the animation finishes.
    /// </summary>
    private System.Collections.IEnumerator RollAndWait()
    {
        gameManager.RollAllDice();

        // Ensure we wait for the dice animation duration
        float rollTime = 7 * 0.1f;  // 7 cycles, 0.1 seconds each
        yield return new WaitForSeconds(rollTime);

        // Re-enable the button
        isRolling = false;
    }
}
