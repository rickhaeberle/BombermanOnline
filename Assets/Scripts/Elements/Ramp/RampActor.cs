using UnityEngine;

public class RampActor : MonoBehaviour
{
    public Floor UpFloor;
    public Floor DownFloor;

    public void GoToUpFloor()
    {
        GameController.Instance.GoToFloor(UpFloor);
    }

    public void GoToDownFloor()
    {
        GameController.Instance.GoToFloor(DownFloor);
    }

}
