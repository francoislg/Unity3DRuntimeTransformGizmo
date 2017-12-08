using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace RuntimeGizmos
{
    [RequireComponent(typeof(Camera))]
    public class TransformGizmo : MonoBehaviour
    {
        public TransformSpace space = TransformSpace.Global;

        //These are the same as the unity editor hotkeys
        public KeyCode SetMoveKey = KeyCode.W;
        public KeyCode SetRotateKey = KeyCode.E;
        public KeyCode SetScaleKey = KeyCode.R;
        public KeyCode SetSpaceToggle = KeyCode.X;

        float minSelectedDistanceCheck = .04f;

        private TransformMoving movingState;
        private TransformRotating rotatingState;
        private TransformScaling scalingState;
        private TransformTypeHandler currentState;

        bool isTransforming;
        Axis selectedAxis = Axis.None;
        AxisInfo axisInfo;
        Transform target;
        Camera myCamera;
        DrawingShapes drawingShapes;

        void Awake()
        {
            movingState = new TransformMoving();
            rotatingState = new TransformRotating(transform);
            scalingState = new TransformScaling(transform);
            currentState = movingState;
            myCamera = GetComponent<Camera>();
            drawingShapes = new DrawingShapes(transform);
            drawingShapes.Initialize();
        }

        void Update()
        {
            HandleKeysForSpaceAndType();
            SelectAxis();
            UpdateSelectedTarget();
            if (target == null) return;

            TransformSelected();
        }

        void LateUpdate()
        {
            if (target == null) return;

            //We run this in lateupdate since coroutines run after update and we want our gizmos to have the updated target transform position after TransformSelected()
            ComputeAxisInfo();
            if (isTransforming) {
                currentState.LateTransforming(axisInfo);
            }
            drawingShapes.Clear();
            currentState.SetShapesToDraw(axisInfo, drawingShapes);
        }

        void OnPostRender()
        {
            if (target == null) return;

            drawingShapes.Draw(selectedAxis);
        }

        void HandleKeysForSpaceAndType()
        {
            TransformTypeHandler currentHandler = currentState;
            if (Input.GetKeyDown(SetMoveKey)) {
                currentState = movingState;
            } else if (Input.GetKeyDown(SetRotateKey)) {
                currentState = rotatingState;
            } else if (Input.GetKeyDown(SetScaleKey)) {
                currentState = scalingState;
            } else if (Input.GetKeyDown(SetSpaceToggle)) {
                space = space == TransformSpace.Global ? TransformSpace.Local : TransformSpace.Local;
            }

            if (currentState != currentHandler) {
                currentState.SetTarget(target);
            }
        }

        void TransformSelected()
        {
            if (selectedAxis != Axis.None && Input.GetMouseButtonDown(0)) {
                StartCoroutine(PerformTransform());
            }
        }
		
		IEnumerator PerformTransform()
		{
			isTransforming = true;

            Vector3 axisDirection = GetSelectedAxisDirection(axisInfo);
            Vector3 planeNormal = (transform.position - target.position).normalized;
            Vector3 originalTargetPosition = target.position;
            TransformData data = new TransformData {
				originalTargetPosition = target.position,
				planeNormal = planeNormal,
                selectedAxis = selectedAxis,
                axisDirection = axisDirection,
				projectedAxis = Vector3.ProjectOnPlane(axisDirection, planeNormal).normalized,
				distanceMultiplier = GetDistanceMultiplier(),
                space = space
			};
			Vector3 previousMousePosition = Vector3.zero;

            currentState.OnBeginTransforming(data);

            Debug.Log(selectedAxis);

			while(!Input.GetMouseButtonUp(0))
			{
				Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);
				Vector3 mousePosition = Geometry.LinePlaneIntersect(mouseRay.origin, mouseRay.direction, originalTargetPosition, planeNormal);
				
				if (previousMousePosition != Vector3.zero && mousePosition != Vector3.zero)
				{
					Vector3 mousePositionMovement = mousePosition - previousMousePosition;
                    currentState.Transforming(mousePositionMovement);
				}

				previousMousePosition = mousePosition;

				yield return null;
			}
            
			isTransforming = false;
		}

		Vector3 GetSelectedAxisDirection(AxisInfo axisInfo)
		{
			if(selectedAxis != Axis.None)
			{
				if(selectedAxis == Axis.X) return axisInfo.xDirection;
				if(selectedAxis == Axis.Y) return axisInfo.yDirection;
				if(selectedAxis == Axis.Z) return axisInfo.zDirection;
				if(selectedAxis == Axis.Any) return Vector3.one;
			}
			return Vector3.zero;
		}
	
		void UpdateSelectedTarget()
		{
			if(selectedAxis == Axis.None && Input.GetMouseButtonDown(0)) {
				RaycastHit hitInfo; 
				if (Physics.Raycast(myCamera.ScreenPointToRay(Input.mousePosition), out hitInfo)) {
					target = hitInfo.transform;
                    currentState.SetTarget(target);
				} else {
					target = null;
				}
			}
		}

		AxisVectors selectedLinesBuffer = new AxisVectors();
		void SelectAxis()
		{
			if(!Input.GetMouseButtonDown(0)) return;
			selectedAxis = Axis.None;

			float xClosestDistance = float.MaxValue;
			float yClosestDistance = float.MaxValue;
			float zClosestDistance = float.MaxValue;
			float allClosestDistance = float.MaxValue;
			float minSelectedDistanceCheck = this.minSelectedDistanceCheck * GetDistanceMultiplier();

            selectedLinesBuffer = drawingShapes.GetBufferedVectors();

            xClosestDistance = ClosestDistanceFromMouseToLines(selectedLinesBuffer.x);
            yClosestDistance = ClosestDistanceFromMouseToLines(selectedLinesBuffer.y);
            zClosestDistance = ClosestDistanceFromMouseToLines(selectedLinesBuffer.z);
            allClosestDistance = ClosestDistanceFromMouseToLines(selectedLinesBuffer.all);

            if (currentState.type == TransformType.Scale && allClosestDistance <= minSelectedDistanceCheck) {
                selectedAxis = Axis.Any;
            } else if (xClosestDistance <= minSelectedDistanceCheck && xClosestDistance <= yClosestDistance && xClosestDistance <= zClosestDistance) {
                selectedAxis = Axis.X;
            } else if (yClosestDistance <= minSelectedDistanceCheck && yClosestDistance <= xClosestDistance && yClosestDistance <= zClosestDistance) {
                selectedAxis = Axis.Y;
            } else if (zClosestDistance <= minSelectedDistanceCheck && zClosestDistance <= xClosestDistance && zClosestDistance <= yClosestDistance) {
                selectedAxis = Axis.Z;
            } else if (currentState.type == TransformType.Rotate && target != null) {
                Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);
                Vector3 mousePlaneHit = Geometry.LinePlaneIntersect(mouseRay.origin, mouseRay.direction, target.position, (transform.position - target.position).normalized);
                if ((target.position - mousePlaneHit).sqrMagnitude <= (drawingShapes.handleLength * GetDistanceMultiplier()).Squared()) selectedAxis = Axis.Any;
            }
		}

		float ClosestDistanceFromMouseToLines(List<Vector3> lines)
		{
			Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);

			float closestDistance = float.MaxValue;
			for(int i = 0; i < lines.Count; i += 2)
			{
				IntersectPoints points = Geometry.ClosestPointsOnSegmentToLine(lines[i], lines[i + 1], mouseRay.origin, mouseRay.direction);
				float distance = Vector3.Distance(points.first, points.second);
				if(distance < closestDistance)
				{
					closestDistance = distance;
				}
			}
			return closestDistance;
		}

		void ComputeAxisInfo()
		{
            float size = drawingShapes.handleLength * GetDistanceMultiplier();
            axisInfo.Set(target, size, space);
        }

		//This helps keep the size consistent no matter how far we are from it.
		float GetDistanceMultiplier()
		{
			if(target == null) return 0f;
			return Mathf.Max(.01f, Mathf.Abs(ExtVector3.MagnitudeInDirection(target.position - transform.position, myCamera.transform.forward)));
		}
    }

}