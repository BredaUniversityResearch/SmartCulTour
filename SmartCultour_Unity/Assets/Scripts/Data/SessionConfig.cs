using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class SessionConfig : MonoBehaviour
{
	SessionConfigData m_data;
	static SessionConfig m_instance;

	//Same content as m_data.parameters, but searchable
	Dictionary<string, ParameterData> m_parameters;
	
	public static SessionConfigData Data
	{
		get
		{
			if (m_instance == null)
				return null;
			return m_instance.m_data;
		}
		set
		{
			if (m_instance == null)
			{
				GameObject obj = new GameObject("SessionConfigGO");
				DontDestroyOnLoad(obj);
				SessionConfig config = obj.AddComponent<SessionConfig>();
				m_instance = config;
			}
			m_instance.m_data = value;
			m_instance.UpdateParameters();
		}
	}

	public static bool SetJsonSaveFile(string a_json, out SessionStateData a_state, out Dictionary<string, List<string>> a_assignment)
	{
		OuterDataModel result = null;
		MemoryTraceWriter traceWriter = new MemoryTraceWriter();
		traceWriter.LevelFilter = System.Diagnostics.TraceLevel.Warning;
		try
		{
			result = JsonConvert.DeserializeObject<OuterDataModel>(a_json, new JsonSerializerSettings
			{
				TraceWriter = traceWriter,
				Error = (sender, errorArgs) =>
				{
					Debug.LogError("Save/config deserialization error: " + errorArgs.ErrorContext.Error);
				},
				Converters = new List<JsonConverter> { }
			});	
		}
		catch
		{
			a_state = null;
            a_assignment = null;
			return false;
		}

		if (result.datamodel.config != null)
		{
			Data = result.datamodel.config;
			foreach (KPIData kpi in result.datamodel.config.kpis)
				kpi.NormaliseWeights();
			a_state = result.datamodel.save;
            a_assignment = result.datamodel.assignment;
            return true;
		}
		a_state = null;
        a_assignment = null;
		return false;
	}

	public static bool SetJsonConfigData(string a_json, bool a_normaliseKPIs = false)
	{
		SessionConfigData result = null;
		MemoryTraceWriter traceWriter = new MemoryTraceWriter();
		traceWriter.LevelFilter = System.Diagnostics.TraceLevel.Warning;
		try
		{
			result = JsonConvert.DeserializeObject<SessionConfigData>(a_json, new JsonSerializerSettings
			{
				TraceWriter = traceWriter,
				Error = (sender, errorArgs) =>
				{
					Debug.LogError("Config deserialization error: " + errorArgs.ErrorContext.Error);
				},
				Converters = new List<JsonConverter> { }
			});	
		}
		catch
		{
			return false;
		}

		if (result != null)
		{
			Data = result;
			if (a_normaliseKPIs)
				foreach (KPIData kpi in result.kpis)
					kpi.NormaliseWeights();
			return true;
		}
		return false;
	}

	public static string GetJsonData()
	{
		return JsonConvert.SerializeObject(m_instance.m_data);
	}

	void UpdateParameters()
	{
		m_parameters = new Dictionary<string, ParameterData>();
		if (m_data != null)
		{
			foreach (ParameterData param in m_data.parameters)
			{
				if(m_parameters.ContainsKey(param.name))
				{
					Debug.LogError("Duplicate parameter: " + param.name);
				}
				else
					m_parameters.Add(param.name, param);

			}
		}
	}

	public static ParameterData GetParameter(string a_parameterName)
	{
		ParameterData result = null;
		m_instance.m_parameters.TryGetValue(a_parameterName, out result);
		return result;
	}
}

public class SessionConfigData
{
	public List<ParameterData> parameters;
	public List<RoleData> roles;
	public List<KPIData> kpis;
}

public class ParameterData : IDisplayableValue
{
	public string name;
	//public string longName;
	public string description;
	//public string question;   //Shorter description in question format
	public bool yesNoQuestion;
	public bool general; //Are always evaluated by all players

	public string Name => name;
	public string Description => description;
}

public class RoleData
{
	public string name;
	public string description;
	public List<string> parameterNames;
	public List<GoalData> goals;
}

public class GoalData : IDisplayableValue
{
	public string name;
	public string description;

	public string Name => name;
	public string Description => description;
}

public class KPIData
{
	public string name;
	public string description;
	public int category;
	public Dictionary<string, float> parameterWeights;

	public void NormaliseWeights()
	{
		if (parameterWeights == null || parameterWeights.Count == 0)
			return;

		float totalWeight = 0;
		foreach (var kvp in parameterWeights)
			totalWeight += Mathf.Abs(kvp.Value);
		List<string> keys = new List<string>(parameterWeights.Keys);
		foreach (var key in keys)
			parameterWeights[key] /= totalWeight;
	}
}

