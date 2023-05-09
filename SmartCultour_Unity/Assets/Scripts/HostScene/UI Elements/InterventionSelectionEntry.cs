using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ColourPalette;

public class InterventionSelectionEntry : MonoBehaviour
{
	[SerializeField] Toggle m_toggle;
	[SerializeField] CustomToggleColorSet m_toggleColourSet;
	[SerializeField] TextMeshProUGUI m_name;
	[SerializeField] ColourAsset m_evenBGColour, m_oddBGColour;

	InterventionData m_intervention;
	Action<InterventionData> m_callback;

	private void Start()
	{
		m_toggleColourSet.colorNormal = transform.GetSiblingIndex() % 2 == 0 ? m_evenBGColour : m_oddBGColour;
		m_toggle.onValueChanged.AddListener(OnToggleChanged);
	}

	public void SetToIntervention(InterventionData a_intervention, ToggleGroup a_group, Action<InterventionData> a_callback)
	{
		m_name.text = a_intervention.name;
		m_toggle.group = a_group;
		m_intervention = a_intervention;
		m_callback = a_callback;
	}

	void OnToggleChanged(bool a_value)
	{
		if (a_value)
			m_callback(m_intervention);
	}

	public void Deselect()
	{
		m_toggle.isOn = false;
	}
}