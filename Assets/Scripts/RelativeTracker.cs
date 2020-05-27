using UnityEngine;

/// <summary>
/// targetとそのOriginとの相対位置を自身に反映
/// </summary>
public class RelativeTracker : MonoBehaviour
{
    public Transform selfOrigin;

    public Transform target;

    public Transform targetOrigin;

    void LateUpdate()
    {
        Vector3 relativePosition = targetOrigin.InverseTransformPoint(target.position);
        Quaternion relativeRotation = Quaternion.Inverse(targetOrigin.rotation) * target.rotation;

        transform.SetPositionAndRotation(
            position: selfOrigin.TransformPoint(relativePosition),
            rotation: selfOrigin.rotation * relativeRotation);
    }
}
