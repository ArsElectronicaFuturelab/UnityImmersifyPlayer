using UnityEngine;

public static class FadeFunctions
{
	// Pre-calculated integral for the equally named functions. 
	// Needs to be multiplied by tMax to get the true integral.
	// Important: If the function gets changed, the integral needs to be recalculated.
	private static readonly float risingQuarticParabolaIntegral = 0.2f; 
	private static readonly float fallingQuarticParabolaIntegral = 0.8f;

	public static float RisingQuarticParabolaIntegral
	{
		get { return risingQuarticParabolaIntegral; }
	}

	public static float FallingQuarticParabolaIntegral
	{
		get { return fallingQuarticParabolaIntegral; }
	}

	// A function that rises y from 0 to 1 over t (where t goes from 0 to tMax).
	// f(x): y = (a * x) ^ 4
	// where a = 1 / tMax
	// and tMax is the time in seconds that is used for the fading process.
	public static float RisingQuarticParabolaFunction(float t, float tMax)
	{
		float y = Mathf.Pow(((1f / tMax) * t), 4f);

		return y;
	}

	// A function that sinks (y) from 1 to 0 over t (where t goes from 0 to tMax).
	// f(x): y = -(a * x) ^ 4
	// where a = 1 / tMax
	// and tMax is the time in seconds that is used for the fading process.
	public static float FallingQuarticParabolaFunction(float t, float tMax)
	{
		float y = (-1f * Mathf.Pow(((1f / tMax) * t), 4f)) + 1f;

		return y;
	}

	// Get the amount of frames, that are played from the video when using the RisingQuarticParabolaFunction.
	// Where tMax is the time that is used for the fading process.
	public static float GetFpsForRisingQuarticFade(float normalSpeedFps, float tMax)
	{
		return normalSpeedFps * risingQuarticParabolaIntegral * tMax;
	}

	// Get the amount of frames, that are played from the video when using the FallingQuarticParabolaFunction.
	// Where tMax is the time in seconds that is used for the fading process.
	public static float GetFpsForFallingQuarticFade(float normalSpeedFps, float tMax)
	{
		return normalSpeedFps * fallingQuarticParabolaIntegral * tMax;
	}
}
