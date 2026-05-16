using UnityEngine;

[CreateAssetMenu(fileName = "NewSlotSymbol", menuName = "Slot Game/Symbol")]
public class SlotSymbol : ScriptableObject
{
    public string symbolname; // e.g., "Cherry", "Jackpot"
    public Sprite symbolSprite;
    public int payoutMultiplier; // how much this symbol pays if it wins

    [Range(0f, 100f)]
    public float winWeight;
}
