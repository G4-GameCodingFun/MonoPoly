using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PropertyItemUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI propertyNameText;
    public TextMeshProUGUI sellValueText;
    public Toggle selectionToggle;
    public Button selectButton;
    public Image backgroundImage;
    
    [Header("Visual States")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color hoverColor = Color.cyan;
    
    private PropertyTile propertyTile;
    private bool isSelected = false;
    
    public void Setup(PropertyTile property)
    {
        propertyTile = property;
        
        if (propertyNameText != null)
        {
            string houseInfo = "";
            if (property.houseCount > 0)
                houseInfo = $" ({property.houseCount} nhà)";
            else if (property.hasHotel)
                houseInfo = " (Khách sạn)";
                
            propertyNameText.text = property.tileName + houseInfo;
        }
        
        if (sellValueText != null)
        {
            int sellValue = property.GetPrice() / 2;
            sellValueText.text = $"Bán: {sellValue}$";
        }
        
        if (selectionToggle != null)
        {
            selectionToggle.isOn = false;
            selectionToggle.onValueChanged.AddListener(OnToggleChanged);
        }
        
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnButtonClicked);
        }
        
        UpdateVisualState();
    }
    
    private void OnToggleChanged(bool isOn)
    {
        isSelected = isOn;
        UpdateVisualState();
    }
    
    private void OnButtonClicked()
    {
        if (selectionToggle != null)
        {
            selectionToggle.isOn = !selectionToggle.isOn;
        }
    }
    
    private void UpdateVisualState()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor : normalColor;
        }
    }
    
    public bool IsSelected()
    {
        return isSelected;
    }
    
    public PropertyTile GetProperty()
    {
        return propertyTile;
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selectionToggle != null)
        {
            selectionToggle.isOn = selected;
        }
        UpdateVisualState();
    }
} 