using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDOfferController : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text offerText;
    public Button yesButton;
    public Button noButton;

    private PropertyTile currentTile;
    private int currentGroupSize;
    private int currentType;

    public void Show(PropertyTile tile, int groupSize, int type)
    {
        currentTile = tile;
        currentGroupSize = groupSize;
        currentType = type;
        panel.SetActive(true);
        offerText.text = $"Bạn sở hữu đủ {groupSize} nhà cùng màu!\nNâng cấp lên khách sạn với giá 1200$?\n(Giá thuê x5, giá bán x3)";
    }

    public void Hide()
    {
        panel.SetActive(false);
        currentTile = null;
    }

    private void Start()
    {
        yesButton.onClick.AddListener(OnYes);
        noButton.onClick.AddListener(Hide);
        Hide();
    }

    private void OnYes()
    {
        if (currentTile != null && currentTile.owner != null && currentTile.owner.CanPay(1200))
        {
            currentTile.owner.money -= 1200;
            currentTile.hasHotel = true;
            currentTile.houseCount = 0;
            currentTile.UpdateVisuals();
            Hide();
        }
    }
} 