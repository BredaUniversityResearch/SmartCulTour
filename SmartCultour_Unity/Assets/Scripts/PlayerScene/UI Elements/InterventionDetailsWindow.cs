using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InterventionDetailsWindow : MonoBehaviour
{
	[SerializeField]
	Transform m_contentLocation;
	[SerializeField]
	GameObject m_parameterPrefab;
	[SerializeField]
	TextMeshProUGUI m_nameText;
	[SerializeField]
	Button m_closeButton;

	bool m_initialised;
	Dictionary<string, InterventionParameterDisplay> m_parameters;
	InterventionDisplay m_currentDisplay;

	void Initialise()
	{
		m_initialised = true;
		m_parameters = new Dictionary<string, InterventionParameterDisplay>();
		foreach(ParameterData param in SessionConfig.Data.parameters)
		{
			InterventionParameterDisplay newEntry = GameObject.Instantiate(m_parameterPrefab, m_contentLocation).GetComponent<InterventionParameterDisplay>();
			m_parameters.Add(param.name, newEntry);
			newEntry.SetToParameter(param);
		}
		m_closeButton.onClick.AddListener(CloseWindow);
	}

	public void SetToIntervention(InterventionData a_intervention, InterventionDisplay a_display)
	{
		if (!m_initialised)
			Initialise();

		if(m_currentDisplay != null)
		{
			m_currentDisplay.m_onChange = null;
		}

		m_currentDisplay = a_display;
		m_currentDisplay.m_onChange = UpdateContent;

		UpdateContent(a_intervention);
	}

	void UpdateContent(InterventionData a_intervention)
	{
		m_nameText.text = a_intervention.name;
		gameObject.SetActive(true);
		foreach (var display in m_parameters)
			display.Value.ResetValue();
		foreach (var kvp in a_intervention.evaluation)
		{
			if (m_parameters.TryGetValue(kvp.Key, out var display))
			{
				display.SetValue(kvp.Value);
			}
			else
			{
				Debug.LogError("Parameter in evaluation that is missing from the parameter list: " + kvp.Key);
			}
		}
	}

	public void CloseWindow()
	{
		gameObject.SetActive(false);

		if (m_currentDisplay != null)
		{
			m_currentDisplay.m_onChange = null;
			m_currentDisplay = null;
		}
	}
}
