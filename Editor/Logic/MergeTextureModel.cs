namespace MaterialCreator {
    public class MergeTextureModel {

        public string MaterialName = "";
        public int ShaderType = 0;
        public int TextureType = 0;
        public bool RemoveOriginal = false;     //deletes the metal/spec, roughness/gloss, color, opacity map after merging them

        public MergeTextureModel(string materialName, int shaderType, int textureType, bool removeOriginal) {
            MaterialName = materialName;
            ShaderType = shaderType;
            TextureType = textureType;
            RemoveOriginal = removeOriginal;
        }
    }
}