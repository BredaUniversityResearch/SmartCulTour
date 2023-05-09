using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using ColourPalette;


class PlayerDetailsDisplay : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI m_playerName;
	[SerializeField] TextMeshProUGUI m_playerRole;
	[SerializeField] TextMeshProUGUI m_playerParameters;
	[SerializeField] Button m_parameterWindowButton;
	[SerializeField] Button m_removePlayerButton;

	[SerializeField] GameObject m_evaluatedIcon;
	[SerializeField] GameObject m_activeIcon;

	private void Start()
	{
		m_parameterWindowButton.onClick.AddListener(OpenParameterAssignmentWindow);
		m_removePlayerButton.onClick.AddListener(RemovePlayer);
	}

	public void UpdateContent(PlayerDetails a_player)
	{
		gameObject.SetActive(true);
		m_playerName.text = a_player.m_name;
		if(a_player.m_role != null)
			m_playerRole.text = a_player.m_role.name;
		else
			m_playerRole.text = "";

		List<string> parameters = null;
		if (HostMain.Instance.m_paramAssignment != null && HostMain.Instance.m_paramAssignment.TryGetValue(a_player.m_name, out parameters))
		{
			m_playerParameters.text = "• " + string.Join("\n• ", parameters);
		}
		else
		{
			m_playerParameters.text = "None";
		}

		m_activeIcon.SetActive(a_player.m_active);
		m_removePlayerButton.gameObject.SetActive(!a_player.m_active);
		m_evaluatedIcon.SetActive(HostMain.Instance.PlayerEvaluatedCurrentIntervention(a_player.m_name));
	}

	void OpenParameterAssignmentWindow()
	{
		HostMain.Instance.OpenAssignmentWindowForPlayer(m_playerName.text);
	}

	void RemovePlayer()
	{
		HostMain.Instance.RemovePlayer(m_playerName.text);
	}
}

