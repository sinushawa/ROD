using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace ROD_core
{
    public class Camera
    {
        public struct CameraSettings
        {
            public Vector3 eye;
            public Vector3 target;
            public Vector3 up;

            public CameraSettings(Vector3 e, Vector3 a, Vector3 u)
            {
                eye = e;
                target = a;
                up = u;
            }
        }

        public CameraSettings cameraOrigin;
        public CameraSettings cameraTransformed;
        public Matrix projection;
        public Vector3 eyeTranslation;
        public Quaternion orbitRotation;
        public Vector3 targetTranslation;
        public Quaternion targetRotation;
        private Vector3 lookAt;

        public Camera() : this(new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 1.0f, 0))
        {
        }
        public Camera(Vector3 _eye, Vector3 _target) : this(_eye, _target, new Vector3(0, 1.0f, 0))
        {
        }
        public Camera(Vector3 _eye, Vector3 _target, Vector3 _up)
        {
            cameraOrigin.eye = _eye;
            cameraOrigin.target = _target;
            cameraOrigin.up = _up;
            cameraTransformed = cameraOrigin;
            eyeTranslation = Vector3.Zero;
            orbitRotation = Quaternion.Identity;
            lookAt = Vector3.Normalize(_target - _eye);
        }

        /// <summary>Create the projection matrix of the camera. 
        /// </summary> 
        /// <param name="_fieldOfView">The angle of view in degrees.</param>
        /// <param name="_width">Width of the viewport in which the camera is going to be rendered.</param>
        /// <param name="_vnear">Distance from the camera of the near clipping plane.</param>
        /// <param name="_vnear">Distance from the camera of the far clipping plane.</param>
        
        public void CreateProjection(float _fieldOfView, int _width, int _height, float _znear, float _zfar)
        {
            CreateProjection(_fieldOfView, (_width / (float)_height), _znear, _zfar);
        }

        public void CreateProjection(float _fieldOfView, float _aspectRatio, float _znear, float _zfar)
        {
            projection=Matrix.PerspectiveFovLH(ROD_core.Mathematics.Math_helpers.ToRadians(_fieldOfView), _aspectRatio, _znear, _zfar);
        }

        public void Zoom(float _zoomScale)
        {
            cameraOrigin.eye += Vector3.Multiply(lookAt, (_zoomScale));
        }

        /// <summary>Turn the camera around the target on global Y axis and local X axis. 
        /// </summary> 
        /// <param name="_yaw">Angle of rotation in degrees around Y.</param>
        /// <param name="_pitch">Angle of rotation in degrees around X.</param>
        public void Orbit(float _yaw, float _pitch)
        {
            Quaternion RotYaw = Quaternion.RotationAxis(Vector3.UnitY, ROD_core.Mathematics.Math_helpers.ToRadians((_yaw)));
            // apply the rotation around Y on rotation holding the orientation of the camera compared to its original position.
            orbitRotation = RotYaw * orbitRotation;
            // determine the local X axis
            Vector3 AxisPitch = Vector3.TransformCoordinate(Vector3.UnitX, Matrix.RotationQuaternion(orbitRotation));
            Quaternion RotPitch = Quaternion.RotationAxis(AxisPitch, ROD_core.Mathematics.Math_helpers.ToRadians((_pitch)));
            orbitRotation = RotPitch * orbitRotation;
        }
        public void Update()
        {
            cameraTransformed.eye = Vector3.TransformCoordinate(cameraOrigin.eye, Matrix.RotationQuaternion(orbitRotation));
            cameraTransformed.up = Vector3.TransformCoordinate(cameraOrigin.up, Matrix.RotationQuaternion(orbitRotation));
        }
        public Matrix GetViewMatrix()
        {
            return Matrix.LookAtLH(cameraTransformed.eye, cameraTransformed.target, cameraTransformed.up);
        }
    }
}
