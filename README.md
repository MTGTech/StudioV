# StudioV

StudioV is working on a mixed reality production platform. Taking advantage of real-time rendering ability of game engine Unity, we produce CGI content that syncing environment, character skeleton tracking animation and facial expression animation in real-time. During production, actors will also be immersed in the virtual environment, which can help actors set up the mood.

The end product of production can distribute to multiple platforms, including VR, AR, 360 video.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

What things you need to install the software and how to install them

```
- Unity 2017.2
- Motion capture system: e.g. Optitrack motion capture system + software Motive 2.0
- VR system: e.g. HTC Vive + steamVR
```

### Installing

A step by step series of examples that tell you have to get a development env running

```
Before configuration of Unity project*

1. Prepare motion capture system
Motion capture system is required to track skeleton and rigidbody movement, and stream tracking data to Unity. 
Follow the instruction in following link to set up motion capture system https://wiki.optitrack.com/index.php?title=OptiTrack_Documentation_Wiki

2. Prepare VR system
VR system allows actors see the scene, other actors and themselves immersively. You need a VR compatible computer to run VR application. 
Follow the link to set up VR system https://support.steampowered.com/kb_article.php?ref=2001-UXCM-4439
* In all instructions, we will only provide explanations of the components that we are using, 
if you want to use other hardware or software, you can try to work on your own or contact studio.
```


#### Before Unity project configuration
```
3.Prepare face tracking
- Eye tracking: hardware required. We are using Tobii VR for eye tracking. Tobii Pro SDK Unity is used for development.
https://www.tobii.com/tech/products/vr/
https://www.tobiipro.com/product-listing/tobii-pro-sdk/

- Facial expression tracking: hardware required. We are using BinaryVR for facial expression tracking.
http://www.binaryvr.com/
```

#### Configure project step by step
```
1. Download this project and open it in Unity 2017.2

2. Import the unity assets/plugins you need, please check Appendix A for a list of assets we use
- Import Photon Unity Networking(PUN), version 1.87 https://www.assetstore.unity3d.com/en/#!/content/1786
1) To use PUN, you need an App ID, follow the instruction to get an App ID for your application
https://doc.photonengine.com/en-us/realtime/current/getting-started/obtain-your-app-id

2) Initial setup of PUN: https://doc.photonengine.com/en-us/pun/current/getting-started/initial-setup

3) Go to PhotonServerSetting, make sure the Hosting protocal is set to Tcp.

4) In PhotonServerSetting find Rpc list and press ”Clear RPCs” and then ”Refresh RPCs”.
- Import MicroLibrary: Download the source code from link https://www.codeproject.com/Articles/98346/Microsecond-and-
Millisecond-NET-Timer, copy the file MicroLibrary.cs to Assets/Scripts/Tool/ in the project
Note: After clicking Clear in Console, there shouldn’t be any complie errors after you import these 2 assets. - Import BinaryVR SDK (Recommended): you can get the SDK after purchase
1) Go to path binarysdk/examples/Unity/Assets/ExampleScene/Scripts/, copy and replace FaceExpressionController.cs into Assets/Scripts/AvatarSpecific/ in the project.
2) Go to path binarysdk/examples/Unity/Assets/BinaryFaceHMD/, copy this folder and place it under Assets folder in the project.
3) Go to path binarysdk/examples/Unity/Assets/StreamingAssets/, copy the file model.bfh in this folder to Assets/StreamingAssets/ in the project
4) Refer to example scene in binarysdk to set it up
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details

## Documentations
[documentation](documentation.pdf) file for details


