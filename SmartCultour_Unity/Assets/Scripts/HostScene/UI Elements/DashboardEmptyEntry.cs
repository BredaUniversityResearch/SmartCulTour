using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using ColourPalette;

public class DashboardEmptyEntry : MonoBehaviour
{
	[SerializeField]
	CustomImage m_bgImage;
	[SerializeField]
	ColourAsset m_evenBGColour, m_oddBGColour;

	private void Start()
	{
		m_bgImage.ColourAsset = transform.GetSiblingIndex() % 2 == 0 ? m_evenBGColour : m_oddBGColour;
	}	
}