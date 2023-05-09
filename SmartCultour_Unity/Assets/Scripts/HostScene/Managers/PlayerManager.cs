using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using TMPro;

public class PlayerManager : MonoBehaviourPunCallbacks
{
	[Header("Prefabs")]
	[SerializeField] GameObject m_playerEntryPrefab;
	[SerializeField] GameObject m_parameterEntryPrefab;

	[Header("General")]
	[SerializeField] Transform m_playerEntryParent;
	[SerializeField] Transform m_parameterEntryParent;
	[SerializeField] Button m_removeInactiveButton;
	[SerializeField] TextMeshProUGUI m_playerHeaderText;

	List<PlayerDetailsDisplay> m_playerEntries = new List<PlayerDetailsDisplay>();
	List<ParameterDetailsDisplay> m_parameterEntries = new List<ParameterDetailsDisplay>();
	[HideInInspector] public Dictionary<string, PlayerDetails> m_playerList = new Dictionary<string, PlayerDetails>();
	float m_timepassed;
	bool m_initialised = false;

	private void Start()
	{
		UpdatePlayerListContent();
		HostMain.Instance.m_onAssignmentChange += UpdateParameterDisplay;
		HostMain.Instance.m_onAssignmentChange += UpdatePlayerListVisuals;
		m_removeInactiveButton.onClick.AddListener(RemoveInactiveClicked);

		foreach (ParameterData param in SessionConfig.Data.parameters)
		{
			ParameterDetailsDisplay newEntry = GameObject.Instantiate(m_parameterEntryPrefab, m_parameterEntryParent).GetComponent<ParameterDetailsDisplay>();
			m_parameterEntries.Add(newEntry);
			newEntry.SetParameter(param);
		}
		UpdateParameterDisplay();
	}

	public void AddAlreadyAssignedPlayers(Dictionary<string, List<string>> a_assignment)
	{
		if (a_assignment == null)
			return;
		foreach(var kvp in a_assignment)
		{
			if(m_playerList.ContainsKey(kvp.Key))
				continue;

			m_playerList.Add(kvp.Key, new PlayerDetails(kvp.Key));
		}
	}

	private void Update()
	{
		m_timepassed += Time.deltaTime;
		if (m_timepassed > 10f)
		{
			m_timepassed = 0;
			UpdatePlayerListContent();
		}
	}

	public void UpdatePlayerListVisuals()
	{
		int activePlayers = 0;
		int index = 0;
		foreach (var kvp in m_playerList)
		{
			if (index < m_playerEntries.Count)
			{
				m_playerEntries[index].UpdateContent(kvp.Value);
			}
			else
			{
				PlayerDetailsDisplay newEntry = GameObject.Instantiate(m_playerEntryPrefab, m_playerEntryParent).GetComponent<PlayerDetailsDisplay>();
				m_playerEntries.Add(newEntry);
				newEntry.UpdateContent(kvp.Value);
			}
			index++;

			if (kvp.Value.m_active)
				activePlayers++;
		}

		m_playerHeaderText.text = $"Players ({activePlayers}/{index} active)";

		for (; index < m_playerEntries.Count; index++)
		{
			m_playerEntries[index].gameObject.SetActive(false);
		}
	}

	public void UpdatePlayerListContent()
	{
		if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.Players == null)
			return;

		foreach (var kvp in m_playerList)
			kvp.Value.m_active = false;
		
		//Add new players to list, activate existing ones
		foreach (var kvp in PhotonNetwork.CurrentRoom.Players)
		{
			if (string.IsNullOrEmpty(kvp.Value.NickName) || kvp.Value.IsLocal)
				continue;

			if (m_playerList.TryGetValue(kvp.Value.NickName, out var player))
				player.UpdateContent(kvp.Value);
			else
				m_playerList.Add(kvp.Value.NickName, new PlayerDetails(kvp.Value));
		}

		UpdatePlayerListVisuals();
	}

	public void UpdateParameterDisplay()
	{
		foreach(ParameterDetailsDisplay param in m_parameterEntries)
		{
			param.UpdateAmount(HostMain.Instance.m_paramAssignment);
		}
		//UpdatePlayerListContent();
	}

	public override void OnPlayerEnteredRoom(Player a_player)
	{
		UpdatePlayerListContent();
	}

	public override void OnPlayerLeftRoom(Player a_player)
	{
		UpdatePlayerListContent();
	}

	public override void OnPlayerPropertiesUpdate(Player a_targetPlayer, ExitGames.Client.Photon.Hashtable a_changedProps)
	{
		if (a_changedProps.ContainsKey(Util.PLAYER_ROLE_KEY))
			UpdatePlayerListContent();
	}

	void RemoveInactiveClicked()
	{
		PopupManager.Instance.CreatePopup("Remove inactive players", $"Are you sure you want to remove all inactive players and clear their parameter assignment?", "Confirm", RemoveAllInactive, "Cancel", null);
	}

	public void RemoveAllInactive()
	{
		List<string> toRemove = new List<string>();
		foreach (var kvp in m_playerList)
			if (!kvp.Value.m_active)
				toRemove.Add(kvp.Key);
		foreach (string name in toRemove)
		{
			HostMain.Instance.m_paramAssignment.Remove(name);
			m_playerList.Remove(name);
		}
		UpdatePlayerListVisuals();
		HostMain.Instance.SubmitParameterAssignment();

	}

	public void RemoveInactivePlayer(string a_name)
	{
		HostMain.Instance.m_paramAssignment.Remove(name);
		m_playerList.Remove(name);
		UpdatePlayerListVisuals();
		HostMain.Instance.SubmitParameterAssignment();
	}
}

