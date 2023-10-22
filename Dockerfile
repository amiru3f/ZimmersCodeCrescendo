FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-native

WORKDIR /build
COPY native-alg/* .

RUN apt update
RUN apt install software-properties-common build-essential libssl-dev -y

COPY . .

RUN make clean pbkdf2.so

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-dotnet

WORKDIR /build

COPY app/* .
RUN dotnet restore
RUN dotnet publish -c Release -o out



FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

COPY --from=build-native /build/pbkdf2.so .
COPY --from=build-dotnet /build/out .

ENTRYPOINT ["dotnet", "app.dll"]
