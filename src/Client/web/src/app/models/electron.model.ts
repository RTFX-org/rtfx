import { AppSettings } from './app-settings';

export type EventName = ElectronEvent['name'];
export type EventFunc<A extends ElectronEvent, T> = A extends { name: T } ? ExtractFunc<A> : never;
export type FuncArgs<A> = A extends (...args: infer Args) => any ? Args : never;
export type FuncReturn<A> = A extends (args: any) => infer Return ? Promise<Return> : Promise<void>;
export type EventFuncAsync<F> = (...args: FuncArgs<F>) => FuncReturn<F>;

export type ExtractFunc<A> = A extends { func: any } ? A['func'] : never;

export type ElectronEvent =
  | {
      name: 'app:quit';
      func: () => void;
    }
  | {
      name: 'app:minimize';
      func: () => void;
    }
  | {
      name: 'app:maximize';
      func: () => void;
    }
  | {
      name: 'app:restore';
      func: () => void;
    }
  | {
      name: 'app:close';
      func: () => void;
    }
  | {
      name: 'app:move';
      func: (x: number, y: number) => void;
    }
  | {
      name: 'settings:get';
      func: () => AppSettings;
    }
  | {
      name: 'settings:set';
      func: (settings: AppSettings) => void;
    };