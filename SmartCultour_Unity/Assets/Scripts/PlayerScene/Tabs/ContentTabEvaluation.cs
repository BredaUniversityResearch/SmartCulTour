using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContentTabEvaluation : AContentTab
{
	[SerializeField]
	GameObject m_activeEvaluationSection, m_inactiveEvaluationSection;

	[SerializeField]
	GameObject m_parameterEvaluationPrefabOpen, m_parameterEvaliationPrefabYesNo;
	[SerializeField]
	Transform m_parameterParent, m_genericParameterParent;
	[SerializeField]
	TextMeshProUGUI m_interventionNameText;

	[SerializeField]
	Button m_submitButton;
	[SerializeField]
	GameObject m_submittedSection;

	List<AParameterEvaluationDisplay> m_genericParameters = new List<AParameterEvaluationDisplay>();
	List<ParameterEvaluationDisplay> m_parameterEvaluations = new List<ParameterEvaluationDisplay>();

	public override void Initialise()
	{
		base.Initialise();
		m_submitButton.onClick.AddListener(SubmitEvaluation);

		//Create general evaluations
		foreach(ParameterData param in SessionConfig.Data.parameters)
		{
			if (param.general)
			{
				AParameterEvaluationDisplay newEntry = GameObject.Instantiate(param.yesNoQuestion ? m_parameterEvaliationPrefabYesNo : m_parameterEvaluationPrefabOpen, m_genericParameterParent).GetComponent<AParameterEvaluationDisplay>();
				m_genericParameters.Add(newEntry);
				newEntry.SetParameter(param);
				if (m_submittedSection.activeSelf)
					newEntry.DisableEvaluation();
			}
		}
	}

	private void Update()
	{
		if(m_activeEvaluationSection.activeInHierarchy)
		{
			foreach(var param in m_parameterEvaluations)
			{
				if(!param.IsComplete)
				{
					m_submitButton.interactable = false;
					return;
				}
			}

			foreach (var param in m_genericParameters)
			{
				if (!param.IsComplete)
				{
					m_submitButton.interactable = false;
					return;
				}
			}
			m_submitButton.interactable = true;
		}
	}

	public void BeginEvaluation(string a_interventionName)
	{
		if (!ActiveTab)
			ShowNotification();

		m_activeEvaluationSection.SetActive(true);
		m_inactiveEvaluationSection.SetActive(false);
		m_submittedSection.SetActive(false);

		m_interventionNameText.text = a_interventionName;
		foreach (var param in m_parameterEvaluations)
			param.ResetEvaluation();
		foreach (var param in m_genericParameters)
			param.ResetEvaluation();

		if (PlayerMain.Instance.PlayerEvaluatedCurrentIntervention(PersistentPlayerData.Instance.m_nickName))
		{
			//Likely loading into game where intervention is already evaluated
			SetUIToSubmittedState();
		}
		else
		{
			m_submitButton.gameObject.SetActive(true);
			m_submitButton.interactable = false;
		}
	}

	public void EndEvaluation()
	{
		m_activeEvaluationSection.SetActive(false);
		m_inactiveEvaluationSection.SetActive(true);
	}

	public void UpdateParameters(List<string> a_parameters)
	{
		if (m_activeEvaluationSection.activeSelf)
		{
			//During evaluation, maintain state of already evaluated parameters
			List<ParameterEvaluationDisplay> newEntries = new List<ParameterEvaluationDisplay>(m_parameterEvaluations.Count);
			List<ParameterEvaluationDisplay> unusedEntries = new List<ParameterEvaluationDisplay>(4);
			HashSet<string> newParams = new HashSet<string>(a_parameters);
			foreach(ParameterEvaluationDisplay entry in m_parameterEvaluations)
			{
				if (entry.gameObject.activeSelf && newParams.Contains(entry.ParamName))
				{
					newEntries.Add(entry);
					newParams.Remove(entry.ParamName);
				}
				else
					unusedEntries.Add(entry);
			}
			int nextUnused = 0;
			foreach(string param in newParams)
			{
				if(nextUnused < unusedEntries.Count)
				{
					unusedEntries[nextUnused].SetParameter(SessionConfig.GetParameter(param));
					unusedEntries[nextUnused].ResetEvaluation();
					if (m_submittedSection.activeSelf)
						unusedEntries[nextUnused].DisableEvaluation();
					newEntries.Add(unusedEntries[nextUnused]);
					nextUnused++;
				}
				else
				{
					ParameterEvaluationDisplay newEntry = GameObject.Instantiate(m_parameterEvaluationPrefabOpen, m_parameterParent).GetComponent<ParameterEvaluationDisplay>();
					newEntries.Add(newEntry);
					newEntry.SetParameter(SessionConfig.GetParameter(param));
					if (m_submittedSection.activeSelf)
						newEntry.DisableEvaluation();
				}
			}
			for (; nextUnused < unusedEntries.Count; nextUnused++)
			{
				unusedEntries[nextUnused].gameObject.SetActive(false);
				newEntries.Add(unusedEntries[nextUnused]);
			}
			m_parameterEvaluations = newEntries;

		}
		else
		{
			int index = 0;
			if (a_parameters != null)
			{
				foreach (string parameterString in a_parameters)
				{
					ParameterData parameter = SessionConfig.GetParameter(parameterString);
					if (index >= m_parameterEvaluations.Count)
					{
						ParameterEvaluationDisplay newEntry = GameObject.Instantiate(m_parameterEvaluationPrefabOpen, m_parameterParent).GetComponent<ParameterEvaluationDisplay>();
						m_parameterEvaluations.Add(newEntry);
						newEntry.SetParameter(parameter);
					}
					else
					{
						m_parameterEvaluations[index].SetParameter(parameter);
						m_parameterEvaluations[index].gameObject.SetActive(true);
					}
					index++;
				}
			}

			for (; index < m_parameterEvaluations.Count; index++)
			{
				m_parameterEvaluations[index].gameObject.SetActive(false);
			}
		}
	}

	void SubmitEvaluation()
	{
		//Get results
		Dictionary<string, float> evaluation = new Dictionary<string, float>();
		foreach (var param in m_parameterEvaluations)
		{
			if(param.gameObject.activeSelf)
				evaluation.Add(param.ParamName, param.Value);
		}
		foreach (var param in m_genericParameters)
			evaluation.Add(param.ParamName, param.Value);
		EvaluationData result = new EvaluationData() { interventionName = m_interventionNameText.text , playerName = PersistentPlayerData.Instance.m_nickName, evaluation = evaluation};

		//Submit results
		PlayerMain.Instance.SubmitEvaluation(result);

		SetUIToSubmittedState();

		//TODO: wait for confirmation of event before continueing
	}

	void SetUIToSubmittedState()
	{
		//Disable parameters
		foreach (var param in m_parameterEvaluations)
			param.DisableEvaluation();
		foreach (var param in m_genericParameters)
			param.DisableEvaluation();

		//Disable submit button
		m_submitButton.gameObject.SetActive(false);
		m_submittedSection.SetActive(true);
	}
}

