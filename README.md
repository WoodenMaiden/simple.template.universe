# Template-VU
Template repository pre-set for implementing GAMA model interfaced with Oculus VR headset ran by Unity

## Installation of the template

### Unity (tested with Unity 2022.3.5f1)


 - Launch Unity Hub and open the "simple.universe.template/Template Unity Project/GAMA-UNITY-VR" project with the "Open" button.
![Image 2023-07-25 16-08-47](https://github.com/project-SIMPLE/simple.template.universe/assets/579256/472b1729-2b4e-4612-b5fd-bee663a0433d)

 - In Package manager (Windows menu -> Package Manager), install :
      * TextMeshPro: import all
      * Oculus Integration: import everything, and for each question after answering the default or recommended answer. It not avaible from the Unity Interface, it can be downloaded from: [https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022](https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022)

Then build the executable for Meta Quest 2:
 - In File -> Build Settings, choose Android as platform. If not available, install the plugin dedicated to Android application generation: In Unity Hub, Installs panel, select your current version of Unity, click on the wheel, than Add module. On the Add modules window, under Platforms, select Android Build Support checkbox, and then select Android SDK & NDK Tools and OpenJDK checkboxes.
 - Click on Switch platform
 - Click on Player Setting. In the Oculus panels, for the 3 panels, click on "Fix All" and "Apply All"
 - The application then can be directly build and deploy on the Meta Quest headset (if this one is connected to the computer), by choosing the Build Setting windows Build and Run (or directly from the File Menu).
 - To be deploy on the Meta Quest 2, the headset should use a developer account. Using [Meta Quest Developper Hub](https://developer.oculus.com/documentation/unity/ts-odh/) can help the deployment and usage of the headset.
 - It is possible to export the package from Unity: Assets Menu -> Export Package, then select the Folder to includes. In order to avoid to produce an heavy package, it is better to keep only the directories: "Editor", "Eloi_CityKit", "nicoversity", "Prefabs", "Scenes", and "Scripts".


### GAMA (tested with GAMA 1.9.1)
 - Import from GAMA the Template GAMA model
 - The Template model is structured as follow:
      * models/UnityLink.gaml: the main file that define the connection between GAMA and Unity. It has to be imported in all projects.
      * Demo/DemoModel.gaml: an example model that shows how to define a VR model from GAMA. It works with the DemoScene from the Unity package.
      * Utilities/ImportGeometriesFromUnity.gaml: a model that have to be run for importing Unity GameObjects into GAMA geometries.
      * Utilities/SentGeometriesToUnity.gaml: a model that have to be run for sending GAMA geometries to Unity (as GameObjects).
          
   

## Usage of the template to enable VR from a GAMA model

### GAMA (tested with GAMA 1.9.1)
- Copy the UnityLink GAMA model in your project, and import it from your model.
- Optionally, copy the image directory of the template into your project (it only contains an image of a VR headset which is used to display the player in GAMA).
- For all the species of agents that you want to send to Unity, make then a child of "agent_to_send" (species simple_agentA parent: agent_to_send{}). Redefine the "index_species" variable. The indice is used in Unity to differentiate the species of agents. This must therefore be a value starting from 0 and incremented by 1 for each new species: example, "0 for simple_agentA", "1 for simple_agentB", ...
- In the init (and eventually in a reflex if the list of agents to send changes during the simulation), fill the "agents_to_send" list with the agents to send (e.g. agents_to_send <- (list(simple_agentA) + list(simple_agentB);).
- In the init section, add all the static geometries that you want to add in the Unity Scene. For that, fill the "background_geoms" with the list of geometries, "background_geoms_heights" with the height of each geometry, and "background_geoms_colliders" with true (add a collider for the geometry, i.e. a physical existence of the geometry in Unity) or false for each geometry. Instead of filling directly these 3 lists, another solution is to use the "add_background_data" action that takes 3 arguments: the list of geometries, the height of these geometries, true/false for the collider (e.g. do add_background_data(block collect each.shape, 5.0, true);
- To display the player position, add the "default_player" species in the display.
- To add the possibility to move the player location from GAMA, make the experiment a child of vr_xp (experiment simple_simulation parent: vr_xp) and add an event in the display that call the "move_player" action (e.g. event #mouse_down action: move_player;)
- It is recommanded to add the "autorun: true" facet to the experiment and to define a "minimum_cycle_duration" for the experiment higher than 0.01.

In the UnityLink model, some parameter values can be defined: 
- bool connect_to_unity: Activate the unity connection; if activated, the model will wait for an connection from Unity to start
- int port: connection port
- string end_message_symbol: symbol used to end the message in Unity - should be also defined in Unity
- precision: as all data (location, rotation) are send as int, number of decimal for the data
- float delay_after_mes: possibility to add a delay after moving the player (in ms)
- float player_agent_perception_radius: allow to reduce the quantity of information sent to Unity - only the agents at a certain distance are sent
- float player_agents_min_dist: allow to not send to Unity agents that are to close (i.e. overlapping)
- bool create_player: allow to create a player agent
- bool move_player_from_unity: let the player moves in GAMA as it moves in Unity
- bool use_physics_for_player: does the player should has a physical exitence in Unity (i.e. cannot pass through specific geometries)
- point location_init: init location of the player in the environment - this information will be sent to Unity to move the player accordingly
- float player_size_GAMA: player size - only used for displaying the player in GAMA

  
### Unity (tested with Unity 2022.3.5f1)
- Define a new 3D project
- Assets Menu -> Import Package -> Custom Package and import the "GAMA-UNITY-VR.unitypackage"
- In Package manager (Windows menu -> Package Manager), install:
      * TextMeshPro: import all
      * Oculus Integration: import everything, and for each question after answering the default or recommended answer. It not avaible from the Unity Interface, it can be downloaded from: [https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022](https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022).
- To be able after to export and run the application for Meta Quest 2, follow the instructions given in the "Installation of the template" section
- From the Current Scene, remove the "Main Camera".
- Add to the scene at least 2 Prefabs (available from Assets/Prefab/GAMA Link): "GAMAConnector" (to enable the connection with GAMA) and "OVRCameraRig_Moving" (for the player).
- To use the physical environment, define a Floor: for that, just add a 3D object Cube to the scene (rename it as you want, and optionally add a material).
- In the GAMAConnector:
     * Define the IP of the computer running GAMA (the Meta Quest Headset and the computer running GAMA have to use the same Wifi).
     * Define the same port as the one defined in the UnityLink GAMA model
     * Define the end message symbol as the one defined in the UnityLink GAMA model
     * Drag and Drop the "OVRCameraRig_Moving" from the same to the Player variable
     * Drag and Drop the "Floor" from the same to the Ground variable
     * For each species of agents to send to Unity from GAMA, add a value in the Agent list (use the "+"): The index in the list correspond to the "index_species" variable (the species with "index_species" of 0, correspond to the first element of the list; the one with "index_species" of 1, correspond to the second element of the list, etc.). For each element of the list (i.e. each species of agents sent), choose a prefab that will be used to display the agent).
     * For each species of agents to send to Unity from Unity, add a value in the rotations list (use the "+"): The rotation to apply to the prefab defined before
     * For each species of agents to send to Unity from Unity, add a value in the rotations Coeff list (use the "+"): The coeff to apply for the rotation to the prefab defined before
     * For each species of agents to send to Unity from Unity, add a value in the YValues list (use the "+"): The Y-offset to apply to the prefab defined before
     * For each species of agents to send to Unity from Unity, add a value in the Sizefactor list (use the "+"): The scaling to apply to the prefab defined before
     * Define the scale to apply to GAMA location for Unity (GamaCRSCoefX and GamaCRSCoefY - defaut value 1.0)
     * Define the Y-Scale of the ground (groundY - defaut value 1.0)
     * Define the Y-offset to apply to the background geometries (offsetYBackgroundGeom - defaut value 0.0)
     * Optionally, for debugging purpose, add a "GAMADebugCanvas" Prefab to the scene (from Assets/Prefab/GAMA Link), and Drag and Drop this GameObject to the Text variable. It is used to display the messages received from GAMA.
 
       

  ### Test and run the application
  - Run the experiment from your GAMA model
  - To test the model, a simple and fast way is to play the application from Unity. The GAMA model should start, the world with the agents should be visible from Unity. If an error appears in one of the Oculus Intgeration Script, a workaround consists in commenting this line: this workaround should not impact the running of the model.
  - To run the application from the VR Headset, deploy the application on the headset and run it. The application is avaibale from Applications with unkown source (that can be selected using the Filter in the Application Library of the headset).


	


