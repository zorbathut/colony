using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Collections;

public class MultiplicativeFader : MonoBehaviour
{
    [SerializeField] float m_Target;

    public void Change(float alpha)
    {
        float targetAlpha = alpha * m_Target;

        foreach (Text text in GetComponents<Text>())
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, targetAlpha);
        }

        foreach (Image image in GetComponents<Image>())
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, targetAlpha);
        }
    }
}
