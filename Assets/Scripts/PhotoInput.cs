using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VR.WSA.WebCam;
using System.IO;
using System;

public class PhotoInput : MonoBehaviour {
    public delegate void OnCaptured(List<byte> image, int width, int height);
    PhotoCapture photoCapture;
    CameraParameters cameraParameters;
    OnCaptured callback = null;

    void Start() {
    }

    public void CapturePhotoAsync(OnCaptured _callback) {
        callback = _callback;

        PhotoCapture.CreateAsync(true, (_photoCapture) => {
            Debug.Log("PhotoInput start");
            this.photoCapture = _photoCapture;
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

            CameraParameters c = new CameraParameters();
            c.hologramOpacity = 1.0f;
            c.cameraResolutionWidth = cameraResolution.width;
            c.cameraResolutionHeight = cameraResolution.height;
            c.pixelFormat = CapturePixelFormat.BGRA32;
            c.hologramOpacity = 0;
            this.cameraParameters = c;
            photoCapture.StartPhotoModeAsync(cameraParameters, onPhotoModeStarted);
        });
    }

    void onPhotoModeStarted(PhotoCapture.PhotoCaptureResult result) {
        if (result.success) {
            //saveToFile();
            photoCapture.TakePhotoAsync(onCapturedPhotoToMemory);
        } else {
            Debug.LogError("Unable to start photo mode");
        }
    }

    void onCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame) {
        if (!result.success) {
            Debug.LogError("Error CapturedPhotoToMemory");
            return;
        }

        // 撮影画像の取得
        List<byte> buffer = new List<byte>();
        photoCaptureFrame.CopyRawImageDataIntoBuffer(buffer);
        photoCapture.StopPhotoModeAsync(onStoppedPhotoMode);

        if (callback != null) {
            callback(new List<byte>(buffer), cameraParameters.cameraResolutionWidth, cameraParameters.cameraResolutionHeight);
        }
    }

#if true
    Texture2D createTexture(List<byte> rawData, int width, int height) {
        Texture2D tex = new Texture2D(width, height, TextureFormat.BGRA32, false);
        tex.LoadRawTextureData(rawData.ToArray());
        tex.Apply();
        return tex;
    }

    string saveToFile(Texture2D tex) {
        string filename = string.Format(@"QrSightImage{0}_n.png", Time.time);
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

        File.WriteAllBytes(filePath, tex.EncodeToPNG());
        return filePath;
    }
#endif
#if true
    string saveToFile() {
        string filename = string.Format(@"CapturedImage{0}_n.jpg", Time.time);
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
        Debug.Log(string.Format("{0} {1}", filePath, filename));
        photoCapture.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, onCapturedPhotoToDisk);
        return filePath;

    }

    void onCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result) {
        if (result.success) {
            Debug.Log("Saved Photo to disk!");
            //photoCapture.StopPhotoModeAsync(onStoppedPhotoMode);
        } else {
            Debug.Log("Failed to save Photo to disk");
        }
    }
#endif
    void onStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result) {
        photoCapture.Dispose();
        photoCapture = null;
    }
}
