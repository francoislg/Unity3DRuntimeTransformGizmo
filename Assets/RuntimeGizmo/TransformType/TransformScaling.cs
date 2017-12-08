using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RuntimeGizmos
{
    public class TransformScaling : TransformTypeHandler
    {
        Transform transform;
        public TransformType type { get { return TransformType.Scale; } }

        public TransformScaling(Transform transform)
        {
            this.transform = transform;
        }

        float scaleSpeedMultiplier = 1.0f;
        float totalScaleAmount = 0;
        Transform target;

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        public void OnBeginTransforming()
        {
            this.totalScaleAmount = 0;
        }

        public void Transforming(Vector3 mouseMovement, TransformData data)
        {
            Vector3 projected = (data.selectedAxis == Axis.Any) ? transform.right : data.projectedAxis;
            float scaleAmount = ExtVector3.MagnitudeInDirection(mouseMovement, projected) * scaleSpeedMultiplier;

            //WARNING - There is a bug in unity 5.4 and 5.5 that causes InverseTransformDirection to be affected by scale which will break negative scaling. Not tested, but updating to 5.4.2 should fix it - https://issuetracker.unity3d.com/issues/transformdirection-and-inversetransformdirection-operations-are-affected-by-scale
            Vector3 localAxis = (data.space == TransformSpace.Local && data.selectedAxis != Axis.Any) ? target.InverseTransformDirection(data.axisDirection) : data.axisDirection;

            if (data.selectedAxis == Axis.Any) target.localScale += (ExtVector3.Abs(target.localScale.normalized) * scaleAmount);
            else target.localScale += (localAxis * scaleAmount);

            totalScaleAmount += scaleAmount;
        }

        public void LateTransforming(AxisInfo axisInfo, Axis selectedAxis)
        {
            if (selectedAxis == Axis.X || selectedAxis == Axis.Any) axisInfo.xAxisEnd += (axisInfo.xDirection * totalScaleAmount);
            if (selectedAxis == Axis.Y || selectedAxis == Axis.Any) axisInfo.yAxisEnd += (axisInfo.yDirection * totalScaleAmount);
            if (selectedAxis == Axis.Z || selectedAxis == Axis.Any) axisInfo.zAxisEnd += (axisInfo.zDirection * totalScaleAmount);
        }

        public void SetShapesToDraw(AxisInfo axisInfo, BuildingShapes buildingShapes, TransformSpace space, float distanceMultiplier)
        {
            buildingShapes.SetHandleLines(target, axisInfo);
            buildingShapes.SetHandleSquares(target, axisInfo, distanceMultiplier);
        }
    }
}
