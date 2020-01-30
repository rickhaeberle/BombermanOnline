using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RampVisibility : MonoBehaviour
{
    public bool IsElementVisible { get; private set; }

    public Ground UpGround;
    public Ground DownGround;

    private RampActor _actor;
    private List<Renderer> _renderers;

    private int _upFloor;
    private int _downFloor;

    private void Start()
    {
        _actor = GetComponent<RampActor>();
        _renderers = GetComponentsInChildren<Renderer>().ToList();

        _upFloor = Mathf.FloorToInt(_actor.UpFloor.transform.position.y);
        _downFloor = Mathf.FloorToInt(_actor.DownFloor.transform.position.y);

        GameManager.Instance.OnFloorChanged += CheckVisibility;

        CheckVisibility(0);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnFloorChanged -= CheckVisibility;
    }

    private void CheckVisibility(int floor)
    {
        IsElementVisible = _upFloor == floor || _downFloor == floor;
        _renderers.ForEach(item => { item.enabled = IsElementVisible; });

        UpGround.SetVisibility(IsElementVisible);
        DownGround.SetVisibility(IsElementVisible);
    }
}

