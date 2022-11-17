import { app, BrowserWindow } from "electron";
import * as url from "url";
import * as path from "path";
import * as electronLocalshortcut from "electron-localshortcut";

let mainWindow: BrowserWindow | null;

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 900,
    height: 700,
    webPreferences: {
      nodeIntegration: true,
    },
    frame: false,
  });

  mainWindow.loadURL(
    url.format({
      pathname: path.join(__dirname, `./rtfx/index.html`),
      protocol: "file:",
      slashes: true,
    })
  );
  // Open the DevTools. If you don't want you delete this
  mainWindow.webContents.openDevTools();

  mainWindow.on("closed", function () {
    mainWindow = null;
  });

  mainWindow.on("focus", () => {
    electronLocalshortcut.register(
      mainWindow!,
      ["CommandOrControl+R", "CommandOrControl+Shift+R", "F5"],
      () => {}
    );
  });

  mainWindow.on("blur", () => {
    electronLocalshortcut.unregisterAll(mainWindow!);
  });
}

app.on("ready", createWindow);

app.on("window-all-closed", function () {
  if (process.platform !== "darwin") app.quit();
});

app.on("activate", function () {
  if (mainWindow === null) createWindow();
});
