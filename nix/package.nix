{
  lib,
  dotnet-sdk_10,
  systemd,
  libusb1,
  buildDotnetModule,
}:

buildDotnetModule rec {
  pname = "infract";
  version = "0.1.0";

  src = ../.;

  dotnet-sdk = dotnet-sdk_10;
  dotnet-runtime = dotnet-sdk_10;

  projectFile = "InFract.sln";
  nugetDeps = ./deps.json;

  buildType = "Release";

  runtimeDeps = [
    systemd
    libusb1
  ];

  postFixup = ''
    wrapProgram $out/bin/InFract \
      --set DOTNET_CLI_TELEMETRY_OPTOUT 1 \
      --set DOTNET_NOLOGO 1 \
      --set DOTNET_SKIP_FIRST_TIME_EXPERIENCE 1 \
      --prefix LD_LIBRARY_PATH : ${
        lib.makeLibraryPath [
          systemd
          libusb1
        ]
      }
  '';

  meta = with lib; {
    description = "Input refractor for exposing proprietary controller features as virtual gamepads";
    homepage = "https://github.com/NaokoAF/InFract";
    license = licenses.mit;
    mainProgram = "InFract";
    platforms = platforms.linux;
  };
}
