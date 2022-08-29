.PHONY: git build

bump-dependencies: # Install required dependencies
	@python.exe -m pip install --upgrade pip
	@pip install bump2version

bump-%: bump-dependencies # Bump the (major, minor, patch) version of the application
	@bumpversion $@

clean:	# Clean the build directory of the application
	@dotnet clean

build: # Build the debug version of the application
	@dotnet build --configuration Debug /p:Platform=x64 --no-restore

build-release: # Build the release version of the application
	@dotnet build --configuration Release /p:Platform=x64 --no-restore

release: bump-minor build-release # Build the release version of the application
	

release-bugfix: bump-patch build-release # Build a bugfix release version of the application

