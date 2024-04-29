using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseButton : NetworkBehaviour
{
    [SerializeField] protected Button button;

    protected virtual void Start()
    {
        AddOnOnClick();
    }

    protected virtual void AddOnOnClick()
    {
        this.button.onClick.AddListener(this.OnClick);
    }

    protected abstract void OnClick();
}
