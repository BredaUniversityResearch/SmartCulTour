using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ColourPalette;

public class ParameterEvaluationDisplay : AParameterEvaluationDisplay
{
	[SerializeField]
	List<Toggle> m_valueToggles;
	[SerializeField]
	RectTransform m_fillBar;
	[SerializeField]
	CustomImage m_fillImage;
	[SerializeField]
	ColourAsset m_activeColour, m_inactiveColour;
	
	private void Start()
	{
		Initialise();
		for (int i = 0; i < m_valueToggles.Count; i++)
		{
			int index = i;
			m_valueToggles[i].onValueChanged.AddListener(b => ToggleChanged(b, index));
		}
	}

	public override void ResetEvaluation()
	{
		//Reset value and interaction
		m_resultValue = 0;
		m_resultSet = false;

		m_toggleGroup.allowSwitchOff = true;
		foreach (Toggle t in m_valueToggles)
		{
			t.interactable = true;
			t.isOn = false;
		}
		m_fillImage.ColourAsset = m_activeColour;
		m_fillBar.gameObject.SetActive(false);
	}

	public override void DisableEvaluation()
	{
		//Disable interaction after submission
		foreach (Toggle t in m_valueToggles)
			t.interactable = false;
		m_fillImage.ColourAsset = m_inactiveColour;
	}

	public void ToggleChanged(bool a_value, int a_index)
	{
		if(a_value)
		{
			m_toggleGroup.allowSwitchOff = false;
			m_resultSet = true;
			m_resultValue = a_index - 2;

			if (a_index == 2)
			{
				m_fillBar.gameObject.SetActive(false);
				return;
			}

			m_fillBar.gameObject.SetActive(true);
			if (m_resultValue < 0)
			{
				m_fillBar.anchorMax = new Vector2(0.5f, 1f);
				if (m_resultValue == -2)
					m_fillBar.anchorMin = new Vector2(0f, 0f);
				else
					m_fillBar.anchorMin = new Vector2(0.25f, 0f);
			}
			else
			{
				m_fillBar.anchorMin = new Vector2(0.5f, 0f);
				if (m_resultValue == 2)
					m_fillBar.anchorMax = new Vector2(1f, 1f);
				else
					m_fillBar.anchorMax = new Vector2(0.75f, 1f);
			}
			m_fillBar.offsetMin = Vector2.zero;
			m_fillBar.offsetMax = Vector2.zero;
		}
	}

	public override float Value => ((float)m_resultValue) * 0.5f;
}