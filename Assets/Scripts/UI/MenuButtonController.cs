using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonController : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioClip MouseOverSfx;
    public AudioClip MouseClickSfx;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (MouseOverSfx == null)
            return;

        SFXPlayerManager.Instance.Play(MouseOverSfx, 0.5f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (MouseClickSfx == null)
            return;

        SFXPlayerManager.Instance.Play(MouseClickSfx, 0.3f);
    }
}
