# m3
An async / task oriented take on match 3

# Plugins

Bubble Font:
https://assetstore.unity.com/packages/2d/fonts/bubble-font-free-version-24987

Awesome Game UI Pack 8
https://assetstore.unity.com/packages/2d/gui/icons/awesome-game-ui-pack-8-174517

FruitFace
https://assetstore.unity.com/packages/2d/gui/icons/fruitface-animated-ui-pack-58686

Casual Puzzle World Sounds
https://assetstore.unity.com/packages/audio/music/casual-puzzle-world-sounds-free-package-123537


# TODO

- Reset alpha from hints
- Split Game config into smaller focused configs
- Make the patterns more data driven instead of using constants
- Create providers for the grid data and pre matches
- VFX
- Add Unit Tests
- Improve / implement proper services

# QA

Why am I not using MVP, MVC, MVVM for UI and some dependency injection for the services (Extenject, VContainer, etc)?
- The idea of this project is to have something small which was quick to implement. It took me 2 days for the initial implementation and then 2 extra days to refactor it using assemblies. It is not intended to become a complex full game implementation, but a sample.