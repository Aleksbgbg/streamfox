import { ElapsedTime } from "@/helpers/elapsed-time";
import { ProgressReport } from "@/helpers/progress-report";
import { UploadedDataReport } from "@/helpers/uploaded-data-report";

export class UploadProgressReporter {
  private readonly _elapsedTime: ElapsedTime;

  public constructor(elapsedTime: ElapsedTime) {
    this._elapsedTime = elapsedTime;
  }

  public createProgressReporter(callback: (progressReport: ProgressReport) => void): (uploadedDataReport: UploadedDataReport) => void {
    this._elapsedTime.beginRecording();
    return (uploadedDataReport: UploadedDataReport) => {
      callback({
        uploadedFraction: uploadedDataReport.loaded / uploadedDataReport.total,
        dataRateBytesPerSecond: uploadedDataReport.loaded / this._elapsedTime.reportElapsedTimeSeconds()
      });
    };
  }
}