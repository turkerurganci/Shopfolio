# Application image (05 8.2.1, MK-69): one release is one artifact -- the application and the migration bundle it carries.
# Build context is the repository root (09 4.3 item 1):  docker build -f deploy/images/app/Dockerfile .
# Base images are pinned by tag and digest (05 8.2.3, MK-10); the monthly maintenance release renews both (05 8.8.2).

# ---- build: .NET SDK image (the tag equals global.json's SDK version, which disallows roll-forward) ----
FROM mcr.microsoft.com/dotnet/sdk:10.0.400-noble@sha256:4beef5b8919dcaa2dc924233bd069257e883cc7a061e09088a97d152d6a48510 AS build
WORKDIR /src

# Restore first, from the files that define the dependency graph only, so source edits do not invalidate the package layer.
COPY global.json nuget.config Directory.Build.props Directory.Packages.props ./
COPY .config/dotnet-tools.json .config/dotnet-tools.json
COPY src/Shopfolio.Web/Shopfolio.Web.csproj src/Shopfolio.Web/packages.lock.json src/Shopfolio.Web/
RUN dotnet tool restore \
    && dotnet restore src/Shopfolio.Web/Shopfolio.Web.csproj --locked-mode

# The analyzer configuration (.editorconfig, incl. the generated-code mark of the migrations folder) must reach the in-image build,
# so the image build sees the same analysis as the local and CI builds. It is copied here, not in the restore layer, as it does not affect restore.
COPY .editorconfig ./
COPY src/Shopfolio.Web src/Shopfolio.Web

# The application: framework-dependent, no native host (the runtime image carries the shared framework).
RUN dotnet publish src/Shopfolio.Web/Shopfolio.Web.csproj -c Release --no-restore -p:UseAppHost=false -o /out/app

# The migration bundle (09 10.6): one single executable file made from the EF Core migrations. A throwaway lock file keeps the
# tracked packages.lock.json untouched (the bundle publishes with a runtime identifier, which would add a target to it).
RUN NuGetLockFilePath=/tmp/throwaway.packages.lock.json \
    dotnet ef migrations bundle --project src/Shopfolio.Web/Shopfolio.Web.csproj --output /out/efbundle --force

# ---- runtime: ASP.NET Core runtime image; runs as the image's non-root user "app" (UID 1654) ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0.14-noble@sha256:de0ee358cdd672f7017a003bed21e9ac9ed48a27c922a59b18175b6e82fa7fee AS final
WORKDIR /app
COPY --from=build /out/app ./
COPY --from=build --chmod=0555 /out/efbundle ./efbundle

# Mount points of the Compose volumes (deploy/compose.yaml). A named volume takes its owner from the image's directory, so
# the directories exist here and belong to the non-root user. The backup-status directory is written by the backup tool (another
# user) and read by the application, so it is open to both; it holds only the status file (05 9.4.10).
RUN mkdir -p /var/lib/shopfolio/images /var/lib/shopfolio/digital /var/log/shopfolio/access /var/log/shopfolio/app /var/lib/shopfolio/backup-status \
    && chown -R app:app /var/lib/shopfolio/images /var/lib/shopfolio/digital /var/log/shopfolio \
    && chmod 1777 /var/lib/shopfolio/backup-status

# No secret enters the image: the database credential and every other secret arrives as a mounted file at run time (09 17 item 2).
# No ENV: the base image already listens on port 8080 (ASPNETCORE_HTTP_PORTS=8080), and the runtime diagnostics stay at their defaults.
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "Shopfolio.Web.dll"]
