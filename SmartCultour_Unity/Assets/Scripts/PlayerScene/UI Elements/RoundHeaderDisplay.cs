using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundHeaderDisplay : MonoBehaviour
{
	[SerializeField]
	TextMeshProUGUI m_roundText;

	public void SetRound(int a_round)
	{
		m_roundText.text = "Round " + a_round.ToString();
	}
}

