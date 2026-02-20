using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
#region Variables
        private Vector3 _offset;
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime;
        private Vector3 _currentVelocity = Vector3.zero;
        private Bounds _wallBounds;
        
    #endregion
    
    #region Unity callbacks
    
        private void Awake()
        {
            _offset = transform.position - target.position;
            CalculateWallBounds();
        }

        private void LateUpdate()
        {
            Vector3 targetPosition = target.position + _offset;
            
            targetPosition.x = Mathf.Clamp(targetPosition.x, _wallBounds.min.x, _wallBounds.max.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, _wallBounds.min.y, _wallBounds.max.y);
            targetPosition.z = Mathf.Clamp(targetPosition.z, _wallBounds.min.z, _wallBounds.max.z);
            
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, smoothTime);
        }
        
    #endregion
    
    private void CalculateWallBounds()
    {
        GameObject wallObject = GameObject.FindWithTag("Wall");

        BoxCollider[] colliders = wallObject.GetComponents<BoxCollider>();
        _wallBounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
        {
            _wallBounds.Encapsulate(colliders[i].bounds);
        }
    }
}