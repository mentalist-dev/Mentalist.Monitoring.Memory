set version=1.1.1

dotnet publish -c Release -o ./published

docker build --no-cache --force-rm -t monitoring-memory:%version% --build-arg VERSION=%version% -f Dockerfile .
docker tag monitoring-memory:%version% monitoring-memory:%version%-win-ltsc2019

docker tag monitoring-memory:%version% mentalistdev/monitoring-memory:%version%
docker push mentalistdev/monitoring-memory:%version%

docker tag monitoring-memory:%version% mentalistdev/monitoring-memory:%version%-win-ltsc2019
docker push mentalistdev/monitoring-memory:%version%-win-ltsc2019

docker image prune --filter label=stage=build -f
