import { type Optional, empty, getValue, hasValue } from "@/types/optional";

export type CallbackFn = () => void;

export class CallbackTimer {
  private readonly _callback: CallbackFn;
  private _timeout: Optional<ReturnType<typeof setTimeout>> = empty();
  private _startTime: Optional<number> = empty();
  private _timeRemainingMs: number;
  private _complete = false;

  public constructor(timeoutMs: number, callback: CallbackFn) {
    this._callback = callback;
    this._timeRemainingMs = timeoutMs;
  }

  public pause() {
    if (this._complete) {
      return;
    }

    if (!hasValue(this._startTime)) {
      return;
    }

    const timeDifferenceMs = performance.now() - getValue(this._startTime);
    this.cancel();
    this._timeRemainingMs -= timeDifferenceMs;
  }

  public resume() {
    if (this._complete) {
      return;
    }

    this._startTime = performance.now();
    this._timeout = setTimeout(this.callback.bind(this), this._timeRemainingMs);
  }

  public cancel() {
    if (!hasValue(this._timeout)) {
      return;
    }

    clearTimeout(getValue(this._timeout));
    this._timeout = empty();
  }

  private callback() {
    this._complete = true;
    this._callback();
  }
}
