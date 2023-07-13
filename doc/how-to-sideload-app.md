# SolAR HoloLens 2 Client

## Presentation

This sample application demonstrate how to use the SolAR Cloud Service client for HoloLens 2.

It will start the HoloLens 2 camera sensor and send this date to the SolAR services, and will apply a transformation on the sample scene (here a little 3D robot) to move it to the SolAR world origin which is located on an **ArUco marker**.

## How to install the app bundle

### Enable Device Portal

In the HoloLens 2:

1. Go to **Settings -> Update -> For developers -> Enable Developer Mode**
2. Scroll down and enable **Device Portal**

Reference: [Setting up HoloLens to use Windows Device Portal](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-the-windows-device-portal#setting-up-HoloLens-to-use-windows-device-portal)

### How to access the Device Portal

1. Go to **Settings -> Update -> For developers**
2. Scroll down to the Device Portal section and note the **Device Portal** IP address either the Wi-Fi one, or, if the HoloLens 2 is connected via USB, the Ethernet one.
3. Open the address in your browser. When using the Wi-Fi IP, be sure that your machine and the HoloLens are both on the same Wi-Fi network.

### Enable ResearchMode

1. In the Device Portal, navigate to **System->ResearchMode**
2. Check **Allow access to sensor streams**

### Install the app bundle (sideloading)

1. In the Device Portal, navigate to **Views->Apps**
2. In the **Deploy Apps section**, select the **Local Storage tab**, and click on **Choose File**.
3. Select the **appxbundle** file in this directory, and click on **Install**.

Reference: [Installing an app](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-the-windows-device-portal#installing-an-app)

## How to use the app

The UI is simple. An MRTK NearMenu is floating in front of the user.
This menu has 2 buttons:

* **IP**: use this to change the IP address where the SolAR Cloud frontend is running.
* **Start/Stop**: Start/Stop the sensor capture and the communication to the services.

In the current configuration, the client will use **ports [5000..5009]**, so you must ensure those are open, and that your frontend service, particularly the required HTTP proxy (namely Envoy) is configured to listen to these ports.

The service will only work with a specific **ArUco marker**. If you don't have it, you can generate and print it by going to [this online generator](https://chev.me/arucogen/) and selecting the following parameters:

* Dictionary: 6x6
* Marker ID: 1
* Marker Size: 158mm
