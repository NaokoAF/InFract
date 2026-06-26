namespace InFract.Emulators.UHid;

public enum UHidBusType : ushort
{
	PCI = 1,
	ISAPNP = 2,
	USB = 3,
	HIL = 4,
	BLUETOOTH = 5,
	VIRTUAL = 6,
}
