using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;


public class KPIManager : MonoBehaviour
{
	public Dictionary<string, KPIValue> LoadKPIs(Dictionary<string, KPIValue> a_KPIValues, List<KPIData> a_KPIData)
	{
		Dictionary<string, KPIValue> result;
		if (a_KPIValues == null)
		{
			result = new Dictionary<string, KPIValue>();
			foreach (KPIData kpi in a_KPIData)
				result.Add(kpi.name, new KPIValue(0f, 0f));
		}
		else
		{
			result = a_KPIValues;
			foreach (KPIData kpi in a_KPIData)
			{
				if(!result.ContainsKey(kpi.name))
					result.Add(kpi.name, new KPIValue(0f, 0f));
			}
			if (result.Count != a_KPIData.Count)
				Debug.LogError("Invalid KPI values received. KPIData was not matched.");
		}
		return result;
	}

	public void CalculateNewValues(List<InterventionData> a_newInterventions, Dictionary<string, KPIValue> a_kpiValues)
	{
		//Set old values to new
		foreach (var kpi in a_kpiValues)
			kpi.Value.oldValue = kpi.Value.currentValue;

		//Combine values within interventions
		Dictionary<string, List<float>> parameterAveragesList = new Dictionary<string, List<float>>();
		float interventionCount = (float)a_newInterventions.Count();

		foreach (var intervention in a_newInterventions)
		{
			foreach (var kvp in intervention.evaluation)
			{
				//Combine multiple parameter evaluations per intervention into an average
				if (parameterAveragesList.TryGetValue(kvp.Key, out var averageList))
				{
					averageList.Add(kvp.Value.Average());
				}
				else
					parameterAveragesList.Add(kvp.Key, new List<float>() { kvp.Value.Average() });
			}
		}

		//Combine values of all interventions
		Dictionary<string, float> parameterAverages = new Dictionary<string,float>();
		foreach (var param in parameterAveragesList)
		{
			parameterAverages.Add(param.Key, param.Value.Sum());
		}

		//Apply changes
		foreach(KPIData kpi in SessionConfig.Data.kpis)
		{
			float change = 0;
			foreach(var weight in kpi.parameterWeights)
			{
				if (parameterAverages.TryGetValue(weight.Key, out float paramValue))
					change += weight.Value * paramValue;
			}
			a_kpiValues[kpi.name].currentValue += change;
		}
	}
}

