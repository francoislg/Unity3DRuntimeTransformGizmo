﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RuntimeGizmos
{
    public class TransformMoving : TransformTypeHandler
    {
        float moveSpeedMultiplier = 1f;

        Transform target;

        public TransformType type { get { return TransformType.Move; } }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        public void OnBeginTransforming()
        {
        }

        public void Transforming(Vector3 mouseMovement, TransformData data)
        {
            float moveAmount = ExtVector3.MagnitudeInDirection(mouseMovement, data.projectedAxis) * moveSpeedMultiplier;
            target.Translate(data.axisDirection * moveAmount, Space.World);
        }

        public void LateTransforming(AxisInfo axisInfo, Axis selectedAxis)
        {
        }

        public void SetShapesToDraw(AxisInfo axisInfo, BuildingShapes buildingShapes, TransformSpace space, float distanceMultiplier)
        {
            buildingShapes.SetHandleLines(target, axisInfo);
            buildingShapes.SetHandleTriangles(axisInfo, distanceMultiplier);
        }
    }
}
