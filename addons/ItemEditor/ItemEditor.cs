#if TOOLS
using Godot;
using System.IO;
using System.Collections.Generic;

[Tool]
public partial class ItemEditor : EditorPlugin
{
	
	#region VARIABLES

	private PackedScene _editorPanel = ResourceLoader.Load<PackedScene>("res://addons/ItemEditor/ItemEditorMain.tscn");
	private Control _editorPanelInstance;
	
	//Controls
	private Button _btnCreate;
	private Button _btnRefresh;
	private LineEdit _filter;
	private FlowContainer _content;
	
	//Paths
	private const string PATH = "res://Pickup Items";
	private string[] _globalPaths;
	
	//cards
	private PackedScene _itemCard = ResourceLoader.Load<PackedScene>("res://addons/ItemEditor/ItemCard.tscn");
	private readonly List<ItemCard> _itemCards = new();
	
	#endregion
	
	#region MAIN
	
	public override void _EnterTree(){
		Init();
	}

	public override void _ExitTree(){
		Cleanup();
	}

	public override bool _HasMainScreen(){
		return true;
	}

	public override void _MakeVisible(bool visible){
		if (_editorPanelInstance != null){
			_editorPanelInstance.Visible = visible;
		}
	}

	public override string _GetPluginName(){
		return "Item Editor";
	}

	public override Texture2D _GetPluginIcon(){
		return GetEditorInterface().GetBaseControl().GetThemeIcon("ResourcePreloader", "EditorIcons");
	}
	
	#endregion
	
	#region INIT

	private void Init(){
		SetupWindow();
		SetupDependencies();
		SetupCallbacks();
		RefreshItems();
	}

	

	private void SetupCallbacks(){
		if (_btnCreate != null) _btnCreate.Pressed += CreateItem;
		if (_btnRefresh != null) _btnRefresh.Pressed += RefreshItems;
		if (_filter != null) _filter.TextChanged += FilterItems;
	}

	private void SetupDependencies(){
		_btnCreate = _editorPanelInstance.GetNode("Toolbar/Margins/Layout/btn_Create") as Button;
		_btnRefresh = _editorPanelInstance.GetNode("Toolbar/Margins/Layout/btn_Refresh") as Button;
		_content = _editorPanelInstance.GetNode("Item Container/Margins/ScrollContainer/Content") as FlowContainer;
		_filter = _editorPanelInstance.GetNode("Toolbar/Margins/Layout/txt_Filter") as LineEdit;
	}

	private void SetupWindow(){
		_editorPanelInstance = (Control)_editorPanel.Instantiate();
		GetEditorInterface().GetEditorMainScreen().AddChild(_editorPanelInstance);
		_MakeVisible(false);
	}

	#endregion
	
	#region CALLBACK METHODS
	private void RefreshItems(){
		_globalPaths = Directory.GetFiles(ProjectSettings.GlobalizePath(PATH), "*.tres");
		PickupItem[] _items = new PickupItem[_globalPaths.Length];
		ClearContents();
		for (int i = 0; i < _globalPaths.Length; i++){
			_items[i] = ResourceLoader.Load<PickupItem>(ProjectSettings.LocalizePath(_globalPaths[i]));
			AddItemToContents(_items[i]);
		}

		_filter.Text = string.Empty;
	}

	private void FilterItems(string filter){
		filter = filter.ToLower();
		if (filter == string.Empty){
			for (int i = 0; i < _itemCards.Count; i++){
				_itemCards[i].Visible = true;
			}
		}
		else{
			for (int i = 0; i < _itemCards.Count; i++){
				_itemCards[i].Visible = _itemCards[i].Item.name.ToLower().Contains(filter) ||
				                        _itemCards[i].Item.statType.ToString().ToLower().Contains(filter) ||
				                        _itemCards[i].Item.description.ToLower().Contains(filter);
			}			
		}
	}

	private void CreateItem(){
		PickupItem temp = new();
		ResourceSaver.Save(temp, $"res://Pickup Items/{temp.GetInstanceId()}.tres");
		PickupItem itemOnDisk = ResourceLoader.Load<PickupItem>($"res://Pickup Items/{temp.GetInstanceId()}.tres");
		AddItemToContents(itemOnDisk);
	}
	
	#endregion
	
	#region HELPER METHODS

	private void ClearContents(){
		foreach (Node child in _content.GetChildren()){
			_content.RemoveChild(child);
			child.QueueFree();
		}
		_itemCards.Clear();
	}

	private void AddItemToContents(PickupItem item){
		Node current = _itemCard.Instantiate();
		_content.AddChild(current);
		if (current is not ItemCard currentScript) return;
		currentScript.SetData(item);
		currentScript.onDestroyed += DeleteItem;
		currentScript.onItemRenamed += OnItemRenamed;
		_itemCards.Add(currentScript);
	}

	private void OnItemRenamed(){
		RefreshFileSystem();
	}

	private void DeleteItem(ItemCard card){
		if (card == null) return;
		_itemCards.Remove(card);
		RefreshFileSystem();
	}

	private void RefreshFileSystem(){
		GetEditorInterface().GetResourceFilesystem().Scan();
	}

	private void Cleanup(){
		ClearCallbacks();
		_editorPanelInstance.QueueFree();
	}

	private void ClearCallbacks(){
		if (_btnCreate != null) _btnCreate.Pressed -= CreateItem;
		if (_btnRefresh != null) _btnRefresh.Pressed -= RefreshItems;
		if (_filter != null) _filter.TextChanged -= FilterItems;
	}

	#endregion
	
	
	
	
}
#endif
