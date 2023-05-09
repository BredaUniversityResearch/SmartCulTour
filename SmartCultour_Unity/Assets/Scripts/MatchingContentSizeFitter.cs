using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchingContentSizeFitter : ContentSizeFitter
{
	[SerializeField] private RectTransform m_matchingTransform;

	[System.NonSerialized] private RectTransform m_Rect;
	private RectTransform rectTransform
	{
		get
		{
			if (m_Rect == null)
				m_Rect = GetComponent<RectTransform>();
			return m_Rect;
		}
	}

	public override void SetLayoutVertical()
	{
		base.SetLayoutVertical();
		m_matchingTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, LayoutUtility.GetPreferredSize(rectTransform, 1));
	}
}
