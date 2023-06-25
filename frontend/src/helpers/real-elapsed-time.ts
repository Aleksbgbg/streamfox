import { ElapsedTime } from "@/helpers/elapsed-time";

export class RealElapsedTime implements ElapsedTime {
  private _startTime = 0;

  public beginRecording(): void {
    this._startTime = RealElapsedTime.timeNow();
  }

  public reportElapsedTimeSeconds(): number {
    return (RealElapsedTime.timeNow() - this._startTime) / 1_000;
  }

  private static timeNow(): number {
    return new Date().getTime();
  }
}
