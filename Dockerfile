# Runs the suite in a container with a real Chrome, so a machine only needs Docker.
#   docker build -t limestone-qa .
#   docker run --rm -v "$PWD/TestResults:/app/TestResults" limestone-qa
FROM mcr.microsoft.com/dotnet/sdk:8.0

RUN apt-get update \
 && apt-get install -y --no-install-recommends wget gnupg ca-certificates fonts-liberation \
 && wget -q -O /tmp/chrome.deb https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb \
 && apt-get install -y --no-install-recommends /tmp/chrome.deb \
 && rm -rf /tmp/chrome.deb /var/lib/apt/lists/*

WORKDIR /app
COPY . .
RUN dotnet restore

ENV UI__HEADLESS=true

ENTRYPOINT ["dotnet", "test", "--logger", "trx;LogFileName=results.trx", "--results-directory", "/app/TestResults"]
