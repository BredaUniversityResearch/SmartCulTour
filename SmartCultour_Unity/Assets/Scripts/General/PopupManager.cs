using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
	static PopupManager m_instance;
	public static PopupManager Instance => m_instance;

	[SerializeField]
	GameObject m_popupPrefab;

	[SerializeField]
	List<DisplayableValuePopupWindow> m_displayableValuePopups;

	List<PopupWindow> m_popupWindowPool;

    void Awake()
    {
		m_instance = this;
		m_popupWindowPool = new List<PopupWindow>();

	}

	public PopupWindow CreateMessagePopup(string a_title, string a_content)
	{
		PopupWindow result = GetPopupWindow();
		result.SetContent(a_title, a_content);
		return result;
	}

	public void CreatePopup(string a_title, string a_content, string a_confirmButtonText, Action a_confirmButtonCallback)
	{
		GetPopupWindow().SetContent(a_title, a_content, a_confirmButtonText, a_confirmButtonCallback);
	}

	public void CreatePopup(string a_title, string a_content, string a_confirmButtonText, Action a_confirmButtonCallback, string a_cancelButtonText, Action a_cancelButtonCallback)
	{
		GetPopupWindow().SetContent(a_title, a_content, a_confirmButtonText, a_confirmButtonCallback, a_cancelButtonText, a_cancelButtonCallback);
	}

	PopupWindow GetPopupWindow()
	{
		if (m_popupWindowPool.Count > 0)
		{
			PopupWindow result = m_popupWindowPool[m_popupWindowPool.Count - 1];
			m_popupWindowPool.RemoveAt(m_popupWindowPool.Count - 1);
			return result;
		}
		else
			return CreateNewPopup();
	}

	PopupWindow CreateNewPopup()
	{
		return GameObject.Instantiate<GameObject>(m_popupPrefab, transform).GetComponent<PopupWindow>();
	}

	public void ReturnPopupToPool(PopupWindow a_popup)
	{
		m_popupWindowPool.Add(a_popup);
	}

	public void OpenDisplayableValuePopup(IDisplayableValue a_value)
	{
		foreach (var window in m_displayableValuePopups)
		{
			if (window.gameObject.activeInHierarchy)
			{
				window.SetValue(a_value);
				return;
			}
		}
	}

	public void CloseDisplayableValuePopup()
	{
		foreach (var window in m_displayableValuePopups)
		{
			window.CloseWindow();
		}
	}
}
