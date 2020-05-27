Shader "DepthMask" {
	SubShader {
		Tags { "Queue" = "Geometry-500" }
		Pass
		{
		  // Depth Testの実施 および Depth Bufferの更新
		  Zwrite On

		  // 色情報は更新しない
		  ColorMask 0
		}
	} 
}