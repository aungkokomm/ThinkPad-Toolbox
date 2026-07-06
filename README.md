# ThinkPad Toolbox

A small, modern Windows utility for ThinkPad laptops. It lets you control the status
LEDs, the CPU fan, and the keyboard backlight, and shows live CPU temperature, fan RPM,
and battery information, all from one tray app.

> Not affiliated with, endorsed by, or sponsored by Lenovo. An independent, free tool.
> "Lenovo" and "ThinkPad" are trademarks of Lenovo.
<img width="457" height="473" alt="image" src="https://github.com/user-attachments/assets/732cfdcb-ff92-4ecc-b189-4130c1f74ee4" />


## Features

- **LEDs**: turn the power, red dot, microphone-mute, and sleep (moon) LEDs on, off, or into their blink state.
- **CPU fan**: switch between firmware **Auto** and **Full** speed.
- **Keyboard backlight**: Off / Low / High, with options to remember the level across sleep/restart and to dim it when apps go full screen.
- **System status**: live CPU temperature (color coded), fan RPM, and battery charge / health / cycle count.
- Lives in the system tray; optional start at Windows logon.

## Requirements

- Windows 10 or 11 (64-bit)
- A ThinkPad (LED/fan/keyboard control talk to the embedded controller and are model dependent)
- Administrator rights (required by the hardware driver; the app elevates itself)

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
