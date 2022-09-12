# Automatic Roadblocks

![Build](https://github.com/yoep/AutomaticRoadblock/workflows/Build/badge.svg)
![Version](https://img.shields.io/github/v/tag/yoep/AutomaticRoadblock?label=version)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

Automatic Roadblocks allows the dispatching of roadblocks during a pursuit based on a selected pursuit level.
When roadblocks are deployed, custom scanner audio is played as well as for the indication that the pursuit level is automatically increased.

## Features

- Automatic roadblock placement during pursuits
- Roadblock hit/bypass detection
- Roadblock cops automatically join the pursuit after a hit/bypass
- Request a roadblock during a pursuit
- Dynamic roadblocks with light sources during the evening/night
- Manual configurable roadblock placement
- Configurable traffic redirection
- Spike strips
- Junction roadblocks

## API/plugin integration

Plugins can use the [Functions](AutomaticRoadblock/Api/Functions.cs) for available api's.

## Development

### Dependencies

- NuGet
- .NET 4.8 SDK
- Rage Plugin Hook SDK
- Rage Native UI
- LSPDFR SDK 4.9+
- Make

### Getting started

To get started using this project, do the following steps.

1. Download all nuget packages through the `restore` target

```bash
make restore
```

2. Try to compile the project

```bash
make build
```