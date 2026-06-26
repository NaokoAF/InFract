namespace InFract.Gamepads.Sony.DualSense;

public interface IDualSenseTarget : IDisposable
{
	SonyGyroCalibrationReport GyroCalibration { get; }
	bool IsEdge { get; }

	DualSenseEffects PollEffects();
	void SendInput(in DualSenseInputReport input);
}
