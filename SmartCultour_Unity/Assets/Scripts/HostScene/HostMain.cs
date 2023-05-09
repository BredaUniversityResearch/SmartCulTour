using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Newtonsoft.Json;
using ExitGames.Client.Photon;
using TMPro;
using System.Globalization;

public class HostMain : MonoBehaviourPunCallbacks, IOnEventCallback
{
	private static HostMain m_instance;
	public static HostMain Instance => m_instance;

	[SerializeField] KPIManager m_kpiManager;
	[SerializeField] InterventionDisplayWindow m_interventionWindow;
	[SerializeField] PlayerManager m_playerManager;
	[SerializeField] Button m_startEvaluationButton, m_endEvaluationButton, m_endRoundButton;
	[SerializeField] CustomInputField m_interventionNameField;
	[SerializeField] ParameterAssignmentWindow m_parameterAssignmentWindow;
	[SerializeField] DashboardManager m_dashboardManager;
	[SerializeField] AutoAssignmentWindow m_autoAssignmentWindow;
	[SerializeField] TextMeshProUGUI m_sessionCodeText;

	bool m_saveDataDirty = false;
	float m_timeSinceSaveDataSubmit;

	[HideInInspector] public SessionStateData m_saveData;
	[HideInInspector] public Dictionary<string, List<string>> m_paramAssignment; //<playername, List<param_name>>
	bool m_initialised = false;

	public event Action m_onAssignmentChange;
	public event Action m_onInterventionsChanged;
	public event Action m_onKPIsChanged;

	void Awake()
    {
		m_instance = this;
		Debug.Log($"Successfully joined room: {PhotonNetwork.CurrentRoom.Name}");
		if (!PhotonNetwork.IsMasterClient && !PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer))
		{
			PopupManager.Instance.CreatePopup("Failed to claim host permission", "Host permissions could not be claimed from the players already in the room, please try again or create a new session.", "Return to login", () =>
			{
				PhotonNetwork.LeaveRoom(false);
				SceneManager.LoadScene(0);
			});
		}

		m_startEvaluationButton.onClick.AddListener(StartEvaluation);
		m_endEvaluationButton.onClick.AddListener(EndEvaluation);
		m_endRoundButton.onClick.AddListener(EndRound);
		m_dashboardManager.CreateEntries(SessionConfig.Data.kpis, SessionConfig.Data.parameters, this);
		m_autoAssignmentWindow.Initialise(m_playerManager);
		m_sessionCodeText.text = "Session code: " + PhotonNetwork.CurrentRoom.Name;
		CultureInfo.CurrentCulture = new CultureInfo("en-US");

		//If loading with save data, start initilized and push our save data
        if(PersistentHostData.Instance.m_paramAssignment != null)
        {
            m_paramAssignment = PersistentHostData.Instance.m_paramAssignment;
            SubmitParameterAssignment();
			m_playerManager.AddAlreadyAssignedPlayers(m_paramAssignment);
			//m_playerManager.UpdateParameterDisplay();
		}
        else
			m_paramAssignment = new Dictionary<string, List<string>>();

