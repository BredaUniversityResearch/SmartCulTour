using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class AContentTab : MonoBehaviour
{
	[SerializeField]
	Toggle m_tabToggle;
	[SerializeField]
	GameObject m_tabNotification;

    public virtual void Initialise()
    {
		m_tabToggle.onValueChanged.AddListener(OnToggleChanged);
	}

    void OnToggleChanged(bool a_newValue)
	{
		gameObject.SetActive(a_newValue);
		if (a_newValue)
			m_tabNotification.SetActive(false);
		else
		{
			PopupManager.Instance.CloseDisplayableValuePopup();
			OnTabClose();
		}
	}

	protected void ShowNotification()
	{
		m_tabNotification.SetActive(true);
	}
	public bool ActiveTab => m_tabToggle.isOn;

	protected virtual void OnTabClose() { }
}
