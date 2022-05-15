using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// FPS - helper class for tracking frames per second 
public static class FPS
{
	public static List<float> m_FPSHist = new List<float>();
	public static float m_FPS = float.MaxValue;
	public static float m_FPSMin = float.MaxValue;
	public static float m_FPSTotal;
	public static int m_FPSLastSample = -1;
	public static int m_DelayFrames;

	public static float GetFPS()
	{
		if (m_DelayFrames > 0)
			m_DelayFrames--;

		if ( Time.time > 1.0f && m_DelayFrames <= 0 )
		{
			if (m_FPSLastSample != Time.frameCount)
			{
				m_FPSLastSample = Time.frameCount;

				if (m_FPSHist.Count == 20)
				{
					m_FPSTotal -= m_FPSHist[0];
					m_FPSHist.RemoveAt(0);
				}

				float deltaTime = Time.smoothDeltaTime;
				if (deltaTime == 0.0f)
					deltaTime = 0.01f;
				float fps = Mathf.Round(1.0f / deltaTime);
				m_FPSHist.Add(fps);
				m_FPSTotal += fps;
				m_FPS = Mathf.Floor(m_FPSTotal / m_FPSHist.Count);

				m_FPSMin = Mathf.Min(m_FPS, m_FPSMin);
			}
		}

		return m_FPS;
	}
}

public class UIText : MonoBehaviour
{
	protected TMP_Text m_Text;
	Color m_Color;

	public Type m_Type;

	public enum Type
	{ 
		FPS,
	}

	private void Start()
	{
		m_Text = GetComponentInChildren<TMP_Text>();
		if (m_Text != null)
			m_Color = m_Text.color;
	}

	string GetFPSText()
	{
		string result = "";

		float fps = FPS.GetFPS();

		if (fps == float.MaxValue)
			return "";

		result = FPSColor( fps ) + "FPS: " + fps + "\n";

		return result;
	}

	static string FPSColor( float fps )
	{
		if ( fps < 40 )
			return "<color=\"red\">";
		if ( fps < 50 )
			return "<color=\"orange\">";
		if ( fps < 60 )
			return "<color=\"yellow\">";
		return "<color=\"white\">";
	}

	void Update()
    {
		if (m_Text != null)
		{
			m_Text.text = GetFPSText();
		}
	}

}
