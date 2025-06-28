using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Number component
/// </summary>
public class NumberComp : MonoBehaviour
{
    [SerializeField] private Image m_Img01;
    [SerializeField] private Image m_Img02;
    [SerializeField] private Sprite[] m_NumberSprites;

    public void SetText(int number)
    {
        var number01 = number % 10;
        var number02 = number / 10;
        var sprite01 = m_NumberSprites[number01];
        var sprite02 = m_NumberSprites[number02];
        m_Img02.sprite = sprite01;
        m_Img01.sprite = sprite02;
        m_Img01.enabled = number02 != 0;
    }
}