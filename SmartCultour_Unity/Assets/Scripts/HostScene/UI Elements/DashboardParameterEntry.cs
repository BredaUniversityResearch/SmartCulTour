using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using ColourPalette;

public class DashboardParameterEntry : MonoBehaviour
{
	[SerializeField]
	TextMeshProUGUI m_nameText;
	
	[SerializeField]
	RectTransform m_barCenter;
	[SerializeField]
	TextMeshProUGUI m_barText;
	[SerializeField]
	RectTransform m_fillBar;
	[SerializeField]
	CustomImage m_barContainerImage, m_fillBarImage, m_bgImage;
	[SerializeField]
	ColourAsset m_increaseColour, m_decreaseColour, m_percentageColour, m_evenBGColour, m_oddBGColour;
	[SerializeField]
	Sprite m_increaseSprite, m_decreaseSprite, m_bothSprite;

	ParameterData m_parameter;

	private void Start()
	{
		m_bgImage.ColourAsset = transform.GetSiblingIndex() % 2 == 0 ? m_evenBGColour : m_oddBGColour;
	}

	public void SetToParameter(ParameterData a_parameter)
	{
		m_parameter = a_parameter;
		m_nameText.text = a_parameter.name;
		m_barContainerImage.sprite = m_parameter.yesNoQuestion ? m_increaseSprite : m_bothSprite;
		if (m_parameter.yesNoQuestion)
		{
			m_barCenter.anchorMin = new Vector2(0f, 0f);
			m_barCenter.anchorMax = new Vector2(0f, 1f);
			m_fillBarImage.ColourAsset = m_percentageColour;
			m_fillBarImage.sprite = m_increaseSprite;
		}
		else
		{
			m_barCenter.anchorMin = new Vector2(0.5f, 0f);
			m_barCenter.anchorMax = new Vector2(0.5f, 1f);
		}
	}

	public void ResetValue()
	{
		m_barText.text = "?";
		m_fillBar.gameObject.SetActive(false);
	}
	
	public void SetForMultipleInterventions(List<float> a_values, float a_maxValue)
	{
		if(m_parameter.yesNoQuestion)
		{
			SetPercentageValue(a_values.Average());
		}
		else
		{
			float combined = a_values.Sum();
			if (combined > 0.01f)
			{
				m_barText.text = "+" + combined.ToString("N1");
				SetIncrease(combined / a_maxValue);
			}
			else if (combined < -0.01f)
			{
				m_barText.text = combined.ToString("N1");
				SetDecrease(combined / a_maxValue);
			}
			else
			{
				m_barText.text = "0";
				m_fillBar.gameObject.SetActive(false);
			}
		}
	}

	public void SetForSingleIntervention(List<float> a_values)
	{
		if (m_parameter.yesNoQuestion)
		{
			if (a_values != null && a_values.Count > 0)
			{
				SetPercentageValue(a_values.Average());
			}
			else
			{
				m_barText.text = "?";
				m_fillBar.gameObject.SetActive(false);
			}
		}
		else
		{
			float avg = 0;
			if (a_values != null && a_values.Count > 0)
				avg = a_values.Average();
			if (avg > 0.01f)
			{
				m_barText.text = "+" + avg.ToString("N1");
				SetIncrease(avg);
			}
			else if (avg < -0.01f)
			{
				m_barText.text = avg.ToString("N1");
				SetDecrease(avg);
			}
			else
			{
				m_barText.text = "0";
				m_fillBar.gameObject.SetActive(false);
			}
		}
	}

	void SetIncrease(float a_normalisedValue)
	{
		m_fillBar.anchorMin = new Vector2(0.5f, 0f);
		m_fillBar.anchorMax = new Vector2(a_normalisedValue * 0.5f + 0.5f, 1f);
		m_fillBar.gameObject.SetActive(true);
		m_fillBar.offsetMin = Vector2.zero;
		m_fillBar.offsetMax = Vector2.zero;
		m_fillBarImage.ColourAsset = m_increaseColour;
		m_fillBarImage.sprite = m_increaseSprite;
	}

	void SetDecrease(float a_normalisedValue)
	{
		m_fillBar.anchorMax = new Vector2(0.5f, 1f);
		m_fillBar.anchorMin = new Vector2(a_normalisedValue * 0.5f + 0.5f, 0f);
		m_fillBar.gameObject.SetActive(true);
		m_fillBar.offsetMin = Vector2.zero;
		m_fillBar.offsetMax = Vector2.zero;
		m_fillBarImage.ColourAsset = m_decreaseColour;
		m_fillBarImage.sprite = m_decreaseSprite;
	}

	void SetPercentageValue(float a_value)
	{
		m_barText.text = a_value.ToString("P0");
		m_fillBar.anchorMax = new Vector2(a_value, 1f);
		m_fillBar.anchorMin = new Vector2(0f, 0f);
		m_fillBar.gameObject.SetActive(true);
		m_fillBar.offsetMin = Vector2.zero;
		m_fillBar.offsetMax = Vector2.zero;
	}
}