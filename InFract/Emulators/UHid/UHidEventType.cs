namespace InFract.Emulators.UHid;

public enum UHidEventType : uint
{
	Destroy = 1,
	Start = 2,
	Stop = 3,
	Open = 4,
	Close = 5,
	Output = 6,
	GetReport = 9,
	GetReportReply = 10,
	Create2 = 11,
	Input2 = 12,
	SetReport = 13,
	SetReportReply = 14,
}
