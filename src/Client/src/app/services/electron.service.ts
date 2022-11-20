import { Injectable } from '@angular/core';
import { ElectronEvent, EventFunc, EventName, FuncArgs, FuncReturn } from '../models/electron.model';

@Injectable({ providedIn: 'root' })
export class ElectronService {
  constructor() {}

  public send<T extends ElectronEvent, A extends EventName>(
    event: A,
    ...args: FuncArgs<EventFunc<T, A>>
  ): FuncReturn<EventFunc<T, A>> {
    return (<any>window).rtfxApi.send(event, ...args);
  }
}
