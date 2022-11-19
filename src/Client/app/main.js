"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const electron_1 = require("electron");
const url = require("url");
const path = require("path");
const fs = require("fs");
const electronLocalshortcut = require("electron-localshortcut");
let mainWindow;
function createWindow() {
    mainWindow = new electron_1.BrowserWindow({
        width: 900,
        height: 700,
        webPreferences: {
            contextIsolation: true,
            preload: path.join(__dirname, 'preload.js'),
        },
        frame: false,
    });
    const compiledPath = path.join(__dirname, `./rtfx/index.html`);
    const servePath = path.join(__dirname, `../dist/rtfx/index.html`);
    const usedPath = fs.existsSync(compiledPath) ? compiledPath : servePath;
    mainWindow.loadURL(url.format({
        pathname: usedPath,
        protocol: 'file:',
        slashes: true,
    }));
    // Open the DevTools. If you don't want you delete this
    mainWindow.webContents.openDevTools();
    mainWindow.on('closed', function () {
        mainWindow = null;
    });
    mainWindow.on('focus', () => {
        electronLocalshortcut.register(mainWindow, ['CommandOrControl+R', 'CommandOrControl+Shift+R', 'F5'], () => { });
    });
    mainWindow.on('blur', () => {
        electronLocalshortcut.unregisterAll(mainWindow);
    });
}
electron_1.app.on('ready', () => setTimeout(createWindow, 400));
electron_1.app.on('window-all-closed', function () {
    if (process.platform !== 'darwin')
        electron_1.app.quit();
});
electron_1.app.on('activate', function () {
    if (mainWindow === null)
        createWindow();
});
electron_1.ipcMain.handle('minimize', () => {
    mainWindow.minimize();
});
electron_1.ipcMain.handle('maximize', () => {
    if (mainWindow.isMaximized()) {
        mainWindow.restore();
    }
    else {
        mainWindow.maximize();
    }
});
electron_1.ipcMain.handle('close', () => {
    mainWindow.close();
});
//# sourceMappingURL=main.js.map