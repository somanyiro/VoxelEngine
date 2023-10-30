public static class Utils
{

	public static float Map(float value, float old_min, float old_max, float new_min, float new_max)
	{
		return new_min + (value - old_min) * (new_max - new_min) / (old_max - old_min);
	}

	public static float Smoothstep (float edge0, float edge1, float x) {
		// Scale, and clamp x to 0..1 range
		x = Clamp((x - edge0) / (edge1 - edge0));
		return x * x * (3.0f - 2.0f * x);
	}

	public static float Clamp(float x, float lowerlimit = 0.0f, float upperlimit = 1.0f) {
		if (x < lowerlimit) return lowerlimit;
		if (x > upperlimit) return upperlimit;
		return x;
	}

	public static float Lerp(float firstFloat, float secondFloat, float by)
	{
		return firstFloat * (1 - by) + secondFloat * by;
	}

	public static float CubicLerp(float firstFloat, float secondFloat, float by)
	{
		float smoothed = MathF.Pow(3*by, 2) - MathF.Pow(2*by, 3);

		return firstFloat * (1 - smoothed) + secondFloat * smoothed;
	}

	public static float[,] GetNoiseData(int seed, int width, int height, float size, float power)
	{
		FastNoiseLite noise = new FastNoiseLite(seed);
		noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);

		float[,] noiseData = new float[width, height];
	
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				noiseData[x,y] = (noise.GetNoise(x*size,y*size)+1)/2*power;
			}
		}

		return noiseData;
	}
}