using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace App.Networking
{
    [System.Serializable]
    public struct LanConnectionInfo
    {
        public string IpAddress;
        public int Port;
        public string Name;

        public LanConnectionInfo(string _fromAddress, string _data)
        {
            IpAddress = _fromAddress.Substring(_fromAddress.LastIndexOf(":") + 1, _fromAddress.Length - (_fromAddress.LastIndexOf(":") + 1));
            string portText = _data.Substring(_data.LastIndexOf(":") + 1, _data.Length - (_data.LastIndexOf(":") + 1));
            Port = 7777;
            int.TryParse(portText, out Port);
            Name = "Default"; // TODO add game names in future?

        }

        public LanConnectionInfo(LanConnectionInfo _connection)
        {
            this.IpAddress = _connection.IpAddress;
            this.Port = _connection.Port;
            this.Name = _connection.Name;
        }
    }
    public class DanceTogetherNetworkDiscovery : NetworkDiscovery
    {
        [SerializeField, Tooltip("How long a game discovery must be innactive to be removed from Lan Adresses")]
        private float refreshTimeout = 3f;

        private Dictionary<LanConnectionInfo, float> lanAdresses = new Dictionary<LanConnectionInfo, float>();

        private NetworkController controller;

        public List<LanConnectionInfo> LanAdresses
        {
            get { return lanAdresses.Keys.ToList(); }
        }

        public void Init(NetworkController _controller)
        {
            controller = _controller;

            if (controller == null)
                Debug.LogWarning("Network Controller for NetworkDiscovery was assigned null. Please assign NetworkDiscovery to NetworkController.");

            //StartClientBroadcast();
            StartCoroutine(CleanUpExpiredEntries());
        }

        public void StartServerBroadcast()
        {
            if (running)
                StopBroadcast();

            base.Initialize();
            base.StartAsServer();
        }
        public void StartClientBroadcast()
        {
            if (running)
                StopBroadcast();

            base.Initialize();
            base.StartAsClient();
        }

        private IEnumerator CleanUpExpiredEntries()
        {
            while (true)
            {
                bool changed = false;
                var keys = lanAdresses.Keys.ToList();

                foreach (var key in keys)
                {
                    if (lanAdresses[key] <= Time.time)
                    {
                        lanAdresses.Remove(key);
                        changed = true;
                        Debug.Log("broadcast removed");
                    }
                }
                if (changed)
                    UpdateMatchInfos();

                yield return new WaitForSeconds(refreshTimeout);
            }
        }

        public override void OnReceivedBroadcast(string fromAddress, string data)
        {
            base.OnReceivedBroadcast(fromAddress, data);
            LanConnectionInfo info = new LanConnectionInfo(fromAddress, data);

            if (lanAdresses.ContainsKey(info) == false)
            {
                lanAdresses.Add(info, Time.time + refreshTimeout);
                UpdateMatchInfos();
            }
            else
            {
                lanAdresses[info] = Time.time + refreshTimeout;
            }
        }

        public void UpdateMatchInfos()
        {
            controller?.GameListController?.RefreshGamesList(LanAdresses);

            if (!controller)
                Debug.Log("Discovery server has not been set up with Init, or Controller is null.");
        }

        private void OnDisable()
        {
            if (running)
            {
                StopBroadcast();
            }
        }

        public void Reset()
        {
            if (running)
                StopBroadcast();
        }

    }
}