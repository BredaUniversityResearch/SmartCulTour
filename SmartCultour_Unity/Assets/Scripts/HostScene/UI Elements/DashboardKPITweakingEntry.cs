using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MathNet.Numerics.Distributions;


public class DashboardKPITweakingEntry : MonoBehaviour
{
	[SerializeField] DashboardKPIEntry m_entry;
	[SerializeField] Slider m_newValueSlider;
	[SerializeField] Slider m_oldValueSlider;
	//[SerializeField] CustomInputField m_newValueAbsText;
	//[SerializeField] CustomInputField m_oldValueAbsText;
	[SerializeField] CustomInputField m_newValueNormText;
	[SerializeField] CustomInputField m_oldValueNormText;
	[SerializeField] CustomInputField m_stdevText;

	Normal m_normalDistr;
	bool m_ignoreCallback;

	float m_newAbs, m_oldAbs, m_newNorm, m_oldNorm;

	void Start()
    {
		//m_newValueAbsText.onSubmit.AddListener(OnNewValueAbsChanged);
		//m_oldValueAbsText.onSubmit.AddListener(OnOldValueAbsChanged);
		m_newValueNormText.onEndEdit.AddListener(OnNewValueNormChanged);
		m_oldValueNormText.onEndEdit.AddListener(OnOldValueNormChanged);
		m_stdevText.onEndEdit.AddListener(OnStdevChanged);
		m_newValueSlider.onValueChanged.AddListener(OnNewValueSliderChange);
		m_oldValueSlider.onValueChanged.AddListener(OnOldValueSliderChange);
	}

	public void SetToKPI(KPIData a_kpi)
	{
		m_entry.SetKPI(a_kpi);
	}

	public void SetToValue(KPIValue a_value)
	{
		m_ignoreCallback = true;
		m_normalDistr = new Normal(0, (double)a_value.stdev);
		m_stdevText.text = a_value.stdev.ToString();
		m_ignoreCallback = false;
		m_newAbs = a_value.currentValue;
		m_oldAbs = a_value.oldValue;
		UpdateNewValueDisplays(false);
		UpdateOldValueDisplays();
	}

	void UpdateNewValueDisplays(bool a_updateBars = true)
	{
		m_ignoreCallback = true;
		//m_newValueAbsText.text = m_newAbs.ToString("N2");
		m_newNorm = (float)m_normalDistr.CumulativeDistribution(m_newAbs);
		m_newValueSlider.value = m_newNorm;
		m_newValueNormText.text = m_newNorm.ToString("N2");
		m_ignoreCallback = false;
		if (a_updateBars)
			m_entry.SetNormalizedValue(m_newNorm, m_oldNorm);
	}

	void UpdateOldValueDisplays(bool a_updateBars = true)
	{
		m_ignoreCallback = true;
		//m_oldValueAbsText.text = m_oldAbs.ToString("N2");
		m_oldNorm = (float)m_normalDistr.CumulativeDistribution(m_oldAbs);
		m_oldValueSlider.value = m_oldNorm;
		m_oldValueNormText.text = m_oldNorm.ToString("N2");
		m_ignoreCallback = false;
		if (a_updateBars)
			m_entry.SetNormalizedValue(m_newNorm, m_oldNorm);
	}

	void OnNewValueSliderChange(float a_newValue)
	{
		if (m_ignoreCallback)
			return;
		m_newAbs = (float)m_normalDistr.InverseCumulativeDistribution(a_newValue);
		UpdateNewValueDisplays();
	}

	void OnOldValueSliderChange(float a_newValue)
	{
		if (m_ignoreCallback)
			return;
		m_oldAbs = (float)m_normalDistr.InverseCumulativeDistribution(a_newValue);
		UpdateOldValueDisplays();
	}

	void OnNewValueAbsChanged(string a_newValue)
	{
		if (m_ignoreCallback)
			return;
		if (float.TryParse(a_newValue, out float result))
		{
			m_newAbs = result;
		}
		else
		{
			m_newAbs = 0;
		}
		UpdateNewValueDisplays();
	}

	void OnOldValueAbsChanged(string a_newValue)
	{
		if (m_ignoreCallback)
			return;
		if (float.TryParse(a_newValue, out float result))
		{
			m_oldAbs = result;
		}
		else
		{
			m_oldAbs = 0;
		}
		UpdateOldValueDisplays();
	}

	void OnNewValueNormChanged(string a_newValue)
	{
		if (m_ignoreCallback)
			return;
		if (float.TryParse(a_newValue, out float result))
		{
			if (result < 0.05f)
			{
				m_ignoreCallback = true;
				m_newValueNormText.text = "0.05";
				m_ignoreCallback = false;
				result = 0.05f;
			}
			else if(result > 0.95f)
			{
				m_ignoreCallback = true;
				m_newValueNormText.text = "0.95";
				m_ignoreCallback = false;
				result = 0.95f;
			}
			m_newAbs = (float)m_normalDistr.InverseCumulativeDistribution(result);
		}
		else
		{
			m_newAbs = 0.5f;
		}
		UpdateNewValueDisplays();
	}

	void OnOldValueNormChanged(string a_newValue)
	{
		if (m_ignoreCallback)
			return;
		if (float.TryParse(a_newValue, out float result))
		{
			if (result < 0.05f)
			{
				m_ignoreCallback = true;
				m_oldValueNormText.text = "0.05";
				m_ignoreCallback = false;
				result = 0.05f;
			}
			else if (result > 0.95f)
			{
				m_ignoreCallback = true;
				m_oldValueNormText.text = "0.95";
				m_ignoreCallback = false;
				result = 0.95f;
			}
			m_oldAbs = (float)m_normalDistr.InverseCumulativeDistribution(result);
		}
		else
		{
			m_oldAbs = 0;
		}
		UpdateOldValueDisplays();
	}

	void OnStdevChanged(string a_newValue)
	{
		if (m_ignoreCallback)
			return;
		if (float.TryParse(a_newValue, out float result))
		{
			if (result < 0.5f)
			{
				m_normalDistr = new Normal(0, 10d);
				m_ignoreCallback = true;
				m_stdevText.text = "10";
				m_ignoreCallback = false;
			}
			else
				m_normalDistr = new Normal(0, result);
		}
		else
		{
			m_normalDistr = new Normal(0, 10d);
			m_ignoreCallback = true;
			m_stdevText.text = "10";
			m_ignoreCallback = false;
		}
		UpdateNewValueDisplays(false);
		UpdateOldValueDisplays();
	}

	public KPIValue GetResult()
	{
		return new KPIValue(m_newAbs, m_oldAbs, (float)m_normalDistr.StdDev);
	}
}
