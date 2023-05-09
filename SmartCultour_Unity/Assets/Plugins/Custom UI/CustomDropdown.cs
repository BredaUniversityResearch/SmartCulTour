using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomDropdown : TMP_Dropdown
{
    public delegate void InteractabilityChangeCallback(bool newState);
    public InteractabilityChangeCallback interactabilityChangeCallback;
    [SerializeField] private Sprite m_normalSprite;
    [SerializeField] private Sprite m_expandSprite;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        if (interactabilityChangeCallback != null)
            interactabilityChangeCallback(state != SelectionState.Disabled);
    }

    protected override GameObject CreateBlocker(Canvas rootCanvas)
    {
		if(targetGraphic != null && m_expandSprite != null)
			((Image) targetGraphic).sprite = m_expandSprite;

		// Create blocker GameObject.
		GameObject blocker = base.CreateBlocker(rootCanvas);

        LayoutElement layout = blocker.AddComponent<LayoutElement>();
        layout.ignoreLayout = true;

        return blocker;
    }

  //  protected override GameObject CreateDropdownList(GameObject template)
  //  {
	 //   RectTransform result = base.CreateDropdownList(template).GetComponent<RectTransform>();
		//result.sizeDelta = new Vector2(result.sizeDelta.x, options.Count * 110f);
		//return result.gameObject;

  //  }
	
    protected override void DestroyBlocker(GameObject blocker)
    {
	    if (targetGraphic != null && m_normalSprite != null)
		    ((Image)targetGraphic).sprite = m_normalSprite;
		base.DestroyBlocker(blocker);
    }
}
