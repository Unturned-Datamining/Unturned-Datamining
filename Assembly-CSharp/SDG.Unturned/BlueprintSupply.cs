namespace SDG.Unturned;

public class BlueprintSupply
{
    private ushort _id;

    private bool _isCritical;

    public ushort amount;

    public ushort hasAmount;

    public ushort id => _id;

    public bool isCritical => _isCritical;

    /// <summary>
    /// If true, items with an "amount" of zero are included in eligible supplies as amount 1.
    /// In practice (as of 2025-03-03), items with zero amount are empty containers such as magazines.
    /// </summary>
    public bool ShouldTreatEmptyAsOne { get; private set; }

    /// <summary>
    /// Controls which items are used first. For example, whether to use the lowest quality items first.
    /// </summary>
    public ECraftingInputPrioritization Prioritization { get; private set; }

    public BlueprintSupply(ushort newID, bool newCritical, byte newAmount, bool newTreatEmptyAsOne, ECraftingInputPrioritization newPrioritization)
    {
        _id = newID;
        _isCritical = newCritical;
        ShouldTreatEmptyAsOne = newTreatEmptyAsOne;
        Prioritization = newPrioritization;
        amount = newAmount;
        hasAmount = 0;
    }
}
