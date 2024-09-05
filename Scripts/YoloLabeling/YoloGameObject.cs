using UnityEngine;
using YoloHolo.Services;

namespace YoloHolo.YoloLabeling
{
    public class YoloGameObject 
    {
        public Vector2 ImagePosition { get; }
        public string Name {get; }
        public GameObject DisplayObject { get; set; }
        public Vector3? PositionInSpace { get; set; }
        public float TimeLastSeen { get; set; }

        public Vector3 topLeft3D { get; private set; } // 已计算
        public Vector3 bottomRight3D { get; private set; } // 已计算

        public Vector3 bottomLeft3D { get; private set; }

        public Vector3 topRight3D { get; private set; }

        private const int MaxLabelDistance = 10;
        private const float SphereCastSize = 0.15f;
        private const string SpatialMeshLayerName  = "Spatial Mesh";

        public YoloGameObject(
            YoloItem yoloItem, Transform cameraTransform, 
            Vector2Int cameraSize, Vector2Int yoloImageSize,
            float virtualProjectionPlaneWidth)
        {
            ImagePosition = new Vector2(
                (yoloItem.Center.x / yoloImageSize.x * cameraSize.x - cameraSize.x / 2) / cameraSize.x,
                (yoloItem.Center.y / yoloImageSize.y * cameraSize.y - cameraSize.y / 2) / cameraSize.y);
            Name = yoloItem.MostLikelyObject;

            // float left = (yoloItem.Center.x - yoloItem.Size.x / 2) / yoloImageSize.x * cameraSize.x;
            // float top = (yoloItem.Center.y - yoloItem.Size.y / 2) / yoloImageSize.y * cameraSize.y;
            // float right = (yoloItem.Center.x + yoloItem.Size.x / 2) / yoloImageSize.x * cameraSize.x;
            // float bottom = (yoloItem.Center.y + yoloItem.Size.y / 2) / yoloImageSize.y * cameraSize.y;

            var virtualProjectionPlaneHeight = virtualProjectionPlaneWidth * cameraSize.y / cameraSize.x;
            FindPositionInSpace(cameraTransform, virtualProjectionPlaneWidth, virtualProjectionPlaneHeight);
            Vector3 topLeft3D = FindPositionInSpace(cameraTransform, yoloItem.TopLeft, virtualProjectionPlaneWidth, virtualProjectionPlaneHeight);
            Vector3 bottomRight3D = FindPositionInSpace(cameraTransform, yoloItem.BottomRight, virtualProjectionPlaneWidth, virtualProjectionPlaneHeight);
            Vector3 topRight3D = new Vector3(bottomRight3D.x, topLeft3D.y, topLeft3D.z);
            Vector3 bottomLeft3D = new Vector3(topLeft3D.x, bottomRight3D.y, bottomRight3D.z);

            Vector2 topLeft = yoloItem.TopLeft;
            Vector2 bottomRight = yoloItem.BottomRight;

            float bboxWidth = bottomRight.x - topLeft.x;  
            float bboxHeight = bottomRight.y - topLeft.y; 
            float actualBboxWidth = bboxWidth * virtualProjectionPlaneWidth;
            float actualBboxHeight = bboxHeight * virtualProjectionPlaneHeight;

            TimeLastSeen = Time.time;
        }



        public void FindPositionInSpace(Transform transform,
            float width, float height)
        {
            var positionOnPlane = transform.position + transform.forward + 
                transform.right * (ImagePosition.x * width) - 
                transform.up * (ImagePosition.y * height);
            PositionInSpace = CastOnSpatialMap(positionOnPlane, transform);
        }

        private Vector3 FindPositionInSpace(Transform transform, Vector2 imagePosition, float width, float height)
        {
            var positionOnPlane = transform.position + transform.forward + 
                transform.right * (imagePosition.x * width) - 
                transform.up * (imagePosition.y * height);
            return CastOnSpatialMap(positionOnPlane, transform) ?? Vector3.zero;
        }



        private Vector3? CastOnSpatialMap(Vector3 positionOnPlane, Transform transform)
        {
            if (Physics.SphereCast(transform.position, SphereCastSize,
                    (positionOnPlane - transform.position),
                    out var hitInfo, MaxLabelDistance, LayerMask.GetMask(SpatialMeshLayerName)))
            {
                return hitInfo.point;
            }
            return null;
        }


    }
}
