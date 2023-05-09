using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class YesNoEvaluationDisplay : AParameterEvaluationDisplay
{
	[SerializeField]
	Toggle m_yesToggle, m_noToggle;
	
	private void Start()
	{
		Initialise();
		m_yesToggle.onValueChanged.AddListener(YesClicked);
		m_noToggle.onValueChanged.AddListener(NoClicked);
	}

	public override void ResetEvaluation()
	{
		//Reset value and interaction
		m_resultValue = 0;
		m_resultSet = false;

		m_toggleGroup.allowSwitchOff = true;
		m_yesToggle.interactable = true;
		m_yesToggle.isOn = false;
		m_noToggle.interactable = true;
		m_noToggle.isOn = false;
	}

	public override void DisableEvaluation()
	{
		//Disable interaction after submission
		m_yesToggle.interactable = false;
		m_noToggle.interactable = false;
	}

	void YesClicked(bool a_value)
	{
		if (a_value)
		{
			m_resultSet = true;
			m_resultValue = 1;
			m_toggleGroup.allowSwitchOff = false;
		}
	}

	void NoClicked(bool a_value)
	{
		if (a_value)
		{
			m_resultSet = true;
			m_resultValue = 0;
			m_toggleGroup.allowSwitchOff = false;
		}
	}
}

