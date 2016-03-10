using UnityEngine;
using System.Collections;
 
public class perlin : MonoBehaviour {

	//texture
	public int pixSize = 256;
	public float xOrginal;
	public float yOrginal;
	public float scaleFactor = 4.0F;
	private Texture2D noiseTex;
	private Color[] pix;
	private Renderer rend;

	//octaves perlin
	public int octaves = 6;
	public float persistence = 2.0f;

	void Start() {
		initTexture ();
		CalcNoise(xOrginal,yOrginal,scaleFactor,octaves,persistence);
	}

	void Update() {
		//
	}

	void initTexture(){
		rend = GetComponent<MeshRenderer>();
		noiseTex = new Texture2D(pixSize, pixSize);
		pix = new Color[noiseTex.width * noiseTex.height];
		rend.material.mainTexture = noiseTex;
		rend.material.SetFloat ("_TexSize", pixSize); //Set TextureSize
	}

	void CalcNoise(float xOrg,float yOrg,float scale,int oct,float pers) {
		float y = 0.0F;
		while (y < noiseTex.height) {
			float x = 0.0F;
			while (x < noiseTex.width) {
				float xCoord = xOrg + x / noiseTex.width * scale;
				float yCoord = yOrg + y / noiseTex.height * scale;
				float total = octavePerlin(xCoord,yCoord,oct,pers);
				pix[Mathf.FloorToInt(y * noiseTex.width + x)] = new Color(total, total, total);
				x++;
			}
			y++;
		}
		noiseTex.SetPixels(pix);
		noiseTex.Apply();
	}

	float octavePerlin(float xDat,float yDat,int octavesDat, float persistenceDat){
		float total = 0;
		float frequency = 1;
		float amplitude = 1;
		float maxValue = 0;
		for (var i=0; i<octavesDat; i++) {
			total += Mathf.PerlinNoise(xDat * frequency , yDat * frequency) * amplitude;

			maxValue += amplitude;
			amplitude *= persistenceDat;
			frequency *= 2;
		}
		return total / maxValue;
	}

}