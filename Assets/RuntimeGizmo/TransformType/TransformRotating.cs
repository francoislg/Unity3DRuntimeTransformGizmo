using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RuntimeGizmos
{
    public class TransformRotating : TransformTypeHandler
    {
        float rotateSpeedMultiplier = 200f;
        Quaternion totalRotationAmount = Quaternion.identity;
        TransformData data;
        Transform target;
        public TransformType type { get { return TransformType.Rotate; } }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        public void OnBeginTransforming(TransformData data)
        {
            totalRotationAmount = Quaternion.identity;
            this.data = data;
        }

        public void Transforming(Vector3 mouseMovement)
        {
            Vector3 projected = (data.selectedAxis == Axis.Any || ExtVector3.IsParallel(data.axisDirection, data.planeNormal)) ? data.planeNormal : Vector3.Cross(data.axisDirection, data.planeNormal);
            float rotateAmount = (ExtVector3.MagnitudeInDirection(mouseMovement, projected) * rotateSpeedMultiplier) / data.distanceMultiplier;
            target.Rotate(data.axisDirection, rotateAmount, Space.World);
            totalRotationAmount *= Quaternion.Euler(data.axisDirection * rotateAmount);
        }

        public void LateTransforming(AxisInfo axisInfo)
        {
        }

        public void SetShapesToDraw(AxisInfo axisInfo, BuildingShapes buildingShapes)
        {
            buildingShapes.SetCircles(target, axisInfo, data.distanceMultiplier);
        }
    }
}
