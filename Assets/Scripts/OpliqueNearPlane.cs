using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OpliqueNearPlane : MonoBehaviour {
    private Camera _camera;

    [SerializeField]
    private Transform _clipPlane;

    void Start() {
    _camera = GetComponent<Camera>();
    }

    void LateUpdate() {
    _camera.UpdateProjectionMatrix(_clipPlane);
    }
}
