using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InterventionDisplay : MonoBehaviour
{
	[SerializeField]
	TextMeshProUGUI m_nameText;

	[SerializeField]
	Button m_moreInfoButton;

	Action<InterventionData, InterventionDisplay> m_detailsFunction;
	public Action<InterventionData> m_onChange;
	InterventionData m_intervention;

	private void Start()
	{
		m_moreInfoButton.onClick.AddListener(ShowMoreInfo);
	}

	public void SetIntervention(InterventionData a_intervention, Action<InterventionData, InterventionDisplay> a_showDetailsFunction)
	{
		m_nameText.text = a_intervention.name;
		m_intervention = a_intervention;
		m_detailsFunction = a_showDetailsFunction;
	}


	public void UpdateIntervention(InterventionData a_intervention)
	{
		m_nameText.text = a_intervention.name;
		m_intervention = a_intervention;
		if (m_onChange != null)
			m_onChange.Invoke(a_intervention);
	}

	void ShowMoreInfo()
	{
		m_detailsFunction?.Invoke(m_intervention, this);
	}
}
