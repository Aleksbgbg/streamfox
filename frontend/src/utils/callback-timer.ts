import { type Optional, empty, getValue, hasValue } from "@/types/optional";

export type CallbackFn = () => void;

export class ContinuousCallbackTimer {
  private readonly _timeoutMs: number;
  private readonly _callback: CallbackFn;

  private _timeout: Optional<ReturnType<typeof setTimeout>> = empty();
  private _startTime: Optional<number> = empty();
  private _timeRemainingMs = 0;

  public constructor(timeoutMs: number, callback: CallbackFn) {
    this._timeoutMs = timeoutMs;
    this._callback = callback;

    this._timeRemainingMs = this._timeoutMs;
  }

  public pause() {
    if (!hasValue(this._startTime)) {
      return;
    }

    this._timeRemainingMs -= performance.now() - getValue(this._startTime);
    this.cancel();
  }

  public resume() {
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

  private restart() {
    this._timeRemainingMs = this._timeoutMs;
    this.resume();
  }

  private callback() {
    this._callback();
    this.restart();
  }
}
