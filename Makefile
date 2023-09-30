.PHONY: git python bumpversion dotnet

bump-dependencies: # Install required dependencies
	@python.exe -m pip install --upgrade pip
	@pip install bump2version --user

bump-%: bump-dependencies # Bump the (major, minor, patch) version of the application
	@bumpversion $*

copy-%: # Copy all compiled files to the Build directory
	@cp -v "AutomaticRoadblock/bin/x64/$*/AutomaticRoadblocks.dll" "Build/Grand Theft Auto V/plugins/LSPDFR/"
	@cp -v "AutomaticRoadblock/bin/x64/$*/AutomaticRoadblocks.ini" "Build/Grand Theft Auto V/plugins/LSPDFR/"
	@cp -v "AutomaticRoadblock/bin/x64/$*/AutomaticRoadblocks.pdb" "Build/Grand Theft Auto V/plugins/LSPDFR/"
	@cp -v "AutomaticRoadblock/bin/x64/$*/plugins/LSPDFR/Automatic Roadblocks/data/"*.xml "Build/Grand Theft Auto V/plugins/LSPDFR/Automatic Roadblocks/data/"

restore: # Restore the nuget packages
	@nuget restore AutomaticRoadblock.sln

clean: # Clean the bin directory of the application
	@dotnet clean

test: clean
	@dotnet test --configuration Debug --no-restore

build-dotnet: clean # Build the debug version of the application
	@dotnet build --configuration "Debug" --no-restore

build-dotnet-release: clean # Build the release version of the application
	@dotnet build --configuration "Release" --no-restore

build: build-dotnet copy-Debug # Prepare & Build the Debug version of the application

build-release: build-dotnet-release copy-Release # Prepare & Build the Release version of the application
	@cp "AutomaticRoadblock\\Api\\Functions.cs" "Build\\API Documentation\\"

release: bump-minor build-release # Build the release version of the application

release-bugfix: bump-patch build-release # Build a bugfix release version of the application

