using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using ColourPalette;
using MathNet.Numerics.Distributions;

public class DashboardKPIEntry : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI m_nameField;
	[SerializeField] TextMeshProUGUI m_changeTextField;
	[SerializeField] RectTransform m_fillBar, m_newValueMarker, m_oldValueMarker, m_changeBar;
	[SerializeField] Image m_changeBarImage, m_oldValueMarkerImage;
	[SerializeField] Color m_increaseChangeColour, m_decreaseChangeColour;
	[SerializeField] float m_halfMarkerWidth;
	[SerializeField] CustomImage m_bgImage;
	[SerializeField] ColourAsset m_evenBGColour, m_oddBGColour;

	Normal m_normalDistr;

	private void Start()
	{
		m_bgImage.ColourAsset = transform.GetSiblingIndex() % 2 == 0 ? m_evenBGColour : m_oddBGColour;
	}

	public void SetKPI(KPIData a_kpi)
	{
		m_nameField.text = a_kpi.name;
	}

	public void SetValue(KPIValue a_value)
	{
		SetValue(a_value.currentValue, a_value.oldValue, a_value.stdev);
	}

	public void SetValue(float a_newValue, float a_oldValue, float a_stdev)
	{
		m_normalDistr = new Normal(0d, a_stdev);
		SetNormalizedValue((float)m_normalDistr.CumulativeDistribution(a_newValue), (float)m_normalDistr.CumulativeDistribution(a_oldValue));
	}

	public void SetNormalizedValue(float a_newValue, float a_oldValue)
	{
		m_fillBar.anchorMax = new Vector2(a_newValue, 1f);
		m_newValueMarker.anchorMin = new Vector2(a_newValue, 0f);
		m_newValueMarker.anchorMax = new Vector2(a_newValue, 1f);

		if (a_newValue > a_oldValue)
		{
			m_changeBar.gameObject.SetActive(true);
			m_oldValueMarker.gameObject.SetActive(true);
			m_changeBar.anchorMin = new Vector2(a_oldValue, 0f);
			m_changeBar.anchorMax = new Vector2(a_newValue, 1f);
			m_changeBar.localScale = new Vector3(-1f, 1f, 1f);
			m_changeBarImage.color = m_increaseChangeColour;
			m_oldValueMarkerImage.color = m_increaseChangeColour;
			m_oldValueMarker.anchorMin = new Vector2(a_oldValue, 0f);
			m_oldValueMarker.anchorMax = new Vector2(a_oldValue, 1f);
			m_oldValueMarker.offsetMin = new Vector2(0, m_oldValueMarker.offsetMin.y);
			m_oldValueMarker.offsetMax = new Vector2(2f*m_halfMarkerWidth, m_oldValueMarker.offsetMax.y);
			if(m_changeTextField != null)
				m_changeTextField.text = a_newValue > a_oldValue + 0.1f ? "++" : "+";
		}
		else if (a_newValue < a_oldValue)
		{
			m_changeBar.gameObject.SetActive(true);
			m_oldValueMarker.gameObject.SetActive(true);
			m_changeBar.anchorMin = new Vector2(a_newValue, 0f);
			m_changeBar.anchorMax = new Vector2(a_oldValue, 1f);
			m_changeBar.localScale = new Vector3(1f, 1f, 1f);
			m_changeBarImage.color = m_decreaseChangeColour;
			m_oldValueMarkerImage.color = m_decreaseChangeColour;
			m_oldValueMarker.anchorMin = new Vector2(a_oldValue, 0f);
			m_oldValueMarker.anchorMax = new Vector2(a_oldValue, 1f);
			m_oldValueMarker.offsetMin = new Vector2(-2f*m_halfMarkerWidth, m_oldValueMarker.offsetMin.y);
			m_oldValueMarker.offsetMax = new Vector2(0, m_oldValueMarker.offsetMax.y);
			if (m_changeTextField != null)
				m_changeTextField.text = a_newValue + 0.1f < a_oldValue  ? "−−" : "−";
		}
		else
		{
			m_changeBar.gameObject.SetActive(false);
			m_oldValueMarker.gameObject.SetActive(false);
			if (m_changeTextField != null)
				m_changeTextField.text = "";
		}
	}
}