using Godot;
using System;
using System.IO;
using System.Text.RegularExpressions;

[Tool]
public partial class ItemCard : Control{
    #region VARIABLES

    //Controls
    [Export] private Button _deleteButton;
    [Export] private OptionButton _statTypes;
    [Export] private TextureButton _icon;
    [Export] private FileDialog _iconDialog;
    [Export] private LineEdit _name;
    [Export] private TextEdit _description;
    [Export] private SpinBox _amount;
    [Export] private ConfirmationDialog _deleteDialog;

    public PickupItem Item{ get; private set; }
    private const string REGEXPATTERN = @"[^\w\s]";

    #endregion

    #region EVENTS AND DELEGATES

    public delegate void OnDestroyed(ItemCard card);

    public OnDestroyed onDestroyed;

    public delegate void OnItemRenamed();

    public OnItemRenamed onItemRenamed;

    private bool _eventsConnected;

    #endregion
    
    #region MAIN

    public override void _EnterTree(){
        SetupCallbacks();
        UpdateStatTypes();
    }
    
    
    #endregion
    
    #region INIT

    /// <summary>
    /// Setup all event callbacks
    /// </summary>
    private void SetupCallbacks(){
        if (_eventsConnected) return;
        
        _deleteButton.Pressed += OnDeletePressed;
        _deleteDialog.Confirmed += OnDeleteConfirmed;
        _name.TextSubmitted += OnNameChanged;
        _name.FocusExited += OnNameChanged;
        _description.TextChanged += OnDescriptionChanged;
        _statTypes.ItemSelected += OnStatTypeChanged;
        _amount.ValueChanged += OnAmountChanged;
        _icon.Pressed += OnChangeIcon;
        _iconDialog.FileSelected += OnIconChanged;
        _iconDialog.Canceled += OnIconCleared;

        _eventsConnected = true;
    }

    /// <summary>
    /// Update the stat type control to reflect stat type enum
    /// </summary>
    private void UpdateStatTypes(){
        //Get all the stat names
        string[] statNames = Enum.GetNames(typeof(StatType));
        //Clear the contents of the stat type dropdown
        _statTypes.Clear();
        //Iterate over all stat names and add them to the dropdown
        for (int i = 0; i < statNames.Length; i++){
            _statTypes.AddItem(statNames[i]);
        }
    }
    
    
    #endregion
    
    #region SETUP

    /// <summary>
    /// Populate control values from given pickup item
    /// </summary>
    /// <param name="item"></param>
    public void SetData(PickupItem item){
        _icon.TextureNormal = item.icon;
        _name.Text = item.name;
        _statTypes.Selected = (int)item.statType;
        _description.Text = item.description;
        _amount.Value = item.amount;
        Item = item;
    }
    #endregion
    
    #region IO

    private void RenameItem(string name){
        File.Delete(ProjectSettings.GlobalizePath(Item.ResourcePath));
        name = CleanInput(name);
        onItemRenamed?.Invoke();
        Item.name = name;
        _name.Text = name;
        ResourceSaver.Save(Item, $"res://Pickup Items/{Item.name}.tres");
        Item = ResourceLoader.Load<PickupItem>($"res://Pickup Items/{Item.name}.tres");
    }

    private void SaveItem(){
        ResourceSaver.Save(Item, Item.ResourcePath);
    }

    /// <summary>
    /// Return a sanitized string safe for file naming
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private string CleanInput(string s){
        if (!Regex.IsMatch(s, REGEXPATTERN)) return s;
        try{
            GD.Print("Unsafe Characters Detected. Sanitizing Name.");
            return Regex.Replace(
                s,
                REGEXPATTERN,
                "",RegexOptions.None,
                TimeSpan.FromSeconds(1.5)
            );
        }
        catch(RegexMatchTimeoutException){
            return string.Empty;
        }
    }
    #endregion
    
    #region CALLBACK METHODs

    private void OnDeleteConfirmed(){
        if (Item == null) return;
        File.Delete(ProjectSettings.GlobalizePath(Item.ResourcePath));
        onDestroyed?.Invoke(this);
        QueueFree();
    }

    private void OnAmountChanged(double value){
        if (Item == null) return;
        Item.amount = (int)value;
        SaveItem();
    }

    private void OnStatTypeChanged(long index){
        if (Item == null) return;
        Item.statType = (StatType)index;
        SaveItem();
    }

    private void OnNameChanged(){
        if (Item == null) return;
        if (_name.Text == Item.name) return;
        RenameItem(_name.Text);
    }

    private void OnNameChanged(string n){
        if (Item == null) return;
        if (n == Item.name) return;
        RenameItem(n);
    }

    private void OnDescriptionChanged(){
        if (Item == null) return;
        Item.description = _description.Text;
        SaveItem();
    }
    
    private void OnIconCleared(){
        if (Item == null) return;
        Item.icon = null;
        _icon.TextureNormal = null;
        SaveItem();
    }

    private void OnIconChanged(string path){
        if(Item == null) return;
        Item.icon = GD.Load<Texture2D>(path);
        _icon.TextureNormal = Item.icon;
        SaveItem();
    }

    private void OnChangeIcon(){
        _iconDialog.Visible = true;
    }

    private void OnDeletePressed(){
        _deleteDialog.Visible = true;
    }
    
    #endregion
}