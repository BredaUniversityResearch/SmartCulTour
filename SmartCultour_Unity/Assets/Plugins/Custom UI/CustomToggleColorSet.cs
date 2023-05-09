using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using ColourPalette;

[RequireComponent(typeof(CustomToggle))]
public class CustomToggleColorSet : SerializedMonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public List<Graphic> targetGraphics;
	public IColourContainer colorNormal;
	public IColourContainer colorSelected;
	public enum HightlightCriteria { No, Yes, Only }

	public bool useHighlightColor;
	[ShowIf("useHighlightColor")]
	public HightlightCriteria highlightIfOn;
	[ShowIf("useHighlightColor")]
	public IColourContainer colorHighlight;
	public bool useDisabledColor;
	[ShowIf("useDisabledColor")]
	public IColourContainer colorDisabled;
	[ShowIf("useDisabledColor")]
	public IColourContainer colorDisabledSelected;

	CustomToggle toggle;
	bool pointerOnToggle = false;
	bool isDisabled = false;


	void Start()
	{
		toggle = GetComponent<CustomToggle>();
		SubscribeToAssetChange();
		if (!toggle.interactable)
			SetDisabled(true);
		if (!UseDisabledColor)
			SetGraphicSetToColor(toggle.isOn ? colorSelected : colorNormal);
		toggle.onValueChanged.AddListener(ValueChanged);
        toggle.interactabilityChangeCallback += HandleInteractabilityChange;
    }

    private void OnDestroy()
    {
        if (toggle != null)
            toggle.interactabilityChangeCallback -= HandleInteractabilityChange;
        UnSubscribeFromAssetChange();
    }

	private void OnDisable()
	{
		pointerOnToggle = false;
	}

	private void ValueChanged(bool a_value)
	{
		if (!UseDisabledColor)
		{
			if (useHighlightColor && Highlight && pointerOnToggle)
				SetGraphicsToHighlight();
			else
				SetGraphicsToNormal();
		}
	}

	private void HandleInteractabilityChange(bool newState)
    {
        if (!newState && !isDisabled)
        {
            SetDisabled(true);
        }
        else if (newState && isDisabled)
        {
            SetDisabled(false);
        }
    }

    private void SetDisabled(bool value)
    {
        if (isDisabled && !value)
        {
            //No longer disabled, set to previous color
            if (pointerOnToggle && useHighlightColor && Highlight)
                SetGraphicsToHighlight();
            else
                SetGraphicsToNormal();
        }
        isDisabled = value;
        //Disabled and color change required
        if (UseDisabledColor)
            SetGraphicsToDisabled();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerOnToggle = true;
        if (useHighlightColor && Highlight && toggle.interactable)
            SetGraphicsToHighlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerOnToggle = false;
        if (useHighlightColor && !UseDisabledColor)
            SetGraphicsToNormal();
    }

    void SetGraphicsToNormal()
    {
        SetGraphicSetToColor(toggle.isOn ? colorSelected : colorNormal);
    }

    void SetGraphicsToHighlight()
    {
        SetGraphicSetToColor(colorHighlight);
    }

    void SetGraphicsToDisabled()
    {
        SetGraphicSetToColor(toggle.isOn ? colorDisabledSelected : colorDisabled);
    }

    void SetGraphicSetToColor(IColourContainer colorAsset)
    {
        foreach (Graphic g in targetGraphics)
            g.color = colorAsset.GetColour();
    }

    void SubscribeToAssetChange()
    {
        if (Application.isPlaying)
        {
            colorNormal?.SubscribeToChanges(OnNormalColourAssetChanged);
            if (useHighlightColor)
                colorHighlight?.SubscribeToChanges(OnHighlightColourAssetChanged);
			if (useDisabledColor)
			{
				colorDisabled?.SubscribeToChanges(OnDisabledColourAssetChanged);
				colorDisabledSelected?.SubscribeToChanges(OnDisabledColourAssetChanged);
			}
        }
    }

    void UnSubscribeFromAssetChange()
    {
        if (Application.isPlaying)
        {
            colorNormal?.UnSubscribeFromChanges(OnNormalColourAssetChanged);
            if (useHighlightColor)
                colorHighlight?.UnSubscribeFromChanges(OnHighlightColourAssetChanged);
			if (useDisabledColor)
			{
				colorDisabled?.UnSubscribeFromChanges(OnDisabledColourAssetChanged);
				colorDisabledSelected?.UnSubscribeFromChanges(OnDisabledColourAssetChanged);
			}
        }
    }

    void OnNormalColourAssetChanged(Color newColour)
    {
        if (!UseDisabledColor && !(useHighlightColor && pointerOnToggle) && !toggle.isOn)
            SetGraphicsToNormal();
    }

    void OnSelectedColourAssetChanged(Color newColour)
    {
        if (!UseDisabledColor && (useHighlightColor || highlightIfOn > 0) && toggle.isOn)
            SetGraphicsToNormal();
    }

    void OnHighlightColourAssetChanged(Color newColour)
    {
        if (useHighlightColor && pointerOnToggle && Highlight && !UseDisabledColor)
            SetGraphicsToHighlight();
    }

    void OnDisabledColourAssetChanged(Color newColour)
    {
        if (UseDisabledColor)
            SetGraphicsToDisabled();
    }

	bool Highlight
	{
		get
		{
			switch(highlightIfOn)
			{
				case HightlightCriteria.No:
					return !toggle.isOn;
				case HightlightCriteria.Only:
					return toggle.isOn;
			}
			return true;
		}
	}

	bool UseDisabledColor
    {
        get
        {
            return isDisabled && useDisabledColor;
        }
    }
}