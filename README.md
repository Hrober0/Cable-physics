# Cable physics made with unity
Real-time simulation of cable behaviors, such as:
- move - connectors  that can be lifted and moved
- connect to the connector - keeps divided into colors and male and female connectors 
- stretching - the cable can extend to the designated length, after exceeding it, the cable disconnects from the connector, returning to the neutral length
- overlapping cables - they have collisions with each other and can be lifted and moved

<br>
Cable behavior video: https://www.youtube.com/watch?v=uCyIoAziExc

The whole system was used in-game named Errata. Errata trailer: https://youtu.be/JyS9zIQbpxQ

## Interesting parts

Main cable script:<br>
https://github.com/Hrober0/Cable-physics/blob/main/Scripts/PhysicCable.cs

Cable connector script:<br>
https://github.com/Hrober0/Cable-physics/blob/main/Scripts/Connector.cs


## Used Technologies

#### Unity and C#
