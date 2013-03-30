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
        public Quaternion revolveRotation;
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
            revolveRotation = Quaternion.Identity;
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
        public void Pan(float _X, float _Y)
        {
            Vector3 movement = new Vector3(_X, _Y, 0.0f);
            eyeTranslation += movement;
            targetTranslation += movement;
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
        public void Revolve(float _yaw, float _pitch)
        {
            Quaternion RotYaw = Quaternion.RotationAxis(Vector3.UnitY, ROD_core.Mathematics.Math_helpers.ToRadians((_yaw)));
            // apply the rotation around Y on rotation holding the orientation of the camera compared to its original position.
            revolveRotation = RotYaw * revolveRotation;
            // determine the local X axis
            Vector3 AxisPitch = Vector3.TransformCoordinate(Vector3.UnitX, Matrix.RotationQuaternion(revolveRotation));
            Quaternion RotPitch = Quaternion.RotationAxis(AxisPitch, ROD_core.Mathematics.Math_helpers.ToRadians((_pitch)));
            revolveRotation = RotPitch * revolveRotation;
        }
        public void Update()
        {

            cameraTransformed.eye = cameraOrigin.eye + eyeTranslation;
            cameraTransformed.target = cameraOrigin.target + targetTranslation;


            // Doesn't work correctly to associate revolve and orbit. orbit never take account of revolve

            // get the vector between eye and orbit pivot point
            Vector3 _eye_to_target = cameraTransformed.eye - cameraTransformed.target;
            // rotate around orbit pivot then get final position by adding orbit pivot vector
            cameraTransformed.eye = Vector3.TransformCoordinate(_eye_to_target, Matrix.RotationQuaternion(orbitRotation)) + cameraTransformed.target;
            // get the vector between target and revolve pivot point
            Vector3 _target_to_eye = cameraTransformed.target - cameraTransformed.eye;
            // rotate around revolve pivot then get final position by adding revolve pivot vector
            cameraTransformed.target = Vector3.TransformCoordinate(_target_to_eye, Matrix.RotationQuaternion(revolveRotation)) + cameraTransformed.eye;
            cameraTransformed.up = Vector3.TransformCoordinate(cameraOrigin.up, Matrix.RotationQuaternion(orbitRotation));
        }
        public Matrix GetViewMatrix()
        {
            return Matrix.LookAtLH(cameraTransformed.eye, cameraTransformed.target, cameraTransformed.up);
        }
    }
}
