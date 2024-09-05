# Holo-Art Interaction Application

This repository houses the code for an augmented reality (AR) application developed using Unity and Visual Studio, designed to run on HoloLens 2.

## Features

- **Gaze-Activated Interaction**: Utilizes Microsoft’s Mixed Reality Toolkit 3 (MRTK3) for eye-tracking and interactive gaze features.
- **Object Detection**: Implements YOLOv8 using PyTorch for real-time object recognition, trained on a custom dataset.
- **Animation Algorithms**: Leverages "Animate Anything" techniques from video diffusion models to animate static images based on user prompts.
- **Language Model Integration**: Uses GPT-4 to provide information about detected artworks.

## Development Environment

- **Unity**: Primary platform for AR development and deployment.
- **Visual Studio**: Used for robust C# support and application scripting.
- **HoloLens 2**: Target device for the AR application.

## Environment Setup

### 1. Setting Up HoloLens 2 with MRTK3
To set up your HoloLens 2 for development using Microsoft's Mixed Reality Toolkit 3 (MRTK3), follow this [setup guide](https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk3-overview/getting-started/setting-up/setup-new-project).

### 2. Set up Eye Tracking
For configuring eye tracking on your HoloLens 2 project, follow the guidelines provided in the [Microsoft MixedReality-EyeTracking Sample](https://github.com/microsoft/MixedReality-EyeTracking-Sample)

### 3. Configuring Unity with Barracuda
For instructions on installing Barracuda, both locally and remotely, see the [Barracuda Installation Guide](https://github.com/Unity-Technologies/barracuda-release/blob/release/3.0.1/Documentation~/Installing.md).

## Jupyter Notebooks for Model Training and Animation

We have prepared Jupyter notebooks to facilitate the training of object detection model, animation via "Animate Anything," and the integration of these animations into a web application using Flask.

### 1. Training the Object Detection Model

Our Jupyter notebook "notebooks/Painting_detection_YoloV8.ipynb" provides the entire process of training an object detection model. This includes:

- **Data Preparation**: How to prepare your dataset.
- **Model Configuration**: Setting up model parameters.
- **Training**: Running the training progress.
- **Validation**: Evaluating the model's performance.

For detailed instructions and further documentation, please refer to the following resource [Ultralytics YOLO Documentation](https://github.com/ultralytics/ultralytics).

### 2. Using "Animate Anything"

Our Jupyter notebook "notebooks/AnimateAnything.ipynb" shows how to use the "Animate Anything" model to animate static images. It includes examples on setting up the environment, loading pre-trained models, and running animations based on user inputs and  package the animation process into a Flask application.



This notebook demonstrates how to use the "Animate Anything" model to animate static images. It includes examples on setting up the environment, loading pre-trained models, and running animations based on user inputs and  package the animation process into a Flask application.

- **Access the notebook here**: [Animate Anything and Flask Integration.ipynb](link-to-your-animate-anything-ipynb)


