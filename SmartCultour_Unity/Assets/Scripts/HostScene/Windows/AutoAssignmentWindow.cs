using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AutoAssignmentWindow : MonoBehaviour
{
	[SerializeField] CustomInputField m_targetParametersField;
	[SerializeField] Button m_reassignAllButton; 
	[SerializeField] Button m_assignRemainingButton;
	[SerializeField] Toggle m_loopToMaxToggle;

	PlayerManager m_playerManager;
	bool m_ignoreCallback;

	public void Initialise(PlayerManager a_playerManager)
	{
		m_playerManager = a_playerManager;
	}

    void Start()
    {
		m_reassignAllButton.onClick.AddListener(ReassignAll);
		m_assignRemainingButton.onClick.AddListener(AssignRemaining);
		m_targetParametersField.onValueChanged.AddListener(TargetParametersChanged);
	}

	void TargetParametersChanged(string a_newValue)
	{
		if (m_ignoreCallback)
			return;

		if(!int.TryParse(a_newValue, out int result) || result < 1)
		{
			m_ignoreCallback = true;
			m_targetParametersField.text = "1";
			m_ignoreCallback = false;			
		}
	}

	void ReassignAll()
	{
		int targetParams = int.Parse(m_targetParametersField.text);

		//Get list of players per role
		List<List<PlayerDetails>> playersPerRole = new List<List<PlayerDetails>>(SessionConfig.Data.roles.Count);
		int i = 0;
		foreach(RoleData role in SessionConfig.Data.roles)
		{
			playersPerRole.Add(new List<PlayerDetails>(4));
			foreach (var kvp in m_playerManager.m_playerList)
			{
				if (kvp.Value.m_role == role)
					playersPerRole[i].Add(kvp.Value);
			}
			i++;
		}

		Dictionary<string, List<string>> paramAssignment = new Dictionary<string, List<string>>();

		if (m_loopToMaxToggle.isOn)
		{
			for (i = 0; i < playersPerRole.Count; i++) //each role
			{
				int nextParam = 0;
				//for(int j = 0; j < targetParams; j++) //target amount of params
				//{
				//	foreach(PlayerDetails player in playersPerRole[i]) //to each player in role
				//	{
				//		if (paramAssignment.TryGetValue(player.m_name, out var assignment))
				//		{
				//			assignment.Add(SessionConfig.Data.roles[i].parameterNames[nextParam]);
				//		}
				//		else
				//		{
				//			paramAssignment.Add(player.m_name, new List<string>() { SessionConfig.Data.roles[i].parameterNames[nextParam] });
				//		}
				//		nextParam++;
				//		if (nextParam == SessionConfig.Data.roles[i].parameterNames.Count)
				//			nextParam = 0;
				//	}
				//}

				foreach (PlayerDetails player in playersPerRole[i]) //each player in role
				{
					int startingParam = nextParam;
					List<string> playerParams = new List<string>(targetParams);
					for (int j = 0; j < targetParams; j++) //assign target amount of params
					{
						playerParams.Add(SessionConfig.Data.roles[i].parameterNames[nextParam]);
						nextParam++;
						if (nextParam == SessionConfig.Data.roles[i].parameterNames.Count)
							nextParam = 0;
						if (nextParam == startingParam) //All params were assigned to this player
							break; 
					}
					paramAssignment.Add(player.m_name, playerParams);
				}
			}
		}
		else
		{
			for (i = 0; i < playersPerRole.Count; i++) //each role
			{
				if (playersPerRole[i].Count == 0)
					continue;
				int nextPlayer = 0;
				int loop = 0;
				foreach(string param in SessionConfig.Data.roles[i].parameterNames) //params in role
				{
					if(nextPlayer == playersPerRole[i].Count)
					{
						loop++;
						if (loop == targetParams) //up to target amount of params
							break;
						nextPlayer = 0;
					}
					if (paramAssignment.TryGetValue(playersPerRole[i][nextPlayer].m_name, out var assignment))
					{
						assignment.Add(param);
					}
					else
					{
						paramAssignment.Add(playersPerRole[i][nextPlayer].m_name, new List<string>() { param });
					}
					nextPlayer++;
				}
			}
		}

		PopupManager.Instance.CreatePopup("Parameters reassigned", "Parameter reassignment has been completed.", "Confirm", null);
		CloseWindow();
		HostMain.Instance.SetAssignment(paramAssignment);
	}

	void AssignRemaining()
	{
		int targetParams = int.Parse(m_targetParametersField.text);
		Dictionary<string, List<string>> paramAssignment = HostMain.Instance.m_paramAssignment;

		//Create overview of unassigned parameters
		HashSet<string> unasssignedParams = new HashSet<string>();
		foreach (ParameterData param in SessionConfig.Data.parameters)
			if(!param.general)
				unasssignedParams.Add(param.name);
		foreach(var kvp in HostMain.Instance.m_paramAssignment)
		{
			foreach (string param in kvp.Value)
				unasssignedParams.Remove(param);
		}

		if (unasssignedParams.Count == 0)
		{
			PopupManager.Instance.CreatePopup("All parameters assigned", "All parameters have been assigned", "Confirm", null);
			return;
		}

		//Sort players by remaining capacity
		List<PlayerAssignmentAmount> playerAssignmentAmount = new List<PlayerAssignmentAmount>(8);
		foreach(var kvp in m_playerManager.m_playerList)
		{
			if (paramAssignment.TryGetValue(kvp.Value.m_name, out var assignment))
			{
				if(assignment.Count < targetParams)
					playerAssignmentAmount.Add(new PlayerAssignmentAmount() { m_amount = assignment.Count, m_player = kvp.Value });
			}
			else
			{
				playerAssignmentAmount.Add(new PlayerAssignmentAmount() { m_amount = 0, m_player = kvp.Value });
			}
		}

		if (playerAssignmentAmount.Count == 0)
		{
			PopupManager.Instance.CreatePopup("No players with capacity", "There are no players that can be assigned more parameters. Increasing the parameter maximum per player might be necessary.", "Confirm", null);
			return;
		}

		playerAssignmentAmount.Sort();
		int nextPlayer = 0;
		while(true)
		{
			//Go to next player with lowest with lowest assignment and check if their role matches any unassigned parameters
			string paramToAssign = null;
			foreach (string param in playerAssignmentAmount[nextPlayer].m_player.m_role.parameterNames)
			{
				if (unasssignedParams.Contains(param))
				{
					paramToAssign = param;
					break;
				}
			}

			if (paramToAssign == null)
			{
				//None of the params can be assigned to this player, skip to next
				nextPlayer++;	
			}
			else
			{
				//Assign param to player and update lists
				PlayerAssignmentAmount player = playerAssignmentAmount[nextPlayer];
				playerAssignmentAmount.RemoveAt(nextPlayer);
				if (paramAssignment.TryGetValue(player.m_player.m_name, out var assignment))
				{
					assignment.Add(paramToAssign);
				}
				else
				{
					paramAssignment.Add(player.m_player.m_name, new List<string>() { paramToAssign });
				}
				unasssignedParams.Remove(paramToAssign);
				player.m_amount++;
				if(player.m_amount < targetParams)
					AddSorted(playerAssignmentAmount, player); //Readd player with new count, if they have more capacity
			}

			if (nextPlayer == playerAssignmentAmount.Count)
			{
				break; //We went through all players, unable to assign params
			}
		}

		CloseWindow();
		if(unasssignedParams.Count == 0)
			PopupManager.Instance.CreatePopup("All parameters assigned", "All parameters have been assigned", "Confirm", null);
		else
			PopupManager.Instance.CreatePopup("Assignment complete", $"{unasssignedParams.Count} parameters could not be assigned because there are no players with associated roles and remaining capacity. Increasing the parameter maximum per player might be necessary.", "Confirm", null);
		HostMain.Instance.SetAssignment(paramAssignment);
	}

	void AddSorted(List<PlayerAssignmentAmount> a_list, PlayerAssignmentAmount a_item)
	{
		if (a_list.Count == 0)
		{
			a_list.Add(a_item);
			return;
		}
		if (a_list[a_list.Count - 1].CompareTo(a_item) <= 0)
		{
			a_list.Add(a_item);
			return;
		}
		if (a_list[0].CompareTo(a_item) >= 0)
		{
			a_list.Insert(0, a_item);
			return;
		}
		int index = a_list.BinarySearch(a_item);
		if (index < 0)
			index = ~index;
		a_list.Insert(index, a_item);
	}

	public void OpenWindow()
	{
		gameObject.SetActive(true);
	}

	public void CloseWindow()
	{
		gameObject.SetActive(false);
	}
}

class PlayerAssignmentAmount : IComparable<PlayerAssignmentAmount>
{
	public PlayerDetails m_player;
	public int m_amount;

	public int CompareTo(PlayerAssignmentAmount other)
	{
		return m_amount.CompareTo(other);
	}
}
