using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using App.Networking;


// simple container to control a lobby image.
public class LobbyPlayerIcon : MonoBehaviour
{
    [SerializeField]
    private Image IconImage;
    [SerializeField]
    private Image CheckMarkImage;
    [SerializeField]
    private TextMeshProUGUI PlayerNumber;

    private DanceTogetherPlayer ownedPlayer;

    // call this on creation.
    public void Initialize(Color _color, int _number)
    {
        UpdateColor(_color);
        UpdateNumber(_number);
    }

    public void Init(DanceTogetherPlayer _player)
    {
        this.ownedPlayer = _player;
        if (_player != null)
        {
            _player.syncVarsChangedEvent += OnNetworkPlayerSyncvarChanged;
        }

        UpdateValues();
    }

    public void UpdateColor(Color _color)
    {
        IconImage.color = _color;
    }
    public void UpdateNumber(int _value)
    {
        PlayerNumber.text = _value.ToString();
    }

    public void IsReady(bool _bool)
    {
        // simply show and hide the image associated.
        CheckMarkImage.enabled = _bool;
    }

    public void UpdateAll(Color _color, int _number, bool _isReady)
    {
        IconImage.color = _color;
        PlayerNumber.text = _number.ToString();
        CheckMarkImage.enabled = _isReady;
    }


    private void OnNetworkPlayerSyncvarChanged(DanceTogetherPlayer _player)
    {
        // Update everything
        UpdateValues();
    }

    private void UpdateValues()
    {
        UpdateAll(ownedPlayer.playerColor.Color, ownedPlayer.PlayerID, ownedPlayer.IsReady);
    }

    private void OnDestroy()
    {
        ownedPlayer.syncVarsChangedEvent -= OnNetworkPlayerSyncvarChanged;
    }
}
