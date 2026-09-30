# TelltaleToolKit Databases

Welcome to the TelltaleToolKit database documentation. This guide provides an overview of the database system used in the project, including data structure standards, file organization, and a table of supported games and formats.
You can find the [changelog here](./CHANGELOG.md).

## Table of Contents

<!-- omit in toc -->

- [TelltaleToolKit Databases](#telltaletoolkit-databases)
  - [Table of Contents](#table-of-contents)
- [Overview](#overview)
  - [Database Categories](#database-categories)
  - [File Naming \& Standards](#file-naming--standards)
  - [Game Descriptor Fields](#game-descriptor-fields)
    - [Game Descriptor Full Example](#game-descriptor-full-example)
  - [Supported Games Matrix](#supported-games-matrix)
  - [Contributing](#contributing)
  - [FAQ](#faq)


# Overview

The library relies on a set of structured databases to support a wide variety of Telltale games. Each game is treated as an independent entry, enabling easy extension and maintenance. The databases are intended for use by both the library and community contributors who wish to add or improve support for games.

## Database Categories

There are three main categories of databases:

1. **Game Descriptors**  
   - **Purpose:** Defines general configuration for each supported game.
   - **Format:** JSON file (extension: `.json`)
   - **Aliases:** Game snapshots, game configurations.

2. **Version Databases**  
   - **Purpose:** Describes metaclass descriptions for a given game.
   - **Format:** JSON file (extension: `.vdb.json`)
   - **Aliases:** Metaclass description databases.
   - **Note:** Version databases **require a game descriptor to function with the same sluggified name**.

3. **Hash Database(s)** (Planned/Experimental)  
   - **Purpose:** Contains SQLite databases with file hashes unique to each game.
   - **Format:** SQLite database file (extension: `.db`)
   - **Aliases:**
   - **Note:** Do not try to implement any hash databases on your own as of now.

## File Naming & Standards

For consistency, all database files follow a sluggified naming convention with the appropriate extension:

```
[slugified-title]-[year]-[month]-[platform]-[demo]
```
- **slugified-title:** Lowercase, spaces replaced with hyphens, special characters removed.
- **year/month:** Release date of the game (e.g., `2012` or `2016-01`).
- **platform:** If applicable, a platform tag (e.g., `xbox360`).
- **demo:** If applicable and the game itself was a demo, a demo tag (e.g., `demo`).

**Examples:**
- `the-walking-dead-2012.json`
- `the-wolf-among-us-2013.vdb.json`

## Game Descriptor Fields

| Field                    | Type                 | Description                                                                                                                                                                                    | Example                                       |
| ------------------------ | -------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------- |
| `Name`                   | `string`             | Display name of the game.                                                                                                                                                                      | `"The Walking Dead: Season 2"`                |
| `Description`            | `string`             | A brief description of the game.                                                                                                                                                               | `"The Walking Dead: Season 2 (2013) for PC."` |
| `BlowfishKey`            | `string` (see below) | Blowfish encryption key for encrypting/decrypting files. Can be a custom key in a string form or an enum name from the [`T3BlowfishKey`](/src/TelltaleToolKit/Utility/T3BlowfishKey.cs) class. | `"Twds2"`                                     |
| `IsTtarch2`              | `bool`               | Whether the game uses the TTArch2 archive format (`true` for TTArch2, `false` for TTArch).                                                                                                     | `true`                                        |
| `TtarchVersion`          | `int`                | The version of the archive format used by the game. If `IsTtarch2` is true, use versions 2-4, otherwise - versions 0-9.                                                                        | `3`                                           |
| `LuaVersion`             | `string`             | Lua scripting engine version used by the game. It can be either `5.0.2`, `5.1.2` or `5.2.3`.                                                                                                   | `"5.1.2"` or `"5.1"`                          |
| `MetaStreamVersion`      | `string`             | Version of the metastream format used in the game. Can be `MBIN`, `MTRE`, `MSV5` or `MSV6`.                                                                                                    | `"MSV5"`                                      |
| `AreSymbolsHashed`       | `bool`               | Whether the game's symbols are hashed (`true` or `false`). This should almost always be `true`. Only `MBIN` are allowed to not be hashed.                                                      | `true`                                        |
| `EnableOodleCompression` | `bool`               | Whether Oodle compression is enabled for game archives.                                                                                                                                        | `false`                                       |

### Game Descriptor Full Example

```json
{
  "Name": "The Walking Dead: Season 2",
  "Description": "The Walking Dead: Season 2 (2013) for PC.",
  "BlowfishKey": "Twds2",
  "IsTtarch2": true,
  "TtarchVersion": 3,
  "LuaVersion": "5.1.2",
  "MetaStreamVersion": "MSV5",
  "AreSymbolsHashed": true,
  "EnableOodleCompression": false,
}
```

## Supported Games Matrix

WIP, please refer to the files themselves or the changelog.

## Contributing

If you'd like to add a new game, update existing entries, or help implement hash databases, please follow these guidelines:

1. Ensure your file names follow the [File Naming & Standards](#file-naming--standards) section.
2. Use the provided data structure reference as a template.
3. Submit a pull request with your changes, including game details and configurations.
4. For new categories or attributes, please open an issue for discussion.
5. You can create your own game databases using one of the [sample projects](/samples/VersionDatabaseCreator) and open a pull request with the new files.

## FAQ

**Q:** Why are there three types of databases?  
**A:** To separate general game configuration, version-specific metadata, and hash-related data, making the toolkit more extensible and maintainable.

**Q:** Can I use these databases in my own tools?  
**A:** Absolutely! All databases are open for community use and extension.

**Q:** What is a "slugified title"?  
**A:** A string, which is in lowercase, has spaces replaced with hyphens, and has
special characters removed (e.g., "The Walking Dead: Season 2" → `the-walking-dead-season-2`).