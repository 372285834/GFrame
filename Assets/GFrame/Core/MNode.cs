using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MNode : MonoBehaviour
{
    private RectTransform m_rectTransform;
    private RectTransform rectTransform
    {
        get
        {
            if (m_rectTransform == null)
                m_rectTransform = GetComponent<RectTransform>();
            return m_rectTransform;
        }
    }
    public bool Visible { get { return this.gameObject.activeInHierarchy; } set { this.gameObject.SetActive(value); } }
}
