using UnityEngine;
using System.Collections;

public static class UIUtil
{
    public static T RectInstantiate<T>(T element, Transform parent) where T : MonoBehaviour
    {
        T newElement = GameObject.Instantiate(element);
        newElement.transform.SetParent(parent, false);
        newElement.GetComponent<RectTransform>().sizeDelta = element.GetComponent<RectTransform>().sizeDelta;
        newElement.GetComponent<RectTransform>().pivot = element.GetComponent<RectTransform>().pivot;
        newElement.GetComponent<RectTransform>().anchoredPosition = element.GetComponent<RectTransform>().anchoredPosition;
        return newElement;
    }
}
