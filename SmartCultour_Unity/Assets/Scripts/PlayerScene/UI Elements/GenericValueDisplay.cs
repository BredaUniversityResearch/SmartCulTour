using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GenericValueDisplay : MonoBehaviour
{
	[SerializeField]
	TextMeshProUGUI m_nameText/*, m_descriptionText*/;

	[SerializeField]
	Button m_moreInfoButton;

	IDisplayableValue m_value;

	private void Start()
	{
		m_moreInfoButton.onClick.AddListener(MoreInfo);
	}

	public void SetValue(IDisplayableValue a_value)
	{
		gameObject.SetActive(true);
		m_value = a_value;
		m_nameText.text = a_value.Name;
		//m_descriptionText.text = a_value.Description;
	}

	void MoreInfo()
	{
		PopupManager.Instance.OpenDisplayableValuePopup(m_value);
	}
}

