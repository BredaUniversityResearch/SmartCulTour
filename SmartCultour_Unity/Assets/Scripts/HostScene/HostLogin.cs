using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Crosstales.FB;
using System.IO;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class HostLogin : MonoBehaviourPunCallbacks, IOnEventCallback
{
	[SerializeField]
	CustomInputField m_sessionNameField;

	[SerializeField]
	CustomInputField m_setupFileField;

	[SerializeField]
	GameObject m_errorMessageContainer;
	[SerializeField]
	TextMeshProUGUI m_errorMessageText;

	[SerializeField]
	Button m_joinRoomButton, m_createRoomButton, m_browseSetupButton;

	[SerializeField]
	TextAsset m_defaultConfig;

	bool m_connectedToMaster = false;
	bool m_initialised;
	bool m_busy;

	bool m_becomingMasterClient;
	float m_timeWaitingForMaster;
	bool m_creating;

	void Start()
	{
		Debug.Log("Start called");
		PhotonNetwork.ConnectUsingSettings();
		m_joinRoomButton.onClick.AddListener(JoinRoom);
		m_createRoomButton.onClick.AddListener(CreateRoom);
		m_browseSetupButton.onClick.AddListener(SelectLoadPath);
		CultureInfo.CurrentCulture = new CultureInfo("en-US");
		PersistentHostData.ClearData();

		//if (gameObject.activeInHierarchy)
		FileBrowser.Instance.OnOpenFilesComplete += OnLoadPathSelected;
		m_initialised = true;

		Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
	}

	public override void OnEnable()
	{
		base.OnEnable();
	}

	public override void OnDisable()
	{
		base.OnDisable();		
	}

	private void OnDestroy()
	{
		if (FileBrowser.Instance != null)
			FileBrowser.Instance.OnOpenFilesComplete -= OnLoadPathSelected;
	}

	void Update()
	{
		bool validRoom = !string.IsNullOrEmpty(m_sessionNameField.text);
		m_errorMessageContainer.SetActive(true);
		if (!m_connectedToMaster)
			m_errorMessageText.text = "Connecting to lobby server...";
		else if (!validRoom)
			m_errorMessageText.text = "Please set the name of the session to create or join.";
		else if (m_becomingMasterClient)
			m_errorMessageText.text = "Claiming host permissions...";
		else if (m_busy)
			m_errorMessageText.text = "Connecting to session...";
		else
		{
			m_errorMessageText.text = "";
			m_errorMessageContainer.SetActive(false);
		}
		m_createRoomButton.interactable = m_joinRoomButton.interactable = m_connectedToMaster && validRoom && !m_busy && !m_becomingMasterClient;
		m_sessionNameField.interactable = m_browseSetupButton.interactable = m_setupFileField.interactable = !m_busy && !m_becomingMasterClient;

		if (m_becomingMasterClient)
		{
			m_timeWaitingForMaster += Time.deltaTime;
			if (m_timeWaitingForMaster > 10f)
			{
				PopupManager.Instance.CreatePopup("Failed to claim host permission", "Host permissions could not be claimed from the players already in the room, please try again or create a new session.", "Continue", null);
				PhotonNetwork.LeaveRoom(false);
				m_becomingMasterClient = false;
			}
		}
	}

	void CreateRoom()
	{
		string config = null;
		if (!string.IsNullOrEmpty(m_setupFileField.text))
		{
			if (File.Exists(m_setupFileField.text))
			{
				using (StreamReader stream = new StreamReader(m_setupFileField.text))
				{
					config = stream.ReadToEnd();
				}
			}
		}
		if (config == null)
			config = m_defaultConfig.text;

		if (SessionConfig.SetJsonSaveFile(config, out var save, out var assignment))
		{
			if(save == null)
				PersistentHostData.Instance.m_loadedState = SessionStateData.GetEmpty();
			else
				PersistentHostData.Instance.m_loadedState = save;

			if(assignment == null)
				PersistentHostData.Instance.m_paramAssignment = new Dictionary<string, List<string>>();
			else
				PersistentHostData.Instance.m_paramAssignment = assignment;
		}
		else
		{
			PopupManager.Instance.CreatePopup("Invalid config", "The selected config file is not valid and could not be used to create a session.", "Continue", null);
			return;
		}

		m_busy = true;
		m_creating = true;
		//Calls OnJoinedRoom on success, OnCreateRoomFailed on failure
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = 0;
		roomOptions.EmptyRoomTtl = 300000;          //5 mins, longer not allowed
		roomOptions.PlayerTtl = 5000;              //5 secs
		roomOptions.CleanupCacheOnLeave = false;    //Ensure player event cache is not cleared when they leave
		roomOptions.BroadcastPropsChangeToAll = true;
		PhotonNetwork.CreateRoom(m_sessionNameField.text, roomOptions, null);
	}

	void JoinRoom()
	{
		m_busy = true;
		m_creating = false;
		//Calls OnJoinedRoom on success, OnJoinRoomFailed on failure
		PhotonNetwork.LocalPlayer.NickName = "Moderator";
		PhotonNetwork.JoinRoom(m_sessionNameField.text);
	}

	void OnLoadPathSelected(bool selected, string singleFile, string[] files)
	{
		Debug.Log("Path selected");
		if (selected)
			m_setupFileField.text = singleFile;
	}

	void SelectLoadPath()
	{
		FileBrowser.Instance.OpenFilesAsync(multiselect: false, "json");
	}

	// Photon Callbacks ---------------------------------------------------------------

	public override void OnConnectedToMaster()
	{
		m_connectedToMaster = true;
		Debug.Log("Connected to master server");
	}

	public override void OnCreatedRoom()
	{
		PhotonNetwork.LocalPlayer.NickName = "Moderator";
		if (!PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { Util.SESSION_CONFIG_KEY, SessionConfig.GetJsonData() } }))
		{
			m_busy = false;
			PopupManager.Instance.CreatePopup("Could not upload config", "The selected config could not be added to the session's properties. You will be disconnected.", "Continue", null);
			PhotonNetwork.LeaveRoom(false);
			return;
		}
	}

	public override void OnJoinedRoom()
	{
		m_busy = false;
		Debug.Log($"Connected to room: {PhotonNetwork.CurrentRoom.Name}");

		if (!m_creating)
		{
			if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Util.SESSION_CONFIG_KEY, out var configString))
			{
				if (!SessionConfig.SetJsonConfigData((string)configString, true))
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
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			if (PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer))
			{
				m_becomingMasterClient = true;
				m_timeWaitingForMaster = 0;
			}
			else
			{
				PopupManager.Instance.CreatePopup("Failed to claim host permission", "Host permissions could not be claimed from the players already in the session, please try again or create a new session.", "Continue", null);
				PhotonNetwork.LeaveRoom(false);
			}
		}
		else
			SceneManager.LoadScene("HostScene");
	}

	public override void OnMasterClientSwitched(Player a_newMasterClient)
	{
		if (m_becomingMasterClient && a_newMasterClient == PhotonNetwork.LocalPlayer)
		{
			SceneManager.LoadScene("HostScene");
		}
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		m_busy = false;
		PopupManager.Instance.CreatePopup("Failed to join room", $"Joining room failed: {message}", "Confirm", null);
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		m_busy = false;
		PopupManager.Instance.CreatePopup("Failed to create room", $"Creating room failed: {message}", "Confirm", null);
	}

	public override void OnErrorInfo(ErrorInfo errorInfo)
	{
		Debug.LogError(errorInfo.Info);
	}

	public void OnEvent(EventData a_photonEvent)
	{
		//Save events we receive while connecting
		if (a_photonEvent.Code == Util.INTERVENTION_EVALUATION_CODE)
		{
			PersistentHostData.Instance.m_cachedEvents.Add(a_photonEvent);
			Debug.Log("Event from cache received");
		}
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		base.OnDisconnected(cause);
		//TODO: try to auto reconnect
		PopupManager.Instance.CreatePopup("Disconnected", "The connection with the server has been lost.", "Reconnect", () =>
		{
			SceneManager.LoadScene(0);
		});
	}
}
