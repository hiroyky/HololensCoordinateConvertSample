using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VR.WSA.Input;
using OpenCVForUnity;
using System.IO;
using System;

public class GazeGestureManager : MonoBehaviour {

    public static GazeGestureManager Instance { get; private set; }
    public GameObject PointerObject;

    GestureRecognizer gestureRecognizer;
    PhotoInput photoInput;
    
    void Awake() {
        Instance = this;
        photoInput = GetComponent<PhotoInput>();
        gestureRecognizer = new GestureRecognizer();
        gestureRecognizer.TappedEvent += GestureRecognizer_TappedEvent;
        gestureRecognizer.StartCapturingGestures();
    }

    void Start() {
    }

    void Update() {

    }

    void GestureRecognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay) {
        photoInput.CapturePhotoAsync((List<byte> image, int width, int height) => {
            var headPosition = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;
            RaycastHit hitInfo;
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo)) {                
                Vector3 worldPoint = hitInfo.point;
                Debug.Log(string.Format("size: {0}, {1}", width, height));

                Mat mat = new Mat(height, width, CvType.CV_8UC4);
                Utils.copyToMat(image.ToArray(), mat);

                OpenCVForUnity.Point matPoint = worldPointToMatPoint(worldPoint, mat);
                Imgproc.circle(mat, matPoint, 20, new Scalar(255, 0, 255), -1);
                saveMat(mat);

                Vector3 worldPoint2;
                if (matPointToWorldPoint(matPoint, mat, out worldPoint2)) {
                    Debug.Log(string.Format("wp1: {0}  wp2:{1}", worldPoint, worldPoint2));
                    var obj = Instantiate(PointerObject);
                    obj.transform.position = worldPoint2;
                    obj.SetActive(true);
                }
            }
        });
    }


    OpenCVForUnity.Point worldPointToMatPoint(Vector3 worldPosition, Mat image) {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(worldPosition);
        Debug.Log("vp1: " + viewportPoint);
        OpenCVForUnity.Point drawPoint = new OpenCVForUnity.Point(viewportPoint.x * image.width(), (1.0 - viewportPoint.y) * image.height());
        return drawPoint;
    }

    bool matPointToWorldPoint(OpenCVForUnity.Point point, Mat image, out Vector3 worldPoint) {
        float viewportPointX = (float)(point.x / (double)image.width());
        float viewportPointY = (float)(1.0 - point.y / (double)image.height());
        Vector3 viewportPoint = new Vector3(viewportPointX, viewportPointY);
        Ray ray = Camera.main.ViewportPointToRay(viewportPoint);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo)) { 
            worldPoint = hitInfo.point;
            return true;
        }
        worldPoint = new Vector3();
        return false;
    }

    void saveMat(Mat mat) {
        string filename = string.Format(@"Image{0}_n.png", Time.time);
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
        Debug.Log(filePath);

        Texture2D tex = new Texture2D(mat.cols(), mat.rows(), TextureFormat.BGRA32, false);
        Utils.fastMatToTexture2D(mat, tex);
        File.WriteAllBytes(filePath, tex.EncodeToJPG());
    }
}
