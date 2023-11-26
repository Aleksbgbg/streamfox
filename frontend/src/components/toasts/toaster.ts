import type { App, InjectionKey } from "vue";
import { type Optional, getValue } from "@/types/optional";
import { injectStrict } from "@/utils/inject";

export enum ToastType {
  Success,
  Failure,
}

export interface Toast {
  type: ToastType;
  message: string;
}

export interface ToastArea {
  add(toast: Toast): void;
  pop(): void;
}

export interface Toaster {
  success(message: string): void;
  failure(message: string): void;

  successAll(messages: string[]): void;
  failureAll(messages: string[]): void;
}

export interface RegisterToastArea {
  register(area: ToastArea): void;
}

export const toastAreaKey = Symbol() as InjectionKey<ToastArea>;
export const toasterKey = Symbol() as InjectionKey<Toaster>;
export const registerToastAreaKey = Symbol() as InjectionKey<RegisterToastArea>;

export function toaster(app: App) {
  let toastArea: Optional<ToastArea> = null;

  function addToast(toast: Toast) {
    getValue(toastArea).add(toast);
  }

  app.provide(toasterKey, {
    success(message) {
      addToast({ type: ToastType.Success, message });
    },
    failure(message) {
      addToast({ type: ToastType.Failure, message });
    },
    successAll(messages) {
      for (const message of messages) {
        this.success(message);
      }
    },
    failureAll(messages) {
      for (const message of messages) {
        this.failure(message);
      }
    },
  });
  app.provide(registerToastAreaKey, {
    register(area) {
      toastArea = area;
    },
  });
}

export function useToaster(): Toaster {
  return injectStrict(toasterKey);
}
