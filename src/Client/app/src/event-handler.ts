import { BrowserWindow, IpcMain, IpcMainInvokeEvent, webFrame } from 'electron';
import {
  EventElectronToWeb,
  EventFunc,
  EventFuncAsync,
  EventNameElectronToWeb,
  EventNameWebToElectron,
  EventWebToElectron,
  FuncArgs,
  FuncReturn
} from 'web-app';

export class EventHandler {
  private readonly _ipcMain: IpcMain;
  private readonly _mainWindow: BrowserWindow;

  constructor(ipcMain: IpcMain, mainWindow: BrowserWindow) {
    this._ipcMain = ipcMain;
    this._mainWindow = mainWindow;
  }

  public on<T extends EventWebToElectron, A extends EventNameWebToElectron>(event: A, handler: EventFuncAsync<EventFunc<T, A>>) {
    this._ipcMain.handle(event, (_: IpcMainInvokeEvent, ...args: any[]) => (handler as any)(...args));
  }

  public send<T extends EventElectronToWeb, A extends EventNameElectronToWeb>(
    event: A,
    ...args: FuncArgs<EventFunc<T, A>>
  ): FuncReturn<T>[] {
    return this._mainWindow.webContents.send('app:on', event, ...args) as any;
  }

  public setListenerDefault<T extends EventElectronToWeb, A extends EventNameElectronToWeb>(
    event: A,
    ...args: FuncArgs<EventFunc<T, A>>
  ) {
    // TODO
  }
}
