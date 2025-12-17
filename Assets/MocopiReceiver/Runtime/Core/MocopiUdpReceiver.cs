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
using Sony.MMF;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mocopi.Receiver.Core
{
    /// <summary>
    /// Class receiving by UDP from the sensor for handling
    /// </summary>
    public sealed class MocopiUdpReceiver
    {
        #region --Fields--
        /// <summary>
        /// Delegate type variables for bone initialization
        /// </summary>
        public ReceiveSkeletonDefinitionEvent OnReceiveSkeletonDefinition = new ReceiveSkeletonDefinitionEvent(
            (
                int[] boneIds, int[] parentBoneIds,
                float[] rotationsX, float[] rotationsY, float[] rotationsZ, float[] rotationsW,
                float[] positionsX, float[] positionsY, float[] positionsZ
            ) => 
            { 
            }

        );

        /// <summary>
        /// Delegate type variables for achieving bone movement
        /// </summary>
        public ReceiveFrameDataEvent OnReceiveFrameData = new ReceiveFrameDataEvent(
            (
                int frameId, float timestamp, double unixTime,
                byte timecodeHour, byte timecodeMin, byte timecodeSec, byte timecodeFrame,
                byte timecodeFrameRate, bool timecodeDropFrame,
                int[] boneIds,
                float[] rotationsX, float[] rotationsY, float[] rotationsZ, float[] rotationsW,
                float[] positionsX, float[] positionsY, float[] positionsZ
            ) => 
            {
            }

        );

        /// <summary>
        /// Delegate type variables for start error event
        /// </summary>
        public ErrorOccurredEvent OnUdpStartFailed = new ErrorOccurredEvent((_) => { });

        /// <summary>
        /// Delegate type variables for receive error event
        /// </summary>
        public ErrorOccurredEvent OnUdpReceiveFailed = new ErrorOccurredEvent((_) => { });

        /// <summary>
        /// Object for exclusive access control
        /// </summary>
        private static object lockObject = new object();

        /// <summary>
        /// Threading task
        /// </summary>
        private Task task;

        /// <summary>
        /// Manage cancellation notifications to cancellation tokens
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Udp client
        /// </summary>
        private UdpClient udpClient;
        #endregion --Fields--

        #region --Constructors--
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">Udp port number</param>
        public MocopiUdpReceiver(int port)
        {
            this.Port = port;
        }
        #endregion --Constructors--

        #region --Finalizers--
        /// <summary>
        /// Destructor
        /// </summary>
        ~MocopiUdpReceiver()
        {
            this.UdpStop();
        }
        #endregion --Finalizers--

        #region --Delegates--
        /// <summary>
        /// Define a delegate for bone initialization
        /// </summary>
        /// <param name="boneIds">Id of bones</param>
        /// <param name="parentBoneIds">Id of parent bones</param>
        /// <param name="rotationsX">rotations in the X direction</param>
        /// <param name="rotationsY">rotations in the Y direction</param>
        /// <param name="rotationsZ">rotations in the Z direction</param>
        /// <param name="rotationsW">rotations in the W direction</param>
        /// <param name="positionsX">X coordinate of position</param>
        /// <param name="positionsY">Y coordinate of position</param>
        /// <param name="positionsZ">Z coordinate of position</param>
        public delegate void ReceiveSkeletonDefinitionEvent(
            int[] boneIds, int[] parentBoneIds,
            float[] rotationsX, float[] rotationsY, float[] rotationsZ, float[] rotationsW,
            float[] positionsX, float[] positionsY, float[] positionsZ
        );

        /// <summary>
        /// Define a delegate to achieve bone movement
        /// </summary>
        /// <param name="frameId">Frame Id</param>
        /// <param name="timestamp">Timestamp</param>
        /// <param name="unixTime">Unix time when sensor sent data</param>
        /// <param name="timecodeHour">Timecode hour</param>
        /// <param name="timecodeMin">Timecode minute</param>
        /// <param name="timecodeSec">Timecode second</param>
        /// <param name="timecodeFrame">Timecode frame</param>
        /// <param name="timecodeFrameRate">Timecode frame rate</param>
        /// <param name="timecodeDropFrame">Timecode drop frame</param>
        /// <param name="boneIds">Id of bones</param>
        /// <param name="rotationsX">rotations in the X direction</param>
        /// <param name="rotationsY">rotations in the Y direction</param>
        /// <param name="rotationsZ">rotations in the Z direction</param>
        /// <param name="rotationsW">rotations in the W direction</param>
        /// <param name="positionsX">X coordinate of position</param>
        /// <param name="positionsY">Y coordinate of position</param>
        /// <param name="positionsZ">Z coordinate of position</param>
        public delegate void ReceiveFrameDataEvent(
            int frameId, float timestamp, double unixTime,
            byte timecodeHour, byte timecodeMin, byte timecodeSec, byte timecodeFrame,
            byte timecodeFrameRate, bool timecodeDropFrame,
            int[] boneIds,
            float[] rotationsX, float[] rotationsY, float[] rotationsZ, float[] rotationsW,
            float[] positionsX, float[] positionsY, float[] positionsZ
        );

        /// <summary>
        /// Define a delegate to error event
        /// </summary>
        /// <param name="e">Content of exception</param>
        public delegate void ErrorOccurredEvent(System.Exception e);
        #endregion --Delegates--

        #region --Properties--
        /// <summary>
        /// Port 
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Is it running
        /// </summary>
        public bool IsRuning { get { return this.task != null && !this.task.IsCanceled && !this.task.IsCompleted; } }
        #endregion --Properties--

        #region --Methods--
        /// <summary>
        /// Start receiving UDP
        /// </summary>
        public void UdpStart()
        {
            this.UdpStop();

            if (this.IsRuning)
            {
                return;
            }

            try
            {
                this.cancellationTokenSource = new CancellationTokenSource();
                this.task = Task.Run(() => this.UdpTaskAsync(this.cancellationTokenSource.Token));
            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat($"[MocopiUdpReceiver] Udp start failed. {e.GetType()} : {e.Message}");
                this.OnUdpStartFailed?.Invoke(e);
            }
        }

        /// <summary>
        /// Stop receiving UDP
        /// </summary>
        public void UdpStop()
        {
            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
                this.task = null;
            }

            if (this.udpClient != null)
            {
                this.udpClient.Close();
                this.udpClient = null;
            }
        }

        /// <summary>
        /// Convert pointers to arrays
        /// </summary>
        /// <typeparam name="T">Generic class</typeparam>
        /// <param name="ptr">Pointer</param>
        /// <param name="length">Length</param>
        /// <returns>Converted array</returns>
        private static T[] PointerToArray<T>(IntPtr ptr, int length)
        {
            int size = Marshal.SizeOf(typeof(T));
            T[] array = new T[length];
            for (int i = 0; i < length; i++)
            {
                IntPtr p = new IntPtr(ptr.ToInt64() + i * size);
                array[i] = Marshal.PtrToStructure<T>(p);
            }

            return array;
        }

        /// <summary>
        /// Convert IP address from ULong type to string
        /// </summary>
        /// <param name="u">String type IP address</param>
        /// <returns>String type IP address</returns>
        private static string ULongToIpAddressString(ulong u)
        {
            short ip1, ip2, ip3, ip4;
            ip4 = (short)(u % 1000);
            u /= 1000;
            ip3 = (short)(u % 1000);
            u /= 1000;
            ip2 = (short)(u % 1000);
            u /= 1000;
            ip1 = (short)(u % 1000);

            return ip1 + "." + ip2 + "." + ip3 + "." + ip4;
        }

        /// <summary>
        /// Convert the data received by UDP to move the avatar
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        private async void UdpTaskAsync(CancellationToken cancellationToken)
        {
            this.udpClient = new UdpClient(this.Port);

            while (!cancellationToken.IsCancellationRequested && this.udpClient != null)
            {
                int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
                try
                {
                    IPEndPoint remoteEP = null;
                    byte[] message = this.udpClient.Receive(ref remoteEP);

                    lock (lockObject)
                    {
                        if (MocopiMotionFormat.IsMmfBytes(message.Length, message))
                        {
                            // Processing of data acquired in "MocopiMotionFormat"
                            this.HandleMocopiMotionFormatData(message);
                        }
                    }
                }
                catch (SocketException e)
                {
                    Debug.Log($"[MocopiUdpReceiver] {e.Message} : {e.GetType()}");
                }
                catch (System.Exception e)
                {
                    Debug.LogErrorFormat($"[MocopiUdpReceiver] Udp receive failed. {e.Message} : {e.GetType()}");
                    this.UdpStop();
                    this.OnUdpReceiveFailed?.Invoke(e);
                    break;
                }

                await Task.Delay(1);
            }
        }

        /// <summary>
        /// Convert "MocopiMotionFormat" data to move the avatar
        /// </summary>
        /// <param name="message">Udp data</param>
        private void HandleMocopiMotionFormatData(byte[] message)
        {
            int bytesSize = message.Length;
            if (MocopiMotionFormat.IsSkeletonDefinitionBytes(bytesSize, message))
            {
                if (MocopiMotionFormat.ConvertBytesToSkeletonDefinition(
                    bytesSize,
                    message,
                    out ulong ulongSenderIp,
                    out int senderPort,
                    out int size,
                    out IntPtr ptrBoneIds,
                    out IntPtr ptrParentBoneIds,
                    out IntPtr ptrRotationsX,
                    out IntPtr ptrRotationsY,
                    out IntPtr ptrRotationsZ,
                    out IntPtr ptrRotationsW,
                    out IntPtr ptrPositionsX,
                    out IntPtr ptrPositionsY,
                    out IntPtr ptrPositionsZ
                ))
                {
                    this.SetSkeletonDefinition(
                        ULongToIpAddressString(ulongSenderIp),
                        senderPort,
                        PointerToArray<int>(ptrBoneIds, size),
                        PointerToArray<int>(ptrParentBoneIds, size),
                        PointerToArray<float>(ptrRotationsX, size),
                        PointerToArray<float>(ptrRotationsY, size),
                        PointerToArray<float>(ptrRotationsZ, size),
                        PointerToArray<float>(ptrRotationsW, size),
                        PointerToArray<float>(ptrPositionsX, size),
                        PointerToArray<float>(ptrPositionsY, size),
                        PointerToArray<float>(ptrPositionsZ, size)
                    );
                }
            }
            else if (MocopiMotionFormat.IsFrameDataBytes(bytesSize, message))
            {
                if (MocopiMotionFormat.ConvertBytesToFrameData(
                    bytesSize,
                    message,
                    out ulong ulongSenderIp,
                    out int senderPort,
                    out int frameid,
                    out float timestamp,
                    out double unixTime,
                    out byte timecodeHour,
                    out byte timecodeMin,
                    out byte timecodeSec,
                    out byte timecodeFrame,
                    out byte timecodeFrameRate,
                    out bool timecodeDropFrame,
                    out int size,
                    out IntPtr ptrBoneIds,
                    out IntPtr ptrRotationsX,
                    out IntPtr ptrRotationsY,
                    out IntPtr ptrRotationsZ,
                    out IntPtr ptrRotationsW,
                    out IntPtr ptrPositionsX,
                    out IntPtr ptrPositionsY,
                    out IntPtr ptrPositionsZ
                ))
                {
                    this.SetSkeletonData(
                        ULongToIpAddressString(ulongSenderIp),
                        senderPort,
                        frameid,
                        timestamp,
                        unixTime,
                        timecodeHour,
                        timecodeMin,
                        timecodeSec,
                        timecodeFrame,
                        timecodeFrameRate,
                        timecodeDropFrame,
                        PointerToArray<int>(ptrBoneIds, size),
                        PointerToArray<float>(ptrRotationsX, size),
                        PointerToArray<float>(ptrRotationsY, size),
                        PointerToArray<float>(ptrRotationsZ, size),
                        PointerToArray<float>(ptrRotationsW, size),
                        PointerToArray<float>(ptrPositionsX, size),
                        PointerToArray<float>(ptrPositionsY, size),
                        PointerToArray<float>(ptrPositionsZ, size)
                    );
                }
            }
        }

        /// <summary>
        /// Set skeleton data for bone initialization
        /// </summary>
        /// <param name="senderIp">Sender IP address</param>
        /// <param name="senderPort">Sender port number</param>
        /// <param name="boneIds">Id of bones</param>
        /// <param name="parentBoneIds">Id of parent bones</param>
        /// <param name="rotationsX">rotations in the X direction</param>
        /// <param name="rotationsY">rotations in the Y direction</param>
        /// <param name="rotationsZ">rotations in the Z direction</param>
        /// <param name="rotationsW">rotations in the W direction</param>
        /// <param name="positionsX">X coordinate of position</param>
        /// <param name="positionsY">Y coordinate of position</param>
        /// <param name="positionsZ">Z coordinate of position</param>
        private void SetSkeletonDefinition(
            string senderIp, int senderPort,
            int[] boneIds, int[] parentBoneIds,
            float[] rotationsX, float[] rotationsY, float[] rotationsZ, float[] rotationsW,
            float[] positionsX, float[] positionsY, float[] positionsZ
        )
        {
            this.OnReceiveSkeletonDefinition?.Invoke(
                boneIds, parentBoneIds,
                rotationsX, rotationsY, rotationsZ, rotationsW,
                positionsX, positionsY, positionsZ
            );
        }

        /// <summary>
        /// Set skeleton data for achieving bone movement
        /// </summary>
        /// <param name="senderIp">Sender IP address</param>
        /// <param name="senderPort">Sender port number</param>
        /// <param name="frameid">Frame Id</param>
        /// <param name="timestamp">Timestamp</param>
        /// <param name="unixTime">Unix time when sensor sent data</param>
        /// <param name="boneIds">Id of bones</param>
        /// <param name="rotationsX">rotations in the X direction</param>
        /// <param name="rotationsY">rotations in the Y direction</param>
        /// <param name="rotationsZ">rotations in the Z direction</param>
        /// <param name="rotationsW">rotations in the W direction</param>
        /// <param name="positionsX">X coordinate of position</param>
        /// <param name="positionsY">Y coordinate of position</param>
        /// <param name="positionsZ">Z coordinate of position</param>
        private void SetSkeletonData(
            string senderIp, int senderPort,
            int frameid, float timestamp, double unixTime,
            byte timecodeHour, byte timecodeMin, byte timecodeSec, byte timecodeFrame,
            byte timecodeFrameRate, bool timecodeDropFrame,
            int[] boneIds,
            float[] rotationsX, float[] rotationsY, float[] rotationsZ, float[] rotationsW,
            float[] positionsX, float[] positionsY, float[] positionsZ
        )
        {
            this.OnReceiveFrameData?.Invoke(
                frameid, timestamp, unixTime,
                timecodeHour, timecodeMin, timecodeSec, timecodeFrame,
                timecodeFrameRate, timecodeDropFrame,
                boneIds,
                rotationsX, rotationsY, rotationsZ, rotationsW,
                positionsX, positionsY, positionsZ
            );
        }
        #endregion --Methods--
    }
}