		if(PersistentHostData.Instance.m_loadedState != null)
		{
			m_saveData = PersistentHostData.Instance.m_loadedState;
			m_saveData.kpiValues = m_kpiManager.LoadKPIs(m_saveData.kpiValues, SessionConfig.Data.kpis);
			m_saveData.VerifyInterventions();
			m_interventionWindow.UpdateInterventions(m_saveData.interventions);
			SubmitSaveData();
			m_onKPIsChanged.Invoke();
			m_onInterventionsChanged.Invoke();
			if(m_onAssignmentChange != null)
				m_onAssignmentChange.Invoke();

			//Resolve events that were cached and not resolved
			foreach (EventData cachedEvent in PersistentHostData.Instance.m_cachedEvents)
				OnEvent(cachedEvent);

			PersistentHostData.ClearData();
			m_initialised = true;
		}
	}

	void Start()
	{
		if (Display.displays.Length > 1)
		{
			Display.displays[1].Activate();
		}
		else
		{
			PopupManager.Instance.CreatePopup("No dashboard", "Only a single display was detected. The dashboard will not be visible.", "Continue", null);
		}
	}

	void Update()
	{
		if (!m_initialised)
		{
			if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Util.SAVE_DATA_KEY, out var saveDataString))
			{
				m_saveData = JsonConvert.DeserializeObject<SessionStateData>((string)saveDataString);
				if (m_saveData == null)
					m_saveData = SessionStateData.GetEmpty();
			}
			else
			{
				m_saveData = SessionStateData.GetEmpty();
			}
			if(m_saveData.evaluationActive)
			{
				m_interventionNameField.text = m_saveData.LastIntervention.name;
				SetEvaluationUIActive(true);
			}

			if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Util.PARAM_ASSIGN_KEY, out var paramAssignmentString))
			{
				m_paramAssignment = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>((string)paramAssignmentString);
				if (m_paramAssignment == null)
					m_paramAssignment = new Dictionary<string, List<string>>();
				m_playerManager.AddAlreadyAssignedPlayers(m_paramAssignment);
				if (m_onAssignmentChange != null)
					m_onAssignmentChange.Invoke();
			}
			else
			{
				m_paramAssignment = new Dictionary<string, List<string>>();
			}

			m_saveData.kpiValues = m_kpiManager.LoadKPIs(m_saveData.kpiValues, SessionConfig.Data.kpis);
			m_interventionWindow.UpdateInterventions(m_saveData.interventions);
			m_onKPIsChanged.Invoke();
			m_onInterventionsChanged.Invoke();

			//Resolve events that were cached and not resolved
			foreach (EventData cachedEvent in PersistentHostData.Instance.m_cachedEvents)
				OnEvent(cachedEvent);

			PersistentHostData.ClearData();
			m_initialised = true;
		}
		else
		{
			m_timeSinceSaveDataSubmit += Time.deltaTime;
			if(m_saveDataDirty && m_timeSinceSaveDataSubmit > 2f)
			{
				SubmitSaveData();		
			}
		}
	}

	void SubmitSaveData()
	{
		m_saveDataDirty = false;
		m_timeSinceSaveDataSubmit = 0;
		if (!PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { Util.SAVE_DATA_KEY, JsonConvert.SerializeObject(m_saveData) } }))
			Debug.LogError("Failed to set room properties for SaveData");
	}

	public void SubmitParameterAssignment()
	{
		if (!PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { Util.PARAM_ASSIGN_KEY, JsonConvert.SerializeObject(m_paramAssignment) } }))
			Debug.LogError("Failed to set room properties for ParameterAssignment");
	}

	public void LoadGame(string a_path)
	{
		//SessionStateData newSaveData;
		//using (StreamReader sr = new StreamReader(a_path))
		//{
		//	try
		//	{
		//		newSaveData = JsonConvert.DeserializeObject<SessionStateData>(sr.ReadToEnd());
		//	}
		//	catch(Exception e)
		//	{
		//		Debug.LogError("Error encountered when deserializing save data: " + e.Message);
		//		PopupManager.Instance.CreatePopup("Loading failed", "The selected file is not contain a valid save file. The following error was encountered during loading: " + e.Message, "Continue", null);
		//		return;
		//	}
		//}
		//if (newSaveData != null)
		//{
		//	UpdateToSaveData(newSaveData);
		//	SubmitSaveData();
		//	PopupManager.Instance.CreatePopup("Loading successful", "The game was successfully loaded.", "Continue", null);
		//}
		//else
		//{
		//	Debug.LogError("Empty save data loaded");
		//	PopupManager.Instance.CreatePopup("Loading failed", "The selected file is empty", "Continue", null);
		//}
	}

	//void UpdateToSaveData(SessionStateData a_newSaveData)
	//{
	//	EndEvaluation();
	//	m_kpiManager.LoadKPIs(a_newSaveData.kpiValues);
	//	m_interventionWindow.UpdateInterventions(a_newSaveData.interventions);
	//	m_saveData = a_newSaveData;
	//}

	public void SaveGame(string a_path, bool a_successPopup = true)
	{
        try
        {
            using (StreamWriter sw = new StreamWriter(a_path, false))
            {
                OuterDataModel outer = new OuterDataModel();
                outer.datamodel = new DataModel() { config = SessionConfig.Data, assignment = m_paramAssignment, save = m_saveData };
                sw.Write(JsonConvert.SerializeObject(outer));
            }
            if (a_successPopup)
            {
                PopupManager.Instance.CreatePopup("Save successful", "The game was successfully saved to: " + a_path, "Continue", null);
            }
        }
        catch(Exception e)
        {
            PopupManager.Instance.CreatePopup("Save failed", $"Saving the game failed, error message: {e.Message}", "Continue", null);
        }
	}

	void StartEvaluation()
	{
		if(m_saveData.evaluationActive)
		{
			Debug.LogError("Already evaluating an intervention: " + m_saveData.LastIntervention.name);
			return;
		}
		if(string.IsNullOrEmpty(m_interventionNameField.text))
		{
			PopupManager.Instance.CreatePopup("No name given", "Please fill in the name of the intervention that will be evaluated", "Continue", null);
			return;
		}

		m_saveData.interventions[m_saveData.interventions.Count-1].Add(new InterventionData(m_interventionNameField.text));
		m_saveData.evaluationActive = true;
		SetEvaluationUIActive(true);
		SubmitSaveData();
	}

	void EndEvaluation()
	{
		if (!m_saveData.evaluationActive)
		{
			return;
		}

		m_saveData.evaluationActive = false;
		SubmitSaveData();
		m_playerManager.UpdatePlayerListContent();
		m_interventionNameField.text = "";
		SetEvaluationUIActive(false);
	}

	void SetEvaluationUIActive(bool a_active)
	{
		m_startEvaluationButton.gameObject.SetActive(!a_active);
		m_endEvaluationButton.gameObject.SetActive(a_active);
		m_interventionNameField.interactable = !a_active;
	}

	void EndRound()
	{
		if (m_saveData.evaluationActive)
		{
			PopupManager.Instance.CreatePopup("Cannot end round", "The round cannot be ended while an intervention is being evaluated.", "Continue", null);
			return;
		}
		if (m_saveData == null || m_saveData.interventions[m_saveData.interventions.Count-1].Count == 0)
		{
			PopupManager.Instance.CreatePopup("Cannot end round", "The round cannot be ended if no interventions have been evaluated.", "Continue", null);
			return;
		}
		PopupManager.Instance.CreatePopup("End round?", "Are you sure you want to end the round?", "End round", ConfirmEndRound, "Cancel", null);
		
	}

	void ConfirmEndRound()
	{
		if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Autosaves")))
			Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Autosaves"));
		SaveGame(Path.Combine(Application.persistentDataPath, "Autosaves", $"AS_{DateTime.Now.ToString("yyyyMMddTHHmmss")}_R{m_saveData.interventions.Count+1}.json"), false);
		if (m_saveData.startingKpiValues == null)
		{
			//After the first round, save initial KPI values
			m_saveData.startingKpiValues = new Dictionary<string, float>();
			foreach (var kvp in m_saveData.kpiValues)
				m_saveData.startingKpiValues.Add(kvp.Key, kvp.Value.currentValue);
		}
		m_kpiManager.CalculateNewValues(m_saveData.interventions[m_saveData.interventions.Count - 1], m_saveData.kpiValues);
		m_saveData.interventions.Add(new List<InterventionData>());
		m_interventionWindow.UpdateInterventions(m_saveData.interventions);
		m_onKPIsChanged.Invoke();
		m_onInterventionsChanged();
		SubmitSaveData();
	}

	public void SetAssignmentForPlayer(string a_playerName, List<string> a_assignment)
	{
		if (a_assignment.Count == 0)
		{
			if (m_paramAssignment.ContainsKey(a_playerName))
				m_paramAssignment.Remove(a_playerName);
		}
		else
		{
			m_paramAssignment[a_playerName] = a_assignment;
		}
		SubmitParameterAssignment();
		if (m_onAssignmentChange != null)
			m_onAssignmentChange.Invoke();
	}

	public void SetAssignment(Dictionary<string, List<string>> a_newAssignement)
	{
		m_paramAssignment = a_newAssignement;
		SubmitParameterAssignment();
		if (m_onAssignmentChange != null)
			m_onAssignmentChange.Invoke();
	}

	public void OpenAssignmentWindowForPlayer(string a_name)
	{
		if(m_paramAssignment.TryGetValue(a_name, out var assignment))
		{
			m_parameterAssignmentWindow.SetToPlayer(a_name, assignment);
		}
		else
			m_parameterAssignmentWindow.SetToPlayer(a_name, null);
	}

	public bool PlayerEvaluatedCurrentIntervention(string a_playerName)
	{
		if(m_saveData == null || !m_saveData.evaluationActive)
			return false;
		List<InterventionData> roundInterventions = m_saveData.interventions[m_saveData.interventions.Count - 1];
		if (roundInterventions.Count == 0)
			return false;
		return roundInterventions[roundInterventions.Count - 1].EvaluatedBy(a_playerName);
	}

	public void RemovePlayer(string a_name)
	{
		m_playerManager.RemoveInactivePlayer(a_name);
	}

	public void SetKPIValues(Dictionary<string, KPIValue> a_newValues)
	{
		m_saveData.kpiValues = a_newValues;
		m_onKPIsChanged.Invoke();
		SubmitSaveData();
	}

	//================================================== PHOTON EVENTS =======================================================================

	//public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	//{
	//	if (propertiesThatChanged.TryGetValue(Util.ROOM_DATA_KEY, out var roomDataString))
	//	{
	//		RoomData newRoomData = JsonConvert.DeserializeObject<RoomData>((string)roomDataString);
	//		if (newRoomData == null)
	//		{
	//			Debug.LogError("Could not parse received room data: " + roomDataString);
	//			return;
	//		}
	//		m_roomData = newRoomData;
	//	}
	//}

	public override void OnMasterClientSwitched(Player a_newMasterClient)
	{
		if(a_newMasterClient != PhotonNetwork.LocalPlayer)
		{
			PopupManager.Instance.CreatePopup("Host permissions lost", "Another moderator connected to the session and claimed host permissions. Only a single host can be connected to a session. Please reconnect to reclaim host permissions or create a new session.", "Return to login", () =>
			{
				PhotonNetwork.LeaveRoom(false);
				SceneManager.LoadScene(0);
			});
		}
	}

	public void OnEvent(EventData a_photonEvent)
	{
		if (a_photonEvent.Code == Util.INTERVENTION_EVALUATION_CODE)
		{
			Hashtable eventContent = (Hashtable)a_photonEvent.CustomData;
			if(eventContent.TryGetValue("details", out var details))
			{
				eventContent.Remove("details"); //Details are removed so the removal filter is cleaner
				EvaluationData evalData = JsonConvert.DeserializeObject<EvaluationData>((string)details);
				if(m_saveData == null)
					PersistentHostData.Instance.m_cachedEvents.Add(a_photonEvent);
				else if (!m_saveData.evaluationActive || m_saveData.LastIntervention.name != evalData.interventionName)
					Debug.LogWarning($"Evaluation result received for an intervention that is no longer active: {evalData.interventionName}, it will be ignored.");
				else
					HandleNewEvaluationResult(evalData);
			}
			//Remove the event from the room cache (filters by eventcontent, which should just be ID at this point)
			PhotonNetwork.RaiseEvent(Util.INTERVENTION_EVALUATION_CODE, eventContent, new RaiseEventOptions() { CachingOption = EventCaching.RemoveFromRoomCache }, SendOptions.SendReliable);
		}
	}

	void HandleNewEvaluationResult(EvaluationData a_result)
	{
		m_saveData.LastIntervention.Add(a_result);
		m_playerManager.UpdatePlayerListContent();
		m_onInterventionsChanged.Invoke();
		m_saveDataDirty = true;
	}

	public override void OnErrorInfo(ErrorInfo errorInfo)
	{
		Debug.LogError(errorInfo.Info);
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		base.OnDisconnected(cause);
		//TODO: try to auto reconnect
		PopupManager.Instance.CreatePopup("Disconnected", "The connection with the server has been lost.", "Return to login", () =>
		{
			SceneManager.LoadScene(0);
		});
	}
}
