using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class PlayerDetails
{
	public string m_name;
	public bool m_active;
	public RoleData m_role;

	public PlayerDetails(string a_name)
	{
		m_name = a_name;
	}

	public PlayerDetails(Player a_player, bool a_active = true)
	{
		m_name = a_player.NickName;
		UpdateContent(a_player, a_active);
	}

	public void UpdateContent(Player a_player, bool a_active = true)
	{
		m_active = a_active;
		if (a_player.CustomProperties.TryGetValue(Util.PLAYER_ROLE_KEY, out var teamIndex))
		{
			m_role = SessionConfig.Data.roles[(int)teamIndex];
		}
		else
			m_role = null;
	}
}

