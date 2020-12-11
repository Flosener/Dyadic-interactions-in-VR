# Dyadic interactions in virtual reality: Wizards

![expRoom](https://user-images.githubusercontent.com/57441991/101847298-3310de80-3b53-11eb-9b0f-2f7eb87624fe.PNG)

This is the repository for the Unity project that builds the basis for a replication study of Experiment 1 in the paper "Representing others’ actions: just like one’s own?" (Sebanz, Knoblich, Prinz, 2003) for a course at the University of Osnabrück.
<br>
<br>
To use this application, start a builded version (Builds/HarryPotter) on two PC's each, both with a VR headset and controllers connected. You also need SteamVR. The current version supports and works properly with the Oculus Rift, though for using other headsets that are supported by SteamVR you only have to re-define the actions via SteamVR Input.
After starting both applications you can individually do experiment 1 and 2. <br>
[!] For joining the (networked) experiment 3, one participant has to join as a host, the other as a client after the host (the order is important!). The client has to use the host's IP4 adress to join. Therefore you cannot use the builded applications right away, but have to build with the corresponding IP4 put into the network address slot of the NetworkManager in the JointExperimentRoom scene. To find the IP4, type "ipconfig" into the command line of the host. <br>
The results of the experiments are saved in the "results.txt" in Assets/Results.

![entranceHall](https://user-images.githubusercontent.com/57441991/101847194-f2b16080-3b52-11eb-89d9-faf2b4c43164.PNG)

### References
- Sebanz, N., Knoblich, G., & Prinz, W. (2003). Representing others' actions: just like one's own?. Cognition, 88(3), B11-B21.
