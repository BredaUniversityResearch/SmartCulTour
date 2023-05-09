using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupWindow : MonoBehaviour
{
	[SerializeField]
	TextMeshProUGUI m_titleText;
	[SerializeField]
	TextMeshProUGUI m_contentText;
	[SerializeField]
	CustomButton m_cancelButton;
	[SerializeField]
	CustomButton m_confirmButton;
	[SerializeField]
	TextMeshProUGUI m_cancelButtonText;
	[SerializeField]
	TextMeshProUGUI m_confirmButtonText;

	Action m_confirmButtonCallback;
	Action m_cancelButtonCallback;

	private void Start()
	{
		m_cancelButton.onClick.AddListener(CancelButtonPressed);
		m_confirmButton.onClick.AddListener(ConfirmButtonPressed);
	}

	public void SetContent(string a_title, string a_content)
	{
		gameObject.SetActive(true);
		m_cancelButton.gameObject.SetActive(false);
		m_confirmButton.gameObject.SetActive(false);
		m_titleText.text = a_title;
		m_contentText.text = a_content;
	}

	public void SetContent(string a_title, string a_content, string a_confirmButtonText, Action a_confirmButtonCallback)
	{
		gameObject.SetActive(true);
		m_cancelButton.gameObject.SetActive(false);
		m_confirmButton.gameObject.SetActive(true);
		m_titleText.text = a_title;
		m_contentText.text = a_content;
		m_confirmButtonText.text = a_confirmButtonText;
		m_confirmButtonCallback = a_confirmButtonCallback;
	}

	public void SetContent(string a_title, string a_content, string a_confirmButtonText, Action a_confirmButtonCallback, string a_cancelButtonText, Action a_cancelButtonCallback)
	{
		gameObject.SetActive(true);
		m_cancelButton.gameObject.SetActive(true);
		m_confirmButton.gameObject.SetActive(true);
		m_titleText.text = a_title;
		m_contentText.text = a_content;
		m_confirmButtonText.text = a_confirmButtonText;
		m_cancelButtonText.text = a_cancelButtonText;
		m_confirmButtonCallback = a_confirmButtonCallback;
		m_cancelButtonCallback = a_cancelButtonCallback;
	}

	void CancelButtonPressed()
	{
		m_cancelButtonCallback?.Invoke();
		CloseWindow();
	}

	void ConfirmButtonPressed()
	{
		m_confirmButtonCallback?.Invoke();
		CloseWindow();
	}

	public void CloseWindow()
	{
		gameObject.SetActive(false);
		PopupManager.Instance.ReturnPopupToPool(this);
	}
}
