using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
 
public class SafeArea : MonoBehaviour
{
	void Awake()
	{
		RectTransform rect = GetComponent<RectTransform>();
		Canvas canvas = GetComponentInParent<Canvas>();

		Rect safeArea = Screen.safeArea;

		Vector2 anchorMin = safeArea.position;
		Vector2 anchorMax = safeArea.position + safeArea.size;
		anchorMin.x /= canvas.pixelRect.width;
		anchorMin.y /= canvas.pixelRect.height;
		anchorMax.x /= canvas.pixelRect.width;
		anchorMax.y /= canvas.pixelRect.height;

		rect.anchorMin = anchorMin;
		rect.anchorMax = anchorMax;
	}
}