# InFract

**In**put Re**fract**or. A tool that communicates with controllers through proprietary interfaces, and "refracts" them into
something the operating system can understand.

InFract exposes controller features that are usually not accessible, like extra buttons and gyro/accelerometer data, by converting
it into a virtual **DualSense**, **DualSense Edge**, **DualShock 4** or **SInput** device (support varies per OS).

* **GameSir Cyclone 2:** Must be in XInput mode. Refracts back buttons, capture button, M button, and gyro.

> [!NOTE]
> Currently only one controller is supported, though more can be added through further reverse engineering.

## Requirements

**Windows:**

* **[.NET 10 Runtime](https://dotnet.microsoft.com/en-us/download)**: For running the application.
* **[VIIPER](https://alia5.github.io/VIIPER/stable/getting-started/installation) (Recommended)
  or [ViGEmBus](https://github.com/nefarius/ViGEmBus/releases/latest) (Legacy)**: For input emulation.
* **[HidHide](https://github.com/nefarius/HidHide/releases/latest)**: For hiding the unrefracted controller from
  applications. Optional, but highly recommended. (InFract does NOT configure HidHide for you!)

> [!WARNING]
> Windows builds aren't heavily tested by me. Your mileage may vary.

**Linux:**

* User must have `hidraw` and `uhid` permissions. Copy `50-infract.rules` to `/etc/udev/rules.d/`, and add yourself to the `input`
  group: `usermod -aG input $USER`.

## Installation

Binaries can be downloaded from the [releases tab](https://github.com/NaokoAF/InFract/releases).

InFract currently has no installation method. As long as the requirements are met, you can run the program as normal on Windows,
or within your terminal on Linux. You can also set it up as a service that runs at start up. Both of these are automated for NixOS users via [a flake](#nix--nixos).

This may be made easier in the future.

## Configuration

InFract is designed to need as little configuration as possible. You shouldn't need to change any settings, unless
you know what you're doing.

Configuration is done through **environment variables** prefixed with `INFRACT_`.

* `INFRACT_EMULATOR`: Input emulator to use.
    * `UHID`: Linux only (Default).
    * `VIIPER`: Windows only (Default).
    * `VIGEM`: Windows only. Legacy option.
* `INFRACT_CONVERTER`: Device to emulate. Varies based on chosen emulator.
    * `DUALSENSE`: Only on `UHID` or `VIIPER` emulators (Default).
    * `DUALSENSEEDGE`: Only on `UHID` or `VIIPER` emulators.
    * `SINPUT`: Only on `UHID` emulator.
    * `DUALSHOCK4`
    * `XBOX360`
* `INFRACT_VIIPER_ADDRESS`: VIIPER server address. Defaults to `localhost`.
* `INFRACT_VIIPER_PORT`: VIIPER server port. Defaults to `3242`.
* `INFRACT_VIIPER_PASSWORD`: VIIPER server password. Defaults to no password.

## Nix / NixOS

This repository exposes a Nix flake:

* `packages.<system>.infract` / `packages.<system>.default`: builds and exposes the binary.
* `nixosModules.default`: enables a systemd service that runs the same wrapper and registers the bundled udev rules.

To just run it:

```sh
nix run github:NaokoAF/InFract?dir=nix#infract
```

Example NixOS flake configuration:

```nix
{
  inputs = {
    infract.url = "github:NaokoAF/InFract?dir=nix";
    inputs.nixpkgs.follows = "nixpkgs";
  };

  outputs = { self, nixpkgs, infract, ... }: {
    nixosConfigurations.my-host = nixpkgs.lib.nixosSystem {
      system = "x86_64-linux";
      modules = [
        infract.nixosModules.default
        {
          services.infract = {
            enable = true;
            enableRules = true;
            environment = {
              INFRACT_CONVERTER = "SINPUT";
            };
          };
        }
      ];
    };
  };
}
```

