using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayableValuePopupWindow : MonoBehaviour
{
	[SerializeField]
	GameObject m_windowContainer;
	[SerializeField]
	TextMeshProUGUI m_titleText;
	[SerializeField]
	TextMeshProUGUI m_contentText;
	[SerializeField]
	CustomButton m_closeButton;
	
	private void Start()
	{
		m_closeButton.onClick.AddListener(CloseWindow);
	}

	public void SetValue(IDisplayableValue a_value)
	{
		m_titleText.text = a_value.Name;
		m_contentText.text = a_value.Description;
		m_windowContainer.SetActive(true);
	}

	public void CloseWindow()
	{
		m_windowContainer.SetActive(false);
	}
}
