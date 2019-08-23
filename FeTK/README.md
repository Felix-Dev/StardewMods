**FeTK** is a [Stardew Valley](http://stardewvalley.net/) mod framework which can be used by other modders to create experience-rich and high-quality mods. Not only does it provide APIs to simplify common mod tasks (such as adding mails to the game) but it also adds additional features on top of those provided by the game to enable completely new experiences (such as dynamic mail content or colored text)!

**This documentation is for modders. If you are a player, please see the Nexus page (TODO) instead.**

## Contents
* [Install](#install)
* [Features](#features)
* [Develop](#develop)
* [Compatibility](#compatibility)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. Install this mod from Nexus mods (TODO).
3. Run the game using SMAPI.

## Features
For a list of APIs this framework provides please check the documentation [here](https://github.com/Felix-Dev/StardewMods/blob/dev-FeTK/FeTK/docs/features.md).

## Develop
Make sure you have the framework [installed](#install). Simply add a reference to the `FeTK.dll` library file to your project. Also include the following mod dependency in the [manifest](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest) of your consuming mod:
```js
"Dependencies": [
    {
      "UniqueID": "Felix-Dev.FeTK",
      "MinimumVersion": "1.0.0" // optional; pick the required minimum version you need
    }
  ]
```

## Compatibility
For compatibility please check the documentation for each particular framework feature.

## See also
* [Release notes](release-notes.md)
* Nexus mod (TODO)
