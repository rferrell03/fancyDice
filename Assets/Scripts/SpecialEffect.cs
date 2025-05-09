using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class SpecialEffect
{
    public abstract float Activate(Face triggeringFace, HashSet<Face> triggerChain = null);
}

public class TriggerSameNum : SpecialEffect
{
    private GameManager gameManager;

    public TriggerSameNum() 
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    override public float Activate(Face triggeringFace, HashSet<Face> triggerChain = null)
    {
        // Initialize the trigger chain if not provided
        if (triggerChain == null)
        {
            triggerChain = new HashSet<Face>();
        }

        // Add the triggering face to the chain
        triggerChain.Add(triggeringFace);

        float moneyGained = 0;
        foreach (Dice d in gameManager.diceList)
        {
            Face top = d.finalRollResult.top;
            Face left = d.finalRollResult.left;
            Face right = d.finalRollResult.right;

            // Only trigger faces that aren't in the current trigger chain
            // This prevents circular triggers (A→B→A) but allows multiple sources (A→C, B→C)
            if (top.Value == triggeringFace.Value && !triggerChain.Contains(top))
            {
                moneyGained += top.Trigger(d, gameManager.floatingTextManager, 0, gameManager.currentMaxFaceValue, triggerChain);
            }
            if (left.Value == triggeringFace.Value && !triggerChain.Contains(left))
            {
                moneyGained += left.Trigger(d, gameManager.floatingTextManager, 1, gameManager.currentMaxFaceValue, triggerChain);
            }
            if (right.Value == triggeringFace.Value && !triggerChain.Contains(right))
            {
                moneyGained += right.Trigger(d, gameManager.floatingTextManager, 2, gameManager.currentMaxFaceValue, triggerChain);
            }
        }
        return moneyGained;
    }
}

public class CascadingTrigger : SpecialEffect
{
    private GameManager gameManager;

    public CascadingTrigger()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    override public float Activate(Face triggeringFace, HashSet<Face> triggerChain = null)
    {
        if (triggerChain == null)
        {
            triggerChain = new HashSet<Face>();
        }

        triggerChain.Add(triggeringFace);
        float moneyGained = 0;

        // Find faces with value one higher than the triggering face
        int targetValue = triggeringFace.Value + 1;
        foreach (Dice d in gameManager.diceList)
        {
            Face top = d.finalRollResult.top;
            Face left = d.finalRollResult.left;
            Face right = d.finalRollResult.right;

            if (top.Value == targetValue && !triggerChain.Contains(top))
            {
                moneyGained += top.Trigger(d, gameManager.floatingTextManager, 0, gameManager.currentMaxFaceValue, triggerChain);
            }
            if (left.Value == targetValue && !triggerChain.Contains(left))
            {
                moneyGained += left.Trigger(d, gameManager.floatingTextManager, 1, gameManager.currentMaxFaceValue, triggerChain);
            }
            if (right.Value == targetValue && !triggerChain.Contains(right))
            {
                moneyGained += right.Trigger(d, gameManager.floatingTextManager, 2, gameManager.currentMaxFaceValue, triggerChain);
            }
        }
        return moneyGained;
    }
}

public class ComboTrigger : SpecialEffect
{
    private GameManager gameManager;

