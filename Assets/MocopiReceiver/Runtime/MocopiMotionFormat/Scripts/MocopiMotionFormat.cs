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
using System;
using System.Runtime.InteropServices;

namespace Sony.MMF
{
    /// <summary>
    /// Class that converts received data so that it can be used (use "mocopi_motion_serializer.dll")
    /// </summary>
    public sealed class MocopiMotionFormat
    {
        #region --Fields--
        /// <summary>
        /// Constant of library name
        /// </summary>
#if UNITY_IOS && !UNITY_EDITOR_OSX
        public const string MOCOPI_MOTION_FORMAT_LIBRARY_NAME = "__Internal";
#else
        public const string MOCOPI_MOTION_FORMAT_LIBRARY_NAME = "mocopi_motion_serializer";
#endif
        #endregion --Fields--
        #region --Methods--
        /// <summary>
        /// Converts bytes to skeleton definition
        /// </summary>
        /// <param name="bytes_size">Byte size</param>
        /// <param name="bytes">Byte data</param>
        /// <param name="sender_ip">Sender IP address</param>
        /// <param name="sender_port">Sender port number</param>
        /// <param name="size">Size</param>
        /// <param name="joint_ids">Id of joints</param>
        /// <param name="parent_joint_ids">Id of parent joints</param>
        /// <param name="rotations_x">rotations in the X direction</param>
        /// <param name="rotations_y">rotations in the Y direction</param>
        /// <param name="rotations_z">rotations in the Z direction</param>
        /// <param name="rotations_w">rotations in the W direction</param>
        /// <param name="positions_x">X coordinate of position</param>
        /// <param name="positions_y">Y coordinate of position</param>
        /// <param name="positions_z">Z coordinate of position</param>
        /// <returns>Whether the conversion was successful or not</returns>
        [DllImport(MOCOPI_MOTION_FORMAT_LIBRARY_NAME)]
        public static extern bool ConvertBytesToSkeletonDefinition(
            int bytes_size,
            byte[] bytes,
            out ulong sender_ip,
            out int sender_port,
            out int size,
            out IntPtr joint_ids,
            out IntPtr parent_joint_ids,
            out IntPtr rotations_x,
            out IntPtr rotations_y,
            out IntPtr rotations_z,
            out IntPtr rotations_w,
            out IntPtr positions_x,
            out IntPtr positions_y,
            out IntPtr positions_z
        );

        /// <summary>
        /// Converts bytes to frame data
        /// </summary>
        /// <param name="bytes_size">Byte size</param>
        /// <param name="bytes">Byte data</param>
        /// <param name="sender_ip">Sender IP address</param>
        /// <param name="sender_port">Sender port number</param>
        /// <param name="frame_id">Frame Id</param>
        /// <param name="timestamp">Timestamp</param>
        /// <param name="unixTime">Unix time when sensor sent data</param>
        /// <param name="size">Size</param>
        /// <param name="joint_ids">Id of joints</param>
        /// <param name="rotations_x">rotations in the X direction</param>
        /// <param name="rotations_y">rotations in the Y direction</param>
        /// <param name="rotations_z">rotations in the Z direction</param>
        /// <param name="rotations_w">rotations in the W direction</param>
        /// <param name="positions_x">X coordinate of position</param>
        /// <param name="positions_y">Y coordinate of position</param>
        /// <param name="positions_z">Z coordinate of position</param>
        /// <returns>Whether the conversion was successful or not</returns>
		[DllImport(MOCOPI_MOTION_FORMAT_LIBRARY_NAME)]
        public static extern bool ConvertBytesToFrameData(
            int bytes_size,
            byte[] bytes,
            out ulong sender_ip,
            out int sender_port,
            out int frame_id,
            out float timestamp,
            out double utc_time,
            out byte timecode_hour,
            out byte timecode_min,
            out byte timecode_sec,
            out byte timecode_frame,
            out byte timecode_frame_rate,
            out bool timecode_drop_frame,
            out int size,
            out IntPtr joint_ids,
            out IntPtr rotations_x,
            out IntPtr rotations_y,
            out IntPtr rotations_z,
            out IntPtr rotations_w,
            out IntPtr positions_x,
            out IntPtr positions_y,
            out IntPtr positions_z
        );

        /// <summary>
        /// Judges if the bytes are in MocopiMotionFormat's format
        /// </summary>
        /// <param name="bytes_size">Byte size</param>
        /// <param name="bytes">Byte data</param>
        /// <returns>Correct format or not</returns>
        [DllImport(MOCOPI_MOTION_FORMAT_LIBRARY_NAME)]
        public static extern bool IsMmfBytes(
            int bytes_size,
            byte[] bytes
        );

        /// <summary>
        /// Judges if the bytes are in skeleton definition
        /// </summary>
        /// <param name="bytes_size">Byte size</param>
        /// <param name="bytes">Byte data</param>
        /// <returns>whether it is a skeleton definition</returns>
        [DllImport(MOCOPI_MOTION_FORMAT_LIBRARY_NAME)]
        public static extern bool IsSkeletonDefinitionBytes(
            int bytes_size,
            byte[] bytes
        );

        /// <summary>
        /// Judges if the bytes are in frame data
        /// </summary>
        /// <param name="bytes_size">Byte size</param>
        /// <param name="bytes">Byte data</param>
        /// <returns>whether it is a frame data</returns>
        [DllImport(MOCOPI_MOTION_FORMAT_LIBRARY_NAME)]
        public static extern bool IsFrameDataBytes(
            int bytes_size,
            byte[] bytes
        );
        #endregion --Methods--
    }
}
