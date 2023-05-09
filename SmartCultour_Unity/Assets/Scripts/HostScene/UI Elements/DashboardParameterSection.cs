using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DashboardParameterSection : MonoBehaviour
{
	[SerializeField]
	GameObject m_parameterPrefab, m_emptyEntryPrefab;
	[SerializeField]
	Transform m_leftParent, m_rightParent;

	bool m_nextEntryRight;
	Dictionary<string, DashboardParameterEntry> m_entries = new Dictionary<string, DashboardParameterEntry>();

	public void AddParameter(ParameterData a_parameter)
	{
		DashboardParameterEntry newEntry = GameObject.Instantiate(m_parameterPrefab, m_nextEntryRight ? m_rightParent : m_leftParent).GetComponent<DashboardParameterEntry>();
		m_entries.Add(a_parameter.name, newEntry);
		newEntry.SetToParameter(a_parameter);
		m_nextEntryRight = !m_nextEntryRight;
	}

	public void FinishedAddingParameters()
	{
		//Add empty entry if required
		if (m_nextEntryRight)
			GameObject.Instantiate(m_emptyEntryPrefab, m_rightParent);
	}

	//Single intervention
	public void SetParameterValues(Dictionary<string, List<float>> a_data)
	{
		ResetEntries();

		if (a_data != null)
		{
			foreach (var data in a_data)
			{
				if (m_entries.TryGetValue(data.Key, out var entry))
					entry.SetForSingleIntervention(data.Value);
			}
		}
	}

	//Multiple interventions
	public void SetParameterValues(List<InterventionData> a_data)
	{
		ResetEntries();

		if (a_data != null)
		{
			//List has (up to) one value per intervention
			Dictionary<string, List<float>> parameterAverages = new Dictionary<string, List<float>>();
			float interventionCount = (float)a_data.Count();

			foreach (var intervention in a_data)
			{
				foreach (var kvp in intervention.evaluation)
				{
					//Combine multiple parameter evaluations per intervention into an average
					if (parameterAverages.TryGetValue(kvp.Key, out var averageList))
					{
						averageList.Add(kvp.Value.Average());
					}
					else
						parameterAverages.Add(kvp.Key, new List<float>() { kvp.Value.Average() });
				}
			}

			foreach (var param in parameterAverages)
			{
				if (m_entries.TryGetValue(param.Key, out var entry))
					entry.SetForMultipleInterventions(param.Value, interventionCount);
			}
		}
	}

	//Multiple rounds of interventions
	public void SetParameterValues(List<List<InterventionData>> a_data)
	{
		ResetEntries();

		if (a_data != null)
		{
			//List has (up to) one value per intervention
			Dictionary<string, List<float>> parameterAverages = new Dictionary<string, List<float>>();
			float interventionCount = (float)a_data.Count();

			foreach (var round in a_data)
			{
				foreach (var intervention in round)
				{
					foreach (var kvp in intervention.evaluation)
					{
						//Combine multiple parameter evaluations per intervention into an average
						if (parameterAverages.TryGetValue(kvp.Key, out var averageList))
						{
							averageList.Add(kvp.Value.Average());
						}
						else
							parameterAverages.Add(kvp.Key, new List<float>() { kvp.Value.Average() });
					}
				}
			}

			foreach (var param in parameterAverages)
			{
				if (m_entries.TryGetValue(param.Key, out var entry))
					entry.SetForMultipleInterventions(param.Value, interventionCount);
			}
		}
	}

	public void ResetEntries()
	{
		foreach (var entry in m_entries)
		{
			entry.Value.ResetValue();
		}
	}

}
