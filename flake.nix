{
  description = "Streamfox";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-25.05";
  };

  outputs = {
    self,
    nixpkgs,
  }: let
    supportedSystems = ["x86_64-linux"];
    forAllSystems = nixpkgs.lib.genAttrs supportedSystems;
  in {
    packages = forAllSystems (
      system: let
        pkgs = nixpkgs.legacyPackages.${system};
        frontend = pkgs.buildNpmPackage {
          pname = "streamfox-frontend";
          version = "0.0.0";

          src = ./frontend/.;
          npmDeps = pkgs.importNpmLock {
            npmRoot = ./frontend/.;
          };

          npmConfigHook = pkgs.importNpmLock.npmConfigHook;
        };
      in {
        default = pkgs.buildGoModule {
          pname = "streamfox-backend";
          version = "0.0.0";

          src = ./backend/.;
          vendorHash = "sha256-/qJxkvWe7IkW/MHZskvPsXIaqjmC83hP8wTUsUWZQNk=";

          postInstall = ''
            cp -r ${frontend}/lib/node_modules/streamfox-frontend/frontend $out/bin/frontend
          '';
        };
      }
    );

    nixosModules.default = {
      config,
      lib,
      pkgs,
      utils,
      ...
    }:
      with lib; let
        description = "Video sharing server";
        cfg = config.services.streamfox;
      in {
        options.services.streamfox = {
          enable = mkEnableOption description;

          port = mkOption {
            type = types.ints.u16;
            description = "Accept HTTP requests on the specified TCP port";
          };

          secureUrls = mkOption {
            type = types.bool;
            description = "Whether to direct users via a https scheme when generating URLs";
          };
        };

        config = let
          dataDir = "/var/lib/streamfox";
          configFile =
            pkgs.writeText ".env"
            ''
              APP_CONFIG_ROOT=${dataDir}/config
              APP_DATA_ROOT=${dataDir}/data
              APP_TOKEN_LIFESPAN_HRS=4368
              APP_SCHEME=${
                if cfg.secureUrls
                then "https"
                else "http"
              }
              APP_PORT=${toString cfg.port}

              DB_HOST=localhost
              DB_PORT=5432
              DB_NAME=streamfox
              DB_USER=streamfox
              DB_PASSWORD=123456

              DEBUG_FORWARD_HOST=localhost
              DEBUG_FORWARD_PORT=8602
            '';
        in
          mkIf cfg.enable {
            users.groups."streamfox" = {};
            users.users."streamfox" = {
              group = "streamfox";
              isSystemUser = true;
            };

            systemd = {
              services.streamfox = {
                inherit description;
                wantedBy = ["multi-user.target"];

                serviceConfig = {
                  ExecStart = utils.escapeSystemdExecArgs [
                    "${self.packages.${pkgs.system}.default}/bin/streamfox-backend"
                    configFile
                  ];
                  WorkingDirectory = "${self.packages.${pkgs.system}.default}/bin";

                  User = "streamfox";
                  Group = "streamfox";

                  Restart = "always";
                  Type = "exec";
                };

                environment."GIN_MODE" = "release";
                path = [pkgs.ffmpeg-headless];
              };

              tmpfiles.rules = [
                # Type Path Mode User Group Age Argument
                "d ${dataDir} 0750 streamfox streamfox - -"
              ];
            };

            services.postgresql = {
              enable = true;

              enableTCPIP = true;
              settings.port = 5432;

              ensureDatabases = ["streamfox"];

              ensureUsers = [
                {
                  name = "streamfox";
                  ensureDBOwnership = true;
                }
              ];

              authentication = ''
                # type database DBuser origin-address auth-method
                host streamfox streamfox localhost trust
              '';
            };
          };
      };
  };
}
