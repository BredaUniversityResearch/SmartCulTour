using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InterventionSelectionWindow : MonoBehaviour
{
	[SerializeField] GameObject m_entryPrefab;
	[SerializeField] Transform m_entryParent;
	[SerializeField] ToggleGroup m_toggleGroup;
	[SerializeField] DashboardManager m_dashboard;

	[SerializeField] CustomButton m_cancelButton;
	[SerializeField] CustomButton m_confirmButton;

	List<InterventionSelectionEntry> m_entries = new List<InterventionSelectionEntry>();
	InterventionData m_selectedIntervention;

	private void Start()
	{
		m_cancelButton.onClick.AddListener(OnCancel);
		m_confirmButton.onClick.AddListener(OnConfirm);
	}

	public void OpenWindow(List<InterventionData> a_interventions)
	{
		gameObject.SetActive(true);
		m_toggleGroup.allowSwitchOff = true;
		m_confirmButton.interactable = false;

		foreach (InterventionSelectionEntry entry in m_entries)
			entry.Deselect();

		for(int i = m_entries.Count; i < a_interventions.Count; i++)
		{
			InterventionSelectionEntry newEntry = GameObject.Instantiate(m_entryPrefab, m_entryParent).GetComponent<InterventionSelectionEntry>();
			newEntry.SetToIntervention(a_interventions[i], m_toggleGroup, InterventionSelected);
			m_entries.Add(newEntry);
		}
	}

	public void InterventionSelected(InterventionData a_intervention)
	{
		m_toggleGroup.allowSwitchOff = false;
		m_confirmButton.interactable = true;
		m_selectedIntervention = a_intervention;
	}

	void OnConfirm()
	{
		m_dashboard.ShowParametersIntervention(m_selectedIntervention);
		gameObject.SetActive(false);
	}

	void OnCancel()
	{
		gameObject.SetActive(false);
	}
}

