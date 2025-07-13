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

    private int RolledValue = 0;
    public GameManager GameManager;

    void Start()
    {
        GetComponent<Image>().sprite = Side6;
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void RollDie()
    {
        StartCoroutine(DieRolling());
    }

    public IEnumerator DieRolling()
    {
        Debug.Log("Rolling");
        for (int i = 0; i <= 20; i++)
        {
            RolledValue = Random.Range(1, 7);
            DisplayDice(RolledValue);
            yield return new WaitForSeconds(0.05f);
        }
        GameManager.AddDiceRoll(RolledValue);
    }

    private void DisplayDice(int RolledValue)
    {
        Image diceImage = GetComponent<Image>();
        switch (RolledValue)
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
                Debug.LogWarning("Invalid dice value: " + RolledValue);
                break;
        }
    }
}