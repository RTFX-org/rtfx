// preload with contextIsolation enabled
import { contextBridge, ipcRenderer } from 'electron';

contextBridge.exposeInMainWorld('rtfxApi', {
  send: (channel: string, ...data: any[]) => ipcRenderer.invoke(channel, ...data),
  on: (channel: string, listener: (...args: any[]) => void) => ipcRenderer.invoke('app:on', channel, listener)
});
