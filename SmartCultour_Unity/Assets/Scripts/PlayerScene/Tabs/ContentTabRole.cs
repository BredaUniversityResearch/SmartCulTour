using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContentTabRole : AContentTab
{
	[SerializeField]
	TextMeshProUGUI m_roleNameText, m_roleDescriptionText;

	[SerializeField]
	Transform m_goalsContent, m_parametersContent;

	[SerializeField]
	GameObject m_displayableValuePrefab;

	[SerializeField]
	GameObject m_noParametersCover;

	List<GenericValueDisplay> m_parameterDisplays = new List<GenericValueDisplay>();

	public override void Initialise()
	{
		base.Initialise();
		m_roleNameText.text = PersistentPlayerData.Instance.m_role.name;
		m_roleDescriptionText.text = PersistentPlayerData.Instance.m_role.description;
		foreach(GoalData goal in PersistentPlayerData.Instance.m_role.goals)
		{
			GenericValueDisplay newEntry = GameObject.Instantiate(m_displayableValuePrefab, m_goalsContent).GetComponent<GenericValueDisplay>();
			newEntry.SetValue(goal);
		}
	}

	public void UpdateParameters(List<string> a_parameters)
	{
		if(a_parameters == null || a_parameters.Count == 0)
		{
			foreach (GenericValueDisplay display in m_parameterDisplays)
				display.gameObject.SetActive(false);
			m_noParametersCover.SetActive(true);
		}
		else
		{
			int index = 0;
			foreach(string parameterString in a_parameters)
			{
				ParameterData parameter = SessionConfig.GetParameter(parameterString);
				if (index >= m_parameterDisplays.Count)
				{
					GenericValueDisplay newEntry = GameObject.Instantiate(m_displayableValuePrefab, m_parametersContent).GetComponent<GenericValueDisplay>();
					m_parameterDisplays.Add(newEntry);
					newEntry.SetValue(parameter);
				}
				else
				{
					m_parameterDisplays[index].SetValue(parameter);
				}
				index++;
			}

			for(; index < m_parameterDisplays.Count; index++)
			{
				m_parameterDisplays[index].gameObject.SetActive(false);
			}

			m_noParametersCover.SetActive(false);
		}

		if (!ActiveTab)
			ShowNotification();
	}
}

