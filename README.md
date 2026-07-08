# ThinkPad Toolbox

A small, modern Windows utility for ThinkPad laptops. It lets you control the status
LEDs, the CPU fan, and the keyboard backlight, and shows live CPU temperature, fan RPM,
and battery information, all from one tray app.

<p align="center">
  <a href="https://github.com/aungkokomm/ThinkPad-Toolbox/releases/latest"><img alt="Download" src="https://img.shields.io/badge/Download-Latest%20Release-2F6FED?style=for-the-badge"></a>
  <a href="https://github.com/aungkokomm/ThinkPad-Toolbox/releases"><img alt="Downloads" src="https://img.shields.io/github/downloads/aungkokomm/ThinkPad-Toolbox/total?style=for-the-badge&label=Downloads&color=8A3FFC"></a>
  <img alt="Platform" src="https://img.shields.io/badge/Windows%2010%2F11-x64-555?style=for-the-badge">
  <img alt="License" src="https://img.shields.io/badge/License-MIT-1FA855?style=for-the-badge">
</p>

> Not affiliated with, endorsed by, or sponsored by Lenovo. An independent, free tool.
> "Lenovo" and "ThinkPad" are trademarks of Lenovo.
<img width="419" height="485" alt="image" src="https://github.com/user-attachments/assets/51e27529-045f-4ca9-846f-8a84de5e5f2c" />

## Why this exists

It started with one small LED I couldn't turn off. Subtle enough to ignore, persistent
enough to annoy. Hunting for a fix, I found ValiNet's excellent
[ThinkPad LED Control](https://github.com/valinet/ThinkPadLEDControl), and it did the job
beautifully.

Then Windows 11 kept tightening its driver rules. Memory Integrity now blocks the old
WinRing0 driver outright, and I didn't want a tool I rely on to quietly stop working one
update later. So I modernized it: rebuilt on .NET 8, added the signed PawnIO driver so it
runs on locked-down Windows 11, and folded in the things I'd always wished it had: fan
control, keyboard backlight, and live temperature, fan, and battery readouts.

ThinkPad Toolbox is that tool.



## Features

- **LEDs**: turn the power, red dot, microphone-mute, and sleep (moon) LEDs on, off, or into their blink state.
- **CPU fan**: switch between firmware **Auto** and **Full** speed.
- **Keyboard backlight**: Off / Low / High, with options to remember the level across sleep/restart and to dim it when apps go full screen.
- **System status**: live CPU temperature (color coded), fan RPM, and battery charge / health / cycle count.
- Lives in the system tray; optional start at Windows logon. (**Win+Shift+L** global hotkey to summon the window from the tray)

## Requirements

- Windows 10 or 11 (64-bit)
- A ThinkPad (LED/fan/keyboard control talk to the embedded controller and are model dependent)
- Administrator rights (required by the hardware driver; the app elevates itself)

## Compatibility

Developed and tested on a **ThinkPad E14 running Windows 11 25H2**, where everything works well.

This tool talks directly to the ThinkPad embedded controller, and that layout differs
between models and generations. On other ThinkPads some or all controls may do nothing,
read the wrong values, or behave unexpectedly. **Use at your own risk.** If anything looks
off (battery, charging, or thermals), quit the app and reboot; that resets the controller.

If it works on your model, opening an issue with your model number helps build a
known-compatible list.

## Download

Grab the latest installer from the [Releases](../../releases) page and run it. It is
self-contained, so no separate .NET runtime is required.

## Drivers

The app talks to the embedded controller through a low-level driver:

- **WinRing0** (default): works out of the box, but is **blocked by Windows 11 when Memory
  Integrity / Core Isolation is enabled**.
- **PawnIO**: a modern, signed driver that works with Memory Integrity on. The installer
  offers to install it (and ticks that option automatically when it detects Memory Integrity).
  You can also get it from [pawnio.eu](https://pawnio.eu). After installing, hold **Shift**
  while launching the app and pick the PawnIO driver.

## Building from source

```
dotnet build LEDControl/LEDControl.csproj -c Release
```

To produce the self-contained app and the installer (Inno Setup required, plus
`installer/redist/PawnIO_setup.exe` from pawnio.eu):

```
dotnet publish LEDControl/LEDControl.csproj -c Release -r win-x64 --self-contained true -p:PublishReadyToRun=true -o publish/v1.0.0
ISCC installer/ThinkPadToolbox.iss
```

## License

ThinkPad Toolbox is released under the [MIT License](LICENSE), Copyright (c) 2026 Aung Ko Ko.

It is a derivative work of **ThinkPad LED Control** by Valentin-Gabriel Radu (ValiNet),
used under the ISC License, and contains embedded-controller code derived from
**TPFanControl**. Those notices are retained in [LICENSE](LICENSE). The bundled
**WinRing0** driver is © OpenLibSys.org; **PawnIO** is a separate, independently licensed
driver.

## Credits

- [ThinkPad LED Control](https://github.com/valinet/ThinkPadLEDControl) by ValiNet, the project this is based on
- TPFanControl, for the embedded-controller access approach
- [PawnIO](https://pawnio.eu) by namazso, the modern signed driver
