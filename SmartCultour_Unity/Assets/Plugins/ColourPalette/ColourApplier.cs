using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ColourPalette
{
    public class ColourApplier : MonoBehaviour
    {
        [SerializeField]
        ColourAsset colourAsset;

        [SerializeField]
        List<Graphic> graphics;

        public ColourAsset ColourAsset
        {
            get { return colourAsset; }
            set
            {
                UnSubscribeFromAssetChange();
                colourAsset = value;
                SubscribeToAssetChange();
            }
        }

        [SerializeField]
        UnityEvent<Color> colourChanged;

        void Start()
        {
            SubscribeToAssetChange();
        }

        void OnDestroy()
        {
            UnSubscribeFromAssetChange();
        }

        void OnColourAssetChanged(Color newColour)
        {
            colourChanged?.Invoke(newColour);
            if (graphics != null)
                foreach (Graphic g in graphics)
                    g.color = newColour;
        }

        void SubscribeToAssetChange()
        {
            if (colourAsset != null)
            {
                colourAsset.valueChangedEvent.AddListener(OnColourAssetChanged);
                OnColourAssetChanged(colourAsset.GetColour());
            }
        }

        void UnSubscribeFromAssetChange()
        {
            if (colourAsset != null)
            {
                colourAsset.valueChangedEvent.RemoveListener(OnColourAssetChanged);
            }
        }
    }
}
