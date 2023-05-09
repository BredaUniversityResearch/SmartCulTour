using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Newtonsoft.Json;
using ExitGames.Client.Photon;
using System.Globalization;
using TMPro;

public class PlayerMain : MonoBehaviourPunCallbacks
{
	private static PlayerMain m_instance;
	public static PlayerMain Instance => m_instance;
	bool m_initialised = false;

	[SerializeField] ContentTabInterventions m_contentInterventions;
	[SerializeField] ContentTabRole m_contentRole;
	[SerializeField] ContentTabEvaluation m_contentEvaluation;
	[SerializeField] TextMeshProUGUI m_sessionCodeText;

	SessionStateData m_saveData;
	public Dictionary<string, List<string>> m_paramAssignment;
	bool m_wasAssignedParameters;
	int m_processedInterventions = 0;

	void Awake()
	{
		m_instance = this;
		Debug.Log($"Successfully joined room: {PhotonNetwork.CurrentRoom.Name}");
		CultureInfo.CurrentCulture = new CultureInfo("en-US");
		m_sessionCodeText.text = "Session code: " + PhotonNetwork.CurrentRoom.Name;
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
			m_contentInterventions.UpdateInterventions(m_saveData.interventions);

			//Parameter assignment
			if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Util.PARAM_ASSIGN_KEY, out var paramAssignmentString))
			{
				m_paramAssignment = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>((string)paramAssignmentString);
				if (m_paramAssignment == null)
				{
					m_paramAssignment = new Dictionary<string, List<string>>();
				}
				else if (m_paramAssignment.TryGetValue(PersistentPlayerData.Instance.m_nickName, out var assignment))
				{
					m_contentRole.UpdateParameters(assignment);
					m_contentEvaluation.UpdateParameters(assignment);
					m_wasAssignedParameters = true;
				}
			}
			else
			{
				m_paramAssignment = new Dictionary<string, List<string>>();
			}

			m_contentInterventions.Initialise();
			m_contentRole.Initialise();
			m_contentEvaluation.Initialise();

			if(m_saveData.evaluationActive) //Only do this after save data, assignment and initialisation
			{
				m_contentEvaluation.BeginEvaluation(m_saveData.LastIntervention.name);
			}
			m_initialised = true;
		}
	}

	public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
		//Parameter assignment
		if (propertiesThatChanged.TryGetValue(Util.PARAM_ASSIGN_KEY, out var paramAssignmentString))
		{
			m_paramAssignment = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>((string)paramAssignmentString);
			if (m_paramAssignment != null && m_paramAssignment.TryGetValue(PersistentPlayerData.Instance.m_nickName, out var assignment))
			{
				m_contentRole.UpdateParameters(assignment);
				m_contentEvaluation.UpdateParameters(assignment);
				m_wasAssignedParameters = true;
			}
			else if (m_wasAssignedParameters)
			{
				m_contentRole.UpdateParameters(null);
				m_contentEvaluation.UpdateParameters(null);
				m_wasAssignedParameters = false;
			}
		}

		if (propertiesThatChanged.TryGetValue(Util.SAVE_DATA_KEY, out var saveDataString))
		{
			bool wasEvaluating = m_saveData.evaluationActive;
			m_saveData = JsonConvert.DeserializeObject<SessionStateData>((string)saveDataString);

			if (m_saveData == null)
			{
				m_saveData = SessionStateData.GetEmpty();
				Debug.LogError("Received save data could not be parsed: " + (string)saveDataString);
			}
			else
			{
				int newNumberInterventions = m_saveData.NumberInterventions;
				m_contentInterventions.UpdateInterventions(m_saveData.interventions);
				if(m_saveData.evaluationActive && !wasEvaluating)
				{
					m_contentEvaluation.BeginEvaluation(m_saveData.LastIntervention.name);
				}
				else if (!m_saveData.evaluationActive && wasEvaluating)
				{
					m_contentEvaluation.EndEvaluation();
				}
				else if(m_saveData.evaluationActive && newNumberInterventions > m_processedInterventions)
				{
					//Evaluated intervention changed
					m_contentEvaluation.EndEvaluation();
					m_contentEvaluation.BeginEvaluation(m_saveData.LastIntervention.name);
				}
				m_processedInterventions = newNumberInterventions;
			}
		}
	}

	public override void OnErrorInfo(ErrorInfo errorInfo)
	{
		Debug.LogError(errorInfo.Info);
	}

	public void SubmitEvaluation(EvaluationData a_evaluation)
	{
		PhotonNetwork.RaiseEvent(Util.INTERVENTION_EVALUATION_CODE, new Hashtable() { { "details", JsonConvert.SerializeObject(a_evaluation) } }, new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCacheGlobal, InterestGroup = 0, Receivers = ReceiverGroup.Others, TargetActors = null }, SendOptions.SendReliable);
		Debug.Log("Evaluations sent");
	}

	//public void OnEvent(EventData a_photonEvent)
	//{
	//	if (a_photonEvent.Code == Util.INTERVENTION_EVALUATION_CODE)
	//	{
	//		Debug.Log("Evaluation event received");
	//	}
	//}

	public bool PlayerEvaluatedCurrentIntervention(string a_playerName)
	{
		if (m_saveData == null || !m_saveData.evaluationActive)
			return false;
		List<InterventionData> roundInterventions = m_saveData.interventions[m_saveData.interventions.Count - 1];
		if (roundInterventions.Count == 0)
			return false;
		return roundInterventions[roundInterventions.Count - 1].EvaluatedBy(a_playerName);
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
