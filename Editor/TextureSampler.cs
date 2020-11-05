using UnityEngine;

namespace MaterialCreator {
    public class TextureSampler {
        private Color[] m_Data;
        private int m_Height;
        private int m_Width;
        private bool m_WarpU;
        private bool m_WarpV;

        public TextureSampler(int width, int height, Color[] data, bool warpU = true, bool warpV = true) {
            m_Data = data;
            m_Width = width;
            m_Height = height;
            m_WarpU = warpU;
            m_WarpV = warpV;
        }

        public TextureSampler(Texture2D source, bool warpU = true, bool warpV = true) {
            SetTexture(source, warpU, warpV);
        }

        public void SetTexture(Texture2D source, bool warpU = true, bool warpV = true) {
            if (source == null) {
                m_Data = new Color[1];
                m_Width = 1;
                m_Height = 1;
            }
            else {
                m_Data = source.GetPixels();
                m_Width = source.width;
                m_Height = source.height;
            }
            m_WarpU = warpU;
            m_WarpV = warpV;
        }

        public Color[] GetData() {
            return m_Data;
        }

        public void Scale(int width, int height) {
            if (m_Width == width && m_Height == height)
                return;
            Color[] d = new Color[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    d[y * width + x] = GetPixelBilinear((x + 0.5f) / width, (y + 0.5f) / height);
            m_Data = d;
            m_Width = width;
            m_Height = height;
        }

        private Color GetPixelBilinear(float u, float v) {
            u *= m_Width;
            v *= m_Height;
            u -= 0.5f;
            v -= 0.5f;

            int x = Mathf.FloorToInt(u);
            int y = Mathf.FloorToInt(v);

            float u_ratio = u - x;
            float v_ratio = v - y;
            float u_opposite = 1 - u_ratio;
            float v_opposite = 1 - v_ratio;

            Color c1 = (GetPixel(x, y) * u_opposite + GetPixel(x + 1, y) * u_ratio) * v_opposite;
            Color c2 = (GetPixel(x, y + 1) * u_opposite + GetPixel(x + 1, y + 1) * u_ratio) * v_ratio;
            return c1 + c2;
        }

        private Color GetPixel(int x, int y) {
            x = x >= 0 ? x : m_WarpU ? x + m_Width : 0;
            x = x < m_Width ? x : m_WarpU ? x - m_Width : m_Width - 1;
            y = y >= 0 ? y : m_WarpV ? y + m_Height : 0;
            y = y < m_Height ? y : m_WarpV ? y - m_Height : m_Height - 1;
            return m_Data[y * m_Width + x];
        }
    }
}