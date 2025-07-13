// File: Dice.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Dice : MonoBehaviour
{
    public Sprite Side1;
    public Sprite Side2;
    public Sprite Side3;
    public Sprite Side4;
    public Sprite Side5;
    public Sprite Side6;

    void Start()
    {
        GetComponent<Image>().sprite = Side6;
    }

    public void DisplayDice(int rolledValue)
    {
        Image diceImage = GetComponent<Image>();
        switch (rolledValue)
        {
            case 1:
                diceImage.sprite = Side1;
                break;
            case 2:
                diceImage.sprite = Side2;
                break;
            case 3:
                diceImage.sprite = Side3;
                break;
            case 4:
                diceImage.sprite = Side4;
                break;
            case 5:
                diceImage.sprite = Side5;
                break;
            case 6:
                diceImage.sprite = Side6;
                break;
            default:
                Debug.LogWarning("Invalid dice value: " + rolledValue);
                break;
        }
    }
}