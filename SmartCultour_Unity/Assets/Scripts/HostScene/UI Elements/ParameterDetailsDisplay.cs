using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using ColourPalette;

class ParameterDetailsDisplay : MonoBehaviour
{

	[SerializeField]
	CustomImage m_bgImage;
	[SerializeField]
	ColourAsset m_bgColourEven;
	[SerializeField]
	ColourAsset m_bgColourOdd;
	[SerializeField]
	TextMeshProUGUI m_parameterName;
	[SerializeField]
	TextMeshProUGUI m_assignedAmount;

	ParameterData m_parameter;

	private void Start()
	{
		m_bgImage.ColourAsset = transform.GetSiblingIndex() % 2 == 0 ? m_bgColourEven : m_bgColourOdd;
	}

	public void SetParameter(ParameterData a_parameter)
	{
		m_parameter = a_parameter;
		m_parameterName.text = a_parameter.name;

		if(m_parameter.general)
			m_assignedAmount.text = "All";
	}

	public void UpdateAmount(Dictionary<string, List<string>> a_paramAssignment)
	{
		if (m_parameter.general)
			return;

		//Can be optimized by determining amount per param before calling this, but not required right now
		int amount = 0;
		foreach(var kvp in a_paramAssignment)
		{
			foreach(string param in kvp.Value)
			{
				if (param == m_parameter.name)
					amount++;
			}
		}
		m_assignedAmount.text = amount.ToString();
	}
}

