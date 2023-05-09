using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Crosstales.FB;

public class MenuWindow : MonoBehaviour
{
	[SerializeField]
	Button m_exitButton;
	[SerializeField]
	Button m_continueButton;
	[SerializeField]
	Button m_returnToLoginButton;
	[SerializeField]
	Button m_creditsButton;
	[SerializeField]
	Button m_loadButton;
	[SerializeField]
	Button m_saveButton;
	[SerializeField]
	Button m_optionsButton;

	private void Start()
	{
		if(m_exitButton != null)
			m_exitButton.onClick.AddListener(QuitGame);
		if (m_continueButton != null)
			m_continueButton.onClick.AddListener(CloseWindow);
		if (m_returnToLoginButton != null)
			m_returnToLoginButton.onClick.AddListener(ReturnToLogin);
		if (m_creditsButton != null)
			m_creditsButton.onClick.AddListener(OpenCredits);
		if (m_optionsButton != null)
			m_optionsButton.onClick.AddListener(OpenOptions);

		if(HostMain.Instance != null)
		{
			if (m_loadButton != null)
				m_loadButton.onClick.AddListener(LoadGame);
			if (m_saveButton != null)
				m_saveButton.onClick.AddListener(SaveGame);
			FileBrowser.Instance.OnOpenFilesComplete += OnLoadPathSelected;
			FileBrowser.Instance.OnSaveFileComplete += OnSavePathSelected;
		}
		else
		{
			if (m_loadButton != null)
				m_loadButton.gameObject.SetActive(false);
			if (m_saveButton != null)
				m_saveButton.gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		if (HostMain.Instance != null)
		{
			FileBrowser.Instance.OnOpenFilesComplete -= OnLoadPathSelected;
			FileBrowser.Instance.OnSaveFileComplete -= OnSavePathSelected;
		}
	}

	void QuitGame()
	{
		if(PhotonNetwork.InRoom)
			PhotonNetwork.LeaveRoom(false);
		Application.Quit();
	}

	void ReturnToLogin()
	{
		if (PhotonNetwork.InRoom)
			PhotonNetwork.LeaveRoom(false);
		SceneManager.LoadScene(0);
	}

	void OpenCredits()
	{
		//TODO: create credit window
		CloseWindow();
	}

	public void CloseWindow()
	{
		gameObject.SetActive(false);
	}

	void SaveGame()
	{
		FileBrowser.Instance.SaveFile("json");
	}

	void OnSavePathSelected(bool selected, string file)
	{
		HostMain.Instance.SaveGame(file);
		CloseWindow();
	}

	void LoadGame()
	{
		FileBrowser.Instance.OpenSingleFile("json");
	}

	void OnLoadPathSelected(bool selected, string singleFile, string[] files)
	{
		if (selected)
		{
			string file = singleFile;
			PopupManager.Instance.CreatePopup("Confirm load", $"Are you sure you want to load the file: {singleFile}.\nAny unsaved data will be lost.", "Load", () => HostMain.Instance.LoadGame(file), "Cancel", null);
		}
	}

	void OpenOptions()
	{
		//TODO: options window
	}
}
