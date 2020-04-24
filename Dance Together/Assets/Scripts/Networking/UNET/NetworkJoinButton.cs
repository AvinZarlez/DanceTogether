using UnityEngine;
using TMPro;

namespace App.Networking
{
    public class NetworkJoinButton : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;

        public LanConnectionInfo Info;

        public void Init(LanConnectionInfo _lanConnectionInfo)
        {
            text.text = (_lanConnectionInfo.Name);
            Info = _lanConnectionInfo;
        }

        public void Clicked()
        {
            NetworkController.s_Instance.JoinSpecificGame(new LanConnectionInfo(Info));
        }
    }
}
