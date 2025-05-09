########################################
#  First stage of multistage build
########################################
#  Use Build image with label `builder`
########################################
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview-alpine AS builder

# Setup working directory for project
WORKDIR /app

COPY ./.editorconfig ./
COPY ./Directory.Build.props ./
COPY ./Core.Build.props ./
COPY ./Core/Core.csproj ./Core/
COPY ./Core.Serialization/Core.Serialization.csproj ./Core.Serialization/
COPY ./Core.Marten/Core.Marten.csproj ./Core.Marten/
COPY ./Core.WebApi/Core.WebApi.csproj ./Core.WebApi/
COPY ./Sample/Tickets/Tickets/Tickets.csproj ./Sample/Tickets/Tickets/
COPY ./Sample/Tickets/Tickets.Api/ ./Sample/Tickets/Tickets.Api/

# Restore NuGet packages
RUN dotnet restore ./Sample/Tickets/Tickets.Api/Tickets.Api.csproj

# Copy project files
COPY ./Core ./Core
COPY ./Core.Serialization ./Core.Serialization
COPY ./Core.Marten ./Core.Marten
COPY ./Core.WebApi ./Core.WebApi
COPY ./Sample/Tickets/Tickets ./Sample/Tickets/Tickets
COPY ./Sample/Tickets/Tickets.Api ./Sample/Tickets/Tickets.Api

# Build project with Release configuration
# and no restore, as it�s already done
RUN dotnet build -c Release --no-restore ./Sample/Tickets/Tickets.Api/Tickets.Api.csproj

## Test project with Release configuration
## and no build, as it�s already done
#RUN dotnet test -c Release --no-build ./Sample/Tickets/Tickets.Api/Tickets.Api.csproj


# Publish project to output folder
# and no build, as it�s already done
WORKDIR /app/Sample/Tickets/Tickets.Api
RUN ls
RUN dotnet publish -c Release --no-build -o out

########################################
#  Second stage of multistage build
########################################
#  Use other build image as the final one
#    that won�t have source codes
########################################
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview-alpine

# Setup working directory for project
WORKDIR /app

# Copy published binaries from the `builder` image
COPY --from=builder /app/Sample/Tickets/Tickets.Api/out .

# Set URL that App will be exposed
ENV ASPNETCORE_URLS="http://*:5000"

# Sets entry point command to automatically
# run application on `docker run`
ENTRYPOINT ["dotnet", "Tickets.Api.dll"]
