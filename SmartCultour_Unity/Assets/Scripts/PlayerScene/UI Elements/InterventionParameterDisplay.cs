using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using ColourPalette;

public class InterventionParameterDisplay : MonoBehaviour
{
	[SerializeField]
	TextMeshProUGUI m_nameText;

	[SerializeField]
	Button m_moreInfoButton;

	[SerializeField]
	RectTransform m_barCenter;
	[SerializeField]
	TextMeshProUGUI m_barText;
	[SerializeField]
	RectTransform m_fillBar;
	[SerializeField]
	CustomImage m_barContainerImage;
	[SerializeField]
	CustomImage m_fillBarImage;
	[SerializeField]
	ColourAsset m_increaseColour, m_decreaseColour, m_percentageColour;
	[SerializeField]
	Sprite m_increaseSprite, m_decreaseSprite, m_bothSprite;

	ParameterData m_parameter;

	private void Start()
	{
		m_moreInfoButton.onClick.AddListener(OnMoreInfoClicked);
	}

	public void SetToParameter(ParameterData a_parameter)
	{
		m_parameter = a_parameter;
		m_nameText.text = a_parameter.name;
		//m_barCenter.SetActive(!m_parameter.yesNoQuestion);
		m_barContainerImage.sprite = m_parameter.yesNoQuestion ? m_increaseSprite : m_bothSprite;
		if(m_parameter.yesNoQuestion)
		{
			m_barCenter.anchorMin = new Vector2(0f, 0f);
			m_barCenter.anchorMax = new Vector2(0f, 1f);
		}
		else
		{
			m_barCenter.anchorMin = new Vector2(0.5f, 0f);
			m_barCenter.anchorMax = new Vector2(0.5f, 1f);
		}
		//m_barCenter.offsetMin = Vector2.zero;
		//m_barCenter.offsetMax = Vector2.zero;
	}

	public void ResetValue()
	{
		m_barText.text = "?";
		m_fillBar.gameObject.SetActive(false);
	}

	public void SetValue(List<float> a_values)
	{
		if (m_parameter.yesNoQuestion)
		{
			if (a_values != null && a_values.Count > 0)
			{
				float avg = a_values.Average();
				m_barText.text = avg.ToString("P0");
				m_fillBar.anchorMax = new Vector2(avg, 1f);
				m_fillBar.anchorMin = new Vector2(0f, 0f);
				m_fillBar.gameObject.SetActive(true);
				m_fillBar.offsetMin = Vector2.zero;
				m_fillBar.offsetMax = Vector2.zero;
				m_fillBarImage.ColourAsset = m_percentageColour;
				m_fillBarImage.sprite = m_increaseSprite;
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
				m_fillBar.anchorMin = new Vector2(0.5f, 0f);
				m_fillBar.anchorMax = new Vector2(avg * 0.25f + 0.5f, 1f);
				m_fillBar.gameObject.SetActive(true);
				m_fillBar.offsetMin = Vector2.zero;
				m_fillBar.offsetMax = Vector2.zero;
				m_fillBarImage.ColourAsset = m_increaseColour;
				m_fillBarImage.sprite = m_increaseSprite;
			}
			else if (avg < -0.01f)
			{
				m_barText.text = avg.ToString("N1");
				m_fillBar.anchorMax = new Vector2(0.5f, 1f);
				m_fillBar.anchorMin = new Vector2(avg * 0.25f + 0.5f, 0f);
				m_fillBar.gameObject.SetActive(true);
				m_fillBar.offsetMin = Vector2.zero;
				m_fillBar.offsetMax = Vector2.zero;
				m_fillBarImage.ColourAsset = m_decreaseColour;
				m_fillBarImage.sprite = m_decreaseSprite;
			}
			else
			{
				m_barText.text = "0";
				m_fillBar.gameObject.SetActive(false);
			}
		}
	}

	void OnMoreInfoClicked()
	{
		PopupManager.Instance.OpenDisplayableValuePopup(m_parameter);
	}
}
