import { AppSettings } from './app-settings';

export type FuncArgs<A> = A extends (...args: infer Args) => any ? Args : never;
export type FuncReturn<A> = A extends (args: any) => infer Return ? Promise<Return> : Promise<void>;
export type EventFuncAsync<F> = (...args: FuncArgs<F>) => FuncReturn<F>;
export type ExtractFunc<A> = A extends { func: any } ? A['func'] : never;
export type EventFunc<A, T> = A extends { name: T } ? ExtractFunc<A> : never;

export type EventNameWebToElectron = EventWebToElectron['name'];
export type EventFuncWebToElectron<A extends EventWebToElectron, T> = EventFunc<A, T>;

export type EventNameElectronToWeb = EventElectronToWeb['name'];
export type EventFuncElectronToWeb<A extends EventElectronToWeb, T> = EventFunc<A, T>;

export type EventWebToElectron =
  | {
      name: 'app:quit';
      func: () => void;
    }
  | {
      name: 'app:minimize';
      func: () => void;
    }
  | {
      name: 'app:maximize_or_restore';
      func: () => void;
    }
  | {
      name: 'app:close';
      func: () => void;
    }
  | {
      name: 'settings:get';
      func: () => AppSettings;
    }
  | {
      name: 'settings:set';
      func: (settings: AppSettings) => void;
    };

export type EventElectronToWeb =
  | {
      name: 'app:window_mode_changed';
      func: (isMaximized: boolean) => void;
    }
  | {
      name: 'app:window_closed';
      func: () => void;
    };
