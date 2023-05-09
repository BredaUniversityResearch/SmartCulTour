using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ColourPalette;

public class ParameterAssignmentToggle : MonoBehaviour
{
	[SerializeField]
	TextMeshProUGUI m_parameterName;
	[SerializeField]
	Toggle m_toggle;
	//[SerializeField]
	//CustomImage m_bgImage;
	//[SerializeField]
	//ColourAsset m_bgColourEven;
	//[SerializeField]
	//ColourAsset m_bgColourOdd;

	bool m_ignoreCallback;
	Action<string, bool> m_selectedCallback;

	private void Start()
	{
		//m_bgImage.ColourAsset = transform.GetSiblingIndex() % 2 == 0 ? m_bgColourEven : m_bgColourOdd;
		m_toggle.onValueChanged.AddListener(OnValueChanged);
	}

	public void SetContent(ParameterData a_parameter, Action<string, bool> a_selectedCallback)
	{
		m_parameterName.text = a_parameter.name;
		m_selectedCallback = a_selectedCallback;
	}

	public void SetValue(bool a_value, bool a_ignoreCallback = true)
	{
		if (a_ignoreCallback)
		{
			m_ignoreCallback = true;
			m_toggle.isOn = a_value;
			m_ignoreCallback = false;
		}
		else
			m_toggle.isOn = a_value;
	}

	void OnValueChanged(bool a_newValue)
	{
		if (!m_ignoreCallback)
			m_selectedCallback.Invoke(Name, a_newValue);
	}

	public bool Toggled => m_toggle.isOn;
	public string Name => m_parameterName.text;
}
