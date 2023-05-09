using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ParameterAssignmentWindow : MonoBehaviour
{
	[SerializeField]
	GameObject m_parameterTogglePrefab, m_parameterGroupPrefab;
	[SerializeField]
	Transform m_contentLocation;
	[SerializeField]
	Button m_cancelButton, m_confirmButton;
	[SerializeField]
	TextMeshProUGUI m_playerNameText;

	bool m_initialised;
	string m_playerName;
	List<ParameterAssignmentToggle> m_parameterToggles;

	void Initialise()
	{
		m_initialised = true;

		m_cancelButton.onClick.AddListener(CloseWindow);
		m_confirmButton.onClick.AddListener(ApplyAssignment);
		m_parameterToggles = new List<ParameterAssignmentToggle>(20);

		foreach (RoleData role in SessionConfig.Data.roles)
		{
			ParameterAssignmentGroup newGroup = GameObject.Instantiate(m_parameterGroupPrefab, m_contentLocation).GetComponent<ParameterAssignmentGroup>();
			List<ParameterAssignmentToggle> groupToggles = new List<ParameterAssignmentToggle>(role.parameterNames.Count);
			foreach (string paramName in role.parameterNames)
			{
				ParameterAssignmentToggle newToggle = GameObject.Instantiate(m_parameterTogglePrefab, m_contentLocation).GetComponent<ParameterAssignmentToggle>();
				newToggle.SetContent(SessionConfig.GetParameter(paramName), OnToggleChanged);
				m_parameterToggles.Add(newToggle);
				groupToggles.Add(newToggle);
			}
			newGroup.SetContent(role.name, groupToggles);
		}
	}

	public void SetToPlayer(string a_playerName, List<string> a_playerParameters)
	{
		if (!m_initialised)
			Initialise();

		m_playerName = a_playerName;
		m_playerNameText.text = "Parameter assignment for: " + a_playerName;
		if (a_playerParameters == null)
		{
			foreach (ParameterAssignmentToggle toggle in m_parameterToggles)
				toggle.SetValue(false);
		}
		else
		{
			foreach (ParameterAssignmentToggle toggle in m_parameterToggles)
				toggle.SetValue(a_playerParameters.Contains(toggle.Name));
		}

		gameObject.SetActive(true);
	}

	void OnToggleChanged(string a_player, bool a_value)
	{
		foreach (ParameterAssignmentToggle toggle in m_parameterToggles)
			if (toggle.Name == a_player)
				toggle.SetValue(a_value);
	}

	void ApplyAssignment()
	{
		List<string> result = new List<string>();
		foreach (ParameterAssignmentToggle toggle in m_parameterToggles)
		{
			if (toggle.Toggled && !result.Contains(toggle.Name))
				result.Add(toggle.Name);
		}
		HostMain.Instance.SetAssignmentForPlayer(m_playerName, result);
		CloseWindow();
	}

	void CloseWindow()
	{
		gameObject.SetActive(false);
	}
}

