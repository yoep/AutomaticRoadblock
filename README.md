# Automatic Roadblocks

This LSPDFR plugin allows the creation of automatic roadblocks during pursuits.

## Features

- Automatic roadblock placement during pursuits
- Roadblock hit/bypass detection
- Roadblock cops automatically join the pursuit after a hit/bypass
- Request a roadblock during a pursuit
- Dynamic roadblocks with light sources during the evening/night
- Manual configurable roadblock placement

## Planned features

- Spikestrips
- Police smart radio integration
- Close road/lane
- Redirect traffic
- Junction roadblocks
- More scenery variations within the roadblocks
- Additional scenery such as other emergency vehicles to manual roadblock placements

## Pursuit

The pursuit option allows dispatching of roadblocks while in a pursuit.

### Levels

| Level | Vehicle type           | Barrier type | Lanes blocked | Cop action            | Additions     |
|-------|------------------------| --- | --- |-----------------------|---------------|
| 1     | Locale                 | Small cones | Lanes in the same direction as pursuit | In car                |               |
| 2     | Locale                 | Big cones | All lanes | Aims at target        |               |
| 3     | Locale/transporter     | Police do not cross | All lanes | Aims/shoots at target |               |
| 4     | Locale/transporter/FBI | Police do not cross | All lanes | Aims/shoots at target | Chase vehicle |
| 5     | FBI/Swat               | Police do not cross | All lanes | Aims/shoots at target |               |