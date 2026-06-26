namespace InFract.Gamepads.Sony.DualShock4;

public interface IDualShock4Target : IDisposable
{
	SonyGyroCalibrationReport GyroCalibration { get; }
	
	DualShock4Effects PollEffects();
	void SendInput(in DualShock4InputReport input);
}
