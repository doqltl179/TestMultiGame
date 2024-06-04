using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(EventTrigger))]
public class AnimationButton : MonoBehaviour {
    protected RectTransform rectTransform;
    protected Button button;

    public bool IsSelected {
        get => isSelected;
        set {
            if(isSelected != value) {


                isSelected = value;
            }
        }
    }
    private bool isSelected = false;

    public bool ButtonEnabled {
        get => button.enabled;
        set {
            if(button.enabled != value) {
                OnButtonEnabledChanged(value);

                button.enabled = value;
            }
        }
    }



#if UNITY_EDITOR
    private void Reset() {
        Button btn = GetComponent<Button>();
        if(btn != null) {
            btn.transition = Selectable.Transition.None;

            Navigation nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;

            Button.ButtonClickedEvent onClickEvent = new Button.ButtonClickedEvent();
            UnityEventTools.AddPersistentListener(onClickEvent, OnClick);
            btn.onClick = onClickEvent;
        }

        EventTrigger et = GetComponent<EventTrigger>();
        if(et != null) {
            et.triggers.Clear();

            AddTriggerEvent(et, EventTriggerType.PointerEnter, OnPointerEnter);
            AddTriggerEvent(et, EventTriggerType.PointerExit, OnPointerExit);
            AddTriggerEvent(et, EventTriggerType.PointerClick, OnPointerClick);
            //AddTriggerEvent(et, EventTriggerType.Select, OnSelect);
            //AddTriggerEvent(et, EventTriggerType.Deselect, OnDeselect);
        }
    }

    private void AddTriggerEvent(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> eventAction) {
        EventTrigger.Entry entry = new EventTrigger.Entry() { eventID = eventType };
        UnityEventTools.AddPersistentListener(entry.callback, eventAction);
        trigger.triggers.Add(entry);
    }
#endif

    protected virtual void Awake() {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
    }

    #region Action
    public virtual void OnButtonEnabledChanged(bool value) { Debug.Log($"{nameof(AnimationButton)}. OnButtonEnabledChanged."); }

    public virtual void OnClick() { Debug.Log($"{nameof(AnimationButton)}. OnClick."); }

    public virtual void OnPointerEnter(BaseEventData eventData) { 
        Debug.Log($"{nameof(AnimationButton)}. OnPointerEnter.");

        PointerEnterAnimation((PointerEventData)eventData);
    }
    public virtual void OnPointerExit(BaseEventData eventData) { 
        Debug.Log($"{nameof(AnimationButton)}. OnPointerExit.");

        PointerExitAnimation((PointerEventData)eventData);
    }

    public virtual void OnPointerClick(BaseEventData eventData) { 
        Debug.Log($"{nameof(AnimationButton)}. OnPointerClick.");

        PointerClickAnimation((PointerEventData)eventData);
    }

    public virtual void OnSelect(BaseEventData eventData) { 
        Debug.Log($"{nameof(AnimationButton)}. OnSelect.");

        SelectAnimation(eventData);
    }
    public virtual void OnDeselect(BaseEventData eventData) { 
        Debug.Log($"{nameof(AnimationButton)}. OnDeselect.");

        DeselectAnimation(eventData);
    }
    #endregion

    protected virtual void PointerEnterAnimation(PointerEventData data) { }
    protected virtual void PointerExitAnimation(PointerEventData data) { }

    protected virtual void PointerClickAnimation(PointerEventData data) { }

    protected virtual void SelectAnimation(BaseEventData data) { }
    protected virtual void DeselectAnimation(BaseEventData data) { }
}
