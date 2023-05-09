using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashboardKPISection : MonoBehaviour
{
	[SerializeField]
	GameObject m_KPIPrefab;
	[SerializeField]
	Transform[] m_KPIParents;

	Dictionary<string, DashboardKPIEntry> m_entries = new Dictionary<string, DashboardKPIEntry>();

	public void AddKPI(KPIData a_kpi)
	{
		DashboardKPIEntry newEntry = GameObject.Instantiate(m_KPIPrefab, m_KPIParents[a_kpi.category]).GetComponent<DashboardKPIEntry>();
		m_entries.Add(a_kpi.name, newEntry);
		newEntry.SetKPI(a_kpi);
	}

	public void SetKPIValues(Dictionary<string, KPIValue> a_data)
	{
		if (a_data != null)
		{
			foreach (var data in a_data)
			{
				if (m_entries.TryGetValue(data.Key, out var entry))
					entry.SetValue(data.Value);
			}
		}
	}

	public void SetKPIValues(Dictionary<string, KPIValue> a_data, Dictionary<string, float> a_initialData)
	{
		if (a_data != null && a_initialData != null)
		{
			foreach (var data in a_data)
			{
				if (m_entries.TryGetValue(data.Key, out var entry) && a_initialData.TryGetValue(data.Key, out float initialValue))
					entry.SetValue(data.Value.currentValue, initialValue, data.Value.stdev);
			}
		}
	}
}
