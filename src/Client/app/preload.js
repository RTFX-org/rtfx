"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
// preload with contextIsolation enabled
const electron_1 = require("electron");
electron_1.contextBridge.exposeInMainWorld('myAPI', {
    doAThing: () => {
        console.log('doAThing');
        return 'doAThing';
    },
});
electron_1.contextBridge.exposeInMainWorld('myWindow', {
    minimize: () => {
        electron_1.ipcRenderer.invoke('minimize');
    },
    maximize: () => {
        electron_1.ipcRenderer.invoke('maximize');
    },
    close: () => {
        electron_1.ipcRenderer.invoke('close');
    },
});
//# sourceMappingURL=preload.js.map