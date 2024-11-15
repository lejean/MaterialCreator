# Material Creator
This tool will create a standard material from a selection of folders or textures through the click of a button.
There are also some handy tools to merge the gloss/opacity texture with the albedo/specular or invert a gloss texture without using third party software.

Youtube link: https://www.youtube.com/watch?v=dqAgE7DTKjQ

![alt text](https://i.imgur.com/gSlxmbJ.gif "Screenshot")

## Install instructions
1. At the top Click the green Code button
2. Copy the HTTPS git link
3. Open the unity package manager
4. Click + and Select "Add package from git URL"
5. Paste link and click Add
6. Open tool by going to "Tools->Material Creator"

**MATERIAL CREATION:**

Select a set of textures or folder(s) containing the textures and push the CREATE MATERIAL button.
A material will be created in the same path as your selection with the assigned textures.

Note: 
- If you select several folders, don't fill in the object name. The creator will try to name the materials itself based on the albedo texture name.
- If you select 1 folder or a set of textures for 1 material you can fill in the object name manually.

**TEXTURE TOOLS:**

There are some extra optional features available for textures

- Merge your gloss and metal/Specular texture into 1 texture (PNG or TGA)
- Merge your opacity and albedo texture into 1 texture (PNG or TGA)
- Invert your gloss texture into roughness for metal <=> specular shaders

Note: 
- There is a checkmark called "Delete original textures", this deletes to original textures after merging them in 1 (USE WITH CAUTION).
- You can select the folder aswell as the textures itself for merging them into a single texture.


