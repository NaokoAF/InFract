namespace InFract.Gamepads;

public record struct GamepadTouch(short X, short Y, ushort Pressure, bool Down);
