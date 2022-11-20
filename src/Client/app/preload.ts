// preload with contextIsolation enabled
import { contextBridge, ipcRenderer } from 'electron';

contextBridge.exposeInMainWorld('rtfxApi', {
  send: (channel: string, data: any) => ipcRenderer.invoke(channel, data)
});
