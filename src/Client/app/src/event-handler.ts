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
  private readonly _ipcMain: Electron.IpcMain;
  private readonly _listeners: {
    [key: string]: {
      func: EventFuncAsync<any>[];
      defaultValue?: any[];
    };
  } = {};

  constructor(ipcMain: Electron.IpcMain) {
    this._ipcMain = ipcMain;

    this._ipcMain.handle(
      'app:on',
      (_: Electron.IpcMainInvokeEvent, event: EventNameElectronToWeb, handler: EventFuncAsync<any>) => {
        console.log('app:on', event);
        this.addListener(event, handler);
      }
    );
  }

  public on<T extends EventWebToElectron, A extends EventNameWebToElectron>(event: A, handler: EventFuncAsync<EventFunc<T, A>>) {
    this._ipcMain.handle(event, (_: Electron.IpcMainInvokeEvent, ...args: any[]) => (handler as any)(...args));
  }

  public send<T extends EventElectronToWeb, A extends EventNameElectronToWeb>(
    event: A,
    ...args: FuncArgs<EventFunc<T, A>>
  ): FuncReturn<T>[] {
    const evt = this._listeners[event].func as EventFuncAsync<T>[] | undefined;
    if (!evt) {
      throw new Error(`Event ${event} not found`);
    }
    return evt.map(handler => handler(...args));
  }

  public setListenerDefault<T extends EventElectronToWeb, A extends EventNameElectronToWeb>(
    event: A,
    ...args: FuncArgs<EventFunc<T, A>>
  ) {
    const listener = this._listeners[event];
    if (!listener) {
      this._listeners[event] = {
        func: [],
        defaultValue: args
      };
    } else {
      listener.defaultValue = args;
    }
  }

  private addListener<T extends EventElectronToWeb, A extends EventNameElectronToWeb>(
    event: A,
    handler: EventFuncAsync<EventFunc<T, A>>
  ) {
    const listener = this._listeners[event];
    if (!listener) {
      this._listeners[event] = {
        func: []
      };
    }
    const evt = listener.func;
    evt.push(handler);
    if (listener.defaultValue) {
      const value = listener.defaultValue as FuncArgs<EventFunc<T, A>>;
      handler(...value);
    }
  }
}
