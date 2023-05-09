using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class SessionStateData
{
	public Dictionary<string, KPIValue> kpiValues;
	public Dictionary<string, float> startingKpiValues;
	public List<List<InterventionData>> interventions;// = new List<List<InterventionData>>() { new List<InterventionData>() };
	public bool evaluationActive;

	public SessionStateData()
	{
		kpiValues = new Dictionary<string, KPIValue>();
	}

	public void VerifyInterventions()
	{
		if (interventions == null)
			interventions = new List<List<InterventionData>>() { new List<InterventionData>() };
		else if (interventions.Count == 0)
			interventions.Add(new List<InterventionData>());
	}

	public static SessionStateData GetEmpty()
	{
		SessionStateData result = new SessionStateData();
		result.interventions = new List<List<InterventionData>>() { new List<InterventionData>() };
		return result;
	}

	[JsonIgnore]
	public InterventionData LastIntervention
	{
		get
		{
			return interventions[interventions.Count - 1][interventions[interventions.Count-1].Count - 1];
		}
	}

	[JsonIgnore]
	public List<InterventionData> CurrentRoundInterventions
	{
		get
		{
			return interventions[interventions.Count - 1];
		}
	}

	[JsonIgnore]
	public List<InterventionData> LastRoundInterventions
	{
		get
		{
			if (interventions.Count < 2)
				return null;
			return interventions[interventions.Count - 2];
		}
	}

	[JsonIgnore]
	public int NumberInterventions
	{
		get
		{
			int result = 0;
			foreach (var round in interventions)
				result += round.Count;
			return result;
		}
	}
}

public class KPIValue
{
	public float currentValue;
	public float oldValue;
	public float stdev = 5f;

	public KPIValue()
	{ }

	public KPIValue(float a_currentValue, float a_oldValue, float a_stdev = 5f)
	{
		currentValue = a_currentValue;
		oldValue = a_oldValue;
		stdev = a_stdev;
	}
}

public class InterventionData
{
	public string name;
	public Dictionary<string, List<float>> evaluation;
	public List<string> evaluatedBy;

	public InterventionData()
	{ }

	public InterventionData(string a_name)
	{
		name = a_name;
		evaluation = new Dictionary<string, List<float>>();
		evaluatedBy = new List<string>();
	}

	public void Add(EvaluationData a_other)
	{
		if (evaluatedBy.Contains(a_other.playerName))
		{
			return;
		}

		foreach(var kvp in a_other.evaluation)
		{
			if (evaluation.TryGetValue(kvp.Key, out var param))
			{
				param.Add(kvp.Value);
			}
			else
			{
				evaluation.Add(kvp.Key, new List<float> { kvp.Value });
			}
		}
		evaluatedBy.Add(a_other.playerName);
	}

	public bool EvaluatedBy(string a_playerName)
	{
		return evaluatedBy.Contains(a_playerName);
	}
}

public class EvaluationData
{
	public string playerName;
	public string interventionName;
	public Dictionary<string, float> evaluation;
}
