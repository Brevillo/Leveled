using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.ExecuteEvents;

public class EventTriggerExecutor : EventTrigger
{
    public bool passEventForward;
    
    private bool executing;
    
    private void ExecuteHierarchy<T>(BaseEventData eventData, EventFunction<T> function)
        where T : IEventSystemHandler
    {
        if (executing) return;
        
        executing = true;
        
        if (passEventForward)
        {
            ExecuteEvents.ExecuteHierarchy(gameObject, eventData, function);
        }

        executing = false;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        ExecuteHierarchy(eventData, pointerEnterHandler);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        ExecuteHierarchy(eventData, pointerExitHandler);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        ExecuteHierarchy(eventData, dragHandler);
    }

    public override void OnDrop(PointerEventData eventData)
    {
        base.OnDrop(eventData);
        ExecuteHierarchy(eventData, dropHandler);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        ExecuteHierarchy(eventData, pointerDownHandler);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        ExecuteHierarchy(eventData, pointerUpHandler);
    }
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        ExecuteHierarchy(eventData, pointerClickHandler);
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        ExecuteHierarchy(eventData, selectHandler);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        ExecuteHierarchy(eventData, deselectHandler);
    }

    public override void OnScroll(PointerEventData eventData)
    {
        base.OnScroll(eventData);
        ExecuteHierarchy(eventData, scrollHandler);
    }

    public override void OnMove(AxisEventData eventData)
    {
        base.OnMove(eventData);
        ExecuteHierarchy(eventData, moveHandler);
    }

    public override void OnUpdateSelected(BaseEventData eventData)
    {
        base.OnUpdateSelected(eventData);
        ExecuteHierarchy(eventData, updateSelectedHandler);
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        base.OnInitializePotentialDrag(eventData);
        ExecuteHierarchy(eventData, initializePotentialDrag);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        ExecuteHierarchy(eventData, beginDragHandler);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        ExecuteHierarchy(eventData, endDragHandler);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        base.OnSubmit(eventData);
        ExecuteHierarchy(eventData, submitHandler);
    }

    public override void OnCancel(BaseEventData eventData)
    {
        base.OnCancel(eventData);
        ExecuteHierarchy(eventData, cancelHandler);
    }
}
