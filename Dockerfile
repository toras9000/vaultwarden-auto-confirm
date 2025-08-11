FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

WORKDIR /work
COPY ./  ./

RUN dotnet publish src -o ./publish


FROM mcr.microsoft.com/dotnet/runtime:9.0

COPY --from=builder /work/publish          /app

CMD ["app/vaultwarden-auto-confirm"]
