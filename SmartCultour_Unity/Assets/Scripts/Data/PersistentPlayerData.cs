using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentPlayerData : MonoBehaviour
{
	private static PersistentPlayerData m_instance;
	public static PersistentPlayerData Instance => m_instance;

	public RoleData m_role;
	public string m_nickName;

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
}
