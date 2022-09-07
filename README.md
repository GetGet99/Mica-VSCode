# Mica VSCode (ALPHA)

A project that puts VSCode on Mica

Disclaimer: ALPHA! This project is still unstable!

## Limitations of Overlay Title Bar
You cannot click on the menu bar. To use any of the menu bar item, please use Alt + [Initials] (Alt + F = File, for example)

## How to install
1. Download and Install [Windows App SDK 1.0.3](https://aka.ms/windowsappsdk/1.0/1.0.3/windowsappruntimeinstall-1.0.3-x64.exe)
2. Download and Install Visual Studio Code if you haven't already (this project does not automatically install VSCode)
3. Extensions [Vibrancy](https://marketplace.visualstudio.com/items?itemName=illixion.vscode-vibrancy-continued) and [Fix VSCode Checksums](https://marketplace.visualstudio.com/items?itemName=lehni.vscode-fix-checksums))
4. Run the following commands after you open (You need to do these after you open for the first time)
```
Ctrl + Shift + P then 'Enable Vibrancy'
Ctrl + Shift + P then 'Fix Checksums: Apply'
```
Please Note: If you update Visual Studio Code, you must reload vibrany `Ctrl + Shift + P then 'Reload Vibrancy'`
5. (Optional) [Add Custom CSS](#-add-custom-css) (see the section below)
6. Close and reopen Mica VSCode if you haven't already

## Add Custom CSS
Optional for better effect

1. Install Extension [Custom CSS and JS Loader](https://marketplace.visualstudio.com/items?itemName=be5invis.vscode-custom-css)
2. Create a CSS file somewhere on your computer
```css
html:lang(en) {
    background: transparent !important;
}
.codicon-chrome-minimize:before,
.codicon-chrome-maximize:before,
.codicon-chrome-close:before {
    content: none !important;
}
```
3. `Ctrl + Shift P` then `Preferences: Open Settings (JSON)`
4. On the last few lines of the settings
```json
    ...
    "vscode_custom_css.imports": [
        /* Add this in! Change D:/ to whatever drive */
        "file://D:/path/to/file.css"
    ]
```
5. `Ctrl + Shift + P` then `Reload Custom CSS and JS`
6. Close and reopen Mica VSCode
7. `Ctrl + Shift + P` then `Fix Checksums: Apply`
8. Close and reopen Mica VSCode
