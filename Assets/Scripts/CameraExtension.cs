using UnityEngine;

public static class CameraExtension {
    /// <summary>
    ///  Return a calculated near-plane projection matrix by transform as clip plane.
    /// </summary>
    /// <param name="camera">カメラ</param>
    /// <param name="clipPlaneTransform">clip planeを表すTransform</param>
    /// <returns>oblique matrix</returns>
    public static Matrix4x4 CalculateObliqueMatrix(this Camera camera, Transform clipPlaneTransform) {
        Vector4 clipPlane = camera.CalculateClipPlane(clipPlaneTransform);

          // Use original "CalculateObliqueMatrix"
        return camera.CalculateObliqueMatrix(clipPlane);
    }

    /// <summary>
    /// カメラ座標空間におけるClipPlaneを計算し返す
    /// </summary>
    /// <param name="camera">カメラ</param>
    /// <param name="clipPlaneTransform">clip planeを表すTransform</param>
    /// <returns>ClipPlane</returns>
    public static Vector4 CalculateClipPlane(this Camera camera, Transform clipPlaneTransform) {
        Vector3 clipPlaneNormal = camera.worldToCameraMatrix.MultiplyVector(clipPlaneTransform.forward);
        Vector3 clipPlanePosition = camera.worldToCameraMatrix.MultiplyPoint(clipPlaneTransform.position);

        return CalculateClipPlane(clipPlaneNormal, clipPlanePosition);
    }

    /// <summary>
    /// ClipPlaneの法線と座標からClipPlaneを計算し返す
    /// </summary>
    /// <param name="clipPlaneNormal">ClipPlaneの法線</param>
    /// <param name="clipPlanePosition">ClipPlane上の任意の座標</param>
    /// <returns>ClipPlane</returns>
    private static Vector4 CalculateClipPlane(Vector3 clipPlaneNormal, Vector3 clipPlanePosition) {
        float distance = -Vector3.Dot(clipPlaneNormal, clipPlanePosition);
        Vector4 clipPlane = new Vector4(clipPlaneNormal.x, clipPlaneNormal.y, clipPlaneNormal.z, distance);

        return clipPlane;
    }

    /// <summary>
    /// Oblique near-plane projection matrix を計算して反映
    /// </summary>
    /// <param name="camera">カメラ</param>
    /// <param name="clipPlaneTransform">clip planeを表すTransform</param>
    public static void UpdateProjectionMatrix(this Camera camera, Transform clipPlaneTransform) {
        UpdateNonStereoProjectionMatrix(camera, clipPlaneTransform);
        UpdateStereoProjectionMatrix(camera, clipPlaneTransform);
    }

    /// <summary>
    /// 非XR向けProjectionMatrixを計算し反映
    /// </summary>
    private static void UpdateNonStereoProjectionMatrix(Camera camera, Transform clipPlaneTransform) {
        // Use original "CalculateObliqueMatrix"
        camera.projectionMatrix = camera.CalculateObliqueMatrix(clipPlaneTransform);
    }

    /// <summary>
    /// XR向けProjectionMatrixを計算し反映
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
    /// 対象の目におけるOblique near-plane projection matrix を計算して返す
    /// </summary>
    /// <returns>対象の目のOblique near-plane projection matrix</returns>
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
    /// Oblique near-plane projection matrix を計算して返す
    /// </summary>
    /// 
    /// <param name="projectionMatrix">
    /// Near-Planeを傾ける対象のprojection matrix。
    /// </param>
    /// 
    /// <param name="clipPlane">
    /// clip planeを表すVector4 (Camera.CalculateObliqueMatrixの引数と同様)
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

    /// <summary>
    /// CalculateObliqueMatrixの改変版
    /// </summary>
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
