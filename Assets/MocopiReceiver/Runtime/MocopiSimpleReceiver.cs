/*
* Copyright (C) 2025 Sony Corporation
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using Mocopi.Receiver.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Mocopi.Receiver
{
    /// <summary>
    /// The simple component for receiving and adapting motion from UDP
    /// </summary>
    public sealed class MocopiSimpleReceiver : MonoBehaviour
    {
        #region --Fields--
        /// <summary>
        /// Avatar list
        /// </summary>
        public List<MocopiSimpleReceiverAvatarSettings> AvatarSettings = new List<MocopiSimpleReceiverAvatarSettings>();

        /// <summary>
        /// Switching variable for UDP reception start timing
        /// </summary>
        public bool IsReceivingOnEnable = true;
        #endregion --Fields--

        #region --Properties--
        /// <summary>
        /// UDP list
        /// </summary>
        private MocopiUdpReceiver[] UdpReceivers { get; set; }
        #endregion --Properties--

        #region --Methods--
        /// <summary>
        /// Perform the processing when activated
        /// </summary>
        private void OnEnable()
        {
            if (IsReceivingOnEnable)
            {
                this.UdpStart();
            }
        }

        /// <summary>
        /// OnDisable
        /// </summary>
        private void OnDisable()
        {
            if (IsReceivingOnEnable)
            {
                this.UdpStop();
            }
        }

        /// <summary>
        /// Destroy the receiver
        /// </summary>
        private void OnDestroy()
        {
            this.UnsetUdpDelegate();
        }

        /// <summary>
        /// Start receiving UDP
        /// </summary>
        private void UdpStart()
        {
            this.UdpStop();

            if (this.UdpReceivers == null || this.UdpReceivers.Length != this.AvatarSettings.Count)
            {
                this.InitializeUdpReceiver();
            }

            for (int i = 0; i < this.UdpReceivers.Length; i++)
            {
                this.UdpReceivers[i]?.UdpStart();
            }
        }

        /// <summary>
        /// Stop receiving UDP
        /// </summary>
        private void UdpStop()
        {
            if (this.UdpReceivers == null)
            {
                return;
            }

            for (int i = 0; i < this.UdpReceivers.Length; i++)
            {
                this.UdpReceivers[i]?.UdpStop();
            }
        }

        /// <summary>
        /// Initialize the UDP receiver
        /// </summary>
        private void InitializeUdpReceiver()
        {
            this.UnsetUdpDelegate();
            this.UdpReceivers = new MocopiUdpReceiver[this.AvatarSettings.Count];
            this.SetUdpDelegate();
        }

        /// <summary>
        /// Set the process to the UDP delegate
        /// </summary>
        private void SetUdpDelegate()
        {
            if (this.UdpReceivers == null)
            {
                return;
            }
            for (int i = 0; i < this.UdpReceivers.Length; i++)
            {
                if (this.AvatarSettings == null || this.AvatarSettings.Count < i)
                {
                    break;
                }

                if (this.AvatarSettings[i].MocopiAvatar == null)
                {
                    continue;
                }

                if (this.UdpReceivers[i] == null)
                {
                    this.UdpReceivers[i] = new MocopiUdpReceiver(this.AvatarSettings[i].Port);
                }

                this.UdpReceivers[i].OnReceiveSkeletonDefinition += this.AvatarSettings[i].MocopiAvatar.InitializeSkeleton;
                this.UdpReceivers[i].OnReceiveFrameData += this.AvatarSettings[i].MocopiAvatar.UpdateSkeleton;
            }
        }

        /// <summary>
        /// Unconfigure the UDP delegate
        /// </summary>
        private void UnsetUdpDelegate()
        {
            if (this.UdpReceivers == null)
            {
                return;
            }

            for (int i = 0; i < this.UdpReceivers.Length; i++)
            {
                if (this.AvatarSettings == null || this.AvatarSettings.Count < i)
                {
                    break;
                }

                if (this.AvatarSettings.Count <= i)
                {
                    continue;
                }

                if (this.UdpReceivers[i] == null)
                {
                    continue;
                }

                this.UdpReceivers[i].OnReceiveSkeletonDefinition -= this.AvatarSettings[i].MocopiAvatar.InitializeSkeleton;
                this.UdpReceivers[i].OnReceiveFrameData -= this.AvatarSettings[i].MocopiAvatar.UpdateSkeleton;
            }
        }

        /// <summary>
        /// Start receiving.
        /// </summary>
        public void StartReceiving()
        {
            if (!IsReceivingOnEnable)
            {
                this.UdpStart();
            }
        }

        /// <summary>
        /// Stop receiving.
        /// </summary>
        public void StopReceiving()
        {
            if (!IsReceivingOnEnable)
            {
                this.UdpStop();
            }
        }

        /// <summary>
        /// Add Avatar to AvatarSettings
        /// </summary>
        /// <param name="port">Port number</param>
        public void AddAvatar(MocopiAvatar mocopiAvatar, int port)
        {
            AvatarSettings.Add(new MocopiSimpleReceiverAvatarSettings(mocopiAvatar, port));
        }
        #endregion --Methods--

        #region --Classes--
        /// <summary>
        /// Hold a pair of an avatar and a port id
        /// </summary>
        [System.Serializable]
        public sealed class MocopiSimpleReceiverAvatarSettings
        {
            /// <summary>
            /// mocopi avatar
            /// </summary>
            public MocopiAvatar MocopiAvatar;

            /// <summary>
            /// Port number
            /// </summary>
            public int Port;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="mocopiAvatar">mocopi avatar</param>
            /// <param name="port">Port number</param>
            public MocopiSimpleReceiverAvatarSettings(MocopiAvatar mocopiAvatar, int port)
            {
                this.MocopiAvatar = mocopiAvatar;
                this.Port = port;
            }
        }
        #endregion --Classes--
    }
}
