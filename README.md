# Cable physics
Real-time simulation of cable behaviors, such as:
- move - connectors  that can be lifted and moved
- connect to the connector - keeps divided into colors and male and female connectors 
- stretching - the cable can extend to the designated length, after exceeding it, the cable disconnects from the connector, returning to the neutral length
- overlapping cables - they have collisions with each other and can be lifted and moved

<br>

Cable behavior demonstration video: [youtube.com/Hrober/cables-physics](https://youtu.be/uCyIoAziExc)

The whole system was used in-game named Errata.<br>
Errata trailer: [youtube.com/Errata/errata-trailer](https://www.youtube.com/watch?v=JyS9zIQbpxQ)

## Project structure

#### Cables scripts

Main cable script:<br>
[github.com/Hrober0/Cable-physics/Assets/PhysicCalbes/Scripts/PhysicCable.cs](https://github.com/Hrober0/Cable-physics/blob/main/Assets/PhysicCalbes/Scripts/PhysicCable.cs)

Cable connector script:<br>
[github.com/Hrober0/Cable-physics/Assets/PhysicCalbes/Scripts/Scripts/Connector.cs](https://github.com/Hrober0/Cable-physics/blob/main/Assets/PhysicCalbes/Scripts/Connector.cs)

#### Interactions scripts
Scripts required to interact with cables:<br>
[github.com/Hrober0/Assets/Interactions](https://github.com/Hrober0/Cable-physics/tree/main/Assets/Interactions)<br>
(Whose dependencies can be easily removed)

#### Sample player controller
Sample first-person player controller used to show sample of interaction with cables:<br>
[github.com/Hrober0/Assets/SamplePlayerController](https://github.com/Hrober0/Cable-physics/tree/main/Assets/SamplePlayerController)

#### Sample scene
Sample scene used to show sample of interaction with cables:<br>
[github.com/Hrober0/Assets/Scenes](https://github.com/Hrober0/Cable-physics/tree/main/Assets/Scenes)<br>
Scene contains plyer, basic environment and some cables and connectors. 

## Used Technologies

#### Unity
#### C#


## Used Packages

#### NaughtyAttributes
NaughtyAttributes is an extension for the Unity Inspector.<br>
It expands the range of attributes that Unity provides so that you can create powerful inspectors without the need of custom editors or property drawers. It also provides attributes that can be applied to non-serialized fields or functions.<br>
Link: https://github.com/dbrizov/NaughtyAttributes
