using UnityEngine;
using TMPro;

namespace App.Networking
{
    public class NetworkJoinButton : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;

        private NetworkController controller;

        public LanConnectionInfo Info;

        public void Init(LanConnectionInfo _lanConnectionInfo, NetworkController _controller)
        {
            controller = _controller;
            text.text = (_lanConnectionInfo.Name);
            Info = _lanConnectionInfo;
        }

        public void Clicked()
        {
            controller.JoinSpecificGame(new LanConnectionInfo(Info));
        }
    }
}
