import { ElapsedTime } from "@/utils/elapsed-time";
import { ProgressReport } from "@/utils/progress-report";
import { UploadedDataReport } from "@/utils/uploaded-data-report";

export class UploadProgressReporter {
  private readonly _elapsedTime: ElapsedTime;

  public constructor(elapsedTime: ElapsedTime) {
    this._elapsedTime = elapsedTime;
  }

  public createProgressReporter(
    reportProgress: (progressReport: ProgressReport) => void
  ): (uploadedDataReport: UploadedDataReport) => void {
    this._elapsedTime.beginRecording();
    return (uploadedDataReport: UploadedDataReport) => {
      reportProgress({
        uploadedFraction: uploadedDataReport.loaded / uploadedDataReport.total,
        dataRateBytesPerSecond:
          uploadedDataReport.loaded / this._elapsedTime.reportElapsedTimeSeconds(),
      });
    };
  }
}
