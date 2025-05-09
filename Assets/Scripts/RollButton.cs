using UnityEngine;

public class RollButton : MonoBehaviour
{
    public GameManager gameManager;

    private bool isRolling = false;

    private void OnMouseDown()
    {
        if (isRolling) return;

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

        float rollTime = 7 * 0.1f;
        yield return new WaitForSeconds(rollTime);

        isRolling = false;
    }
}
