using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;

public class OpponentPointer : MonoBehaviour
{
    // Still needs to be reworked!
    [SerializeField] private Camera Camera;
    [SerializeField] private Sprite arrowSprite;
    [SerializeField] private Sprite crossSprite;
    [SerializeField] private Color color_default;
    [SerializeField] private Color color_transparent;

    private List<Pointer> questPointerList;

    public static float pointerLerpSpeed = 3f;

    private void Awake()
    {
        questPointerList = new List<Pointer>();
    }

    public Pointer CreatePointer(Vector3 targetPosition)
    {
        GameObject pointerGameObject = Instantiate(PrefabsHolder.instance.ui_opponentPointer_prefab);

        pointerGameObject.SetActive(true);
        pointerGameObject.transform.SetParent(transform, false);

        Pointer questPointer = new Pointer(targetPosition, pointerGameObject, Camera, arrowSprite, crossSprite, color_default, color_transparent);
        questPointerList.Add(questPointer);
        return questPointer;
    }

    public void DestroyPointer(Pointer questPointer)
    {
        questPointerList.Remove(questPointer);
        questPointer.DestroySelf();
    }

    public class Pointer
    {
        public Vector3 targetPosition;
        private GameObject pointerGameObject;
        private Sprite arrowSprite;
        private Sprite crossSprite;
        private Sprite emptySprite;
        private Camera uiCamera;
        private RectTransform pointerRectTransform;
        private Image pointerImage;
        private Color defaultColor;
        private Color transparentColor;

        public Image clockFollowerImage;

        Vector3 followerOffset = new Vector3(38f,0,0);
        Vector3 followerHintOffset = new Vector3(38f, 0, 0);

        public Pointer(Vector3 targetPosition, GameObject pointerGameObject, Camera uiCamera, Sprite arrowSprite, Sprite crossSprite, Color defCol, Color transpColor)
        {
            this.targetPosition = targetPosition;
            this.pointerGameObject = pointerGameObject;
            this.uiCamera = uiCamera;
            this.arrowSprite = arrowSprite;
            this.crossSprite = crossSprite;
            this.defaultColor = defCol;
            this.transparentColor = transpColor;

            pointerRectTransform = pointerGameObject.GetComponent<RectTransform>();
            pointerImage = pointerGameObject.GetComponent<Image>();
        }

        float borderSize = 45f;
        Vector3 imageTargetPos;
        public void Update()
        {
            imageTargetPos = uiCamera.WorldToScreenPoint(targetPosition); 

            if (imageTargetPos.z < 0) { imageTargetPos *= -1; }

            bool imageOffScreen = imageTargetPos.x <= 0 || imageTargetPos.x >= Screen.width 
                || imageTargetPos.y <= 0 || imageTargetPos.y >= Screen.height;

            if (imageOffScreen && EventManager.isAlive)
            {
                pointerImage.sprite = arrowSprite;
                pointerImage.color = defaultColor;

                Vector3 cappedTargetScreenPosition = imageTargetPos;
                                                                                                                                                      
                cappedTargetScreenPosition.x = Mathf.Clamp(cappedTargetScreenPosition.x, borderSize, Screen.width - borderSize);
                cappedTargetScreenPosition.y = Mathf.Clamp(cappedTargetScreenPosition.y, borderSize, Screen.height - borderSize);

                pointerRectTransform.position = Vector3.Lerp(pointerRectTransform.position, cappedTargetScreenPosition, pointerLerpSpeed * Time.deltaTime);

                RotatePointerTowardsTargetPosition();
            }
            else
            {
                pointerImage.sprite = crossSprite;
                pointerImage.color = transparentColor;

                Vector3 cappedTargetScreenPosition = imageTargetPos;

                cappedTargetScreenPosition.x = Mathf.Clamp(cappedTargetScreenPosition.x, borderSize, Screen.width - borderSize);
                cappedTargetScreenPosition.y = Mathf.Clamp(cappedTargetScreenPosition.y, borderSize, Screen.height - borderSize);

                pointerRectTransform.position = Vector3.Lerp(pointerRectTransform.position, cappedTargetScreenPosition, pointerLerpSpeed * Time.deltaTime);

                RotatePointerTowardsTargetPosition();
            }
        }

        private void RotatePointerTowardsTargetPosition()
        {
            Vector3 toPosition = imageTargetPos;
            Vector3 fromPosition = pointerRectTransform.position;
            fromPosition.z = 0f;
            Vector3 dir = (toPosition - fromPosition).normalized;
            float angle = UtilsClass.GetAngleFromVectorFloat(dir);
            pointerRectTransform.localEulerAngles = new Vector3(0, 0, angle);
        }

        public void DestroySelf()
        {
            Destroy(pointerGameObject);
        }
    }
}
