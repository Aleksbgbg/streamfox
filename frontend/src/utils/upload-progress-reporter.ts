export interface UploadReport {
  uploadedBytes: number;
  totalBytes: number;
}

export interface ProgressReport {
  uploadedFraction: number;
  dataRateBytesPerSec: number;
}

export type UploadReportFunc = (uploadReport: UploadReport) => void;
export type ProgressReportFunc = (progressReport: ProgressReport) => void;

export interface Timer {
  beginRecording(): void;
  elapsedTimeSecs(): number;
}

function timeNow(): number {
  return new Date().getTime();
}

class RealTimer implements Timer {
  private _startTime = 0;

  public beginRecording(): void {
    this._startTime = timeNow();
  }

  public elapsedTimeSecs(): number {
    return (timeNow() - this._startTime) / 1_000;
  }
}

export function createProgressReporter(
  onReportProgress: ProgressReportFunc,
  timer: Timer = new RealTimer()
): UploadReportFunc {
  timer.beginRecording();
  return function (uploadReport) {
    onReportProgress({
      uploadedFraction: uploadReport.uploadedBytes / uploadReport.totalBytes,
      dataRateBytesPerSec: uploadReport.uploadedBytes / timer.elapsedTimeSecs(),
    });
  };
}
