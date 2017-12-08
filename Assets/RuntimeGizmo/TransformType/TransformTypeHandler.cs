using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RuntimeGizmos
{
    public interface TransformTypeHandler
    {
        TransformType type { get; }
        void SetTarget(Transform target);
        void OnBeginTransforming();
        void Transforming(Vector3 mouseMovement, TransformData data);
        void LateTransforming(AxisInfo axisInfo, Axis selectedAxis);
        void SetShapesToDraw(AxisInfo axisInfo, BuildingShapes buildingShapes, TransformSpace space, float distanceMultiplier);
    }

    public struct TransformData
    {
        public Vector3 originalTargetPosition;
        public Vector3 planeNormal;
        public Vector3 axisDirection;
        public Axis selectedAxis;
        public Vector3 projectedAxis;
        public float distanceMultiplier;
        public TransformSpace space;
    }
}
