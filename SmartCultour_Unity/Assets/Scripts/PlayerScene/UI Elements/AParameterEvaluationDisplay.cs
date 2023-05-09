using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ColourPalette;

public abstract class AParameterEvaluationDisplay : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI m_nameText;
	//[SerializeField]
	//protected TextMeshProUGUI m_questionText;
	[SerializeField]
	protected Button m_moreInfoButton;
	[SerializeField]
	protected ToggleGroup m_toggleGroup;

	protected int m_resultValue;
	protected bool m_resultSet;
	protected ParameterData m_parameter;

	protected void Initialise()
	{
		m_moreInfoButton.onClick.AddListener(MoreInfo);
	}

	public void SetParameter(ParameterData a_parameter)
	{
		m_parameter = a_parameter;
		m_nameText.text = a_parameter.name;
		//m_questionText.text = a_parameter.question;
	}

	public abstract void ResetEvaluation();
	public abstract void DisableEvaluation();

	void MoreInfo()
	{
		PopupManager.Instance.OpenDisplayableValuePopup(m_parameter);
	}

	public bool IsComplete => m_resultSet;
	public string ParamName => m_parameter.name;
	public virtual float Value => (float)m_resultValue;
}

