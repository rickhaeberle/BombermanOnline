using UnityEngine;

public class ElementVisibility : MonoBehaviour
{

    public bool IsElementVisible { get; private set; }

    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _renderer.enabled = false;
        IsElementVisible = false;

        GameManager.Instance.OnFloorChanged += CheckVisibility;
    }

    private void Start()
    {
        CheckVisibility(0);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnFloorChanged -= CheckVisibility;
    }

    private void CheckVisibility(int floor)
    {
        bool visible = false;

        var position = transform.position;
        position.y = Mathf.FloorToInt(transform.position.y);

        var hits = Physics.OverlapSphere(position, 0.3f);
        foreach (var item in hits)
        {
            var ramp = item.GetComponent<RampHitbox>();
            if (ramp != null)
            {
                visible = ramp.Actor.GetComponentInChildren<RampVisibility>().IsElementVisible;
                break;
            }

            var ground = item.GetComponent<Ground>();
            if (ground != null)
            {
                visible = ground.GetComponentInChildren<Renderer>().enabled;
            }
        }

        IsElementVisible = visible;
        _renderer.enabled = visible;
    }
}
