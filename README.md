# Template-VU
Template repository pre-set for implementing GAMA model interfaced with Oculus VR headset ran by Unity

# To install and run the template:

## For Unity (tested with Unity 2022.3.3f1):


 - Open the "simple.universe.template/GAMA-UNITY-VR" project from Unity Hub.
 - In Packager manager (Windows menu -> Package Manager), install :
      * TextMeshPro: import all
      * Oculus Integration: import everything, and for each question after answering the default or recommended answer. It not avaible from the Unity Interface, it can be downloaded from: [https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022](https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022)

Then build the executable for Meta Quest 2: 
 - In File -> Build Settings, choose Android as platform. If not available, install the plugin dedicated to Android application generation: In Unity Hub, Installs panel, select your current version of Unity, click on the wheel, than Add module. On the Add modules window, under Platforms, select Android Build Support checkbox, and then select Android SDK & NDK Tools and OpenJDK checkboxes.
 - Click on Switch platform
 - Click on Player Setting. In the Oculus panels, for the 3 panels, click on "Fix All" and "Apply All"
 - The application then can be directly build and deploy on the Meta Quest headset (if this one is connected to the computer), by choosing the Build Setting windows Build and Run (or directly from the File Menu).
 - To be deploy on the Meta Quest 2, the headset should use a developer account. Using [Meta Quest Developper Hub](https://developer.oculus.com/documentation/unity/ts-odh/) can help the deployment and usage of the headset. 


## For GAMA (tested with GAMA 1.9.1):
 - Import from GAMA the Template GAMA model
 - 2 gama models are defined inside:
      * UnityLink.gaml: the main file that define the connection between GAMA and Unity. It has to be imported in all projects.
      * DemoModel.gaml: a example model that shows how to define a VR model from GAMA. It works with the DemoScene from the Unity package.
          
   



