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
using UnityEditor;
using UnityEngine;

namespace Mocopi.Receiver
{
    /// <summary>
    /// Abstract class for applying mocopi bone information to avatars
    /// </summary>
    public abstract class MocopiAvatarBase : MonoBehaviour
    {
        #region --Methods--
        /// <summary>
        /// Initialize avatar bone information
        /// </summary>
        /// <param name="boneIds">mocopi Avatar bone id list</param>
        /// <param name="parentBoneIds">List of IDs of parent bones for each bone</param>
        /// <param name="rotationsX">Rotation angle of each bone in initial posture</param>
        /// <param name="rotationsY">Rotation angle of each bone in initial posture</param>
        /// <param name="rotationsZ">Rotation angle of each bone in initial posture</param>
        /// <param name="rotationsW">Rotation angle of each bone in initial posture</param>
        /// <param name="positionsX">Position of each bone in initial pose</param>
        /// <param name="positionsY">Position of each bone in initial pose</param>
        /// <param name="positionsZ">Position of each bone in initial pose</param>
        public virtual void InitializeSkeleton(
            int[] boneIds, int[] parentBoneIds,
            float[] rotationsX, float[] rotationsY, float[] rotationsZ, float[] rotationsW,
            float[] positionsX, float[] positionsY, float[] positionsZ
        )
        {
        }

        /// <summary>
        /// Update avatar bone information
        /// </summary>
        /// <param name="frameId">Frame Id</param>
        /// <param name="timestamp">Timestamp</param>
        /// <param name="unixTime">Unix time when sensor sent data</param>
		/// <param name="timecodeHour">Value of timecode hour</param>
		/// <param name="timecodeMin">Value of timecode minute</param>
		/// <param name="timecodeSec">Value of timecode second</param>
		/// <param name="timecodeFrame">Value of timecode frame</param>
		/// <param name="timecodeDropFrame">Value of timecode drop frame</param>
        /// <param name="boneIds">mocopi Avatar bone id list</param>
        /// <param name="rotationsX">Rotation angle of each bone</param>
        /// <param name="rotationsY">Rotation angle of each bone</param>
        /// <param name="rotationsZ">Rotation angle of each bone</param>
        /// <param name="rotationsW">Rotation angle of each bone</param>
        /// <param name="positionsX">Position of each bone</param>
        /// <param name="positionsY">Position of each bone</param>
        /// <param name="positionsZ">Position of each bone</param>
        public virtual void UpdateSkeleton(
            int frameId, float timestamp, double unixTime,
            byte timecodeHour, byte timecodeMin, byte timecodeSec, byte timecodeFrame,
            byte timecodeFrameRate, bool timecodeDropFrame,
            int[] boneIds,
            float[] rotationsX, float[] rotationsY, float[] rotationsZ, float[] rotationsW,
            float[] positionsX, float[] positionsY, float[] positionsZ
        )
        {
        }
        #endregion --Methods--
    }
}