    public ComboTrigger()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    override public float Activate(Face triggeringFace, HashSet<Face> triggerChain = null)
    {
        if (triggerChain == null)
        {
            triggerChain = new HashSet<Face>();
        }

        triggerChain.Add(triggeringFace);
        float moneyGained = 0;

        // Collect all face values
        List<int> faceValues = new List<int>();
        foreach (Dice d in gameManager.diceList)
        {
            faceValues.Add(d.finalRollResult.top.Value);
            faceValues.Add(d.finalRollResult.left.Value);
            faceValues.Add(d.finalRollResult.right.Value);
        }

        // Check for N of a kind
        Dictionary<int, int> valueCounts = new Dictionary<int, int>();
        foreach (int value in faceValues)
        {
            if (!valueCounts.ContainsKey(value))
                valueCounts[value] = 0;
            valueCounts[value]++;
        }

        // Check for straight
        bool isStraight = false;
        if (faceValues.Count >= 5)
        {
            List<int> sortedValues = new List<int>(faceValues);
            sortedValues.Sort();
            isStraight = true;
            for (int i = 1; i < sortedValues.Count; i++)
            {
                if (sortedValues[i] != sortedValues[i-1] + 1)
                {
                    isStraight = false;
                    break;
                }
            }
        }

        // If we found a valid combo, trigger all faces
        if (isStraight || valueCounts.Values.Any(count => count >= 3))
        {
            foreach (Dice d in gameManager.diceList)
            {
                Face top = d.finalRollResult.top;
                Face left = d.finalRollResult.left;
                Face right = d.finalRollResult.right;

                if (!triggerChain.Contains(top))
                {
                    moneyGained += top.Trigger(d, gameManager.floatingTextManager, 0, gameManager.currentMaxFaceValue, triggerChain);
                }
                if (!triggerChain.Contains(left))
                {
                    moneyGained += left.Trigger(d, gameManager.floatingTextManager, 1, gameManager.currentMaxFaceValue, triggerChain);
                }
                if (!triggerChain.Contains(right))
                {
                    moneyGained += right.Trigger(d, gameManager.floatingTextManager, 2, gameManager.currentMaxFaceValue, triggerChain);
                }
            }
        }

        return moneyGained;
    }
}

public class EvenTrigger : SpecialEffect
{
    private GameManager gameManager;

    public EvenTrigger()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    override public float Activate(Face triggeringFace, HashSet<Face> triggerChain = null)
    {
        if (triggerChain == null)
        {
            triggerChain = new HashSet<Face>();
        }

        triggerChain.Add(triggeringFace);
        float moneyGained = 0;

        foreach (Dice d in gameManager.diceList)
        {
            Face top = d.finalRollResult.top;
            Face left = d.finalRollResult.left;
            Face right = d.finalRollResult.right;

            if (top.Value % 2 == 0 && !triggerChain.Contains(top))
            {
                moneyGained += top.Trigger(d, gameManager.floatingTextManager, 0, gameManager.currentMaxFaceValue, triggerChain);
            }
            if (left.Value % 2 == 0 && !triggerChain.Contains(left))
            {
                moneyGained += left.Trigger(d, gameManager.floatingTextManager, 1, gameManager.currentMaxFaceValue, triggerChain);
            }
            if (right.Value % 2 == 0 && !triggerChain.Contains(right))
            {
                moneyGained += right.Trigger(d, gameManager.floatingTextManager, 2, gameManager.currentMaxFaceValue, triggerChain);
            }
        }
        return moneyGained;
    }
}

public class OddTrigger : SpecialEffect
{
    private GameManager gameManager;

    public OddTrigger()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    override public float Activate(Face triggeringFace, HashSet<Face> triggerChain = null)
    {
        if (triggerChain == null)
        {
            triggerChain = new HashSet<Face>();
        }

        triggerChain.Add(triggeringFace);
        float moneyGained = 0;

        foreach (Dice d in gameManager.diceList)
        {
            Face top = d.finalRollResult.top;
            Face left = d.finalRollResult.left;
            Face right = d.finalRollResult.right;

            if (top.Value % 2 == 1 && !triggerChain.Contains(top))
            {
                moneyGained += top.Trigger(d, gameManager.floatingTextManager, 0, gameManager.currentMaxFaceValue, triggerChain);
            }
            if (left.Value % 2 == 1 && !triggerChain.Contains(left))
            {
                moneyGained += left.Trigger(d, gameManager.floatingTextManager, 1, gameManager.currentMaxFaceValue, triggerChain);
            }
            if (right.Value % 2 == 1 && !triggerChain.Contains(right))
            {
                moneyGained += right.Trigger(d, gameManager.floatingTextManager, 2, gameManager.currentMaxFaceValue, triggerChain);
            }
        }
        return moneyGained;
    }
}
