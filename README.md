# InputVisualizer

View your PC gamepad and classic retro console controller inputs in an entirely new way.
InputVisualizer allows you to see your controller input graphically over time, including press duration and button mash frequency.
Useful for speedrunners and streamers alike.

**Features**

- PC gamepad and keyboard support with customizable input mapping
- RetroSpy/NintendoSpy support for NES, SNES, and Sega Genesis consoles
- SD2SNES/FXPAK PRO support for SNES hardware using Usb2Snes/QUsb2Snes/SNI
- Customizable display with background transparency for easy integration into streaming layouts
- Displays button press frequency and duration metrics

![image](https://github.com/kfmike/InputVisualizer/assets/57804306/993da0c6-c26b-4080-b35a-cb3d063ad2dc)

# OBS Settings

Add InputVisualizer as a "Game Capture" source.

**Transparent Background**
- Check "Allow Transparency" in the game input source properties.
- Set the InputVisualizer background color alpha value to zero (default)

# SD2SNES/FXPAK PRO Game Support
Please check the following document to see an incomplete list of games that are currently supported:

https://docs.google.com/spreadsheets/d/1nq40DwiOmKDQm1oxOPezcIoIM7wi8jIx71q46V8Fz0k/edit?usp=sharing

You can update InputVisualizer with the current list by replacing the usb2snesGameList.json file with the most recent version here:

https://github.com/kfmike/InputVisualizer/blob/master/usb2snesGameList.json

# Disclaimer
This is a fun personal project for me, so I initially added features for what I do most often.
I will do my best to listen to feedback and continue to add new stuff, but it might take time.
I appreciate your patience and support!

# Credits

Game framework and UI libraries: 
  - MonoGame (https://www.monogame.net/)
  - Myra (https://github.com/rds1983/Myra)

RetroSpy signal reader code:
  - https://retro-spy.com/
  - https://github.com/retrospy/RetroSpy



