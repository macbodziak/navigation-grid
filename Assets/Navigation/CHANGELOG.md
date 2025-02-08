# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.12.0]  - 2024-12-10

### Fixed
- Checking if Nodes were adjecent in HexGrid was excluded form Build version and only in Editor build causing Build Errors
- Added missing using statement in NavGridInspector that prevented build 

## [0.11.0]  - 2024-12-10
### Added
-  Added cancellation token to Actors MoveAlongPathAsync method, so it can be canceled as a task

### Changed
- made Actors MoveAlongPathCoroutine public. Now it can be started directly as a coroutine without calling proxy method 

### Fixed
- fixed Actor not stopping movement when canceled in Coroutine

## [0.10.0] - 2024-12-10
### Added
-  WalkalbeArea now has a method to check if a grid node is adjacent to the walkable area. Useful for detecting if an enemy can be reach at the border of the walkable area

### Changed
- moved SerializableDictionary to Utilitites folder and placed it under the namespace Navigation.Utilities

## [0.9.0] - 2024-12-08
### First Release

*This initial release introduces the Grid Navigation system.*