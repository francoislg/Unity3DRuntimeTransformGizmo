using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RuntimeGizmos
{
    public interface DrawingShapesHandler
    {
        void Initialize();
        AxisVectors GetBufferedVectors();
        void Draw(Axis selectedAxis);
    }

    public interface BuildingShapes
    {
        void SetHandleLines(Transform target, AxisInfo axisInfo);
        void SetHandleTriangles(AxisInfo axisInfo, float distanceMultiplier);
        void SetHandleSquares(Transform target, AxisInfo axisInfo, float distanceMultiplier);
        void SetCircles(Transform target, AxisInfo axisInfo, float distanceMultiplier);
    }

    public class DrawingShapes : DrawingShapesHandler, BuildingShapes
    {
        public float handleLength = .25f;
        float triangleSize = .03f;
        float boxSize = .01f;
        int circleDetail = 40;

        Color xColor = new Color(1, 0, 0, 0.8f);
        Color yColor = new Color(0, 1, 0, 0.8f);
        Color zColor = new Color(0, 0, 1, 0.8f);
        Color allColor = new Color(.7f, .7f, .7f, 0.8f);
        Color selectedColor = new Color(1, 1, 0, 0.8f);

        AxisVectors handleLines = new AxisVectors();
        AxisVectors handleTriangles = new AxisVectors();
        AxisVectors handleSquares = new AxisVectors();
        AxisVectors circlesLines = new AxisVectors();
        AxisVectors drawCurrentCirclesLines = new AxisVectors();

        static Material lineMaterial;

        void SetMaterial()
        {
            if (lineMaterial == null) {
                lineMaterial = new Material(Shader.Find("Custom/Lines"));
                #region Shader code
                /*
				Shader "Custom/Lines"
				{
					SubShader
					{
						Pass
						{
							Blend SrcAlpha OneMinusSrcAlpha
							ZWrite Off
							ZTest Always
							Cull Off
							Fog { Mode Off }

							BindChannels
							{
								Bind "vertex", vertex
								Bind "color", color
							}
						}
					}
				}
				*/
                #endregion
            }
        }

        Transform transform;

        public DrawingShapes(Transform transform)
        {
            this.transform = transform;
        }

        public void Initialize()
        {
            SetMaterial();
        }

        public AxisVectors GetBufferedVectors()
        {
            AxisVectors vectors = new AxisVectors();
            vectors.Add(handleLines);
            vectors.Add(handleTriangles);
            vectors.Add(handleSquares);
            vectors.Add(circlesLines);
            return vectors;
        }

        public void Draw(Axis selectedAxis)
        {
            lineMaterial.SetPass(0);

            Color xColor = (selectedAxis == Axis.X) ? selectedColor : this.xColor;
            Color yColor = (selectedAxis == Axis.Y) ? selectedColor : this.yColor;
            Color zColor = (selectedAxis == Axis.Z) ? selectedColor : this.zColor;
            Color allColor = (selectedAxis == Axis.Any) ? selectedColor : this.allColor;

            DrawLines(handleLines.x, xColor);
            DrawLines(handleLines.y, yColor);
            DrawLines(handleLines.z, zColor);

            DrawTriangles(handleTriangles.x, xColor);
            DrawTriangles(handleTriangles.y, yColor);
            DrawTriangles(handleTriangles.z, zColor);

            DrawSquares(handleSquares.x, xColor);
            DrawSquares(handleSquares.y, yColor);
            DrawSquares(handleSquares.z, zColor);
            DrawSquares(handleSquares.all, allColor);

            AxisVectors rotationAxisVector = circlesLines;/*
            if (isTransforming && space == TransformSpace.Global && type == TransformType.Rotate) {
                rotationAxisVector = drawCurrentCirclesLines;

                AxisInfo axisInfo = new AxisInfo();
                axisInfo.xDirection = totalRotationAmount * Vector3.right;
                axisInfo.yDirection = totalRotationAmount * Vector3.up;
                axisInfo.zDirection = totalRotationAmount * Vector3.forward;
                SetCircles(axisInfo, drawCurrentCirclesLines);
            }*/

            DrawCircles(rotationAxisVector.x, xColor);
            DrawCircles(rotationAxisVector.y, yColor);
            DrawCircles(rotationAxisVector.z, zColor);
            DrawCircles(rotationAxisVector.all, allColor);
        }

        public void Clear()
        {
            handleLines.Clear();
            handleTriangles.Clear();
            handleSquares.Clear();
            circlesLines.Clear();
        }

        public void SetHandleLines(Transform target, AxisInfo axisInfo)
        {
            handleLines.x.Add(target.position);
            handleLines.x.Add(axisInfo.xAxisEnd);
            handleLines.y.Add(target.position);
            handleLines.y.Add(axisInfo.yAxisEnd);
            handleLines.z.Add(target.position);
            handleLines.z.Add(axisInfo.zAxisEnd);
        }

        public void SetHandleTriangles(AxisInfo axisInfo, float distanceMultiplier)
        {
            float triangleLength = triangleSize * distanceMultiplier;
            AddTriangles(axisInfo.xAxisEnd, axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, triangleLength, handleTriangles.x);
            AddTriangles(axisInfo.yAxisEnd, axisInfo.yDirection, axisInfo.xDirection, axisInfo.zDirection, triangleLength, handleTriangles.y);
            AddTriangles(axisInfo.zAxisEnd, axisInfo.zDirection, axisInfo.yDirection, axisInfo.xDirection, triangleLength, handleTriangles.z);
        }

        void AddTriangles(Vector3 axisEnd, Vector3 axisDirection, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float size, List<Vector3> resultsBuffer)
        {
            Vector3 endPoint = axisEnd + (axisDirection * (size * 2f));
            Square baseSquare = GetBaseSquare(axisEnd, axisOtherDirection1, axisOtherDirection2, size / 2f);

            resultsBuffer.Add(baseSquare.bottomLeft);
            resultsBuffer.Add(baseSquare.topLeft);
            resultsBuffer.Add(baseSquare.topRight);
            resultsBuffer.Add(baseSquare.topLeft);
            resultsBuffer.Add(baseSquare.bottomRight);
            resultsBuffer.Add(baseSquare.topRight);

            for (int i = 0; i < 4; i++) {
                resultsBuffer.Add(baseSquare[i]);
                resultsBuffer.Add(baseSquare[i + 1]);
                resultsBuffer.Add(endPoint);
            }
        }

        public void SetHandleSquares(Transform target, AxisInfo axisInfo, float distanceMultiplier)
        {
            float boxLength = boxSize * distanceMultiplier;
            AddSquares(axisInfo.xAxisEnd, axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, boxLength, handleSquares.x);
            AddSquares(axisInfo.yAxisEnd, axisInfo.yDirection, axisInfo.xDirection, axisInfo.zDirection, boxLength, handleSquares.y);
            AddSquares(axisInfo.zAxisEnd, axisInfo.zDirection, axisInfo.xDirection, axisInfo.yDirection, boxLength, handleSquares.z);
            AddSquares(target.position - (axisInfo.xDirection * boxLength), axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, boxLength, handleSquares.all);
        }

        void AddSquares(Vector3 axisEnd, Vector3 axisDirection, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float size, List<Vector3> resultsBuffer)
        {
            Square baseSquare = GetBaseSquare(axisEnd, axisOtherDirection1, axisOtherDirection2, size);
            Square baseSquareEnd = GetBaseSquare(axisEnd + (axisDirection * (size * 2f)), axisOtherDirection1, axisOtherDirection2, size);

            resultsBuffer.Add(baseSquare.bottomLeft);
            resultsBuffer.Add(baseSquare.topLeft);
            resultsBuffer.Add(baseSquare.bottomRight);
            resultsBuffer.Add(baseSquare.topRight);

            resultsBuffer.Add(baseSquareEnd.bottomLeft);
            resultsBuffer.Add(baseSquareEnd.topLeft);
            resultsBuffer.Add(baseSquareEnd.bottomRight);
            resultsBuffer.Add(baseSquareEnd.topRight);

            for (int i = 0; i < 4; i++) {
                resultsBuffer.Add(baseSquare[i]);
                resultsBuffer.Add(baseSquare[i + 1]);
                resultsBuffer.Add(baseSquareEnd[i + 1]);
                resultsBuffer.Add(baseSquareEnd[i]);
            }
        }

        Square GetBaseSquare(Vector3 axisEnd, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float size)
        {
            Square square;
            Vector3 offsetUp = ((axisOtherDirection1 * size) + (axisOtherDirection2 * size));
            Vector3 offsetDown = ((axisOtherDirection1 * size) - (axisOtherDirection2 * size));
            //These arent really the proper directions, as in the bottomLeft isnt really at the bottom left...
            square.bottomLeft = axisEnd + offsetDown;
            square.topLeft = axisEnd + offsetUp;
            square.bottomRight = axisEnd - offsetDown;
            square.topRight = axisEnd - offsetUp;
            return square;
        }

        public void SetCircles(Transform target, AxisInfo axisInfo, float distanceMultiplier)
        {
            float circleLength = handleLength * distanceMultiplier;
            AddCircle(target.position, axisInfo.xDirection, circleLength, circlesLines.x);
            AddCircle(target.position, axisInfo.yDirection, circleLength, circlesLines.y);
            AddCircle(target.position, axisInfo.zDirection, circleLength, circlesLines.z);
            AddCircle(target.position, (target.position - transform.position).normalized, circleLength, circlesLines.all, false);
        }

        private void AddCircle(Vector3 origin, Vector3 axisDirection, float size, List<Vector3> resultsBuffer, bool depthTest = true)
        {
            Vector3 up = axisDirection.normalized * size;
            Vector3 forward = Vector3.Slerp(up, -up, .5f);
            Vector3 right = Vector3.Cross(up, forward).normalized * size;

            Matrix4x4 matrix = new Matrix4x4();

            matrix[0] = right.x;
            matrix[1] = right.y;
            matrix[2] = right.z;

            matrix[4] = up.x;
            matrix[5] = up.y;
            matrix[6] = up.z;

            matrix[8] = forward.x;
            matrix[9] = forward.y;
            matrix[10] = forward.z;

            Vector3 lastPoint = origin + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
            Vector3 nextPoint = Vector3.zero;
            float multiplier = 360f / circleDetail;

            Plane plane = new Plane((transform.position - origin).normalized, origin);

            for (var i = 0; i < circleDetail + 1; i++) {
                nextPoint.x = Mathf.Cos((i * multiplier) * Mathf.Deg2Rad);
                nextPoint.z = Mathf.Sin((i * multiplier) * Mathf.Deg2Rad);
                nextPoint.y = 0;

                nextPoint = origin + matrix.MultiplyPoint3x4(nextPoint);

                if (!depthTest || plane.GetSide(lastPoint)) {
                    resultsBuffer.Add(lastPoint);
                    resultsBuffer.Add(nextPoint);
                }

                lastPoint = nextPoint;
            }
        }

        void DrawLines(List<Vector3> lines, Color color)
        {
            GL.Begin(GL.LINES);
            GL.Color(color);

            for (int i = 0; i < lines.Count; i += 2) {
                GL.Vertex(lines[i]);
                GL.Vertex(lines[i + 1]);
            }

            GL.End();
        }

        void DrawTriangles(List<Vector3> lines, Color color)
        {
            GL.Begin(GL.TRIANGLES);
            GL.Color(color);

            for (int i = 0; i < lines.Count; i += 3) {
                GL.Vertex(lines[i]);
                GL.Vertex(lines[i + 1]);
                GL.Vertex(lines[i + 2]);
            }

            GL.End();
        }

        void DrawSquares(List<Vector3> lines, Color color)
        {
            GL.Begin(GL.QUADS);
            GL.Color(color);

            for (int i = 0; i < lines.Count; i += 4) {
                GL.Vertex(lines[i]);
                GL.Vertex(lines[i + 1]);
                GL.Vertex(lines[i + 2]);
                GL.Vertex(lines[i + 3]);
            }

            GL.End();
        }

        void DrawCircles(List<Vector3> lines, Color color)
        {
            GL.Begin(GL.LINES);
            GL.Color(color);

            for (int i = 0; i < lines.Count; i += 2) {
                GL.Vertex(lines[i]);
                GL.Vertex(lines[i + 1]);
            }

            GL.End();
        }
    }
}
