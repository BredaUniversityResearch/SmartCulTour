using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ParameterAssignmentGroup : MonoBehaviour
{
	[SerializeField]
	TextMeshProUGUI m_groupName;
	[SerializeField]
	Button m_toggleAllButton, m_toggleNoneButton;

	List<ParameterAssignmentToggle> m_groupParameters;

	private void Start()
	{
		m_toggleAllButton.onClick.AddListener(ToggleAll);
		m_toggleNoneButton.onClick.AddListener(ToggleNone);
	}

	public void SetContent(string a_groupName, List<ParameterAssignmentToggle> a_toggles)
	{
		m_groupName.text = a_groupName;
		m_groupParameters = a_toggles;
	}

	void ToggleAll()
	{
		foreach (ParameterAssignmentToggle param in m_groupParameters)
			param.SetValue(true, false);
	}

	void ToggleNone()
	{
		foreach (ParameterAssignmentToggle param in m_groupParameters)
			param.SetValue(false, false);
	}
}
