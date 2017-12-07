using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RuntimeGizmos
{
    public class TransformRotatingAll : TransformTypeHandler
    {
        Transform transform;
        public TransformType type { get { return TransformType.Rotate; } }

        public TransformRotatingAll(Transform transform)
        {
            this.transform = transform;
        }

        float allRotateSpeedMultiplier = 20f;
        Quaternion totalRotationAmount = Quaternion.identity;
        TransformData data;
        Transform target;
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
            Vector3 rotation = transform.TransformDirection(new Vector3(Input.GetAxis("Mouse Y"), -Input.GetAxis("Mouse X"), 0));
            target.Rotate(rotation * allRotateSpeedMultiplier, Space.World);
            totalRotationAmount *= Quaternion.Euler(rotation * allRotateSpeedMultiplier);
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
