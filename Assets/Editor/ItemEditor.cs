using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemEditor : EditorWindow
{
    private ScrollView _scrollView;
    private List<ItemData> _savedItems = new List<ItemData>();

    [MenuItem("Tools/Item Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<ItemEditor>();
        window.titleContent = new GUIContent("Item Editor");
    }

    private void OnEnable()
    {
        var root = this.rootVisualElement;
        root.style.flexDirection = FlexDirection.Row;
        
        var itemsListBox = new Box();
        itemsListBox.style.flexGrow = 1f;
        itemsListBox.style.flexShrink = 0f;
        itemsListBox.style.flexBasis = 0f;
        itemsListBox.style.flexDirection = FlexDirection.Column;

        var newItemBox = new Box();
        newItemBox.style.flexGrow = 3f;
        newItemBox.style.flexShrink = 0f;
        newItemBox.style.flexBasis = 0f;

        SetupItemList(itemsListBox);
        SetupFields(newItemBox);
        
        root.Add(itemsListBox);
        root.Add(newItemBox);

        LoadData();
    }

    private void SetupFields(Box parent)
    {
        var newItemLabel = new Label("New Item");
        newItemLabel.style.alignSelf = Align.Center;
        newItemLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        parent.Add(newItemLabel);

        var nameField = new TextField("Name: ");
        var rarityField = new EnumField("Rarity: ", ItemRarity.Common);
        var priceField = new IntegerField("Price: ");
        parent.Add(nameField);
        parent.Add(rarityField);
        parent.Add(priceField);
        
        var saveItemButton = new Button();
        saveItemButton.text = "Save Item";
        saveItemButton.clicked += () => {
            if (string.IsNullOrWhiteSpace(nameField.value) == false)
            {
                var item = new ItemData();
                item.id = new Guid().GetHashCode();
                item.name = nameField.value;
                item.rarity = (ItemRarity)rarityField.value;
                item.price = priceField.value;

                _savedItems.Add(item);

                //  set default values to clear fields
                nameField.value = "";
                rarityField.value = ItemRarity.Common;
                priceField.value = 0;

                CreateListItem(item);
                SaveData();
            }
        };

        parent.Add(saveItemButton);
    }

    private void SetupItemList(Box parent)
    {
        var listLabel = new Label("Item List");
        listLabel.style.alignSelf = Align.Center;
        listLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        parent.Add(listLabel);

        _scrollView = new ScrollView();
        _scrollView.showHorizontal = false;
        _scrollView.style.flexGrow = 1f;
        parent.Add(_scrollView);
    }

    private void CreateListItem(ItemData itemData)
    {
        var itemElement = new VisualElement();
        itemElement.style.flexDirection = FlexDirection.Row;
        itemElement.focusable = true;

        var remove = new Button();
        remove.text = "-";
        remove.clicked += () => {
            _scrollView.contentContainer.Remove(itemElement);
            _savedItems.Remove(itemData);
            SaveData();
        };
        itemElement.Add(remove);

        var nameButton = new Button();
        nameButton.text = itemData.name;
        nameButton.style.flexGrow = 1f;
        itemElement.Add(nameButton);

        _scrollView.contentContainer.Add(itemElement);
    }

    public void SaveData()
    {
        var path = Application.dataPath + "/Resources/items.json";

        var itemsFile = new ItemsFile();
        itemsFile.data = new List<ItemData>(_savedItems);
        var itemsFileAsJson = JsonUtility.ToJson(itemsFile, true);
        System.IO.File.WriteAllText(path, itemsFileAsJson);
    }

    public void LoadData()
    {
        var path = Application.dataPath + "/Resources/items.json";

        if (System.IO.File.Exists(path))
        {
            var file = System.IO.File.ReadAllText(path);
            _savedItems = JsonUtility.FromJson<ItemsFile>(file).data;
            
            foreach (var item in _savedItems)
            {
                CreateListItem(item);
            }
        }
    }
}

public enum ItemRarity
{
    Common,
    Rare,
    Unique
}

[System.Serializable]
public class ItemData
{
    public int id;
    public string name;
    public ItemRarity rarity;
    public int price;
}

public class ItemsFile
{
    public List<ItemData> data;
}