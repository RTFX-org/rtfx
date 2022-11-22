import { ElectronEvent, EventFunc, EventFuncAsync, EventName } from 'web-app';

export class EventHandler {
  private readonly _ipcMain: Electron.IpcMain;

  constructor(ipcMain: Electron.IpcMain) {
    this._ipcMain = ipcMain;
  }

  public on<T extends ElectronEvent, A extends EventName>(event: A, handler: EventFuncAsync<EventFunc<T, A>>) {
    this._ipcMain.handle(event, (event: Electron.IpcMainInvokeEvent, ...args: any[]) => (handler as any)(...args));
  }
}
