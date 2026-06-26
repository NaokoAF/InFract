namespace InFract.Gamepads.Microsoft.Xbox360;

public interface IXbox360Target : IDisposable
{
	Xbox360Effects PollEffects();
	void SendInput(in Xbox360InputReport input);
}
