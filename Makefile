.PHONY: git python bumpversion dotnet

bump-dependencies: # Install required dependencies
	@python.exe -m pip install --upgrade pip
	@pip install bump2version --user

bump-%: bump-dependencies # Bump the (major, minor, patch) version of the application
	@bumpversion $*

clean:	# Clean the build directory of the application
	@dotnet clean

test: clean
	@dotnet test AutomaticRoadblocksTests\AutomaticRoadblocksTests.csproj --configuration Debug /p:Platform=x64 --no-restore

build: clean # Build the debug version of the application
	@dotnet build --configuration Debug /p:Platform=x64 --no-restore

build-release: # Build the release version of the application
	@dotnet build --configuration Release /p:Platform=x64 --no-restore
	@xcopy "AutomaticRoadblock\bin\x64\Release\AutomaticRoadblocks.dll" "Build\Grand Theft Auto V\plugins\LSPDFR\" /f /y
	@xcopy "AutomaticRoadblock\bin\x64\Release\AutomaticRoadblocks.ini" "Build\Grand Theft Auto V\plugins\LSPDFR\" /f /y
	@xcopy "AutomaticRoadblock\bin\x64\Release\AutomaticRoadblocks.pdb" "Build\Grand Theft Auto V\plugins\LSPDFR\" /f /y

release: bump-minor build-release # Build the release version of the application
	

release-bugfix: bump-patch build-release # Build a bugfix release version of the application

