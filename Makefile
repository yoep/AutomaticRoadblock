.PHONY: git python bumpversion dotnet

bump-dependencies: # Install required dependencies
	@python.exe -m pip install --upgrade pip
	@pip install bump2version --user

bump-%: bump-dependencies # Bump the (major, minor, patch) version of the application
	@bumpversion $*

restore: # Restore the nuget packages
	@nuget restore AutomaticRoadblock.sln

clean:	# Clean the build directory of the application
	@dotnet clean

test: clean
	@dotnet test --configuration Debug /p:Platform=x64 --no-restore

build: clean # Build the debug version of the application
	@dotnet build --configuration Debug /p:Platform=x64 --no-restore

build-release: # Build the release version of the application
	@dotnet build AutomaticRoadblock/AutomaticRoadblock.csproj --configuration Release /p:Platform=x64 --no-restore
	@xcopy "AutomaticRoadblock\bin\x64\Release\AutomaticRoadblocks.dll" "Build\Grand Theft Auto V\plugins\LSPDFR\" /f /y
	@xcopy "AutomaticRoadblock\bin\x64\Release\AutomaticRoadblocks.ini" "Build\Grand Theft Auto V\plugins\LSPDFR\" /f /y
	@xcopy "AutomaticRoadblock\bin\x64\Release\AutomaticRoadblocks.pdb" "Build\Grand Theft Auto V\plugins\LSPDFR\" /f /y
	@xcopy "AutomaticRoadblock\bin\x64\Release\AutomaticRoadblocks.xml" "Build\API Documentation\" /f /y
	@xcopy "AutomaticRoadblock\Api\Functions.cs" "Build\API Documentation\" /f /y

release: bump-minor build-release # Build the release version of the application
	

release-bugfix: bump-patch build-release # Build a bugfix release version of the application

