using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContentTabInterventions : AContentTab
{
	[SerializeField]
	Transform m_contentLocation;
	[SerializeField]
	GameObject m_roundHeaderPrefab, m_interventionDisplayPrefab, m_noInterventionsObject;
	[SerializeField]
	InterventionDetailsWindow m_interventionDetailsWindow;

	int m_currentRound = -1;
	int m_nextInterventionInCurrentRound = 0;
	List<InterventionDisplay> m_interventionDisplays = new List<InterventionDisplay>();
	
	public void UpdateInterventions(List<List<InterventionData>> a_interventions)
	{
		if(m_currentRound == -1)
		{
			AddHeaderForRound(1);
			m_currentRound = 0;
			m_noInterventionsObject.SetActive(true);
			m_noInterventionsObject.transform.SetAsLastSibling();
		}

		if (a_interventions == null)
			return;

		//Update existing interventions
		int index = 0;
		foreach(List<InterventionData> round in a_interventions)
		{
			foreach(InterventionData intervention in round)
			{
				if (index >= m_interventionDisplays.Count)
					break;
				m_interventionDisplays[index].UpdateIntervention(intervention);
				index++;
			}
		}

		//Add potential new interventions
		for(int i = m_currentRound; i < a_interventions.Count; i++)
		{
			if(i > m_currentRound)
			{
				AddHeaderForRound(i+1);
				if (a_interventions[i].Count == 0)
				{
					m_noInterventionsObject.SetActive(true);
					m_noInterventionsObject.transform.SetAsLastSibling();
				}
				m_nextInterventionInCurrentRound = 0;
			}

			m_currentRound = i;

			for(int j = m_nextInterventionInCurrentRound; j < a_interventions[m_currentRound].Count; j++)
			{
				if(j == 0)
					m_noInterventionsObject.SetActive(false);

				AddIntervention(a_interventions[m_currentRound][j]);
				m_nextInterventionInCurrentRound = j+1;
			}
		}
	}

	void AddHeaderForRound(int a_round)
	{
		RoundHeaderDisplay newEntry = GameObject.Instantiate(m_roundHeaderPrefab, m_contentLocation).GetComponent<RoundHeaderDisplay>();
		newEntry.SetRound(a_round);
	}

	void AddIntervention(InterventionData a_intervention)
	{
		InterventionDisplay newEntry = GameObject.Instantiate(m_interventionDisplayPrefab, m_contentLocation).GetComponent<InterventionDisplay>();
		m_interventionDisplays.Add(newEntry);
		newEntry.SetIntervention(a_intervention, OpenInterventionDetails);
	}

	void OpenInterventionDetails(InterventionData a_intervention, InterventionDisplay a_display)
	{
		m_interventionDetailsWindow.SetToIntervention(a_intervention, a_display);
	}

	protected override void OnTabClose()
	{
		m_interventionDetailsWindow.CloseWindow();
	}
}

