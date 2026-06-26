# GameSir Cyclone 2 Reverse Engineering

> Throughout this article I refer to the **GameSir Connect** app as **GSC** for brevity.
  This is the software used for configuring GameSir controllers on PC.

The controller has 3 output modes: Xbox, Switch, and DS4. While in Xbox mode, the controller does both XUSB and HID
at the same time. The HID interface is used for keyboard and mouse output, but also has a vendor-defined
interface GSC uses for configuring the device.

The 3 important report IDs are:

* `0x0F`: Output. Used by GSC to send configuration commands to the controller.
* `0x12`: Input. Used by the controller to send input reports while GSC is open.
* `0x10`: Input. Used by the controller to reply to configuration commands.

## Input

To start receiving input reports, GSC sends a `Heartbeat` command through the HID interface **once a second**.

**Heartbeat HID Report**: `0x0F 0xF2`

While this is active, the controller sends input reports with report ID `0x12`, along the usual XUSB packets.

### Input Report Layout

These reports use the same format as the PS4 controller and the DS4 mode, except a few unused bytes
near the end (53-61) are repurposed by GameSir to expose "raw" values. These include the unconfigured buttons,
sticks, and triggers, ignoring all user binding and deadzones.

Index skips the initial `0x12` report ID byte.

| Index | Value                    |
|:------|:-------------------------|
| 0     | Left Stick X             |
| 1     | Left Stick Y             |
| 2     | Right Stick X            |
| 3     | Right Stick Y            |
| 4-6   | Buttons                  |
| 7     | Left Trigger             |
| 8     | Right Trigger            |
| 9-10  | Timestamp (5.33us units) |
| 11    | Unknown                  |
| 12-13 | Gyro X (2000 deg/s)      |
| 14-15 | Gyro Y (2000 deg/s)      |
| 16-17 | Gyro Z (2000 deg/s)      |
| 18-19 | Accel X (4 Gs)           |
| 20-21 | Accel Y (4 Gs)           |
| 22-23 | Accel Z (4 Gs)           |
| 24-52 | Unknown                  |
| 53    | Raw Left Stick X         |
| 54    | Raw Left Stick Y         |
| 55    | Raw Right Stick X        |
| 56    | Raw Right Stick Y        |
| 57-59 | Raw Buttons              |
| 60    | Raw Left Trigger         |
| 61    | Raw Right Trigger        |
| 62    | Unknown                  |

### Button Bit Layout

First 4 bits are the D-pad's direction as a number from 0 to 7.  
The `L4`, `R4` and `M` buttons only appear at index 59, and NOT on index 6.  
Button naming derived from the Cyclone 2's manual.

| Byte | Bit 0 | Bit 1 | Bit 2 | Bit 3 | Bit 4 | Bit 5 | Bit 6 | Bit 7 |
|:-----|:------|:------|:------|:------|:------|:------|:------|:------|
| 0    | D-pad | D-pad | D-pad | D-pad | X     | A     | B     | Y     |
| 1    | LB    | RB    | LT    | RT    | View  | Menu  | LS    | RS    |
| 2    | Home  | Share |       | L4    | R4    | M     |       |       |

| D-pad Direction | Value |
|:----------------|:------|
| North           | 0x00  |
| Northeast       | 0x01  |
| East            | 0x02  |
| Southeast       | 0x03  |
| South           | 0x04  |
| Southwest       | 0x05  |
| West            | 0x06  |
| Northwest       | 0x07  |

## Commands
The Cyclone 2 has a lot of commands, a lot of witch I haven't fully figured out yet. These are only a few
of them, and there may be some mistakes.

`Register` commands refer to the controller's on-board memory/storage. Each profile has a different set of registers,
along with some global registers that apply to all profiles. What each address means still needs further studying.

### From GSC to Controller (`0x0F`)
* **Heartbeat**: `0xF2`. Enables HID input reports. Must be sent once a second to stay enabled.
* **Rumble**: `0x20 0x66 0x55 XX YY`. XX = Left, YY = Right. Unsure what the `0x66 0x55` bytes are.
* **Write Register**: `0x03 XX YY YY ZZ`. XX = Profile, YY = Address (big endian), ZZ = Length (0-58). Remaining
  bytes hold data.
* **Read Register**: `0x04 XX YY YY ZZ`. Same format as `Write Register`, but without data.
* **Read Register (Alt)**: `0x10 XX YY YY ZZ`. Seems similar to `Read Register`, but GSC uses this instead
  when receiving `Register Changed` (Maybe this reads from live memory rather than flash storage?).
* **Register Loaded (?)**: `0x06`. Uncertain. Sent at GSC startup after all registers have been read.
* **Set Profile**: `0x07 XX`. XX is the profile ID (1, 2, 3, or 4).
* **Get Profile**: `0x0B`. Controller responds with `Get Profile Response`.

### From Controller to GSC (`0x10`)
* **Read Register Response**: `0x05 XX YY YY ZZ`. Same format as `Write Register`.
* **Write Register/Set Profile Ack**: `0x06`. Response to `Write Register` or `Set Profile`. No data.
* **Get Profile Response**: `0x0C XX`. XX is the current profile ID.
* **Register Changed**: `0x0F XX YY YY 0x00 ZZ`. Sent when the user alters a register using the `M` button.
  XX = Profile, YY = Address (big endian), ZZ = Length. No data. Uncertain, but the fourth byte (`0x00`) is likely
  part of ZZ (big endian).

## Limitations

* When using the wireless dongle, HID input reports are limited to 200 Hz.
* XUSB packets are still sent while HID is active. Could lead to doubled inputs.
* The `Share` button is bound to `Capture` by default. This registers as a `LWin + LShift + S` keyboard press,
  triggering a screenshot on Windows. This can be unbound through GSC or by altering registers.
* The `M` button has behavior built into firmware that we can't disable.
