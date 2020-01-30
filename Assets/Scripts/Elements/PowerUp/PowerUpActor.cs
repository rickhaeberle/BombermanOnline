using UnityEngine;
using UnityEngine.Networking;

public class PowerUpActor : NetworkBehaviour
{
    public GameObject FireUp;
    public GameObject FireDown;
    public GameObject SpeedUp;
    public GameObject SpeedDown;
    public GameObject BombUp;
    public GameObject BombDown;
    public GameObject Kick;
    public GameObject Remote;

    public AudioClip PowerUpTakenSfx;

    public PowerUpVisibility _visibility;

    public float TurnSpeed = 45f;

    private bool _mute = false;

    [SyncVar(hook = "ÖnBuffChanged")]
    public EPowerUp Buff = EPowerUp.None;

    private void Awake()
    {
        FireUp.SetActive(false);
        FireDown.SetActive(false);
        SpeedUp.SetActive(false);
        SpeedDown.SetActive(false);
        BombUp.SetActive(false);
        BombDown.SetActive(false);
        Kick.SetActive(false);
        Remote.SetActive(false);

        _visibility = GetComponent<PowerUpVisibility>();
    }

    private void Update()
    {
        transform.RotateAround(transform.position, transform.up, Time.deltaTime * TurnSpeed);
    }

    private void OnDestroy()
    {
        if (GameController.Instance.IsGameOver)
            return;

        if (!_visibility.IsElementVisible)
            return;

        if (_mute)
            return;

        SFXPlayerManager.Instance.Play(PowerUpTakenSfx, 0.55f);
    }

    public void SetBuff(EPowerUp buff)
    {
        if (Buff != EPowerUp.None)
            return;

        Buff = buff;
    }

    public void ÖnBuffChanged(EPowerUp buff)
    {
        Buff = buff;

        switch (buff)
        {
            case EPowerUp.FireUp: FireUp.SetActive(true); break;
            case EPowerUp.FireDown: FireDown.SetActive(true); break;
            case EPowerUp.SpeedUp: SpeedUp.SetActive(true); break;
            case EPowerUp.SpeedDown: SpeedDown.SetActive(true); break;
            case EPowerUp.BombUp: BombUp.SetActive(true); break;
            case EPowerUp.BombDown: BombDown.SetActive(true); break;
            case EPowerUp.Kick: Kick.SetActive(true); break;
            case EPowerUp.Remote: Remote.SetActive(true); break;
        }
    }

    public void Taken(bool mute)
    {
        _mute = mute;
        NetworkServer.Destroy(gameObject);
    }

    public void Destroyed()
    {
        _mute = true;
        NetworkServer.Destroy(gameObject);
    }
}
