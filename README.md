# Material Creator
This tool will create a standard material from a selection of folders or textures through the click of a button.
You can also merge your metal/spec-gloss and albedo-opacity textures without using third party software.

Open tool by going to "Tools->Material Creator"

**MATERIAL:**

Just select your folders containing your material's textures or the textures itself and push the CREATE MATERIAL button.
A material will be created in the same path as your selection with the assigned textures.

If you select several folders, don't fill in the object name. The creator will try to name the materials itself based on the albedo texture name.

If you select 1 folder or a set of textures for 1 material you can fill in the object name manually.

**TEXTURES:**

There's an option to merge your gloss and opacity to your metal/spec and albedo texture alpha channels respectively without using third party tools.
Select the directory containing the textures or the "metalness & gloss" or "albedo & opacity" separately and push the merge buttons.
If you have a roughness instead of a gloss texture you can check the "Invert roughness" checkmark.

Note: 
Check the "delete original textures" checkmark if you're sure your texture selection is correct.
Otherwise the material creator might not assign the newly merged texture.
The option is merely there if you want to try out the texture merging first without deleting both textures.
