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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mocopi.Receiver
{
    /// <summary>
    /// A class for controlling avatars
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class MocopiAvatar : MocopiAvatarBase
    {
        #region --Fields--
        /// <summary>
        /// Table of mocopi sensor bone information
        /// </summary>
        public static readonly Dictionary<string, int> HUMAN_BONE_NAME_TO_MOCOPI_BONE_ID = new Dictionary<string, int>()
        {
            { "Hips",           0 },
            { "Spine",          3 },
            { "Chest",          5 },
            { "UpperChest",     6 },
            { "Neck",           8 },
            { "Head",           10 },
            { "LeftShoulder",   11 },
            { "LeftUpperArm",   12 },
            { "LeftLowerArm",   13 },
            { "LeftHand",       14 },
            { "RightShoulder",  15 },
            { "RightUpperArm",  16 },
            { "RightLowerArm",  17 },
            { "RightHand",      18 },
            { "LeftUpperLeg",   19 },
            { "LeftLowerLeg",   20 },
            { "LeftFoot",       21 },
            { "LeftToes",    22 },
            { "RightUpperLeg",  23 },
            { "RightLowerLeg",  24 },
            { "RightFoot",      25 },
            { "RightToes",   26 }
        };

        /// <summary>
        /// Table of mocopi sensor bone information
        /// </summary>
        public static readonly Dictionary<string, int> MOCOPI_BONE_NAME_TO_MOCOPI_BONE_ID = new Dictionary<string, int>()
        {
            { "root",       0 },
            { "torso_1",    1 },
            { "torso_2",    2 },
            { "torso_3",    3 },
            { "torso_4",    4 },
            { "torso_5",    5 },
            { "torso_6",    6 },
            { "torso_7",    7 },
            { "neck_1",     8 },
            { "neck_2",     9 },
            { "head",       10 },
            { "l_shoulder", 11 },
            { "l_up_arm",   12 },
            { "l_low_arm",  13 },
            { "l_hand",     14 },
            { "r_shoulder", 15 },
            { "r_up_arm",   16 },
            { "r_low_arm",  17 },
            { "r_hand",     18 },
            { "l_up_leg",   19 },
            { "l_low_leg",  20 },
            { "l_foot",     21 },
            { "l_toes",     22 },
            { "r_up_leg",   23 },
            { "r_low_leg",  24 },
            { "r_foot",     25 },
            { "r_toes",     26 }
        };

        /// <summary>
        /// Avatar list
        /// </summary>
        public static readonly List<MocopiAvatar> Avatars = new List<MocopiAvatar>();

        /// <summary>
        /// Properties for adjusting the smoothness of motion movements
        /// </summary>
        [Range(0, 1f)] public float MotionSmoothness = 0;

        /// <summary>
        /// Enable motion buffering
        /// </summary>
        public bool IsBufferingEnabled = true;

        /// <summary>
        /// Buffer delay recovery ratio
        /// </summary>
        [Range(0, 1f)] public float DelayRecoveryRate = 0.001f;

        /// <summary>
        /// Remove AnimatorController on update or not
        /// </summary>
        private bool isRemoveAnimatorControllerOnUpdate = true;

        /// <summary>
        /// Last used MotionSmoothness value
        /// </summary>
        private float lastMotionSmoothness = 0;

        /// <summary>
        /// Bones list
        /// </summary>
        private readonly List<MocopiBone> bones = new List<MocopiBone>();

        /// <summary>
        /// Bones position
        /// </summary>
        private readonly Dictionary<MocopiBone, Vector3> bonePositions = new Dictionary<MocopiBone, Vector3>();

        /// <summary>
        /// bones rotation
        /// </summary>
        private readonly Dictionary<MocopiBone, Quaternion> boneRotations = new Dictionary<MocopiBone, Quaternion>();

        /// <summary>
        /// List of frame arrival times
        /// </summary>
        private readonly List<float> frameArrivalTimes = new List<float>();

        /// <summary>
        /// Frame counter
        /// </summary>
        private int fpsFrameCounter = 0;

        /// <summary>
        /// Whether skeleton initialization is reserved
        /// </summary>
        private bool isSkeletonInitializeReserved = false;

        /// <summary>
        /// Whether the skeleton has been initialized
        /// </summary>
        private bool isSkeletonInitialized = false;

        /// <summary>
        /// Was the skeleton updated
        /// </summary>
        private bool isSkeletonUpdated = false;

        /// <summary>
        /// Avatar definition
        /// </summary>
        private Avatar avatar;

        /// <summary>
        /// HumanPose before update
        /// </summary>
        private HumanPoseHandler humanPoseHandlerSrc;

        /// <summary>
        /// HumanPose after update
        /// </summary>
        private HumanPoseHandler humanPoseHandlerDst;

        /// <summary>
        /// Human temporary pose
        /// </summary>
        private HumanPose temppose = new HumanPose();

        /// <summary>
        /// Human smoothness pose
        /// </summary>
        private HumanPose smoothnesspose = new HumanPose();

        /// <summary>
        /// Skeleton definition data
        /// </summary>
        private SkeletonDefinitionData skeletonDefinitionData;

        /// <summary>
        /// Previous skeleton definition data
        /// </summary>
        private SkeletonDefinitionData? oldSkeletonDefinitionData;

        /// <summary>
        /// Skeleton update data
        /// </summary>
        private SkeletonData skeletonData;

        /// <summary>
        /// Avatar object
        /// </summary>
        private GameObject avatarRootObj;

        /// <summary>
        /// Max pose buffering count
        /// </summary>
        private const int BUFFER_COUNT = 256;

        /// <summary>
        /// Sensor FPS
        /// </summary>
        private const float SENSOR_FPS = 50.0f;

        /// <summary>
        /// Max delay time
        /// </summary>
        private const double MAX_DELAY_TIME = 1.0 / SENSOR_FPS * BUFFER_COUNT;

        /// <summary>
        /// Initialization time at the start of reception
        /// </summary>
        private const float INITIALIZATION_TIME_AT_START_RECEPTION = 5.0f;

        /// <summary>
        /// Pose buffer
        /// </summary>
		private (int frameId, double timestamp, HumanPose pose)[] poseBuffer = new (int frameId, double timestamp, HumanPose pose)[BUFFER_COUNT];

        /// <summary>
        /// Last index buffered
        /// </summary>
        private int lastBufferIndex = -1;

        /// <summary>
        /// Index of the last used buffer
        /// </summary>
        private int lastUsedIndex = -1;

        /// <summary>
        /// mocopi timestamp at buffer start
        /// </summary>
		private double startTimestamp = 0;

        /// <summary>
        /// Current delay time
        /// </summary>
		private double currentDelayTime = 0;

        /// <summary>
        /// Last delay time
        /// </summary>
		private double lastDelayTime = 0;

        /// <summary>
        /// Realtime at buffer start
        /// </summary>
		private double startRealtime = 0;

        /// <summary>
        /// Last received frame id
        /// </summary>
        private int lastReceivedFrameId = -1;

        /// <summary>
        /// Last received timestamp
        /// </summary>
		private double lastReceivedTimestamp = 0f;

        /// <summary>
        /// Last received real-timestamp
        /// </summary>
        private double lastReceivedRealTimestamp = 0f;

        /// <summary>
        /// Last buffer reset time
        /// </summary>
        private float lastBufferResetTime = 0f;

        /// <summary>
        /// Frame count for BVH
        /// </summary>
        private int frameCount;

        /// <summary>
        /// Application is background
        /// </summary>
        private bool isAppBackground = false;

        #endregion --Fields--

        #region --Properties--
        /// <summary>
        /// Animator
        /// </summary>
        public Animator Animator { get; private set; }

        /// <summary>
        /// Whether or not to stop updating the skeleton
        /// </summary>
        public bool IsLockSkeletonUpdate { get; set; }

        /// <summary>
        /// Number of frames arriving
        /// </summary>
        public float FrameArrivalRate { get; private set; }
        #endregion --Properties--

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
        /// <remarks><see cref="MocopiAvatarBase.InitializeSkeleton(int[], int[], float[], float[], float[], float[], float[], float[], float[])"/></remarks>
        public override void InitializeSkeleton(
            int[] boneIds, int[] parentBoneIds,
            float[] rotationsX, float[] rotationsY, float[] rotationsZ, float[] rotationsW,
            float[] positionsX, float[] positionsY, float[] positionsZ
        )
        {
            this.skeletonDefinitionData.BoneIds = boneIds;
            this.skeletonDefinitionData.ParentBoneIds = parentBoneIds;
            this.skeletonDefinitionData.RotationsX = rotationsX;
            this.skeletonDefinitionData.RotationsY = rotationsY;
            this.skeletonDefinitionData.RotationsZ = rotationsZ;
            this.skeletonDefinitionData.RotationsW = rotationsW;
            this.skeletonDefinitionData.PositionsX = positionsX;
            this.skeletonDefinitionData.PositionsY = positionsY;
            this.skeletonDefinitionData.PositionsZ = positionsZ;

            this.isSkeletonInitializeReserved = true;
        }

        /// <summary>
        /// Update avatar bone information
        /// </summary>
        /// <param name="frameId">Frame Id</param>
        /// <param name="timestamp">Timestamp</param>
        /// <param name="unixTime">Unix time when sensor sent data</param>
        /// <param name="boneIds">mocopi Avatar bone id list</param>
		/// <param name="timecodeHour">Value of timecode hour</param>
		/// <param name="timecodeMin">Value of timecode minute</param>
		/// <param name="timecodeSec">Value of timecode second</param>
		/// <param name="timecodeFrame">Value of timecode frame</param>
		/// <param name="timecodeDropFrame">Value of timecode drop frame</param>
        /// <param name="rotationsX">Rotation angle of each bone</param>
        /// <param name="rotationsY">Rotation angle of each bone</param>
        /// <param name="rotationsZ">Rotation angle of each bone</param>
        /// <param name="rotationsW">Rotation angle of each bone</param>
        /// <param name="positionsX">Position of each bone</param>
        /// <param name="positionsY">Position of each bone</param>
        /// <param name="positionsZ">Position of each bone</param>
        /// <remarks><see cref="MocopiAvatarBase.UpdateSkeleton(int[], float[], float[], float[], float[], float[], float[], float[])"/></remarks>
        public override void UpdateSkeleton(
            int frameId, float timestamp, double unixTime,
            byte timecodeHour, byte timecodeMin, byte timecodeSec, byte timecodeFrame,
            byte timecodeFrameRate, bool timecodeDropFrame,
            int[] boneIds,
            float[] rotationsX, float[] rotationsY, float[] rotationsZ, float[] rotationsW,
            float[] positionsX, float[] positionsY, float[] positionsZ
        )
        {
            this.skeletonData.FrameId = frameId;
            this.skeletonData.Timestamp = timestamp;
            this.skeletonData.UnixTime = unixTime;
            this.skeletonData.Timecode.Hour = timecodeHour;
            this.skeletonData.Timecode.Minute = timecodeMin;
            this.skeletonData.Timecode.Second = timecodeSec;
            this.skeletonData.Timecode.Frame = timecodeFrame;
            this.skeletonData.Timecode.DropFrame = timecodeDropFrame;
            this.skeletonData.BoneIds = boneIds;
            this.skeletonData.RotationsX = rotationsX;
            this.skeletonData.RotationsY = rotationsY;
            this.skeletonData.RotationsZ = rotationsZ;
            this.skeletonData.RotationsW = rotationsW;
            this.skeletonData.PositionsX = positionsX;
            this.skeletonData.PositionsY = positionsY;
            this.skeletonData.PositionsZ = positionsZ;

            this.isSkeletonUpdated = true;

            this.fpsFrameCounter++;
        }

        /// <summary>
        /// Reset buffer
        /// </summary>
        public void ResetBuffer()
        {
            lastUsedIndex = -1;
            lastBufferResetTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Awake
        /// </summary>
        private void Awake()
        {
            if (!Avatars.Contains(this))
            {
                Avatars.Add(this);
            }

            this.Animator = GetComponent<Animator>();
            ResetBuffer();
        }

        /// <summary>
        /// OnEnable
        /// </summary>
        private void OnEnable()
        {
            ResetBuffer();
        }

        /// <summary>
        /// OnApplicationPause
        /// </summary>
        /// <param name="pauseStatus">Pause status</param>
        private void OnApplicationPause(bool pauseStatus)
        {
            ChangeBackgroundStatus(pauseStatus);
        }

        /// <summary>
        /// OnApplicationFocus
        /// </summary>
        /// <param name="hasFocus">Has focus</param>
        private void OnApplicationFocus(bool hasFocus)
        {
            ChangeBackgroundStatus(!hasFocus);
        }

        /// <summary>
        /// Change background status
        /// </summary>
        /// <param name="isBackground">background or not</param>
        private void ChangeBackgroundStatus(bool isBackground)
        {
            if (isBackground == this.isAppBackground) return;

            if (isBackground == false)
            {
                //Reset buffer when app active
                ResetBuffer();
            }

            this.isAppBackground = isBackground;
        }

        /// <summary>
        /// Update
        /// </summary>
        private void Update()
        {
            if (this.isSkeletonInitializeReserved)
            {
                this.InvokeInitializeSkeleton();
                this.isSkeletonInitializeReserved = false;
            }

            if (this.isSkeletonInitialized)
            {
                // if the skeleton needs to be updated
                if (this.isSkeletonUpdated)
                {
                    this.InvokeUpdateSkeleton();
                    this.isSkeletonUpdated = false;

                    // buffering avatar pose
                    this.BufferAvatarPose();
                }

                // update avatar pose
                this.UpdateAvatarPose();
            }
        }

        /// <summary>
        /// LateUpdate
        /// </summary>
        private void LateUpdate()
        {
            if (this.isSkeletonInitialized)
            {
                this.UpdateFrameArrivalRate();
            }
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        private void OnDestroy()
        {
            if (Avatars.Contains(this))
            {
                Avatars.Remove(this);
            }
        }

        /// <summary>
        /// Update frame arrivals
        /// </summary>
        private void UpdateFrameArrivalRate()
        {
            int removeCount = 0;
            foreach (float time in this.frameArrivalTimes)
            {
                if (time + 1f < Time.time)
                {
                    removeCount++;
                }
                else
                {
                    break;
                }
            }

            this.frameArrivalTimes.RemoveRange(0, removeCount);

            for (int i = 0; i < this.fpsFrameCounter; i++)
            {
                this.frameArrivalTimes.Add(Time.time);
            }

            this.FrameArrivalRate = this.frameArrivalTimes.Count;

            this.fpsFrameCounter = 0;
        }

        /// <summary>
        /// Initialize skeleton
        /// </summary>
        private void InvokeInitializeSkeleton()
        {
            // Don't process if the skeleton definition data has not been updated
            if (this.oldSkeletonDefinitionData != null)
            {
                var newdef = this.skeletonDefinitionData;
                var olddef = this.oldSkeletonDefinitionData.Value;
                bool isSkeletonChanged =
                    !newdef.BoneIds.SequenceEqual(olddef.BoneIds) ||
                    !newdef.ParentBoneIds.SequenceEqual(olddef.ParentBoneIds) ||
                    !newdef.RotationsX.SequenceEqual(olddef.RotationsX) ||
                    !newdef.RotationsY.SequenceEqual(olddef.RotationsY) ||
                    !newdef.RotationsZ.SequenceEqual(olddef.RotationsZ) ||
                    !newdef.RotationsW.SequenceEqual(olddef.RotationsW) ||
                    !newdef.PositionsX.SequenceEqual(olddef.PositionsX) ||
                    !newdef.PositionsY.SequenceEqual(olddef.PositionsY) ||
                    !newdef.PositionsZ.SequenceEqual(olddef.PositionsZ);

                if (isSkeletonChanged == false)
                {
                    return;
                }
            }

            this.isSkeletonInitialized = false;

            // Regenerate when new skeleton definition data comes
            Destroy(this.avatarRootObj);
            this.avatarRootObj = new GameObject("SkeletonRoot");
            this.avatarRootObj.transform.SetParent(transform);
            this.avatarRootObj.transform.localPosition = Vector3.zero;
            this.avatarRootObj.transform.localRotation = Quaternion.identity;
            this.avatarRootObj.transform.localScale = Vector3.one;

            foreach (MocopiBone bone in this.bones)
            {
                Destroy(bone.Transform.gameObject);
            }

            this.bones.Clear();

            this.bonePositions.Clear();
            this.boneRotations.Clear();

            for (int i = 0; i < this.skeletonDefinitionData.BoneIds.Length; i++)
            {
                string boneName = this.GetMocopiBoneNameByBoneId(this.skeletonDefinitionData.BoneIds[i]);

                if (string.IsNullOrEmpty(boneName))
                {
                    continue;
                }

                GameObject obj = new GameObject(boneName);

                // align with Unity's coordinate space
                Vector3 position = this.ConvertPluginDataToVector3(
                    this.skeletonDefinitionData.PositionsX[i],
                    this.skeletonDefinitionData.PositionsY[i],
                    this.skeletonDefinitionData.PositionsZ[i]
                );
                Quaternion rotation = this.ConvertPluginDataToQuaternion(
                    this.skeletonDefinitionData.RotationsX[i],
                    this.skeletonDefinitionData.RotationsY[i],
                    this.skeletonDefinitionData.RotationsZ[i],
                    this.skeletonDefinitionData.RotationsW[i]
                );

                obj.transform.localPosition = position;
                obj.transform.localRotation = rotation;

                MocopiBone bone = new MocopiBone
                {
                    BoneName = boneName,
                    Id = this.skeletonDefinitionData.BoneIds[i],
                    ParentId = this.skeletonDefinitionData.ParentBoneIds[i],
                    Transform = obj.transform
                };

                this.bones.Add(bone);

                this.bonePositions.Add(bone, position);
                this.boneRotations.Add(bone, rotation);
            }

            foreach (MocopiBone bone in this.bones)
            {
                MocopiBone parent = this.bones.Find(_ => _.Id == bone.ParentId);

                Vector3 position = bone.Transform.localPosition;
                Quaternion rotation = bone.Transform.localRotation;

                if (parent != null)
                {
                    bone.Transform.SetParent(parent.Transform);
                }

                if (bone.Transform.parent == null)
                {
                    bone.Transform.SetParent(this.avatarRootObj.transform);
                }

                bone.Transform.localPosition = position;
                bone.Transform.localRotation = rotation;
            }

            HumanBone[] humanBones = new HumanBone[this.bones.Count];
            SkeletonBone[] skeletonBones = new SkeletonBone[this.bones.Count + 1];

            int humanBoneIndex = 0;
            foreach (string name in HumanTrait.BoneName)
            {
                int id = this.GetBoneIdByHumanBoneName(name);

                if (id >= 0)
                {
                    HumanBone humanBone = new HumanBone
                    {
                        humanName = name,
                        boneName = this.GetMocopiBoneNameByBoneId(id)
                    };
                    humanBone.limit.useDefaultValues = true;

                    humanBones[humanBoneIndex++] = humanBone;
                }
            }

            SkeletonBone baseSkeletonBone = new SkeletonBone
            {
                name = this.avatarRootObj.name,
                position = Vector3.zero,
                rotation = Quaternion.identity,
                scale = Vector3.one
            };

            skeletonBones[0] = baseSkeletonBone;

            for (int i = 0; i < this.bones.Count; i++)
            {
                MocopiBone bone = this.bones[i];

                SkeletonBone skeletonBone = new SkeletonBone
                {
                    name = bone.BoneName,
                    position = bone.Transform.localPosition,
                    rotation = bone.Transform.localRotation,
                    scale = Vector3.one
                };

                skeletonBones[i + 1] = skeletonBone;
            }

            HumanDescription humanDescription = new HumanDescription
            {
                human = humanBones,
                skeleton = skeletonBones,
                upperArmTwist = 0.5f,
                lowerArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                armStretch = 0.05f,
                legStretch = 0.05f,
                feetSpacing = 0.0f,
                hasTranslationDoF = false
            };

            Destroy(this.avatar);
            this.avatar = AvatarBuilder.BuildHumanAvatar(this.avatarRootObj, humanDescription);

            if (this.humanPoseHandlerSrc != null)
            {
                this.humanPoseHandlerSrc.Dispose();
            }

            if (this.humanPoseHandlerDst != null)
            {
                this.humanPoseHandlerDst.Dispose();
            }

            this.humanPoseHandlerSrc = new HumanPoseHandler(this.avatar, this.avatarRootObj.transform);
            this.humanPoseHandlerDst = new HumanPoseHandler(this.Animator.avatar, transform);

            this.oldSkeletonDefinitionData = this.skeletonDefinitionData;

            this.isSkeletonInitialized = true;
        }

        /// <summary>
        /// Update skeleton
        /// </summary>
        private void InvokeUpdateSkeleton()
        {
            if (this.IsLockSkeletonUpdate)
            {
                return;
            }

            // If there is an AnimatorController, remove it because it interferes with updating the skeleton
            if (this.isRemoveAnimatorControllerOnUpdate && this.Animator.runtimeAnimatorController != null)
            {
                this.Animator.runtimeAnimatorController = null;
            }

            for (int i = 0; i < this.skeletonData.BoneIds.Length; i++)
            {
                MocopiBone bone = this.bones.Find(_ => _.Id == this.skeletonData.BoneIds[i]);
                if (bone == null)
                {
                    continue;
                }

                // align with Unity's coordinate space
                Vector3 position = this.ConvertPluginDataToVector3(
                    this.skeletonData.PositionsX[i],
                    this.skeletonData.PositionsY[i],
                    this.skeletonData.PositionsZ[i]
                );
                Quaternion rotation = this.ConvertPluginDataToQuaternion(
                    this.skeletonData.RotationsX[i],
                    this.skeletonData.RotationsY[i],
                    this.skeletonData.RotationsZ[i],
                    this.skeletonData.RotationsW[i]
                );

                if (bone.ParentId < 0)
                {
                    this.bonePositions[bone] = position;
                }

                this.boneRotations[bone] = rotation;
            }
        }

        /// <summary>
        /// Buffering avatar pose
        /// </summary>
        private void BufferAvatarPose()
        {
            foreach (MocopiBone bone in this.bones)
            {
                bone.Transform.localPosition = this.bonePositions[bone];
                bone.Transform.localRotation = this.boneRotations[bone];
            }

            int frameId = skeletonData.FrameId;
            double timestamp = skeletonData.Timestamp;
            double realtimestamp = Time.realtimeSinceStartup;
            int index = (frameId == -1 ? this.frameCount++ : frameId) % BUFFER_COUNT;

            if (lastBufferIndex == -1)
            {
                lastBufferIndex = index;
            }

            if (index - lastBufferIndex < 0)
            {
                for (int i = lastBufferIndex + 1; i < BUFFER_COUNT; i++)
                {
                    this.poseBuffer[i].frameId = 0;
                }

                for (int i = 0; i < index; i++)
                {
                    this.poseBuffer[i].frameId = 0;
                }
            }
            else
            {
                for (int i = lastBufferIndex + 1; i < index; i++)
                {
                    this.poseBuffer[i].frameId = 0;
                }
            }

            // First receive
            if (lastUsedIndex == -1 || timestamp < lastReceivedTimestamp || timestamp > lastReceivedTimestamp + MAX_DELAY_TIME || frameId == -1 || 
                lastReceivedFrameId == -1 || lastBufferResetTime + INITIALIZATION_TIME_AT_START_RECEPTION > Time.realtimeSinceStartup)
            {
                //Clear buffer
                for (int i = 0; i < BUFFER_COUNT; i++)
                {
                    this.poseBuffer[i].frameId = 0;
                }

                startRealtime = Time.realtimeSinceStartup;
                currentDelayTime = 0.0f;
                startTimestamp = timestamp;
                lastDelayTime = 0;
                lastUsedIndex = index;
                lastReceivedFrameId = frameId;
                lastReceivedTimestamp = timestamp;
                lastReceivedRealTimestamp = realtimestamp;
            }

            var timestampDiff = timestamp - lastReceivedTimestamp;
            var realtimestampDiff = realtimestamp - lastReceivedRealTimestamp;
            var delayTime = timestampDiff + realtimestampDiff - Time.unscaledDeltaTime * 2.0;

            if (delayTime < 0) delayTime = 0;

            if (frameId == -1)
            {
                currentDelayTime = 0; //BVH
            }
            else if (currentDelayTime < delayTime)
            {
                if (timestampDiff / realtimestampDiff < 0.2) // When packets arrive too late
                {
                    lastDelayTime = delayTime;
                }
                else if (lastDelayTime == 0) // When there is a delay due to packet loss, etc.
                {
                    currentDelayTime = delayTime;
                }
                else // When the last packet was significantly delayed
                {
                    if (delayTime / lastDelayTime > 0.8) // When both the previous packet and the current packet are delayed
                    {
                        currentDelayTime = (delayTime + lastDelayTime) / 2;
                    }
                    else // This packet was not late
                    {
                        currentDelayTime = delayTime;
                    }

                    lastDelayTime = 0;
                }
            }
            else
            {
                lastDelayTime = 0;
                currentDelayTime = currentDelayTime * (1 - DelayRecoveryRate) + delayTime * DelayRecoveryRate;
            }

            if (currentDelayTime > MAX_DELAY_TIME)
            {
                // Reset delay time
                currentDelayTime = 0f;
            }

            lastBufferIndex = index;
            lastReceivedFrameId = frameId;
            lastReceivedTimestamp = timestamp;
            lastReceivedRealTimestamp = realtimestamp;

            this.poseBuffer[lastBufferIndex].frameId = frameId;
            this.poseBuffer[lastBufferIndex].timestamp = timestamp;
            this.humanPoseHandlerSrc.GetHumanPose(ref this.poseBuffer[lastBufferIndex].pose);
        }

        /// <summary>
        /// Find next pose
        /// </summary>
        /// <param name="startIndex">Start index</param>
        /// <returns>next pose</returns>
        private (int bufferIndex, (int frameId, double timestamp, HumanPose pose) data) FindNextPose(int startIndex)
        {
            var next = (startIndex, poseBuffer[startIndex]);

            for (int i = 1; i < BUFFER_COUNT; i++)
            {
                int index = (startIndex + i) % BUFFER_COUNT;
                if (poseBuffer[index].frameId != 0 && poseBuffer[index].frameId > poseBuffer[startIndex].frameId)
                {
                    next = (index, poseBuffer[index]);
                    break;
                }
            }

            return next;
        }

        /// <summary>
        /// Get current timestamp
        /// </summary>
        /// <returns>current timestamp</returns>
        private double GetCurrentTimestamp()
        {
            return startTimestamp - currentDelayTime + Time.realtimeSinceStartup -
                startRealtime;
        }

        /// <summary>
        /// Updated avatar pose
        /// </summary>
        private void UpdateAvatarPose()
        {
            if (lastUsedIndex == -1) return;

            var targetPose = temppose;

            if (IsBufferingEnabled)
            {
                var lastData = poseBuffer[lastUsedIndex];
                var next = FindNextPose(lastUsedIndex);

                // Here, if next is before the current time, search for next
                if (next.data.timestamp < GetCurrentTimestamp())
                {
                    lastData = next.data;
                    lastUsedIndex = next.bufferIndex;
                    next = FindNextPose(next.bufferIndex);
                }

                var nextData = next.data;

                float t = Mathf.Clamp01(lastData.frameId == nextData.frameId ? 1.0f : (float)((GetCurrentTimestamp() - lastData.timestamp) / (nextData.timestamp - lastData.timestamp)));

                LerpHumanPose(ref targetPose, ref lastData.pose, ref nextData.pose, t);

                var lerptimestamp = t == 0 ? lastData.timestamp : lastData.timestamp + (nextData.timestamp - lastData.timestamp) / (1 / t);
            }
            else
            {
                targetPose = this.poseBuffer[lastBufferIndex].pose;
            }

            if (this.MotionSmoothness > 0)
            {
                float fps = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60f;
                float lerpValue = Mathf.Lerp(1f, 0.3f, Mathf.Clamp01(Time.deltaTime * fps * this.MotionSmoothness));

                if (lastMotionSmoothness != MotionSmoothness)
                {
                    lastMotionSmoothness = MotionSmoothness;
                    smoothnesspose = targetPose;
                }

                LerpHumanPose(ref this.smoothnesspose, ref targetPose, ref this.smoothnesspose, lerpValue);
                this.humanPoseHandlerDst.SetHumanPose(ref this.smoothnesspose);
            }
            else
            {
                this.humanPoseHandlerDst.SetHumanPose(ref targetPose);
            }
        }

        /// <summary>
        /// Leap human pose
        /// </summary>
        /// <param name="outHumanPose">Human pose</param>
        /// <param name="a">Starting point information</param>
        /// <param name="b">Destination information</param>
        /// <param name="t">Ratio</param>
        private void LerpHumanPose(ref HumanPose outHumanPose, ref HumanPose a, ref HumanPose b, float t)
        {
            if (outHumanPose.muscles == null)
            {
                outHumanPose.muscles = new float[a.muscles.Length];
            }

            outHumanPose.bodyPosition = Vector3.Lerp(a.bodyPosition, b.bodyPosition, t);
            outHumanPose.bodyRotation = Quaternion.Lerp(a.bodyRotation, b.bodyRotation, t);

            for (int i = 0; i < outHumanPose.muscles.Length; i++)
            {
                outHumanPose.muscles[i] = Mathf.Lerp(a.muscles[i], b.muscles[i], t);
            }
        }

        /// <summary>
        /// Get name of bone
        /// </summary>
        /// <param name="boneId">mocopi Avatar bone ID</param>
        /// <returns>mocopi Sensor bone name</returns>
        private string GetMocopiBoneNameByBoneId(int boneId)
        {
            foreach (KeyValuePair<string, int> pair in MOCOPI_BONE_NAME_TO_MOCOPI_BONE_ID)
            {
                if (pair.Value == boneId)
                {
                    return pair.Key;
                }
            }

            return "";
        }

        /// <summary>
        /// Get id of bone
        /// </summary>
        /// <param name="humanBoneName">mocopi Sensor bone name</param>
        /// <returns>mocopi Avatar bone ID</returns>
        private int GetBoneIdByHumanBoneName(string humanBoneName)
        {
            if (HUMAN_BONE_NAME_TO_MOCOPI_BONE_ID.ContainsKey(humanBoneName))
            {
                return HUMAN_BONE_NAME_TO_MOCOPI_BONE_ID[humanBoneName];
            }

            return -1;
        }

        /// <summary>
        /// Convert the obtained location information to Unity coordinates
        /// </summary>
        /// <param name="x">bone position</param>
        /// <param name="y">bone position</param>
        /// <param name="z">bone position</param>
        /// <returns>Converted Unity coordinates</returns>
        private Vector3 ConvertPluginDataToVector3(double x, double y, double z)
        {
            return new Vector3(
                -(float)x,
                (float)y,
                (float)z
            );
        }

        /// <summary>
        /// Convert the obtained rotation angle to Unity coordinates
        /// </summary>
        /// <param name="x">bone rotation</param>
        /// <param name="y">bone rotation</param>
        /// <param name="z">bone rotation</param>
        /// <param name="w">bone rotation</param>
        /// <returns>Converted Unity Coordinates Quaternion</returns>
        private Quaternion ConvertPluginDataToQuaternion(double x, double y, double z, double w)
        {
            return new Quaternion(
                -(float)x,
                (float)y,
                (float)z,
                -(float)w
            );
        }

        /// <summary>
        /// Get last timecode values
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        /// <param name="sec"></param>
        /// <param name="frame"></param>
        public void GetTimecode(out int hour, out int min, out int sec, out int frame)
        {
            hour = skeletonData.Timecode.Hour;
            min = skeletonData.Timecode.Minute;
            sec = skeletonData.Timecode.Second;
            frame = skeletonData.Timecode.Frame;
        }
        #endregion --Methods--

        #region --Structs--
        /// <summary>
        /// skeleton definition data structure
        /// </summary>
        private struct SkeletonDefinitionData
        {
            /// <summary>
            /// mocopi Avatar bone id list
            /// </summary>
            public int[] BoneIds;

            /// <summary>
            /// List of IDs of parent bones for each bone
            /// </summary>
            public int[] ParentBoneIds;

            /// <summary>
            /// Rotation angle of each bone in initial posture
            /// </summary>
            public float[] RotationsX;

            /// <summary>
            /// Rotation angle of each bone in initial posture
            /// </summary>
            public float[] RotationsY;

            /// <summary>
            /// Rotation angle of each bone in initial posture
            /// </summary>
            public float[] RotationsZ;

            /// <summary>
            /// Rotation angle of each bone in initial posture
            /// </summary>
            public float[] RotationsW;

            /// <summary>
            /// Position of each bone in initial pose
            /// </summary>
            public float[] PositionsX;

            /// <summary>
            /// Position of each bone in initial pose
            /// </summary>
            public float[] PositionsY;

            /// <summary>
            /// Position of each bone in initial pose
            /// </summary>
            public float[] PositionsZ;
        }

        /// <summary>
        /// Skeleton update data structure
        /// </summary>
        private struct SkeletonData
        {
            /// <summary>
            /// Frame ID
            /// </summary>
            public int FrameId;

            /// <summary>
            /// Timestamp
            /// </summary>
            public float Timestamp;

            /// <summary>
            /// Unix time when sensor sent data
            /// </summary>
            public double UnixTime;

            /// <summary>
            /// Timecode
            /// </summary>
            public Timecode Timecode;

            /// <summary>
            /// mocopi Avatar bone id list
            /// </summary>
            public int[] BoneIds;

            /// <summary>
            /// Rotation angle of each bone in initial posture
            /// </summary>
            public float[] RotationsX;

            /// <summary>
            /// Rotation angle of each bone in initial posture
            /// </summary>
            public float[] RotationsY;

            /// <summary>
            /// Rotation angle of each bone in initial posture
            /// </summary>
            public float[] RotationsZ;

            /// <summary>
            /// Rotation angle of each bone in initial posture
            /// </summary>
            public float[] RotationsW;

            /// <summary>
            /// Position of each bone in initial pose
            /// </summary>
            public float[] PositionsX;

            /// <summary>
            /// Position of each bone in initial pose
            /// </summary>
            public float[] PositionsY;

            /// <summary>
            /// Position of each bone in initial pose
            /// </summary>
            public float[] PositionsZ;
        }

        /// <summary>
        /// Timecode structure
        /// </summary>
        private struct Timecode
        {
            /// <summary>
            /// Timecode hour
            /// </summary>
            public int Hour;

            /// <summary>
            /// Timecode minute
            /// </summary>
            public int Minute;

            /// <summary>
            /// Timecode second
            /// </summary>
            public int Second;

            /// <summary>
            /// Timecode frame
            /// </summary>
            public int Frame;

            /// <summary>
            /// Timecode drop frame
            /// </summary>
            public bool DropFrame;
        }
        #endregion --Structs--

        #region --Classes--
        /// <summary>
        /// Class that manages mocopi bone information
        /// </summary>
        private sealed class MocopiBone
        {
            /// <summary>
            /// Bone name
            /// </summary>
            public string BoneName;

            /// <summary>
            /// Bone id
            /// </summary>
            public int Id;

            /// <summary>
            /// Parent bone id
            /// </summary>
            public int ParentId;

            /// <summary>
            /// Transform
            /// </summary>
            public Transform Transform;
        }
        #endregion --Classes--
    }
}
