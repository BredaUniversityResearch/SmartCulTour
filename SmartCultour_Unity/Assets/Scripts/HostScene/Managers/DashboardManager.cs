using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class DashboardManager : MonoBehaviour
{
	[SerializeField] DashboardKPISection m_kpiSection;
	[SerializeField] DashboardParameterSection m_parameterSection;
	[SerializeField] TextMeshProUGUI m_titleText;
	[SerializeField] Toggle m_toggleKPILastRound;
	[SerializeField] Toggle m_toggleKPIAllTime;
	[SerializeField] Toggle m_toggleParamCurrent;
	[SerializeField] Toggle m_toggleParamLastRound;
	[SerializeField] Toggle m_toggleParamAllTime;
	[SerializeField] Button m_specificInterventionButton;
	[SerializeField] ToggleGroup m_toggleGroup;
	[SerializeField] InterventionSelectionWindow m_interventionSelectionWindow;

	InterventionData m_currentSpecificIntervention;

	public void CreateEntries(List<KPIData> a_kpis, List<ParameterData> a_parameters, HostMain a_host)
	{
		foreach (KPIData kpi in a_kpis)
			m_kpiSection.AddKPI(kpi);
		foreach (ParameterData parameter in a_parameters)
			m_parameterSection.AddParameter(parameter);
		m_parameterSection.FinishedAddingParameters();

		a_host.m_onInterventionsChanged += OnInterventionsChanged;
		a_host.m_onKPIsChanged += OnKPIsChanged;

		m_toggleKPILastRound.onValueChanged.AddListener(ShowKPIsLastRound);
		m_toggleKPIAllTime.onValueChanged.AddListener(ShowKPIsAllTime);
		m_toggleParamCurrent.onValueChanged.AddListener(ShowParametersCurrent);
		m_toggleParamLastRound.onValueChanged.AddListener(ShowParametersLastRound);
		m_toggleParamAllTime.onValueChanged.AddListener(ShowParametersAllTime);
		m_specificInterventionButton.onClick.AddListener(OpenInterventionSelection);
	}

	void OnKPIsChanged()
	{
		if (m_toggleKPILastRound.isOn)
			ShowKPIsLastRound(true);
		else if (m_toggleKPIAllTime.isOn)
			ShowKPIsAllTime(true);
	}

	void OnInterventionsChanged()
	{
		if (m_toggleParamCurrent.isOn)
			ShowParametersCurrent(true);
		else if (m_toggleParamLastRound.isOn)
			ShowParametersLastRound(true);
		else if (m_toggleParamAllTime.isOn)
			ShowParametersAllTime(true);
		else if (!m_specificInterventionButton.interactable)
			ShowParametersIntervention(m_currentSpecificIntervention);
	}

	void OpenInterventionSelection()
	{
		m_interventionSelectionWindow.OpenWindow(HostMain.Instance.m_saveData.interventions.SelectMany(i => i).ToList<InterventionData>());
	}

	public void ShowKPIsLastRound(bool a_value)
	{
		if (!a_value)
			return;
		m_toggleGroup.allowSwitchOff = false;
		m_specificInterventionButton.interactable = true;
		m_kpiSection.gameObject.SetActive(true);
		m_parameterSection.gameObject.SetActive(false);
		m_titleText.text = "Current KPI values";
		m_kpiSection.SetKPIValues(HostMain.Instance.m_saveData.kpiValues);
	}

	public void ShowKPIsAllTime(bool a_value)
	{
		if (!a_value)
			return;
		m_toggleGroup.allowSwitchOff = false;
		m_specificInterventionButton.interactable = true;
		m_kpiSection.gameObject.SetActive(true);
		m_parameterSection.gameObject.SetActive(false);
		m_titleText.text = "KPI changes since game start";
		m_kpiSection.SetKPIValues(HostMain.Instance.m_saveData.kpiValues, HostMain.Instance.m_saveData.startingKpiValues);
	}

	public void ShowParametersCurrent(bool a_value)
	{
		if (!a_value)
			return;
		m_toggleGroup.allowSwitchOff = false;
		m_specificInterventionButton.interactable = true;
		m_kpiSection.gameObject.SetActive(false);
		m_parameterSection.gameObject.SetActive(true);
		m_titleText.text = "Parameter changes this round";
		m_parameterSection.SetParameterValues(HostMain.Instance.m_saveData.CurrentRoundInterventions);
	}

	public void ShowParametersLastRound(bool a_value)
	{
		if (!a_value)
			return;
		m_toggleGroup.allowSwitchOff = false;
		m_specificInterventionButton.interactable = true;
		m_kpiSection.gameObject.SetActive(false);
		m_parameterSection.gameObject.SetActive(true);
		m_titleText.text = "Parameter changes this round";
		m_parameterSection.SetParameterValues(HostMain.Instance.m_saveData.LastRoundInterventions);
	}

	public void ShowParametersAllTime(bool a_value)
	{
		if (!a_value)
			return;
		m_toggleGroup.allowSwitchOff = false;
		m_specificInterventionButton.interactable = true;
		m_kpiSection.gameObject.SetActive(false);
		m_parameterSection.gameObject.SetActive(true);
		m_titleText.text = "Parameter changes all game";
		m_parameterSection.SetParameterValues(HostMain.Instance.m_saveData.interventions);
	}

	public void ShowParametersIntervention(InterventionData a_intervention)
	{
		m_currentSpecificIntervention = a_intervention;
		m_specificInterventionButton.interactable = false;
		m_toggleGroup.allowSwitchOff = true;
		m_toggleGroup.SetAllTogglesOff();
		m_kpiSection.gameObject.SetActive(false);
		m_parameterSection.gameObject.SetActive(true);
		m_titleText.text = a_intervention.name;
		m_parameterSection.SetParameterValues(a_intervention.evaluation);
	}
}
