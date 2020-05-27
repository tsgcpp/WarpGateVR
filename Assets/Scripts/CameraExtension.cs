using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraExtension {
    /// <summary>
    ///  Return a calculated near-plane projection matrix by transform as clip plane.
    /// </summary>
    /// <param name="camera">Camera object.</param>
    /// <param name="clipPlaneTransform">
    ///  Transform as a clip plane (xy-axis as a plane, z-axis as a plane normal).
    /// </param>
    /// <returns>oblique matrix</returns>
    public static Matrix4x4 CalculateObliqueMatrix(this Camera camera, Transform clipPlaneTransform) {
        Vector4 clipPlane = camera.CalculateClipPlane(clipPlaneTransform);

          // Use original "CalculateObliqueMatrix"
        return camera.CalculateObliqueMatrix(clipPlane);
    }

    /// <summary>
    ///  Return a calculated clip plane by the transform as a clip plane.
    /// </summary>
    /// <param name="camera">Camera object.</param>
    /// <param name="clipPlaneTransform">
    ///  Transform as a clip plane (xy-axis as a plane, z-axis as a plane normal).
    /// </param>
    /// <returns>clip plane</returns>
    public static Vector4 CalculateClipPlane(this Camera camera, Transform clipPlaneTransform) {
        Vector3 clipPlaneNormal = camera.worldToCameraMatrix.MultiplyVector(clipPlaneTransform.forward);
        Vector3 clipPlanePosition = camera.worldToCameraMatrix.MultiplyPoint(clipPlaneTransform.position);

        return CalculateClipPlane(clipPlaneNormal, clipPlanePosition);
    }

    /// <summary>
    ///  Return a calculated oblique matrix by the plane normal and arbitary poisition in it.
    /// </summary>
    /// <param name="clipPlaneNormal">a clip plane normal (camera space).</param>
    /// <param name="clipPlanePosition">a arbitary position in clip plane (camera space).</param>
    /// <returns>oblique matrix.</returns>
    private static Vector4 CalculateClipPlane(Vector3 clipPlaneNormal, Vector3 clipPlanePosition) {
        float distance = -Vector3.Dot(clipPlaneNormal, clipPlanePosition);
        Vector4 clipPlane = new Vector4(clipPlaneNormal.x, clipPlaneNormal.y, clipPlaneNormal.z, distance);

        return clipPlane;
    }

    /// <summary>
    /// Update projection matrices by the transform as a clip plane.
    /// </summary>
    /// <param name="camera">Camera object.</param>
    /// <param name="clipPlaneTransform">
    ///  Transform as a clip plane (xy-axis as a plane, z-axis as a plane normal).
    /// </param>
    public static void UpdateProjectionMatrix(this Camera camera, Transform clipPlaneTransform) {
        UpdateNonStereoProjectionMatrix(camera, clipPlaneTransform);
        UpdateStereoProjectionMatrix(camera, clipPlaneTransform);
    }

    /// <summary>
    ///  Update Non-XR projection matrix.
    /// </summary>
    private static void UpdateNonStereoProjectionMatrix(Camera camera, Transform clipPlaneTransform) {
        // Use original "CalculateObliqueMatrix"
        camera.projectionMatrix = camera.CalculateObliqueMatrix(clipPlaneTransform);
    }

    /// <summary>
    ///  Update stereo (VR) projection matrices.
    /// </summary>
    private static void UpdateStereoProjectionMatrix(Camera camera, Transform clipPlaneTransform) {
        // Reset customized projection matrix
        camera.ResetStereoProjectionMatrices();

        // Need both projection matrices to be calculated.
        // If one matrix is changed, the other is changed to unit.
        Matrix4x4 leftProjMatrix = CalculateObliqueMatrix(camera, Camera.StereoscopicEye.Left, clipPlaneTransform);
        Matrix4x4 RightProjMatrix = CalculateObliqueMatrix(camera, Camera.StereoscopicEye.Right, clipPlaneTransform);

        camera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, leftProjMatrix);
        camera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, RightProjMatrix);
    }

    /// <summary>
    ///  Return a calculated near-plane projection matrix of the specified eye.
    /// </summary>
    /// <returns>A oblique matrix of the specified stereo eye</returns>
    private static Matrix4x4 CalculateObliqueMatrix(Camera camera, Camera.StereoscopicEye eye, Transform clipPlaneTransform) {

        // Require the projection matrix of the specified eye to be reset previously
        // but it is impossible because of no method to reset only either,
        // so this method should be private for now.

        Matrix4x4 baseViewMatrix = camera.GetStereoViewMatrix(eye);
        Matrix4x4 baseProjMatrix = camera.GetStereoProjectionMatrix(eye);

        Vector3 clipPlaneNormal = baseViewMatrix.MultiplyVector(clipPlaneTransform.forward);
        Vector3 clipPlanePosition = baseViewMatrix.MultiplyPoint(clipPlaneTransform.position);

        Vector4 clipPlane = CalculateClipPlane(clipPlaneNormal, clipPlanePosition);

        return CalculateObliqueMatrix(baseProjMatrix, clipPlane);
    }
    
    /// <summary>
    ///　Oblique near-plane projection matrix を計算して返す
    /// </summary>
    /// 
    /// <param name="projectionMatrix">
    /// Near-Planeを傾ける対象のprojection matrix。
    /// </param>
    /// 
    /// <param name="clipPlane">
    /// clip planeを表すVector4 (Camera.CalculateObliqueMatrixの引数と同様)]
    /// </param>
    /// <returns> Oblique near-plane projection matrix </returns>
    public static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projectionMatrix, Vector4 clipPlane)
    {
        Vector4 nearClipInProj = new Vector4(
            Mathf.Sign(clipPlane.x), Mathf.Sign(clipPlane.y),
            1.0f, 1.0f);
        Vector4 q = projectionMatrix.inverse * nearClipInProj;
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));

        projectionMatrix.SetRow(2, c - projectionMatrix.GetRow(3));

        return projectionMatrix;
    }

    public static Matrix4x4 CalculateObliqueMatrixSimple(Matrix4x4 projectionMatrix, Vector4 clipPlane) {
        Vector4 q = new Vector4(
          (Mathf.Sign(clipPlane.x) + projectionMatrix.m02) / projectionMatrix.m00,
          (Mathf.Sign(clipPlane.y) + projectionMatrix.m12) / projectionMatrix.m11,
          -1.0f,
          (1.0f + projectionMatrix.m22) / projectionMatrix.m23
        );

        Vector4 c = clipPlane * (2.0f / Vector4.Dot(clipPlane, q));
        c.z += 1.0f;

        projectionMatrix.SetRow(2, c);

        return projectionMatrix;
    }
}
