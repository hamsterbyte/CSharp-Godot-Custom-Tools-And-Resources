using Godot;

[GlobalClass, Tool]
public partial class PickupItem : Resource
{
    #region VARIABLES

    [Export] public Texture2D icon;
    [Export] public string name;
    [Export] public StatType statType;
    [Export] public string description;
    [Export] public int amount;

    #endregion
}

public enum StatType{
    Health,
    Mana,
    Armor
}
