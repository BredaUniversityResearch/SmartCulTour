using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Globalization;

public class PlayerLogin : MonoBehaviourPunCallbacks
{
	[SerializeField] GameObject m_joinSessionWindow;
	[SerializeField] CustomInputField m_sessionNameField;

	[SerializeField] GameObject m_selectRoleWindow;
	[SerializeField] CustomDropdown m_roleDropdown;
	[SerializeField] CustomInputField m_playerNameField;
	[SerializeField] TextMeshProUGUI m_stateText;

	[SerializeField]
	Button m_joinSessionButton, m_nextButton, m_backButton;

	bool m_connectedToMaster = false;
	bool m_reconnecting = true;
	bool m_busy;

	void Start()
	{
		PhotonNetwork.ConnectUsingSettings();
		m_joinSessionButton.onClick.AddListener(JoinSession);
		m_nextButton.onClick.AddListener(StartGame);
		m_backButton.onClick.AddListener(ReturnToSessionSelect);
		CultureInfo.CurrentCulture = new CultureInfo("en-US");
		m_sessionNameField.text = PlayerPrefs.GetString("PreviousSession", "");
	}

	void Update()
	{
		bool validName = !string.IsNullOrEmpty(m_playerNameField.text);
		bool validSession = !string.IsNullOrEmpty(m_sessionNameField.text);

		m_joinSessionButton.interactable = m_connectedToMaster && !m_busy && validSession;
		m_nextButton.interactable = validName;

		if (!m_connectedToMaster)
			m_stateText.text = "Connecting to lobby...";
		else if(m_busy)
			m_stateText.text = "Connecting to session...";
		else if (!validSession)
			m_stateText.text = "Set a session code";
		else
			m_stateText.text = "";

		if(!m_connectedToMaster && !m_reconnecting)
		{
			m_reconnecting = true;
			PhotonNetwork.ConnectUsingSettings();
		}
	}

	void JoinSession()
	{
		//Calls OnJoinedRoom on success, OnJoinRoomFailed on failure
		m_busy = true;
		PhotonNetwork.LocalPlayer.NickName = ""; // Reset name before joining, so no conflicts are created
		PlayerPrefs.SetString("PreviousSession", m_sessionNameField.text);
		PhotonNetwork.JoinRoom(m_sessionNameField.text);
	}

	void StartGame()
	{
		PhotonNetwork.LocalPlayer.NickName = m_playerNameField.text;
		RoleData currentTeam = SessionConfig.Data.roles[m_roleDropdown.value];
		PlayerPrefs.SetInt("PreviousRole", m_roleDropdown.value);
		PlayerPrefs.SetString("PreviousName", m_playerNameField.text);

		Debug.Log($"Connected to room: {PhotonNetwork.CurrentRoom.Name}");
		PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { Util.PLAYER_ROLE_KEY, m_roleDropdown.value } });
		PersistentPlayerData.Instance.m_role = currentTeam;
		PersistentPlayerData.Instance.m_nickName = m_playerNameField.text;
		SceneManager.LoadScene("PlayerScene");
	}

	void ReturnToSessionSelect()
	{
		m_joinSessionWindow.SetActive(true);
		m_selectRoleWindow.SetActive(false);
		PhotonNetwork.LeaveRoom();
	}

	// Photon Callbacks ---------------------------------------------------------------

	public override void OnConnectedToMaster()
	{
		m_reconnecting = false;
		m_connectedToMaster = true;
		Debug.Log("Connected to master server");
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		m_reconnecting = false;
		m_connectedToMaster = false;
		ReturnToSessionSelect();
		PopupManager.Instance.CreatePopup("Disconnected", "Connection to the server was unexpedtedly lost. Please check you have a working internet connection and try again. Disconnection reason: " + cause.ToString(), "Confirm", null);
	}

	public override void OnJoinedRoom()
	{
		// joined a room successfully
		m_busy = false;
		if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Util.SESSION_CONFIG_KEY, out var configString))
		{
			if (!SessionConfig.SetJsonConfigData((string)configString))
			{
				PopupManager.Instance.CreatePopup("Invalid config", "The joined session does not contain a valid config file, please try another session.", "Continue", null);
				PhotonNetwork.LeaveRoom(false);
				return;
			}
		}
		else
		{
			PopupManager.Instance.CreatePopup("Missing config", "The joined session does not contain a config file, please try another session.", "Continue", null);
			PhotonNetwork.LeaveRoom(false);
			return;
		}

		m_roleDropdown.ClearOptions();
		List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
		foreach (RoleData role in SessionConfig.Data.roles)
		{
			options.Add(new TMP_Dropdown.OptionData(role.name));
		}
		m_roleDropdown.options = options;
		int previousRole = PlayerPrefs.GetInt("PreviousRole", 0);
		if (previousRole >= options.Count)
			previousRole = 0;
		m_roleDropdown.value = previousRole;
		m_playerNameField.text = PlayerPrefs.GetString("PreviousName", "");

		m_joinSessionWindow.SetActive(false);
		m_selectRoleWindow.SetActive(true);
		
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		m_busy = false;
		PopupManager.Instance.CreatePopup("Failed to join session", $"Joining session failed: {message}", "Confirm", null);
	}
}
