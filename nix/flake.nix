{
  description = "Nix package, app, and NixOS module for InFract";

  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";

  outputs =
    { self, nixpkgs }:
    let
      lib = nixpkgs.lib;
      systems = [
        "x86_64-linux"
        "aarch64-linux"
      ];
      forAllSystems = lib.genAttrs systems;
    in
    {
      packages = forAllSystems (
        system:
        let
          pkgs = import nixpkgs { inherit system; };
        in
        {
          default = pkgs.callPackage ./package.nix { };
          infract = self.packages.${system}.default;
        }
      );

      apps = forAllSystems (system: {
        default = {
          type = "app";
          program = lib.getExe self.packages.${system}.default;
        };
        infract = self.apps.${system}.default;
      });

      devShells = forAllSystems (
        system:
        let
          pkgs = import nixpkgs { inherit system; };
        in
        {
          default = pkgs.mkShell {
            packages = [ pkgs.dotnet-sdk_10 ];
          };
        }
      );

      nixosModules.default =
        { config, pkgs, ... }:
        let
          cfg = config.services.infract;
        in
        {
          options.services.infract = {
            enable = lib.mkEnableOption "InFract controller refractor";

            enableRules = lib.mkOption {
              type = lib.types.bool;
              default = true;
              description = "Enable udev rules for infract controllers";
            };

            package = lib.mkOption {
              type = lib.types.package;
              default = self.packages.${pkgs.stdenv.hostPlatform.system}.default;
              defaultText = lib.literalExpression "inputs.infract.packages.\${pkgs.system}.default";
              description = "InFract package to use.";
            };

            user = lib.mkOption {
              type = lib.types.str;
              default = "infract";
              description = "User account that runs the InFract service.";
            };

            group = lib.mkOption {
              type = lib.types.str;
              default = "input";
              description = "Group account that runs the InFract service.";
            };

            environment = lib.mkOption {
              type = lib.types.attrsOf lib.types.str;
              default = { };
              example = {
                INFRACT_CONVERTER = "SINPUT";
              };
              description = "Environment variables passed to InFract.";
            };
          };

          config = lib.mkIf cfg.enable {
            # Required for virtual controller creation
            boot.kernelModules = [
              "uinput"
              "uhid"
            ];

            users.groups.input = { };

            users.users.${cfg.user} = {
              isSystemUser = true;
              group = cfg.group;
              extraGroups = [ "input" ];
              home = "/var/lib/infract";
              createHome = true;
            };

            services.udev.extraRules = lib.mkIf cfg.enableRules ''
              # UHID access
              KERNEL=="uhid", GROUP="input", MODE="0660", TAG+="uaccess"

              # GameSir access
              SUBSYSTEM=="usb", ATTRS{idVendor}=="3537", GROUP="input", MODE="0666", TAG+="uaccess"
            '';

            systemd.services.infract = {
              description = "InFract controller refractor";
              wantedBy = [ "multi-user.target" ];
              after = [
                "systemd-udevd.service"
                "network.target"
              ];

              serviceConfig = {
                ExecStart = lib.getExe cfg.package;
                Restart = "on-failure";
                User = cfg.user;
                Group = cfg.group;

                AmbientCapabilities = [ "CAP_SYS_ADMIN" ];
                CapabilityBoundingSet = [ "CAP_SYS_ADMIN" ];

                DeviceAllow = [
                  "/dev/uhid rw"
                  "char-input rw"
                  "char-usb_device rw"

                ];

                StateDirectory = "infract";
                WorkingDirectory = "/var/lib/infract";

                ProtectSystem = "full";
                ProtectHome = true;
                PrivateDevices = false;
              };

              environment = cfg.environment // {
                HOME = "/var/lib/infract";
                DOTNET_BUNDLE_EXTRACT_BASE_DIR = "/var/lib/infract/.dotnet/bundle";
              };
            };
          };
        };
    };
}
