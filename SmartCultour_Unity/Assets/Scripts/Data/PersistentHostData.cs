using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;

public class PersistentHostData : MonoBehaviour
{
	private static PersistentHostData m_instance;
	public static PersistentHostData Instance => m_instance;

	public List<EventData> m_cachedEvents;
	public SessionStateData m_loadedState;
    public Dictionary<string, List<string>> m_paramAssignment;

    void Awake()
	{
		if (m_instance != null)
		{
			Destroy(gameObject);
			return;
		}

		m_instance = this;
		DontDestroyOnLoad(this);
	}

	public static void ClearData()
	{
		m_instance.m_cachedEvents = new List<EventData>();
		m_instance.m_loadedState = null;
        m_instance.m_paramAssignment = null;
    }
}
