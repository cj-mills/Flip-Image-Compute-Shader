using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flip : MonoBehaviour
{
    [Tooltip("The compute shader that contains the flip operations")]
    public ComputeShader computeShader;

    [Tooltip("The screen to which the test image is attached")]
    public GameObject screen;

    [Tooltip("Toggle whether to flip the image across the x-axis")]
    public bool flipXAxis;

    [Tooltip("Toggle whether to flip the image across the y-axis")]
    public bool flipYAxis;

    [Tooltip("Toggle whether to flip the image across the diagonal axis")]
    public bool flipDiag;

    // Stores a reference to the Main Camera object
    private GameObject mainCamera;
    // A copy of the original test image
    private RenderTexture image;


    // Start is called before the first frame update
    void Start()
    {
        // Get a reference to the image texture attached to the screen
        Texture screenTexture = screen.GetComponent<MeshRenderer>().material.mainTexture;

        // Create a new RenderTexture with the same dimensions as the test image
        image = new RenderTexture(screenTexture.width, screenTexture.height, 24, RenderTextureFormat.ARGB32);
        // Copy the screenTexture to the image RenderTexture
        Graphics.Blit(screenTexture, image);

        // Get a reference to the Main Camera GameObject
        mainCamera = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void Update()
    {
        // Allocate a temporary RenderTexture with the original image dimensions
        RenderTexture rTex = RenderTexture.GetTemporary(image.width, image.height, 24, image.format);
        // Copy the original image
        Graphics.Blit(image, rTex);

        // Temporarily store the flipped image
        RenderTexture tempTex;

        if (flipXAxis)
        {
            // Allocate a temporary RenderTexture
            tempTex = RenderTexture.GetTemporary(rTex.width, rTex.height, 24, rTex.format);
            // Perform flip operation
            FlipImage(rTex, tempTex, "FlipXAxis");

            // Free the resources allocated for the Tempoarary RenderTexture
            RenderTexture.ReleaseTemporary(rTex);

            // Allocate a temporary RenderTexture
            rTex = RenderTexture.GetTemporary(tempTex.width, tempTex.height, 24, rTex.format);
            // Copy the flipped image
            Graphics.Blit(tempTex, rTex);

            // Free the resources allocated for the Tempoarary RenderTexture
            RenderTexture.ReleaseTemporary(tempTex);
        }

        if (flipYAxis)
        {
            // Allocate a temporary RenderTexture
            tempTex = RenderTexture.GetTemporary(rTex.width, rTex.height, 24, rTex.format);
            // Perform flip operation
            FlipImage(rTex, tempTex, "FlipYAxis");

            // Free the resources allocated for the Tempoarary RenderTexture
            RenderTexture.ReleaseTemporary(rTex);
            // Allocate a temporary RenderTexture
            rTex = RenderTexture.GetTemporary(tempTex.width, tempTex.height, 24, rTex.format);
            // Copy the flipped image
            Graphics.Blit(tempTex, rTex);

            // Free the resources allocated for the Tempoarary RenderTexture
            RenderTexture.ReleaseTemporary(tempTex);
        }

        if (flipDiag)
        {
            // Allocate a temporary RenderTexture
            tempTex = RenderTexture.GetTemporary(rTex.height, rTex.width, 24, rTex.format);
            // Perform flip operation
            FlipImage(rTex, tempTex, "FlipDiag");

            // Free the resources allocated for the Tempoarary RenderTexture
            RenderTexture.ReleaseTemporary(rTex);
            // Assign a reference to the active RenderTexture
            rTex = RenderTexture.active;

            // Free the resources allocated for the Tempoarary RenderTexture
            RenderTexture.ReleaseTemporary(tempTex);
        }

        // Apply the new RenderTexture
        screen.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", rTex);
        // Adjust the scree dimensions to fit the new RenderTexture
        screen.transform.localScale = new Vector3(rTex.width, rTex.height, screen.transform.localScale.z);

        // Adjust the camera size to account for updates to the screen
        mainCamera.GetComponent<Camera>().orthographicSize = rTex.height / 2;

        // Free the resources allocated for the Tempoarary RenderTexture
        RenderTexture.ReleaseTemporary(rTex);
    }

    /// <summary>
    /// Perform a flip operation of the GPU
    /// </summary>
    /// <param name="image">The image to be flipped</param>
    /// <param name="tempTex">Stores the flipped image</param>
    /// <param name="functionName">The name of the function to execute in the compute shader</param>
    private void FlipImage(RenderTexture image, RenderTexture tempTex, string functionName)
    {
        // Specify the number of threads on the GPU
        int numthreads = 8;
        // Get the index for the PreprocessResNet function in the ComputeShader
        int kernelHandle = computeShader.FindKernel(functionName);

        /// Allocate a temporary RenderTexture
        RenderTexture result = RenderTexture.GetTemporary(tempTex.width, tempTex.height, 24, tempTex.format);
        // Enable random write access
        result.enableRandomWrite = true;
        // Create the RenderTexture
        result.Create();

        // Set the value for the Result variable in the ComputeShader
        computeShader.SetTexture(kernelHandle, "Result", result);
        // Set the value for the InputImage variable in the ComputeShader
        computeShader.SetTexture(kernelHandle, "InputImage", image);
        // Set the value for the height variable in the ComputeShader
        computeShader.SetInt("height", image.height);
        // Set the value for the width variable in the ComputeShader
        computeShader.SetInt("width", image.width);

        // Execute the ComputeShader
        computeShader.Dispatch(kernelHandle, tempTex.width / numthreads, tempTex.height / numthreads, 1);

        // Copy the flipped image to tempTex
        Graphics.Blit(result, tempTex);

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(result);
    }
}
