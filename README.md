**Unity Procedural Terrain Generator**
------------------------------------------------------------------------------------------
A procedural terrain generator for Unity using the MeshRenderer and MeshFilter components.

**Requirements:**
-------------
  - Shader Graph
  - URP, HDRP, or Built-In (Unity 2021 and up)

**Features:**
---------
  - Complete control over how terrain looks
  - A seed system for creating different terrains with the same settings
  - An animation curve and a height multiplier value for complete control over the height of the terrain
  - Multiple noise layers for more realistic looking terrain
  - Auto centering of terrain when changing terrain size
  - Mesh simplification for increased performance
  - Option to use the 32 bit mesh index or the 16 bit mesh index
  - Tooltips explaining every setting
  - Easy to set up
  - Shader graph shader included along with terrain textures
  - Option to use a falloff map for islands
  - Complete control over the falloff map
  - Option to print the vertex count of the terrain mesh
  - Everything is generated from a single script
  - Well commented code base
  - Example scene included

**How To Set Up:**
-------------
  - Download the "TerrainGenerator" script along with the "TerrainGeneratorEditor" script and the shader graph shader and the terrain textures
  - Put the downloaded scripts and the shader and textures into your Unity project
  - If you don't have on, create an "Editor" folder in your "Assets" folder and move "TerrainGeneratorEditor" to it
  - In your Unity scene, create an empty GameObject and assign the "TerrainGenerator" script to it
  - Assign the "TerrainMat" material into the "Mat" slot in the "TerrainGenerator" script
  - Thats it!

Example Video:
https://github.com/user-attachments/assets/74582ce0-5348-4b9f-a9ea-0e8c1cecb0c8

