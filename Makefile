.PHONY: git build

clean:	# Clean the build directory of the application
	@dotnet clean

build: # Build the debug version of the application
	@dotnet build --configuration Debug /p:Platform=x64 --no-restore

release: # Build the release version of the application
	@dotnet build --configuration Release /p:Platform=x64 --no-restore
