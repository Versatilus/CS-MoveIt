﻿using System;
using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;

namespace MoveIt
{
    class AlignMirrorAction : CloneAction
    {
        private bool containsNetwork = false;

        public Vector3 mirrorPivot;
        public float mirrorAngle;
        private Bounds originalBounds;

        public override void Do()
        {
            originalBounds = GetTotalBounds(false);

            base.Do();
        }

        public void DoProcess()
        {
            Matrix4x4 matrix4x = default;
            foreach (Instance instance in m_clones)
            {
                if (instance.isValid)
                {
                    InstanceState state = null;

                    foreach (KeyValuePair<Instance, Instance> pair in m_origToClone)
                    {
                        if (pair.Value.id.RawData == instance.id.RawData)
                        {               
                            if (pair.Value is MoveableSegment)
                            { // Segments need original state because nodes move before clone's position is saved
                                state = pair.Key.SaveToState();
                            }
                            else
                            { // Buildings need clone state to access correct subInstances. Others don't matter, but clone makes most sense
                                state = pair.Value.SaveToState();
                            }
                            break;
                        }
                    }
                    if (state == null)
                    {
                        throw new NullReferenceException($"Original for cloned object not found.");
                    }

                    float faceDelta = getMirrorFacingDelta(state.angle, mirrorAngle);
                    float posDelta = getMirrorPositionDelta(state.position, mirrorPivot, mirrorAngle);

                    matrix4x.SetTRS(mirrorPivot, Quaternion.AngleAxis(posDelta * Mathf.Rad2Deg, Vector3.down), Vector3.one);

                    instance.Transform(state, ref matrix4x, 0f, faceDelta, mirrorPivot, followTerrain);
                }
            }

            bool fast = MoveItTool.fastMove != Event.current.shift;
            UpdateArea(originalBounds, !fast || containsNetwork);
            UpdateArea(GetTotalBounds(false), !fast);
        }

        public static float getMirrorFacingDelta(float startAngle, float mirrorAngle)
        {
            return (startAngle - ((startAngle - mirrorAngle) * 2) - startAngle) % ((float)Math.PI * 2);
        }

        public static float getMirrorPositionDelta(Vector3 start, Vector3 mirrorOrigin, float angle)
        {
            Vector3 offset = start - mirrorOrigin;
            return (angle + Mathf.Atan2(offset.x, offset.z)) * 2;
        }
    }
}
