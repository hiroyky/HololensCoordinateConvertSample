using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity;

public class ImageUtil {
    public static Mat toMat(List<byte> image, int width, int height, int type) {
        Mat mat = new Mat(height, width, type);
        Utils.copyToMat(image, mat);
        return mat;
    }
}
