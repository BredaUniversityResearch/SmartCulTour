using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class KPITweakingWindow : MonoBehaviour
{
	[SerializeField] GameObject m_tweakingEntryPrefab;
	[SerializeField] Transform m_contentParent;
	[SerializeField] Button m_confirmButton;
	[SerializeField] Button m_cancelButton;

	bool m_initialised;
	Dictionary<string, DashboardKPITweakingEntry> m_entries;

	void Initialise()
	{
		m_initialised = true;

		m_confirmButton.onClick.AddListener(AcceptChanges);
		m_cancelButton.onClick.AddListener(CloseWindow);

		m_entries = new Dictionary<string, DashboardKPITweakingEntry>();
		foreach (KPIData data in SessionConfig.Data.kpis)
		{
			DashboardKPITweakingEntry entry = GameObject.Instantiate(m_tweakingEntryPrefab, m_contentParent).GetComponent<DashboardKPITweakingEntry>();
			entry.SetToKPI(data);
			m_entries.Add(data.name, entry);
		}
	}

	public void OpenWindow()
	{
		if (!m_initialised)
			Initialise();

		foreach(var kvp in HostMain.Instance.m_saveData.kpiValues)
		{
			if(m_entries.TryGetValue(kvp.Key, out var entry))
			{
				entry.SetToValue(kvp.Value);
			}
			else
			{
				Debug.LogError("KPIs changed after game start");
			}
		}
		gameObject.SetActive(true);
	}

	void AcceptChanges()
	{
		gameObject.SetActive(false);
		Dictionary<string, KPIValue> result = new Dictionary<string, KPIValue>();
		foreach (var kvp in m_entries)
			result.Add(kvp.Key, kvp.Value.GetResult());
		HostMain.Instance.SetKPIValues(result);
	}

	void CloseWindow()
	{
		gameObject.SetActive(false);
	}
}

