namespace InFract.Gamepads.SInput;

public interface ISInputTarget : IDisposable
{
	SInputEffects PollEffects();
	void SendInput(in SInputReport input);
}
