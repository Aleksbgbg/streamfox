import { type Option, none, some } from "@/types/option";

export type CallbackFn = () => void;

export class ContinuousCallbackTimer {
  private readonly _timeoutMs: number;
  private readonly _callback: CallbackFn;

  private _timeout: Option<ReturnType<typeof setTimeout>> = none();
  private _startTime: Option<number> = none();
  private _timeRemainingMs = 0;

  public constructor(timeoutMs: number, callback: CallbackFn) {
    this._timeoutMs = timeoutMs;
    this._callback = callback;

    this._timeRemainingMs = this._timeoutMs;
  }

  public pause() {
    if (this._startTime.isNone()) {
      return;
    }

    this._timeRemainingMs -= performance.now() - this._startTime.get();
    this.cancel();
  }

  public resume() {
    this._startTime = some(performance.now());
    this._timeout = some(setTimeout(this.callback.bind(this), this._timeRemainingMs));
  }

  public cancel() {
    if (this._timeout.isNone()) {
      return;
    }

    clearTimeout(this._timeout.get());
    this._timeout = none();
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
