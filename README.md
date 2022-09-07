# Mica VSCode (ALPHA)

A project that puts VSCode on Mica.

Disclaimer: ALPHA! This is still unstable!

## Requirements
1. Visual Studio Code (this project does not automatically install vscode!)
2. Run following commands after you open (2 extensions should be automatically installed: [Vibrancy](https://marketplace.visualstudio.com/items?itemName=eyhn.vscode-vibrancy) and [Fix VSCode Checksums](https://marketplace.visualstudio.com/items?itemName=lehni.vscode-fix-checksums))
3. You need to do these after you open for the first time
```
Ctrl + Shift + P then 'Reload Vibrancy'
Ctrl + Shift + P then 'Fix Checksums: Apply'
```
4. (Optional) [Add Custom CSS](#-add-custom-css) (see section below)
5. Close and reopen Mica VSCode if you haven't already

## Add Custom CSS
Optional for better effect

1. Install Extension [Custom CSS and JS Loader](https://marketplace.visualstudio.com/items?itemName=be5invis.vscode-custom-css)
2. Create a css file somewhere in your computer
```css
html:lang(en) {
    background: transparent !important;
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
