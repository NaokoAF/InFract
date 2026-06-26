namespace InFract.Gamepads.Sony;

public class SonyTouchpadHandler
{
	public SonyTouch Finger1 => fingers[0];
	public SonyTouch Finger2 => fingers[1];

	private byte counter;
	private SonyTouch[] fingers = new SonyTouch[2];
	private Dictionary<int, int> touchIndexes = new();

	public SonyTouchpadHandler()
	{
		fingers.AsSpan().Fill(new SonyTouch() { Counter = 0x80 });
	}

	public void Update(int index, int x, int y, bool down)
	{
		int fingerIndex = touchIndexes.GetValueOrDefault(index, -1);

		// check if this touch has an associated index
		if (down && fingerIndex == -1)
		{
			// find first inactive finger
			for (int i = 0; i < fingers.Length; i++)
			{
				if ((fingers[i].Counter & 0x80) == 0) continue;

				// assign index
				touchIndexes[index] = fingerIndex = i;

				// increment counter and clear last bit
				ref SonyTouch finger = ref fingers[fingerIndex];
				finger.Counter = (byte)(++counter & 0x7F);
				break;
			}
		}

		if (fingerIndex != -1)
		{
			ref SonyTouch finger = ref fingers[fingerIndex];
			if (down)
			{
				// update position
				finger.XLow = (byte)x;
				finger.XHighYLow = (byte)((y << 4) | (x >> 8));
				finger.YHigh = (byte)(y >> 4);
			}
			else
			{
				// set last bit and clear finger index
				finger.Counter |= 0x80;
				touchIndexes[index] = -1;
			}
		}
	}
}
