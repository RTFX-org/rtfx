import { Injectable } from '@angular/core';
import {
  EventElectronToWeb,
  EventFunc,
  EventNameElectronToWeb,
  EventNameWebToElectron,
  EventWebToElectron,
  FuncArgs,
  FuncReturn
} from '../models/electron.model';

@Injectable({ providedIn: 'root' })
export class ElectronService {
  constructor() {}

  public send<T extends EventWebToElectron, A extends EventNameWebToElectron>(
    event: A,
    ...args: FuncArgs<EventFunc<T, A>>
  ): FuncReturn<EventFunc<T, A>> {
    return (<any>window).rtfxApi.send(event, ...args);
  }

  public on<T extends EventElectronToWeb, A extends EventNameElectronToWeb>(
    event: A,
    listener: (...args: FuncArgs<EventFunc<T, A>>) => void
  ): void {
    (<any>window).rtfxApi.on(event, listener);
  }
}
